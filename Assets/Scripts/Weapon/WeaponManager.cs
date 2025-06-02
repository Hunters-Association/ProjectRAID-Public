using System.Collections;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using ProjectRaid.Core;
using ProjectRaid.Data;
using ProjectRaid.Extensions;
using ProjectRaid.EditorTools;
using DG.Tweening;
using TMPro;

/// <summary>
/// 플레이어의 무기를 관리한다. (장착, 꺼내기/집어넣기, Animator Override 적용)
/// </summary>
[DisallowMultipleComponent]
public class WeaponManager : MonoBehaviour
{
    public enum HandType { Left, Right }
    public enum TestMode { ID, Prefab, Object }

    #region INSPECTOR
    [FoldoutGroup("레퍼런스", ExtendedColor.White)]
    [SerializeField] private PlayerController player;
    [SerializeField] private MultiParentConstraint weaponConstraint;
    // [SerializeField] private IKTargetFollower iKLeftFollower;
    // [SerializeField] private IKTargetFollower iKRightFollower;
    [SerializeField] private Transform weaponParent;

    [FoldoutGroup("UI", ExtendedColor.White)]
    [SerializeField] private CanvasGroup comboUI;
    [SerializeField] private TextMeshProUGUI comboCount;

    [FoldoutGroup("Tween 설정", ExtendedColor.Cyan)]
    [SerializeField] private float rotateDuration = 0.15f;
    [SerializeField] private float transitionDuration = 0.25f;

    [FoldoutGroup("무기 (테스트)", ExtendedColor.DodgerBlue)]
    [SerializeField] private TestMode testMode = TestMode.ID;
    [SerializeField, Min(1000)] private int testWeaponID = 1000;
    [SerializeField] private AttackComponent testWeaponPrefab;
    [SerializeField] private AttackComponent testWeaponObject;
    #endregion

    private Animator animator;
    private AudioSource audioSource;
    private float motionValue;

    [field: SerializeField] public bool IsWeaponInHand { get; private set; }

    private void Start()
    {
        if (player != null)
        {
            animator = player.Animator;
            audioSource = player.AudioSource;
        }
        else
        {
            if (!TryGetComponent(out animator))
                Debug.LogWarning("[WeaponManager] Animator를 찾을 수 없습니다.");

            if (!TryGetComponent(out audioSource))
                Debug.LogWarning("[WeaponManager] AudioSource를 찾을 수 없습니다.");

            audioSource = GetComponent<AudioSource>();
        }

        if (testMode is TestMode.Object)
        {
            TestEquip(testWeaponObject);
        }
        else
        {
            StartCoroutine(WaitUntilManagerInitialize());
        }
    }

    private IEnumerator WaitUntilManagerInitialize()
    {
        yield return new WaitUntil(() => GameManager.Instance.IsDataInitialized);

        switch (testMode)
        {
            case TestMode.ID:       Equip(testWeaponID); break;
            case TestMode.Prefab:   Equip(testWeaponPrefab.GetWeaponData()); break;
            // case TestMode.Object:   TestEquip(testWeaponObject); break;

            default: break;
        }
    }

    #region PUBLIC API
    public AttackComponent CurrentWeapon { get; private set; }
    public WeaponData CurrentData => CurrentWeapon ? CurrentWeapon.GetWeaponData() : null;
    public CanvasGroup ComboUI => comboUI;
    public TextMeshProUGUI ComboCount => comboCount;

    public void Equip(int id) => Equip(GameManager.Instance.Database.GetItem(id));
    public void Equip(ItemData data) => Equip((WeaponData)data.Equipment);
    
    /// <summary>
    /// 무기를 장착하고, 기존 무기가 있다면 교체
    /// </summary>
    public void Equip(WeaponData data)
    {
        if (data == null)
        {
            Debug.LogError($"[WeaponManager] WeaponData가 없습니다. - 장착이 중단됩니다.");
            return;
        }

        // 1) 기존 무기 제거
        if (CurrentWeapon != null)
        {
            Destroy(CurrentWeapon.gameObject);
            CurrentWeapon = null;
        }

        // 2) 모델 인스턴스화 & 세팅
        var item = GameManager.Instance.Database.GetItem(data.EquipmentID);
        if (item == null || item.Prefab == null) return;
        GameObject instance = Instantiate(item.Prefab, weaponParent);
        CurrentWeapon = instance.GetOrAddComponent<AttackComponent>();
        
        SheatheWeaponOnBack();
    }

    public void TestEquip(AttackComponent weapon)
    {
        if (weapon == null)
        {
            Debug.LogError($"[WeaponManager] Weapon이 없습니다. - 장착이 중단됩니다.");
            return;
        }

        var data = weapon.GetWeaponData();
        if (data == null)
        {
            Debug.LogError($"[WeaponManager] WeaponData가 없습니다. - 장착이 중단됩니다.");
            return;
        }

        player.CombatController.SetHitbox(weapon);
        player.IKTargetFollower.SetFollowTarget(weapon.IKTarget);

        CurrentWeapon = weapon;
        SheatheWeaponOnBack();
        // animator.SetInteger(PlayerAnimatorParams.WeaponID, data.GetAnimatorWeaponIndex());
    }

    public void SetMotionValue(float value = 1f) => motionValue = value;
    public void PlayWeaponSFX(WeaponSFX type)
    {
        // 라이플 무기의 경우 발사까지 여기서 처리 (임시 코드)
        if (CurrentData.Class is WeaponClass.Rifle && type is WeaponSFX.Attack)
        {
            CurrentWeapon.Fire(motionValue);
            GameManager.Instance.CameraShake.Shake();
        }

        var clip = CurrentData.GetRandomSFX(type);
        if (clip != null && clip.Clip != null) audioSource.PlayOneShot(clip.Clip, clip.Volume);
    }

    // public void DrawWeapon() => animator.SetTrigger(PlayerAnimatorParams.Equip);
    // public void SheatheWeapon() => animator.SetTrigger(PlayerAnimatorParams.Unequip);

    // public void SetCombatState(bool enter) => animator.SetBool(PlayerAnimatorParams.InCombat, enter); 
    #endregion

    #region ANIMATION EVENT HOOKS
    public void AttachToHand()
    {
        if (CurrentWeapon == null) return;

        IsWeaponInHand = true;

        Sequence sequence = DOTween.Sequence()
            .AppendInterval(rotateDuration)
            .AppendCallback(EquipWeaponInRightHand);
    }

    public void AttachToBack()
    {
        if (CurrentWeapon == null) return;

        IsWeaponInHand = false;

        Sequence sequence = DOTween.Sequence()
            .AppendInterval(rotateDuration)
            .AppendCallback(SheatheWeaponOnBack)
            .AppendCallback(() => player.SetCanRun(true));
    }

    public void SetWeaponHand(HandType hand)
    {
        switch (hand)
        {
            case HandType.Left:
                EquipWeaponInLeftHand();
                break;
            case HandType.Right:
                EquipWeaponInRightHand();
                break;
        }
    }
    #endregion

    #region PRIVATE HELPERS
    private void SheatheWeaponOnBack()
    {
        DOTween.Kill(this); // 중복 트윈 방지

        if (IsWeaponInHand) return;

        // weaponConstraint.DOSourceWeights(transitionDuration, 1f, 0f, 0f, 0f).SetEase(Ease.InOutSine);
        weaponConstraint.DOSourceWeights(transitionDuration, 1f, 0f, 0f).SetEase(Ease.InOutSine);
        CurrentWeapon.transform.DOLocalMove(CurrentData.backOffset.position, transitionDuration).SetEase(Ease.InOutSine);
        CurrentWeapon.transform.DOLocalRotate(CurrentData.backOffset.rotation, transitionDuration).SetEase(Ease.InOutSine);
    }

    private void EquipWeaponInRightHand()
    {
        DOTween.Kill(this); // 중복 트윈 방지

        if (!IsWeaponInHand) return;

        // iKLeftFollower.SetFollowTarget(CurrentWeapon.GripLeft);
        // iKRightFollower.SetFollowTarget(CurrentWeapon.GripRight);
        // weaponConstraint.DOSourceWeights(transitionDuration, 0f, 1f, 0f, 0f).SetEase(Ease.OutQuad);
        weaponConstraint.DOSourceWeights(transitionDuration, 0f, 1f, 0f).SetEase(Ease.OutQuad);
        CurrentWeapon.transform.DOLocalMove(CurrentData.handOffset.position, transitionDuration).SetEase(Ease.OutQuad);
        CurrentWeapon.transform.DOLocalRotate(CurrentData.handOffset.rotation, transitionDuration).SetEase(Ease.OutQuad);
    }

    private void EquipWeaponInLeftHand()
    {
        DOTween.Kill(this); // 중복 트윈 방지

        if (!IsWeaponInHand) return;

        // iKLeftFollower.SetFollowTarget(CurrentWeapon.GripRight);
        // iKRightFollower.SetFollowTarget(CurrentWeapon.GripLeft);
        // weaponConstraint.DOSourceWeights(transitionDuration, 0f, 0f, 1f, 0f).SetEase(Ease.OutQuad);
        weaponConstraint.DOSourceWeights(transitionDuration, 0f, 0f, 1f).SetEase(Ease.OutQuad);
        CurrentWeapon.transform.DOLocalMove(CurrentData.handOffset.position, transitionDuration).SetEase(Ease.OutQuad);
        CurrentWeapon.transform.DOLocalRotate(CurrentData.handOffset.rotation, transitionDuration).SetEase(Ease.OutQuad);
    }
    #endregion
}
