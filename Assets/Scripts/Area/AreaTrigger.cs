using UnityEngine;

public class AreaTrigger : MonoBehaviour
{
    public AreaInfo areaInfo;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager.Instance.MinimapSystem.UpdateArea(areaInfo);
        }
    }
}
