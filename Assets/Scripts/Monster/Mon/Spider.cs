using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Spider : Monster
{
    [Header("거미 전용 설정")]
    [Tooltip("배회 시 기본 반경에 곱할 배율")]
    public float wanderRadiusMultiplier = 1.5f;


    // --- 초기화 ---
    protected override void Awake()
    {
        base.Awake();
        agent = GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.speed = MoveSpeed;
        }
        else Debug.LogError("Spider 프리팹에 NavMeshAgent 없음!", this);
    }

    public override float WanderRadius => (monsterData?.WanderRadius ?? base.WanderRadius) * wanderRadiusMultiplier;
    public void LookAwayFromTarget(Transform target)
    {
        if (target != null && agent != null && agent.enabled)
        {
            // agent.updateRotation = false; // 여기서도 필요하면 설정

            // 타겟에서 거미를 향하는 방향 벡터 계산
            Vector3 directionToSpider = (transform.position - target.position).normalized;
            directionToSpider.y = 0; // 수평 방향만

            // 방향 벡터가 유효하면 회전 적용
            if (directionToSpider.sqrMagnitude > 0.0001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(directionToSpider);
                transform.rotation = targetRotation;
                // Debug.Log($"[{gameObject.name}] LookAwayFromTarget (Spider): 회전 적용됨 -> {targetRotation.eulerAngles}");
            }
        }
    }

    public override void ResetMonster() { base.ResetMonster(); }
}
