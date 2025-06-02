using UnityEngine;

public interface IHostile
{
    Transform TargetTransform { get; }
    void OnAggro();
    void OnLoseAggro();
}