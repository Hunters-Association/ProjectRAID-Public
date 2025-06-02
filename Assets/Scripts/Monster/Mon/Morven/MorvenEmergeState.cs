using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MorvenEmergeState : MonsterBaseState
{
    private Morven morven;
    private float emergeTimer;
    private Coroutine emergeCoroutine;
    private Vector3 emergePosition;

    // 생성자에서 나타날 위치를 받아올 수 있도록 수정 (선택적)
    public MorvenEmergeState(Monster contextMonster, Vector3 targetPosition) : base(contextMonster)
    {
        morven = contextMonster as Morven;
        if (morven == null) Debug.LogError("MorvenEmergeState created with non-Morven monster!");
        this.emergePosition = targetPosition;
    }

    public override void EnterState()
    {
         //Debug.Log($"[{morven.gameObject.name}] Entering Emerge State at {emergePosition}.");

        // 1. 위치 설정 및 NavMeshAgent 준비
        if (morven.agent != null)
        {
            // Warp 전에 Agent 활성화 필요할 수 있음
            if (!morven.agent.enabled)
            {
                Debug.Log($"[{morven.gameObject.name}] Agent가 비활성화 상태여서 활성화합니다.");
                morven.agent.enabled = true;
            }
            //Debug.Log($"[{morven.gameObject.name}] Warp 호출 전 현재 위치: {morven.transform.position}");

            // Warp 실행 및 성공 여부 저장
            bool warpSuccess = morven.agent.Warp(this.emergePosition);

            // ★★★ 로그 3: Warp 호출 직후 결과 및 위치 확인 ★★★
            //Debug.Log($"[{morven.gameObject.name}] Warp 시도 결과: {warpSuccess}. Warp 직후 위치: {morven.transform.position}");
            // Warp는 NavMesh 위 가장 가까운 지점으로 이동시킴
            if (!morven.agent.Warp(emergePosition))
            {
                Debug.LogWarning($"[{morven.gameObject.name}] Failed to warp agent to emerge position {emergePosition}. Staying at previous position.");
                // Warp 실패 시 현재 위치에서 나타나도록 처리하거나, 다른 위치 재탐색 필요
                // emergePosition = morven.transform.position; // 임시: 현재 위치 사용
            }
            morven.agent.isStopped = true; // 나오는 중에는 움직이지 않음
        }
        else // Agent 없으면 그냥 위치 이동
        {
            Debug.LogWarning($"[{morven?.gameObject.name}] NavMeshAgent가 없습니다! Transform.position을 직접 설정합니다.");
            morven.transform.position = emergePosition;
        }

        // 2. 상태 설정
        morven.SetInvulnerable(true); // ★ 나오는 중에는 무적 
       // morven.ShowVisuals();         // 모습 보이기 시작

        // TODO: 땅에서 나오는 애니메이션 트리거
        // morven.animator.SetTrigger("Emerge");

        // 3. 타이머 시작
        emergeTimer = morven.EmergeDuration;
        if (emergeCoroutine != null) morven.StopCoroutine(emergeCoroutine);
        if (morven != null) // Null 체크 추가
            emergeCoroutine = morven.StartCoroutine(EmergeProcess());
    }

    private IEnumerator EmergeProcess()
    {
        float elapsed = 0f;
        // 여기서 시각적 효과 시작 (예: 파티클, 땅 흔들림, 몸 솟아오르기)

        while (elapsed < emergeTimer)
        {
            elapsed += Time.deltaTime;
            // 시간에 따라 시각적 요소 처리 (예: 점점 나타나기)
            // float emergeProgress = elapsed / emergeTimer;
            // morven.visualRoot?.transform.Translate(Vector3.up * Time.deltaTime * (morven.visualRoot.transform.localScale.y / emergeTimer)); // 예시

            yield return null;
        }

        // 시간 다 되면 완료 처리
        OnEmergeComplete();
    }

    private void OnEmergeComplete()
    {
        //Debug.Log($"[{morven?.gameObject.name}] Emerge 완료. 현재 위치: {morven?.transform.position}");
        morven.SetInvulnerable(false); 
        // 이동 가능하도록 Agent 설정
        if (morven.agent != null && morven.agent.enabled)
        {
            morven.agent.isStopped = false;
        }
        // 다음 상태 결정 (플레이어 감지 여부에 따라)
        if (morven.DetectPlayer())
        {
            morven.ChangeState(MonsterState.Attack);
        }
        else
        {
            morven.ChangeState(MonsterState.Idle);
        }
    }

    public override void UpdateState()
    {
        // 코루틴이 타이머 관리
    }

    public override void ExitState()
    {
        if (emergeCoroutine != null)
        {
            morven.StopCoroutine(emergeCoroutine);
            emergeCoroutine = null;
        }
        // Agent가 멈춰있었다면 다음 상태에서 풀어줘야 함 (EmergeComplete에서 처리)
    }

    public override void OnTakeDamage(DamageInfo info)
    {
        // 나오는 중에는 무적이므로 이 함수는 호출되지 않아야 함 (Monster.cs에서 체크)
        // Debug.LogWarning($"[{morven.gameObject.name}] Took damage while Emerging (Should be invulnerable!)");
    }
}
