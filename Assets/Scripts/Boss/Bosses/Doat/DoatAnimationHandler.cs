using ProjectRaid.EditorTools;
using System;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public class BossStepParticle
{
    public enum BossStepPos
    {
        FrontLeft,
        FrontRight,
        BackLeft,
        BackRight,
        Count
    }

    public BossStepPos stepPos;
    public Transform pos;
    public ParticleSystem stepParticle;
}

public class DoatAnimationHandler : MonoBehaviour
{
    [HideInInspector] public BossDoat doat;
    [SerializeField] private AudioClipDatas clipDatas;

    [FoldoutGroup("AttackColliders", ExtendedColor.White)]
    public BossHitbox tailAttack;
    public BossHitbox leftLegAttack;
    public BossHitbox rightLegAttack;
    public BossHitbox frontAllLegAttack;
    public BossHitbox bodyPressAttack;
    public BossHitbox shootAttack;
    public BossHitbox biteAttack;
    public BossHitbox roarAttack;

    [FoldoutGroup("VFX", ExtendedColor.LightSkyBlue)]
    [SerializeField] private ParticleSystem particle;
    [SerializeField] private ParticleSystem chargeParticle;
    [SerializeField] private ParticleSystem frontAllLegParticle;

    [SerializeField] private BossStepParticle[] stepParticles;

    [FoldoutGroup("SFX", ExtendedColor.Gold)]
    [SerializeField] private AudioSource source;
    [SerializeField] private AudioClip clip;
    [SerializeField, Range(0f, 1f)] private float volume = 1f;

    [SerializeField] private AudioSource oneShotSource;
    [SerializeField] private AudioSource playSource;

    private void Awake()
    {
        if (oneShotSource == null)
        {
            Debug.Log("DoatOneShotSource가 연결이 되어있지 않습니다");
        }

        if (playSource == null)
        {
            Debug.Log("DoatplaySource가 연결이 되어있지 않습니다");
        }

        if(clipDatas == null)
        {
            Debug.Log("clipDatas가 연결이 되어있지 않습니다.");
        }
        else
        {
            clipDatas.Init();
        }
    }

    private void Start()
    {
        doat = GetComponentInParent<BossDoat>();
    }

    private void Update()
    {
    }

    public void OnTailAttack()
    {
        if (tailAttack != null)
        {
            tailAttack.gameObject.SetActive(true);

            for (int i = 0; i < tailAttack.lowHitboxList.Count; i++)
            {
                tailAttack.lowHitboxList[i].gameObject.SetActive(true);
            }
        }
    }

    public void OffTailAttack()
    {
        if (tailAttack != null)
        {
            for (int i = 0; i < tailAttack.lowHitboxList.Count; i++)
            {
                tailAttack.lowHitboxList[i].gameObject.SetActive(false);
            }

            tailAttack.gameObject.SetActive(false);
        }
    }

    public void OnLeftLegAttack()
    {
        if (leftLegAttack != null)
        {
            leftLegAttack.gameObject.SetActive(true);
        }
    }

    public void OffLeftLegAttack()
    {
        if (leftLegAttack != null)
            leftLegAttack.gameObject.SetActive(false);
    }

    public void OnRightLegAttack()
    {
        if (rightLegAttack != null)
        {
            rightLegAttack.gameObject.SetActive(true);
        }
    }

    public void OffRightLegAttack()
    {
        if (rightLegAttack != null)
        {
            rightLegAttack.gameObject.SetActive(false);
        }
    }

    public void OnFrontAllLegAttack()
    {
        if (frontAllLegAttack != null)
        {
            frontAllLegAttack.gameObject.SetActive(true);
        }
    }

    public void OffFrontAllLegAttack()
    {
        if (frontAllLegAttack != null)
            frontAllLegAttack.gameObject.SetActive(false);
    }

    public void OnBiteAttack()
    {
        if (biteAttack != null)
        {
            biteAttack.gameObject.SetActive(true);
        }
    }

    public void OffBiteAttack()
    {
        if (biteAttack != null)
            biteAttack.gameObject.SetActive(false);
    }

    public void OnShootAttack()
    {
        if (shootAttack != null)
        {
            shootAttack.gameObject.SetActive(true);
        }
    }

    public void OffShootAttack()
    {
        if (shootAttack != null)
            shootAttack.gameObject.SetActive(false);
    }

    public void OnRoar()
    {
        if (roarAttack != null)
        {
            roarAttack.gameObject.SetActive(true);
        }
    }

    public void OffRoar()
    {
        if (roarAttack != null)
            roarAttack.gameObject.SetActive(false);
    }

    #region SFX

    public void OnStep(int index)
    {
        OnSfxOneShot("DoatStep");

        OnStepParticle((BossStepParticle.BossStepPos)index);
    }

    public void OnWalkStep(int index)
    {
        if(index == 0)
        {
            OnStep((int)BossStepParticle.BossStepPos.FrontLeft);
            OnStep((int)BossStepParticle.BossStepPos.BackRight);
        }
        else
        {
            OnStep((int)BossStepParticle.BossStepPos.FrontRight);
            OnStep((int)BossStepParticle.BossStepPos.BackLeft);
        }
    }

    public void OnBreathParticleStart()
    {
        if (particle == null || source == null)
        {
            if (particle == null) Debug.LogWarning("[DoatAnimationHandler] 파티클 시스템이 등록되지 않았습니다!", this);
            if (source == null) Debug.LogWarning("[DoatAnimationHandler] 오디오 소스가 등록되지 않았습니다!", this);
            return;
        }

        if (particle.isPlaying)
            particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        particle.Play();
        source.clip = clip;
        source.volume = volume;
        source.Play();
    }

    public void OffBreathParticleEnd()
    {
        if (particle == null || source == null)
        {
            if (particle == null) Debug.LogWarning("[DoatAnimationHandler] 파티클 시스템이 등록되지 않았습니다!", this);
            if (source == null) Debug.LogWarning("[DoatAnimationHandler] 오디오 소스가 등록되지 않았습니다!", this);
            return;
        }

        if (particle.isPlaying)
            particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        source.Stop();
    }

    public void OnSpawnSFX()
    {
        OnSfxSound("DoatSpawn");
    }

    public void OnRoarSFX()
    {
        OnSfxSound("DoatRoar");
    }

    public void OnChargeSFX()
    {
        OnSfxSound("DoatCharge");
    }

    public void OnSleepSFX()
    {
        OnSfxSound("DoatSleep", true);
    }

    public void OnDeadStart()
    {
        OnSfxSound("DoatDead");
    }

    public void OnGrowlSFX()
    {
        OnSfxSound("DoatIdle", true);
    }

    public void OnHurtSFX()
    {
        OnSfxSound("DoatHurt");
    }

    public void OnDestructionSFX()
    {
        OnSfxSound("DoatDestruction");
    }

    public void OnLeftLegAttackSFX()
    {
        OnSfxSound("DoatLeftLegAttack");
    }

    public void OnRightLegAttackSFX()
    {
        OnSfxSound("DoatRightLegAttack");
    }

    public void OnFrontAllLegAttackSFX()
    {
        OnSfxSound("DoatFrontAllLegAttack");
    }

    public void OnBiteAttackSFX()
    {
        OnSfxSound("DoatBiteAttack");
    }
    public void OnTailAttackSFX()
    {
        OnSfxSound("DoatTailAttack");
    }

    public void OnBodyPressAttack()
    {
        OnSfxSound("DoatBodyPressAttack");
    }

    public void OnSfxSound(string name, bool isLoop = false)
    {
        if(!(playSource.loop && isLoop))
            OffSfxSound();

        ClipData clipData = clipDatas.GetClipData(name);

        if (clipData == null) return;

        AudioClip clip = clipDatas.GetAudioClip(clipData.clipName);

        if (playSource.clip == clip && isLoop) return;

        playSource.clip = clip;
        playSource.volume = clipData.volume;
        playSource.loop = isLoop;
        playSource.Play();
    }

    public void OnSfxOneShot(string name)
    {
        ClipData clipData = clipDatas.GetClipData(name);

        if (clipData == null) return;

        AudioClip clip = clipDatas.GetAudioClip(clipData.clipName);

        if (oneShotSource.clip == clip) return;

        oneShotSource.clip = clip;
        oneShotSource.volume = clipData.volume;
        oneShotSource.PlayOneShot(clip);
    }

    public void OffSfxSound()
    {
        playSource.Stop();
    }

    #endregion

    #region VFX

    public void OnStepParticle(BossStepParticle.BossStepPos stepPos)
    {
        ParticleSystem stepParticle = stepParticles[(int)stepPos].stepParticle;
        Vector3 position = stepParticles[(int)stepPos].pos.position;
        stepParticle.transform.position = position;

        PlayParticleSystem(stepParticle);
    }

    public void OnCharge()
    {
        if (chargeParticle == null)
        {
            if (chargeParticle == null) Debug.LogWarning("Charge 파티클이 등록되지 않았습니다!", this);
            return;
        }

        PlayParticleSystem(chargeParticle);
    }

    public void OnFrontAllLeg()
    {
        if (frontAllLegParticle == null)
        {
            if (frontAllLegParticle == null) Debug.LogWarning("frontAllLeg 파티클이 등록되지 않았습니다!", this);
            return;
        }

        OnSfxOneShot("DoatStump");

        PlayParticleSystem(frontAllLegParticle);
    }

    private void PlayParticleSystem(ParticleSystem particle)
    {
        if (particle.isPlaying)
            particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        particle.Play();
    }

    #endregion
}
