using UnityEngine;

public class ChestInteractable : BaseInteractable
{
    public override void Interact(PlayerController player)
    {
        player.InventoryUI.OnShow();
        player.CurrentUI = player.InventoryUI;

        // if (UIManager.Instance != null) // UIManager 인스턴스 확인
        // {
        //     // UIManager를 통해 InventoryUI 토글
        //     bool isActive = UIManager.Instance.IsUIActive<InventoryUI>();
        //     if (isActive)
        //     {
        //         UIManager.Instance.HideUI<InventoryUI>();
        //     }
        //     else
        //     {
        //         UIManager.Instance.ShowUI<InventoryUI>();
        //     }
        //     // Debug.Log(...);
        // }
        // else
        // {
        //     Debug.LogError("[ChestInteractable] UIManager Instance not found!");
        // }
    }
}
