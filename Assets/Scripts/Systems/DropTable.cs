using UnityEngine;

[System.Serializable]
public class DropTable
{
    public GameObject item;
    [Range(0f, 1f)]
    public float rate;
}
