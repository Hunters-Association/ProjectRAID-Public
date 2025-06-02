using ProjectRaid.EditorTools;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class LaviesAnimationHandler : MonoBehaviour
{
    [FoldoutGroup("AttackColliders", ExtendedColor.White)]
    public Collider attackCollider;
    public Collider roarCollider;

    [FoldoutGroup("VFX", ExtendedColor.Red)]
    public ParticleSystem stepParticle;
    public ParticleSystem attackParticle;

    public void OnAttack()
    {
        if(attackCollider != null)
            attackCollider.gameObject.SetActive(true);

        if (attackParticle != null)
            attackParticle.Play();
    }

    public void OffAttack()
    {
        if (attackCollider != null)
            attackCollider.gameObject.SetActive(false);
    }

    public void OnRoar()
    {
        if (roarCollider != null)
            roarCollider.gameObject.SetActive(true);
    }

    public void OffRoar()
    {
        if (roarCollider != null)
            roarCollider.gameObject.SetActive(false);
    }

    public void OnStep()
    {
        if(stepParticle != null)
            stepParticle.Play();
    }
}
