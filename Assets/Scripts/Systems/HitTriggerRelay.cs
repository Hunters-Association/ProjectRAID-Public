using UnityEngine;
using ProjectRaid.EditorTools;

public class HitTriggerRelay : MonoBehaviour
{
    [SerializeField] private AttackComponent attackComponent;

    public void Setup(AttackComponent weapon) => attackComponent = weapon;
    private void OnTriggerEnter(Collider other) => attackComponent.OnTriggerEnterHook(other);
}
