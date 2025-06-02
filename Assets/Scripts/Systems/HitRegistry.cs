using System.Collections.Generic;
using UnityEngine;

public class HitRegistry : MonoBehaviour
{
    private readonly HashSet<GameObject> hitTargets = new();

    public void Register(GameObject target)
    {
        hitTargets.Add(target);
    }

    public bool HasHit(GameObject target)
    {
        return hitTargets.Contains(target);
    }

    public void ClearHits()
    {
        hitTargets.Clear();
    }
}
