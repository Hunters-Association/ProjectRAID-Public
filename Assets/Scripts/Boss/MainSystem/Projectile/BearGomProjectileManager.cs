using ProjectRaid.Extensions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BearGomProjectileManager : MonoBehaviour
{
    public Boss boss;

    public List<GameObject> projectileList = new List<GameObject>();
    public GameObject projectileObject;

    private void Start()
    {
        Init();
    }

    public void Init()
    {
        if (projectileObject == null)
            return;

        for (int i = 0; i < 5; i++)
        {
            GameObject projectile = CreateNewObject();
            projectileList.Add(projectile);
        }
    }

    public GameObject CreateNewObject()
    {
        GameObject newProjectile = Instantiate(projectileObject, transform);
        newProjectile.SetActive(false);

        if(newProjectile.TryGetComponent(out BearGomProjectile bearGomProjectile))
        {
            bearGomProjectile.boss = boss;
            bearGomProjectile.projectileManager = this;
        }

        return newProjectile;
    }

    public GameObject GetProjectile()
    {
        if (projectileObject == null) return null;

        // 만약 리스트에 있다면 리턴
        if(projectileList.Count != 0)
        {
            GameObject projectile = projectileList[0];
            projectileList.Remove(projectile);

            return projectile;
        }

        return CreateNewObject();
    }

    public void ReturnProjectile(GameObject projectile)
    {
        projectile.SetActive(false);
        projectileList.Add(projectile);
    }
}
