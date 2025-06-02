using UnityEngine;

public class FootPrint : BaseScanable, IInteractable
{
    [SerializeField] private InteractableData interactableData;
    [SerializeField] private BossSO bossData;
    [SerializeField] private Collider interactableCollider;
    [HideInInspector]public FootPrintSpawner footPrintSpawner;

    public int footPrintPoint = 25;              // 조사 포인트

    public bool canInteractable;        // 상호작용이 가능한가?
    [SerializeField] private bool isPlayerNear;

    public override void Init()
    {
        base.Init();
        canInteractable = true;
        interactableCollider = GetComponent<Collider>();
    }

    public override void ActiveOutLine()
    {
        if (!canInteractable) return;

        base.ActiveOutLine();
    }

    public override void CancleOutLine()
    {
        if (isPlayerNear && canInteractable) return;
        base.CancleOutLine();
    }

    public InteractableData GetInteractableData()
    {
        if (interactableData == null) Debug.LogWarning("[FootPrint] 상호작용 데이터가 등록되지 않았습니다.");
        return interactableData;
    }

    public void Interact(PlayerController player)
    {
        if(canInteractable)
        {
            canInteractable = false;
            CancleOutLine();
            interactableCollider.enabled = false;
        }

        FootPrintNavigation footPrintNav = player.transform.GetComponentInChildren<FootPrintNavigation>();
        if(footPrintNav != null)
        {
            if(bossData != null)
                footPrintNav.footPrintBossID = bossData.bossID;

            footPrintNav.AddFootPrintPoint(footPrintPoint);
        }
    }

    public void HideHighlight() 
    {
        isPlayerNear = false;
        CancleOutLine(); 
    }

    public void ShowHighlight() 
    {
        isPlayerNear = true;
        ActiveOutLine(); 
    }
}
