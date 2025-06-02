using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MorvenBurrowState : MonsterBaseState
{
    private Morven morven;
    private float burrowTimer;
    private Coroutine burrowCoroutine;

    public MorvenBurrowState(Monster contextMonster) : base(contextMonster)
    {
        morven = contextMonster as Morven;
        if (morven == null) Debug.LogError("MorvenBurrowState created with non-Morven monster!");
    }

    public override void EnterState()
    {
        // Debug.Log($"[{morven.gameObject.name}] Entering Burrow State.");
        morven.StopMovement(); // 이동 중지
        morven.SetInvulnerable(false); // ★ 땅 파는 중에는 맞을 수 있음 (기획서: Vulnerable during animation) ★

        // TODO: 땅 파는 애니메이션 트리거
        // morven.animator.SetTrigger("Burrow");

        burrowTimer = morven.BurrowDuration; // MonsterData에서 시간 가져오기
        if (burrowCoroutine != null) morven.StopCoroutine(burrowCoroutine);
        burrowCoroutine = morven.StartCoroutine(BurrowProcess());
    }

    private IEnumerator BurrowProcess()
    {
        float elapsed = 0f;
        // 여기서 시각적 효과 시작 (예: 파티클, 몸 살짝 가라앉히기)

        while (elapsed < burrowTimer)
        {
            elapsed += Time.deltaTime;
            // 시간에 따라 시각적 요소 처리 (예: 점점 투명해지거나 땅 속으로 내려가기)
            // float burrowProgress = elapsed / burrowTimer;
            // morven.visualRoot?.transform.Translate(Vector3.down * Time.deltaTime * (morven.visualRoot.transform.localScale.y / burrowTimer)); // 예시

            yield return null;
        }

        // 시간 다 되면 완료 처리
        OnBurrowComplete();
    }

    private void OnBurrowComplete()
    {
        // Debug.Log($"[{morven.gameObject.name}] Burrow Complete.");
        morven.SetInvulnerable(true); // ★ 땅 속에서는 무적 (기획서: Invulnerable after animation) ★
        //morven.HideVisuals(); // 모습 완전히 숨김
        if (morven.agent != null && morven.agent.enabled)
        {
            morven.agent.isStopped = true;
            morven.agent.enabled = false; // NavMeshAgent 비활성화 (땅속 이동 시 불필요)
        }
        morven.ChangeState(MonsterState.Burrowed); // 땅 속 상태로 전환
    }

    public override void UpdateState()
    {
        // 코루틴이 타이머 관리하므로 여기서는 할 일 없음
    }

    public override void ExitState()
    {
        if (burrowCoroutine != null)
        {
            morven.StopCoroutine(burrowCoroutine);
            burrowCoroutine = null;
        }
        // 특별히 정리할 내용 없음 (완료 시 Burrowed 상태로 넘어감)
    }

    public override void OnTakeDamage(DamageInfo info)
    {
        // 땅 파는 중 데미지 받으면?
        // 기획서상으로는 취소 없이 계속 파지만, 원한다면 여기서 상태 전환 가능
        // 예: HP가 특정량 이하로 떨어지면 즉시 Burrowed 상태로 전환?
    }
}
