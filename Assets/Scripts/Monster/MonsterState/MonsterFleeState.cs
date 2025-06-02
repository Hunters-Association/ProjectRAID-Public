using UnityEngine;
using UnityEngine.AI;

public class MonsterFleeState : MonsterBaseState
{
    private Transform fleeTargetTransform;
    private Vector3 fleeTargetPosition;
    private float visibilityCheckTimer;
    private const float VISIBILITY_CHECK_INTERVAL = 0.5f; private bool reachedDestination = false;
    private Transform playerTransform;
    private Camera playerCamera; // 참조 필요

    public MonsterFleeState(Monster contextMonster) : base(contextMonster) { }

    public override void EnterState()
    {

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
            playerCamera = Camera.main;
        }

        FindFleeTarget();
        if (fleeTargetTransform == null)
        {
            monster.ChangeState(MonsterState.Idle);
            return;
        }
        float searchRadius = 2.0f;
        if (NavMesh.SamplePosition(fleeTargetTransform.position, out NavMeshHit hit, searchRadius, NavMesh.AllAreas))
        {
            fleeTargetPosition = hit.position;
            monster.StartMovement(fleeTargetPosition);
            visibilityCheckTimer = Time.time + VISIBILITY_CHECK_INTERVAL;
        }
        else
        {
            monster.ChangeState(MonsterState.Idle);
        }
    }
    public override void UpdateState()
    {
        if (fleeTargetTransform == null)
        {
            monster.ChangeState(MonsterState.Idle);
            return;
        }
        if (!reachedDestination && monster.HasReachedDestination())
        {
            reachedDestination = true;
            monster.StopMovement(); if (CheckVisibilityAndTryEscape()) return;
        }
        if (Time.time >= visibilityCheckTimer)
        {
            if (CheckVisibilityAndTryEscape()) return;
            visibilityCheckTimer = Time.time + VISIBILITY_CHECK_INTERVAL;
        }
    }
    public override void ExitState()
    {
        fleeTargetTransform = null; monster.StopMovement();
    }
    private void FindFleeTarget()
    {
        GameObject closestPoint = null;
        float minDistanceSqr = Mathf.Infinity;
        string targetTag = monster.monsterData?.fleeTargetTag ?? "FleePoint"; // MonsterData에서 태그 읽기 (기본값 FleePoint)
        GameObject[] fleePoints = GameObject.FindGameObjectsWithTag(targetTag);
        if (fleePoints.Length == 0)
        {

            fleeTargetTransform = null;
            return;
        }
        foreach (GameObject point in fleePoints)
        {
            float distSqr = (point.transform.position - monster.transform.position).sqrMagnitude;
            if (distSqr < minDistanceSqr)
            {
                minDistanceSqr = distSqr; closestPoint = point;
            }
        }
        if (closestPoint != null) fleeTargetTransform = closestPoint.transform;
        else fleeTargetTransform = null;
    } // 혹시 모를 경우 대비 }
    private bool CheckVisibilityAndTryEscape()
    {
        if (playerTransform == null || playerCamera == null) return false;
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(playerCamera);
        Collider col = monster.monsterCollider;
        if (col != null && !GeometryUtility.TestPlanesAABB(planes, col.bounds))
        {
            monster.HandleSuccessfulFlee();
            return true;
        }
        Vector3 monsterCenter = col != null ? col.bounds.center : monster.transform.position + Vector3.up * 0.5f;
        Vector3 dirToMonster = monsterCenter - playerCamera.transform.position;
        float distToMonster = dirToMonster.magnitude;
        int layerMaskToIgnore = LayerMask.GetMask("Player", "EnemyOnly", "Hurtbox", "Hitbox", "Weapon", "PlayerOnly");
        int obstacleMask = ~layerMaskToIgnore;
        RaycastHit hit;
        if (Physics.Raycast(playerCamera.transform.position, dirToMonster.normalized, out hit, distToMonster - 0.1f, obstacleMask))
        {
            if (!hit.collider.transform.IsChildOf(monster.transform) && hit.collider.transform != monster.transform)
            { monster.HandleSuccessfulFlee(); return true; }
        }
        return false;
    }

    public override void OnTakeDamage(DamageInfo info)
    {
        Debug.Log($"{monster.gameObject.name} took damage while Fleeing. Ignoring state change.");
    }
}

