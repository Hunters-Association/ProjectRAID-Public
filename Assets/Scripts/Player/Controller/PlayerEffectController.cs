using UnityEngine;
using ProjectRaid.EditorTools;
using System.Collections.Generic;

public enum PlayerAttackVfxType
{
    None,
    Fire01,
    Fire02,
    Fire03,
    Fire04,
    Fire05,
    Fire06,
    Slash01,
    Slash02,
    Slash03,
    Slash04,
    Slash05,
    Slash06,
}

[System.Serializable]
public class VfxEntry
{
    public PlayerAttackVfxType type;
    public ParticleSystem particle;
}

public class PlayerEffectController : MonoBehaviour
{
    [FoldoutGroup("VFX", ExtendedColor.Cyan)]
    [SerializeField] private List<VfxEntry> vfxEntries;

    public Dictionary<PlayerAttackVfxType, ParticleSystem> PlayerAttackVfxs { get; } = new();

    private void Awake()
    {
        foreach (var entry in vfxEntries)
        {
            if (entry.type == PlayerAttackVfxType.None || entry.particle == null) continue;
            PlayerAttackVfxs.Add(entry.type, entry.particle);
        }
    }

    public void Play(PlayerAttackVfxType type)
    {
        PlayerAttackVfxs[type].Play();
    }
}
