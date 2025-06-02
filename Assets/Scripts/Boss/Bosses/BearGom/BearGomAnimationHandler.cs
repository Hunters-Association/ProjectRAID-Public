using ProjectRaid.EditorTools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BearGomAnimationHandler : MonoBehaviour
{
    private BossBearGom bearGom;
    private BearGomProjectileManager projectileManager;
    public Transform lavaBreathPos;

    [FoldoutGroup("AttackColliders", ExtendedColor.White)]
    public BossHitbox bodyPressAttack;
    public BossHitbox biteAttack;
    public BossHitbox rightLegAttack;
    public BossHitbox frontLegAttack;
    public BossHitbox roarAttack;

    private void Start()
    {
        bearGom = GetComponentInParent<BossBearGom>();
        projectileManager = bearGom.projectileManager;
    }

    public void LavaBreath()
    {
        GameObject lavaBreath = projectileManager.GetProjectile();

        if (lavaBreathPos != null)
        {
            lavaBreath.transform.position = lavaBreathPos.position;
            lavaBreath.SetActive(true);
        }
    }

    public void OnBiteAttack()
    {
        if (biteAttack != null)
            biteAttack.gameObject.SetActive(true);
    }

    public void OffBiteAttack()
    {
        if (biteAttack != null)
            biteAttack.gameObject.SetActive(false);
    }

    public void OnRightLegAttack()
    {
        if (rightLegAttack != null)
            rightLegAttack.gameObject.SetActive(true);
    }

    public void OffRightLegAttack()
    {
        if (rightLegAttack != null)
            rightLegAttack.gameObject.SetActive(false);
    }

    public void OnFrontLegAttack()
    {
        if (frontLegAttack != null)
            frontLegAttack.gameObject.SetActive(true);
    }

    public void OffFrontLegAttack()
    {
        if (frontLegAttack != null)
            frontLegAttack.gameObject.SetActive(false);
    }

    public void OnRoar()
    {
        if (roarAttack != null)
            roarAttack.gameObject.SetActive(true);
    }

    public void OffRoar()
    {
        if (roarAttack != null)
            roarAttack.gameObject.SetActive(false);
    }
}
