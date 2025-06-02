using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class EffectStarter : MonoBehaviour
{
    private ParticleSystemRenderer psRenderer;
    private MaterialPropertyBlock block;

    void Start()
    {
        psRenderer = GetComponent<ParticleSystemRenderer>();
        block ??= new MaterialPropertyBlock();

        psRenderer.GetPropertyBlock(block);
        block.SetFloat("_StartTime", Time.time);
        psRenderer.SetPropertyBlock(block);
    }
}
