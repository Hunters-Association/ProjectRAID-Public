using UnityEngine;

public class FallGuys : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            GameManager.Instance.ReloadScene();
        }
    }
}
