using System;
using System.Collections;
using System.Collections.Generic;
using System.Transactions;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class WalkCompletePoint : MonoBehaviour
{
    [SerializeField] private SphereCollider sphereCollider;
    [SerializeField] private ParticleSystem pointParticle;

    public event Action onComplete;

    public void Init()
    {
        sphereCollider = GetComponent<SphereCollider>();
    }

    public void SetActive(bool isActive)
    {
        gameObject.SetActive(isActive);

        if (isActive)
            pointParticle.Play(true);
        else
            pointParticle.Stop(true, ParticleSystemStopBehavior.StopEmitting);    
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            // 다음 튜토리얼 진행
            onComplete?.Invoke();
            sphereCollider.enabled = false;
        }
    }
}
