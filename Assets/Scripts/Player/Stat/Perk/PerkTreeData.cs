using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "PerkTree", menuName = "Data/Player/Perk Tree")]
public class PerkTreeData : ScriptableObject
{
    public List<PerkNodeData> nodes = new();
}

[System.Serializable]
public class PerkNodeData
{
    public PerkData perk;
    public Vector2 position;
}
