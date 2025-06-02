using DG.Tweening;
using ProjectRaid.EditorTools;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    [Header("데이터")]
    [SerializeField] private PlayerController player;
    [SerializeField] private TutorialSO tutorialData;

    [Header("튜토리얼 텍스트")]
    [SerializeField] private TutorialText tutorialText;

    [FoldoutGroup("튜토리얼 텍스트 유지 시간", ExtendedColor.White)]
    //[SerializeField] private float textShowDuration;
    [SerializeField] private float textShowDelay = 1f;

    [FoldoutGroup("튜토리얼 오브젝트", ExtendedColor.Blue)]
    [SerializeField] private WalkCompletePoint walkCompletePoint;
    [SerializeField] private FootPrintSpawner footPrintSpawner;
    [SerializeField] private GlowingHealingHerb healingHub;
    [SerializeField] private BossSpawner bossSpawner;
    [SerializeField] private BossHealth practiceBossHealth;

    // 투명 벽
    [Header("투명 벽")]
    [SerializeField] private GameObject bossWall;
    [SerializeField] private GameObject portalWall;

    private Coroutine showTextCo;
    public bool isNext = false;

    private Sequence tutorialSequence;

    private AudioSource audioSource;

    public void StartTutorialStep() => isNext = false;
    public void EndTutorialStep() => isNext = true;

    private void PlayerInputEnable()
    { 
        player.InputHandler.PlayerInput.actions.Enable();
        player.InputHandler.PlayerInput.actions["Quest"].Disable();
        player.InputHandler.PlayerInput.actions["Inventory"].Disable();
    }
    private void PlayerInputDisable()
    {
        player.InputHandler.PlayerInput.actions.Disable();
        player.InputHandler.PlayerInput.actions["Look"].Enable();
    }

    private void Awake()
    {
        if(PlayerPrefs.HasKey("TutorialFinish"))
        {
            GameManager.Instance.LoadScene(2);
        }

        audioSource = GetComponent<AudioSource>();

        tutorialSequence = DOTween.Sequence();

        walkCompletePoint.Init();
        tutorialText.Init();

        walkCompletePoint.onComplete += EndTutorialStep;
        walkCompletePoint.SetActive(false);

        healingHub.gameObject.SetActive(false);

        SetTutorialSequence();

        PlayerInputDisable();
    }

    private void Start()
    {
        StartTutorial();
    }

    private void StartTutorial()
    {
        tutorialSequence.Play();
    }

    #region SetTutorial

    // 예상 한글자 읽는 속도
    private const float readSpeed = 0.1f;
    private void SetTutorialText(string text)
    {
        tutorialText.SetText(text);
    }

    private float CalculateReadSpeed(string text)
    {
        return text.Length * readSpeed;
    }

    private void SetTutorialSequence()
    {
        SetGreetingText();
        SetWalkText();
        SetFootPrintText();
        SetHealingText();
        SetKillBossText();
        SetFinishText();
    }

    private void SetGreetingText()
    {
        Sequence sequence = DOTween.Sequence();
        sequence.OnStart(() => { SetTutorialText(tutorialData.greetingsText);});

        sequence.Append(tutorialText.ShowText(CalculateReadSpeed(tutorialData.greetingsText)));
        
        tutorialSequence.Append(sequence);
    }

    private void SetWalkText()
    {
        Action onComplete = () => 
        { 
            walkCompletePoint.SetActive(true);

            PlayerInputEnable();

            tutorialSequence.Pause();

            StartCoroutine(WaitCompletePoint());
        };

        Sequence sequence = DOTween.Sequence();
        sequence.OnStart(() => { SetTutorialText(tutorialData.walkText); });
        sequence.Append(tutorialText.ShowText(CalculateReadSpeed(tutorialData.walkText), onComplete));

        tutorialSequence.Append(sequence);
    }

    private IEnumerator WaitCompletePoint()
    {
        yield return new WaitUntil(() => isNext);

        walkCompletePoint.SetActive(false);

        StartTutorialStep();
        PlayerInputDisable();

        audioSource.PlayOneShot(tutorialData.completeClip);

        tutorialSequence.Play();
    }

    private void SetFootPrintText()
    {
        footPrintSpawner.footPrintNav.onNavTarget += EndTutorialStep;

        Action onComplete = () =>
        {
            // 흔적 활성화
            footPrintSpawner.SpawnFootPrint(footPrintSpawner.spawnBossID);

            tutorialSequence.Pause();

            PlayerInputEnable();

            StartCoroutine(WaitCompleteFootPrint());
        };

        Sequence sequence = DOTween.Sequence();
        sequence.SetDelay(textShowDelay);

        sequence.AppendCallback(()=>SetTutorialText(tutorialData.footPrintText[0]));
        sequence.Append(tutorialText.ShowText(CalculateReadSpeed(tutorialData.footPrintText[0])));

        sequence.AppendCallback(() => SetTutorialText(tutorialData.footPrintText[1]));
        sequence.Append(tutorialText.ShowText(CalculateReadSpeed(tutorialData.footPrintText[1])));

        sequence.AppendCallback(() => SetTutorialText(tutorialData.footPrintText[2]));
        sequence.Append(tutorialText.ShowText(CalculateReadSpeed(tutorialData.footPrintText[2]), onComplete));

        tutorialSequence.Append(sequence);
    }

    private IEnumerator WaitCompleteFootPrint()
    {
        yield return new WaitUntil(() => isNext);

        StartTutorialStep();

        PlayerInputDisable();

        tutorialSequence.Play();
    }

    private void SetHealingText()
    {
        Action onComplete = () =>
        {
            healingHub.OnUse += EndTutorialStep;

            tutorialSequence.Pause();

            PlayerInputEnable();

            StartCoroutine(WaitCompleteHealing());
        };

        Sequence sequence = DOTween.Sequence();

        // 플레이어 체력을 1로 설정
        sequence.OnStart(()=> player.Stats.Runtime.SetHealth(1));

        sequence.AppendCallback(() => {SetTutorialText(tutorialData.healingText[0]);});
        sequence.Append(tutorialText.ShowText(CalculateReadSpeed(tutorialData.healingText[0])));

        sequence.AppendCallback(() => { SetTutorialText(tutorialData.healingText[1]); });
        sequence.Append(tutorialText.ShowText(CalculateReadSpeed(tutorialData.healingText[1]), onComplete));
        sequence.AppendCallback(()=> healingHub.gameObject.SetActive(true));

        tutorialSequence.Append(sequence);
    }

    private IEnumerator WaitCompleteHealing()
    {
        yield return new WaitUntil(() => isNext);

        StartTutorialStep();

        PlayerInputDisable();

        tutorialSequence.Play();
    }
    private void SetKillBossText()
    {
        Action onComplete = () =>
        {
            bossWall.SetActive(false);

            tutorialSequence.Pause();

            PlayerInputEnable();

            StartCoroutine(WaitCompleteKillBoss());
        };

        Sequence sequence = DOTween.Sequence();

        // 플레이어 체력을 1로 설정
        sequence.OnStart(() => 
        {
            practiceBossHealth = bossSpawner.spawnBosses[0].GetComponent<BossHealth>();
            practiceBossHealth.OnDead += EndTutorialStep;
        });

        sequence.AppendCallback(() => { SetTutorialText(tutorialData.killBossText[0]); });
        sequence.Append(tutorialText.ShowText(CalculateReadSpeed(tutorialData.killBossText[0])));

        sequence.AppendCallback(() => { SetTutorialText(tutorialData.killBossText[1]); });
        sequence.Append(tutorialText.ShowText(CalculateReadSpeed(tutorialData.killBossText[1])));

        sequence.AppendCallback(() => { SetTutorialText(tutorialData.killBossText[2]); });
        sequence.Append(tutorialText.ShowText(CalculateReadSpeed(tutorialData.killBossText[2]), onComplete));

        tutorialSequence.Append(sequence);
    }

    private IEnumerator WaitCompleteKillBoss()
    {
        yield return new WaitUntil(() => isNext);

        StartTutorialStep();

        tutorialSequence.Play();
    }
    private void SetFinishText()
    {
        Action onComplete = () =>
        {
            // 튜토리얼 완료 저장
            PlayerPrefs.SetInt("TutorialFinish", 1);

            portalWall.SetActive(false);
        };

        Sequence sequence = DOTween.Sequence();

        sequence.AppendCallback(() => { SetTutorialText(tutorialData.finishText[0]); });
        sequence.Append(tutorialText.ShowText(CalculateReadSpeed(tutorialData.finishText[0])));

        sequence.AppendCallback(() => { SetTutorialText(tutorialData.finishText[1]); });
        sequence.Append(tutorialText.ShowText(CalculateReadSpeed(tutorialData.finishText[1])));

        sequence.AppendCallback(() => { SetTutorialText(tutorialData.finishText[2]); });
        sequence.Append(tutorialText.ShowText(CalculateReadSpeed(tutorialData.finishText[2]), onComplete));

        tutorialSequence.Append(sequence);
    }
    #endregion
}
