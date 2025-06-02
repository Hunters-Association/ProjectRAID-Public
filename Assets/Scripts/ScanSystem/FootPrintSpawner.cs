using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using ProjectRaid.EditorTools;

[Serializable]
public class FootPrintSpawnData
{
    public BossSO bossData;
    public GameObject footPrintObject;
    public Transform[] setPoints;
    [Range(10, 20)] public float spawnRange = 10f;
    [Range(1, 3)] public int maxCount = 2;
}

public class FootPrintSpawner : MonoBehaviour
{
    [SerializeField] private bool isTutorial;

    [SerializeField] public int spawnBossID;
    [SerializeField] private FootPrintSpawnData[] spawnDatas;
    [SerializeField, Range(5, 10)] private float maxHeight = 5;
    [SerializeField] private LayerMask groundMask;

    private FootPrintSpawnData spawnData;
    private int objectPerRange;     // 오브젝트 당 범위
    private GaugeBarView screenHP;

    public FootPrintNavigation footPrintNav;
    public List<GameObject> spawnFootPrints = new();

    private void Awake()
    {
        maxHeight = maxHeight == 0 ? 5 : maxHeight;
    }

    private void Start()
    {
        if (!isTutorial)
        {
            StartCoroutine(WaitQuestInit());
        }
    }

    private IEnumerator WaitQuestInit()
    {
        yield return new WaitUntil(() => QuestManager.Instance.PlayerQuestDataManager != null);

        screenHP = footPrintNav.GetComponentInParent<PlayerController>().ScreenHP;

        foreach (var boss in footPrintNav.spawnBosses)
        {
            boss.GetComponent<BossHealthBarTest>().screenHealthBar = screenHP;
        }

        int currentActiveQuest = 0;
        if (QuestManager.Instance.PlayerQuestDataManager.QuestData.activeQuests.Count > 0)
        {
            currentActiveQuest = QuestManager.Instance.PlayerQuestDataManager.QuestData.activeQuests.Keys.Max();
        }
        if (currentActiveQuest != 0)
        {
            OnQuestAcceptedHandler(QuestManager.Instance.Database.GetQuestByID(currentActiveQuest));
        }

        SubscribeEvent();
    }

    public void SubscribeEvent()
    {
        QuestManager.Instance.PlayerQuestDataManager.OnQuestAccepted -= OnQuestAcceptedHandler;
        QuestManager.Instance.PlayerQuestDataManager.OnQuestAccepted += OnQuestAcceptedHandler;
    }

    public void OnQuestAcceptedHandler(QuestData questData)
    {
        int targetID = (questData.objectives[0] as KillMonsterObjectiveDefinition).targetMonsterID;

        SpawnFootPrint(targetID);
        footPrintNav.SetCurrentQuest(questData);
    }

    public void ClearSpawnFootPrints()
    {
        // 새로 생성하면서 footPrintNavigation totalFootPrintPoint 초기화 시키기
        if(footPrintNav != null)
            footPrintNav.Init();

        for (int i = 0; i < spawnFootPrints.Count; i++)
        {
            Destroy(spawnFootPrints[i]);
        }

        spawnFootPrints.Clear();
    }

    public void SpawnFootPrint(int id)
    {
        spawnData = GetSpawnData(id);

        if (spawnData == null)
        {
            Debug.Log("흔적과 일치하는 ID가 아닙니다");
            return;
        }

        // =========================================================
        foreach (var boss in footPrintNav.spawnBosses)
        {
            if (boss == null) continue;
            
            var health = boss != null ? boss.GetComponent<BossHealthBarTest>() : null;
            var isTarget = boss.bossData.bossID == id;
            health.isScreenHealthTarget = isTarget;

            if (isTarget)
            {
                if(health.screenHealthBar == null)
                {
                    health.screenHealthBar = footPrintNav.GetComponentInParent<PlayerController>().ScreenHP;
                }
                health.screenHealthBar.SetRatio(1f, GaugeBarType.Width);
            }
        }
        // =========================================================

        ClearSpawnFootPrints();

        objectPerRange = (int)(spawnData.spawnRange / spawnData.maxCount) + 1;

        for (int i = 0; i < spawnData.setPoints.Length; i++)
        {
            Vector3 setPoint = spawnData.setPoints[i].position;
            float range = spawnData.spawnRange;

            int spawnCount = UnityEngine.Random.Range(1, spawnData.maxCount + 1);
            for (int j = 0; j < spawnCount; j++)
            {
                // 흔적 생성할 랜덤 위치 설정
                Vector3 randomPoint = UnityEngine.Random.insideUnitSphere * range;
                randomPoint = new Vector3(randomPoint.x, maxHeight, randomPoint.z);
                randomPoint = setPoint + randomPoint;

                // 랜덤 위치를 통한 생성 위치 설정
                Vector3 spawnPoint = GetSpawnPoint(randomPoint, out Vector3 normal);
                spawnPoint = spawnPoint != Vector3.zero ? spawnPoint : setPoint;

                // 흔적 생성
                GameObject footPrintObj = Instantiate(spawnData.footPrintObject, spawnPoint, Quaternion.identity, transform);

                // 흔적 Rotation 설정
                Vector3 forward = Vector3.ProjectOnPlane(Vector3.forward, normal).normalized;
                Quaternion quaternion = Quaternion.LookRotation(forward, normal);
                footPrintObj.transform.rotation = quaternion;

                FootPrint footPrint = footPrintObj.GetComponentInChildren<FootPrint>();
                if (footPrint != null)
                {
                    footPrint.footPrintSpawner = this;
                }

                spawnFootPrints.Add(footPrintObj);
            }
        }
    }

    public FootPrintSpawnData GetSpawnData(int id)
    {
        FootPrintSpawnData spawnData = null;

        for (int i = 0; i < spawnDatas.Length; i++)
        {
            if (spawnDatas[i].bossData.bossID == id)
            {
                spawnData = spawnDatas[i];
            }
        }
        return spawnData;
    }

    public Vector3 GetSpawnPoint(Vector3 point, out Vector3 normal)
    {
        int maxAttemps = 50;
        NavMeshHit navMeshHit;
        RaycastHit rayHit;
        normal = Vector3.up;

        for (int i = 0; i < maxAttemps; i++)
        {
            // 아래 방향
            if (Physics.Raycast(point + Vector3.up * 10f, Vector3.down, out rayHit, 100f, groundMask))
            {
                point = rayHit.point;
            }
            else
                point = Vector3.zero;

            if (NavMesh.SamplePosition(point, out navMeshHit, maxHeight, NavMesh.AllAreas))
            {
                if (IsSpawnPossible(navMeshHit.position))
                {
                    normal = rayHit.normal;
                    normal = normal.normalized;
                    return point;
                }
            }
        }

        return point;
    }

    // 생성할 포인트에 겹칠 흔적이 있는지 확인
    public bool IsSpawnPossible(Vector3 spawnPoint)
    {
        Collider[] colliders = Physics.OverlapSphere(spawnPoint, objectPerRange, LayerMask.GetMask("Interactable"));

        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].GetComponent<IInteractable>() != null)
                return false;
        }

        return true;
    }
}
