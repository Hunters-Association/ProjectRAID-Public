using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossHealth : MonoBehaviour
{
    public float maxHP;
    public float hp;

    public event Action OnFirstHit;             // 처음 맞았을 때
    public event Action OnHit;                  // 맞았을 때 실행될 이벤트
    public event Action OnDead;                 // 죽었을 때 실행될 이벤트
    public event Action OnRetreat;              // 위험한 체력이 되었을 때 실행될 이벤트

    [SerializeField] private BossBodyPartBase[] bossBodyParts;

    public float retreatRate = 0.3f;           // run 상태로 변경될 체력 비율 일단은 30퍼센트로 지정
    private bool isRetreatState;                    // 휴식 상태로 전환이 된 적 있는지 확인

    private void OnEnable()
    {
        isRetreatState = false;

        if (hp <= 0)
            hp = maxHP;
    }

    private void Start()
    {
        bossBodyParts = GetComponentsInChildren<BossBodyPartBase>();
        
        for (int i = 0; i < bossBodyParts.Length; i++)
        {
            bossBodyParts[i].onTakeDamage += TakeDamage;
        }
    }

    public void TakeDamage(float damage)
    {
        if (hp <= 0) return;

        // 처음 맞았을 때
        if (hp == maxHP)
            OnFirstHit?.Invoke();

        hp = Mathf.Max(hp - damage, 0);

        OnHit?.Invoke();

        if(hp == 0)
        {
            // 죽은 상태로 변환
            OnDead?.Invoke();
        }
        else if(IsRetreatHealth())
        {
            isRetreatState = true;
            OnRetreat?.Invoke();
        }
    }

    public void InitHP(float maxHP)
    {
        this.maxHP = maxHP;
        hp = maxHP;
    }

    public bool IsRetreatHealth()
    {
        return hp < maxHP * 0.3f && !isRetreatState;
    }
}
