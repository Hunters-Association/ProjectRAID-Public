using System.Net.NetworkInformation;
using UnityEngine;

public enum DoatWingDirection
{
    Left,
    Right,
}

public class DoatWingParts : BossBodyDestructionListPart
{
    [SerializeField] private DoatWingDirection wingDirection;
    [SerializeField] private SkinnedMeshRenderer wing;

    protected override void Init()
    {
        base.Init();

        if(wing == null)
        {
            Debug.Log("날개 skinnedMeshRenderer가 연결이 되어있지 않습니다");
        }

        SetOriginMaterial();

        onDestPart += SetDestructionMaterial;
    }

    private void SetDestructionMaterial()
    {
        Material[] materials = wing.materials;

        if (wingDirection == DoatWingDirection.Left)
            materials[0] = destructionMaterial;
        else
            materials[1] = destructionMaterial;

        wing.materials = materials;
    }

    private void SetOriginMaterial()
    {
        Material[] materials = wing.materials;

        for (int i = 0; i < wing.materials.Length; i++)
        {
            materials[i] = originMaterial;
        }

        wing.materials = materials;
    }
}
