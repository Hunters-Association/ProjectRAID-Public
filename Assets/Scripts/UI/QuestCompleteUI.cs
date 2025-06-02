using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using ProjectRaid.EditorTools;

public class QuestCompleteUI : BaseUI, IBlockingUI
{
    public TextMeshProUGUI questName;
    public AudioSource source;
    public AudioClip clip;
    public float volume = 0.25f;
    public bool hasReward = false;

    public Boss questBoss;

    public bool BlocksGameplay => true;

    public override void OnShow()
    {
        base.OnShow();

        DOVirtual.DelayedCall(5f, () =>
        {
            // questName.text = "";
            // hasReward = false;
            
            switch (questBoss)
            {
                case BossLavies: AnalyticsManager.SendFunnelStep(20); break;
                case BossDoat: AnalyticsManager.SendFunnelStep(25); break;

                default: break;
            }

            GameManager.Instance.LoadScene(2); // 아카데미
        });

        if (source != null && clip != null)
        {
            source.PlayOneShot(clip, volume);
        }
    }
}
