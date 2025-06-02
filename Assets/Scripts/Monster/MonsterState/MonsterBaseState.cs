using UnityEngine;

public abstract class MonsterBaseState
{
    protected Monster monster;
    protected Animator animator; // 참조는 유지, 사용은 최소화

    public MonsterBaseState(Monster contextMonster)
    {
        this.monster = contextMonster;
        if (contextMonster != null) this.animator = contextMonster.animator;
    }
    public abstract void EnterState();
    public abstract void UpdateState();
    public abstract void ExitState();
    public abstract void OnTakeDamage(DamageInfo info);
    
}
