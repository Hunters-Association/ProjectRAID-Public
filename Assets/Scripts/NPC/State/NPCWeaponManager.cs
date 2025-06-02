using DG.Tweening;
using ProjectRaid.Data;
using ProjectRaid.EditorTools;
using UnityEngine;

/// <summary>
/// 플레이어의 무기를 관리한다. (장착, 꺼내기/집어넣기, Animator Override 적용)
/// </summary>
[DisallowMultipleComponent]
public class NPCWeaponManager : MonoBehaviour
{
    #region INSPECTOR
    [FoldoutGroup("레퍼런스", ExtendedColor.White)]
    [SerializeField] private Animator animator;
    [SerializeField] private Transform weaponInHand;
    [SerializeField] private Transform weaponOnBack;

    [FoldoutGroup("Tween 설정", ExtendedColor.SeaGreen)]
    [SerializeField] private float rotateDuration = 0.15f;
    [SerializeField] private float moveDuration = 0.25f;

    [FoldoutGroup("무기 (디버그)", ExtendedColor.DodgerBlue)]
    [SerializeField] private AttackComponent weapon;
    #endregion


    #region PUBLIC API
    public AttackComponent CurrentWeapon { get; private set; }
    public WeaponData CurrentData => CurrentWeapon ? CurrentWeapon.GetWeaponData() : null;

    private void Awake()
    {
        CurrentWeapon= weapon;
    }
    /// <summary>
    /// 무기를 장착하고, 기존 무기가 있다면 교체
    /// </summary>
    public void Equip(WeaponData data)
    {
        if (data == null)
        {
            Debug.LogError("[WeaponManager] WeaponData 가 없습니다. - 장착이 중단됩니다.");
            return;
        }

        // 1) 기존 무기 제거
        //if (CurrentWeapon != null)
        //{
        //    Destroy(CurrentWeapon.gameObject);
        //    CurrentWeapon = null;
        //}

        // 2) 모델 인스턴스화 & 세팅        
        CurrentWeapon = weapon;
    }
    
    #endregion

    #region ANIMATION EVENT HOOKS
    public void OnAttachWeaponToRightHand()
    {        
        if (CurrentWeapon == null) return;
        Debug.Log("무기 손");
        Sequence sequence = DOTween.Sequence();
        sequence.AppendInterval(rotateDuration)
                .AppendCallback(EquipWeaponInHand);
    }

    public void OnAttachWeaponToBackOff()
    {
        
        if (CurrentWeapon == null) return;
        Debug.Log("무기 등");
        Sequence sequence = DOTween.Sequence();
        sequence.AppendInterval(rotateDuration)
                .AppendCallback(SheatheWeaponOnBack);
                
    }
    #endregion

    #region PRIVATE HELPERS
    private void EquipWeaponInHand()
    {
        CurrentWeapon.transform.SetParent(weaponInHand);
        CurrentWeapon.transform.DOLocalMove(Vector3.zero, moveDuration).SetEase(Ease.OutQuad);
        CurrentWeapon.transform.DOLocalRotate(Vector3.zero, moveDuration).SetEase(Ease.OutQuad);
    }

    private void SheatheWeaponOnBack()
    {
        CurrentWeapon.transform.SetParent(weaponOnBack);
        CurrentWeapon.transform.DOLocalMove(Vector3.zero, moveDuration).SetEase(Ease.InOutSine);
        CurrentWeapon.transform.DOLocalRotate(Vector3.zero, moveDuration).SetEase(Ease.InOutSine);
    }
    #endregion
}
