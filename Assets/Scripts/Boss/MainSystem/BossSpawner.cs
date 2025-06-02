using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SpawnBossInfo
{
    public GameObject bossPrefab;
    public Transform nestTr;
}

public class BossSpawner : MonoBehaviour
{
    public SpawnBossInfo[] spawnBossInfo;
    public List<Boss> spawnBosses = new();
    public FootPrintNavigation footPrintNav;

    private void Start()
    {
        // 처음에 설정되어있는 모든 보스들을 소환
        SpawnAllBoss();

        footPrintNav.spawnBosses = spawnBosses.ToArray(); 
    }

    private void SpawnAllBoss()
    {
        for (int i = 0; i < spawnBossInfo.Length; i++)
        {
            GameObject bossPrefab = spawnBossInfo[i].bossPrefab;
            if (bossPrefab == null) continue;

            Transform nestPoint = spawnBossInfo[i].nestTr;
            if (nestPoint == null) continue;

            GameObject bossObj = Instantiate(bossPrefab, nestPoint.position, Quaternion.identity);

            if (bossObj.TryGetComponent(out Boss boss))
            {
                spawnBosses.Add(boss);

                if (spawnBossInfo[i].nestTr != null)
                    boss.nest = spawnBossInfo[i].nestTr;
            }
            if(bossObj.TryGetComponent(out BossHealth bossHealth))
            {
                bossHealth.OnDead += () => { StartCoroutine(WaitReSpawn(boss)); };
            }
        }
    }

    private IEnumerator WaitReSpawn(Boss boss)
    {
        while (boss.gameObject.activeSelf)
        {
            yield return null;
        }

        yield return new WaitForSeconds(boss.bossData.bossSTime - boss.enableTime);

        // 스폰 지역으로 리스폰
        ReSpawn(boss);
    }

    private void ReSpawn(Boss boss)
    {
        boss.transform.position = boss.nest.position;
        boss.gameObject.SetActive(true);
    }
}
