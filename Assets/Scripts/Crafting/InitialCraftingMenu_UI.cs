using UnityEngine;
using UnityEngine.UI;

public class InitialCraftingMenu_UI : BaseUI, IBlockingUI // ★ BaseUI 상속 확인 ★
{
    [Header("UI 참조 (인스펙터 연결)")]
    [Tooltip("무기 제작 UI를 열 버튼")]
    [SerializeField] private Button weaponCraftButton;

    [Tooltip("방어구 제작 UI를 열 버튼 (기능 구현 시)")]
    [SerializeField] private Button armorCraftButton;

    [Tooltip("파츠 제작 UI를 열 버튼 (기능 구현 시)")]
    [SerializeField] private Button partsCraftButton;

    [Tooltip("치장품 제작 UI를 열 버튼 (기능 구현 시)")]
    [SerializeField] private Button cosmeticCraftButton;

    [Tooltip("이 메뉴 UI를 닫는 버튼")]
    [SerializeField] private Button closeButton;

    public bool BlocksGameplay => true;

    // ★ WeaponCrafting_UI 직접 참조 제거 ★
    // [SerializeField] private WeaponCrafting_UI weaponCraftingUI;

    /// <summary>
    /// BaseUI의 Initialize 메서드를 재정의하여 버튼 리스너를 설정합니다.
    /// UIManager가 UI를 처음 로드하거나 풀에서 가져올 때 호출됩니다.
    /// </summary>
    public override void Initialize()
    {
        // 이미 초기화되었다면 중복 실행 방지
        if (IsInitialized) return;

        base.Initialize(); // 부모 Initialize 호출 (IsInitialized = true 설정 등)
        Debug.Log($"[{gameObject.name}] Initializing...");

        // 필수 참조 확인
        if (weaponCraftButton == null) Debug.LogError("Weapon Craft Button 참조 누락!", this);
        if (armorCraftButton == null) Debug.LogWarning("Armor Craft Button 참조 누락.", this); // 기능 구현 전이면 Warning
        if (partsCraftButton == null) Debug.LogWarning("Parts Craft Button 참조 누락.", this);
        if (cosmeticCraftButton == null) Debug.LogWarning("Cosmetic Craft Button 참조 누락.", this);
        if (closeButton == null) Debug.LogError("Close Button 참조 누락!", this);

        // 버튼 클릭 리스너 연결
        weaponCraftButton.onClick.AddListener(OnWeaponCraftClicked);
        armorCraftButton.onClick.AddListener(OnArmorCraftClicked);
        partsCraftButton.onClick.AddListener(OnPartsCraftClicked);
        cosmeticCraftButton.onClick.AddListener(OnCosmeticCraftClicked);
        // ★ 닫기 버튼은 BaseUI의 Close() 메서드 호출 ★
        closeButton.onClick.AddListener(Close);
    }

    // --- 버튼 클릭 이벤트 핸들러 ---

    private void OnWeaponCraftClicked()
    {
        Debug.Log("무기 제작 버튼 클릭됨");
        // ★ UIManager를 통해 다음 UI 표시 및 현재 UI 숨김 ★
        UIManager.Instance.HideUI<InitialCraftingMenu_UI>(); // 자신은 숨기기
        UIManager.Instance.ShowUI<WeaponCrafting_UI>(); // 이름으로 찾아서 열기
    }

    private void OnArmorCraftClicked()
    {
        Debug.Log("방어구 제작 버튼 클릭됨");
        UIManager.Instance.HideUI<InitialCraftingMenu_UI>();
        UIManager.Instance.ShowUI<ArmorCrafting_UI>();
    }

    private void OnPartsCraftClicked()
    {
        Debug.Log("파츠 제작 버튼 클릭됨 - 기능 미구현");
    }

    private void OnCosmeticCraftClicked()
    {
        Debug.Log("치장품 제작 버튼 클릭됨 - 기능 미구현");
    }

    // --- UI 라이프사이클 메서드 (BaseUI 재정의) ---

    /// <summary>
    /// UI가 표시될 때 호출됩니다. (UIManager에 의해)
    /// </summary>
    public override void OnShow()
    {
        base.OnShow();

        // TODO: 필요시 사운드 재생 등
    }

    /// <summary>
    /// UI가 숨겨질 때 호출됩니다. (UIManager에 의해)
    /// </summary>
    public override void OnHide()
    {
        base.OnHide();
    }
}
