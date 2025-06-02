using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering.Universal;
using ProjectRaid.EditorTools;
using System;
using DG.Tweening;

public class FootPrintNavigation : MonoBehaviour
{
    [FoldoutGroup("내비게이션", ExtendedColor.LightSkyBlue)]
    public Transform target;
    public NavMeshPath path;

    [FoldoutGroup("내비게이션/흔적", ExtendedColor.LightSkyBlue)]
    public Boss[] spawnBosses;     // 스폰된 보스 목록
    public int footPrintBossID;    // 흔적 보스
    public int totalFootPrintPoint;             // 총 누적된 포인트

    [FoldoutGroup("내비게이션/발자취", ExtendedColor.LightSkyBlue)]
    public bool targetDetected;
    [SerializeField] private float detectRange;
    [SerializeField] private Material decalMaterial;
    [SerializeField] private Color debugColor;
    private Vector2 decalSize;

    [FoldoutGroup("오브젝트 풀링", ExtendedColor.LightPink)]
    [SerializeField] DecalObjectPool objectPool;
    private GameObject decalObjParent;
    private float lastDecalTime;
    private readonly List<GameObject> decalObjects = new();
    private int footPrintCount = 0;

    private CanvasGroup hpCanvas;
    private QuestData currentQuest;
    private Boss currentBoss;
    private float lastBattleStartTime = 0f;

    private const int ACTIVE_POINTS = 100;      // 길안내 필요 포인트
    private const float PATH_RECALC_INTERVAL = 0.2f;
    private const float DECAL_DELAY_TIME = 0.05f;
    private const float DETECT_RANGE_MARGIN = 0.5f;

    public event Action onNavTarget;

    public bool isTutorial;

    private void Awake()
    {
        path = new();
        decalObjParent = new GameObject("DecalObjects");
    }

    private void Start()
    {
        if (objectPool == null)
        {
            objectPool = GetComponentInChildren<DecalObjectPool>();
            objectPool.Init();
        }
        spawnBosses = FindObjectsOfType<Boss>();
        decalSize.x = 5f;

        hpCanvas = GetComponentInParent<PlayerController>().ScreenHP.GetComponent<CanvasGroup>();
    }

    public void Init()
    {
        target = null;
        targetDetected = false;
        totalFootPrintPoint = 0;
        footPrintCount = 0;
    }

    private Transform GetBossTransform(int bossID)
    {
        Transform bossTransform = null;

        for (int i = 0; i < spawnBosses.Length; i++)
        {
            if (spawnBosses[i].bossData.bossID == bossID)
            {
                bossTransform = spawnBosses[i].transform;
            }
        }

        return bossTransform;
    }

    public void SetNavgationTarget(Transform target)
    {
        this.target = target;
        
        foreach (var boss in spawnBosses)
        {
            if (boss == target.GetComponentInParent<Boss>())
            {
                if (currentBoss != null) currentBoss.bossHealth.OnDead -= HandleBossDead;
                boss.bossHealth.OnDead += HandleBossDead;
                currentBoss = boss;
            }
        }
    }

    public void SetCurrentQuest(QuestData data) => currentQuest = data;

    void Update()
    {
        IsInTargetDetectRange();

        if (IsDrawPathPossible())
            DrawPath();
        else
            ClearPath();
    }

    private void HandleBossDead()
    {
        DOTween.Kill("HPFade");

        hpCanvas.DOFade(0f, 0.5f)
            .SetEase(Ease.OutCubic)
            .SetId("HPFade");

        if (target.TryGetComponent(out Boss boss))
        {
            switch (boss)
            {
                case BossLavies:    AnalyticsManager.SendFunnelStep(19); break;
                case BossDoat:      AnalyticsManager.SendFunnelStep(24); break;

                default: break;
            }

            if (isTutorial) return;
            AnalyticsManager.EndBattle(currentQuest.questID, Time.time - lastBattleStartTime);
            lastBattleStartTime = 0f;
        }

        var questCompleteUI = UIManager.Instance.ShowUI<QuestCompleteUI>();
        questCompleteUI.questName.text = currentQuest != null ? currentQuest.questName : "";
        questCompleteUI.questBoss = boss;
    }

    // 타겟이 감지 범위 안에 있는가?
    private void IsInTargetDetectRange()
    {
        if (target == null) return;

        // 감지 범위 안에 들어왔다면
        float distance = Vector3.Distance(transform.position, target.position);

        if (distance <= detectRange && !targetDetected)
        {
            targetDetected = true;

            DOTween.Kill("HPFade");

            hpCanvas.DOFade(1f, 0.5f)
                .SetEase(Ease.OutCubic)
                .SetId("HPFade");

            if (target.TryGetComponent(out Boss boss))
            {
                switch (boss)
                {
                    case BossLavies:    AnalyticsManager.SendFunnelStep(18); break;
                    case BossDoat:      AnalyticsManager.SendFunnelStep(23); break;

                    default: break;
                }


                lastBattleStartTime = Time.time;

                if (isTutorial) return;
                AnalyticsManager.StartBattle(currentQuest.questID, lastBattleStartTime);
            }
        }
        else if (distance > detectRange && !targetDetected)
        {
            targetDetected = false;
        }
    }

    // Path를 그릴 수 있는가?
    private bool IsDrawPathPossible()
    {
        // 타겟이 존재하고 타겟이 감지 범위 안에 들어온적이 없을 때
        return target != null && !targetDetected;
    }

    public void AddFootPrintPoint(int footPrintPoint)
    {
        totalFootPrintPoint += footPrintPoint;
        footPrintCount++;

        if (footPrintBossID == 201)
        {
            switch (footPrintCount)
            {
                case 1: AnalyticsManager.SendFunnelStep(12); break;
                case 2: AnalyticsManager.SendFunnelStep(13); break;
                case 3: AnalyticsManager.SendFunnelStep(14); break;
                case 4: AnalyticsManager.SendFunnelStep(15); break;
                case 5: AnalyticsManager.SendFunnelStep(16); break;

                default: break;
            }
        }

        if (totalFootPrintPoint >= ACTIVE_POINTS)
        {
            onNavTarget?.Invoke();

            // 포인트가 활성화 포인트까지 누적이 되었다면 target 설정
            SetNavgationTarget(GetBossTransform(footPrintBossID));

            if (footPrintBossID == 201)
                AnalyticsManager.SendFunnelStep(17);
        }
    }

    public void ClearPath()
    {
        int count = decalObjects.Count;

        for (int i = 0; i < count; i++)
        {
            GameObject returnObj = decalObjects[0];

            objectPool.ReturnObj(returnObj);

            decalObjects.RemoveAt(0);
        }

        decalObjects.Clear();
    }

    private void DrawPath()
    {
        if (Time.time - lastDecalTime < DECAL_DELAY_TIME) return;

        if (NavMesh.CalculatePath(transform.position, target.position, NavMesh.AllAreas, path))
        {
            // 기존 경로 클리어
            ClearPath();

            ActiveDecalProjector(path);

            lastDecalTime = Time.time;
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = debugColor;
        Gizmos.DrawWireSphere(transform.position, detectRange);

        float radius = 1f;

        Gizmos.color = Color.red;

        if (path == null) return;
        for (int i = 0; i < path.corners.Length; i++)
        {
            Gizmos.DrawWireSphere(path.corners[i], radius);
        }

        for (int i = 0; i < path.corners.Length - 1; i++)
        {
            Vector3 a = path.corners[i];
            Vector3 b = path.corners[i + 1];

            Vector3 a_To_b = (b - a);
            Vector3 dir = a_To_b.normalized;

            Gizmos.color = Color.blue;
            Gizmos.DrawRay(a, dir * 15);

            Vector3 right = Vector3.Cross(Vector3.up, dir);

            Gizmos.color = Color.red;
            Gizmos.DrawRay(a, right * 15);

            Gizmos.color = Color.green;
            Vector3 up = Vector3.Cross(dir, right);
            Gizmos.DrawRay(a, up * 15);

            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(a, -up * 15);
        }
    }

    public void ActiveDecalProjector(NavMeshPath path)
    {

        for (int i = 0; i < path.corners.Length - 1; i++)
        {
            Vector3 a = path.corners[i];
            Vector3 b = path.corners[i + 1];

            // 경로 방향
            Vector3 a_To_b = (b - a);
            Vector3 dir = a_To_b.normalized;

            // 경로의 가운데 방향, Decal 오브젝트의 위치
            Vector3 a_To_b_Center = (a_To_b / 2);
            Vector3 position = a + a_To_b_Center;

            // 가져온 오브젝트 설정

            // 오브젝트 생성
            GameObject decalObj = objectPool.GetObject();
            decalObjects.Add(decalObj);
            decalObj.transform.SetParent(decalObjParent.transform);

            // 오브젝트 설정
            decalObj.transform.position = position;
            decalObj.transform.rotation = Quaternion.LookRotation(dir);
            decalObj.transform.Rotate(Vector3.right * 90);

            //Decal Projector 설정
            DecalProjector decal = decalObj.GetComponent<DecalProjector>();
            decal.material = decalMaterial;
            decalSize.y = a_To_b.magnitude;
            decal.size = new Vector3(decalSize.x, decalSize.y, 10f);
            decal.uvScale = new Vector2(1f, decalSize.y / decalSize.x);
            decal.uvBias = new Vector2(0f, -(decalSize.y / decalSize.x)/2);

            // 반대로 찍히는 현상 방지
            if (Vector3.Dot(dir, decalObj.transform.up.normalized) < 0)
                decalObj.transform.Rotate(Vector3.forward, 180, Space.Self);
        }
    }
}
