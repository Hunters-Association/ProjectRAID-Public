using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerEventRelay : MonoBehaviour
{
    [SerializeField] private AttackComponent attackComponent;
    
    private void OnTriggerEnter(Collider other)
    {
        attackComponent.OnTriggerEnterHook(other);
    }
}
