using ProjectRaid.EditorTools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TutorialText", menuName = "Data/TutorialText")]
public class TutorialSO : ScriptableObject
{
    [FoldoutGroup("튜토리얼 텍스트", ExtendedColor.White)]
    [TextArea]
    public string greetingsText;    // 인사말

    [TextArea]
    public string walkText;         // 목적지 튜토리얼

    [TextArea]
    public List<string> footPrintText;    // 흔적 튜토리얼

    [TextArea]
    public List<string> healingText;      // 회복 튜토리얼

    [TextArea]
    public List<string> killBossText;     // 토벌 튜토리얼

    [TextArea]
    public List<string> finishText;       // 완료 튜토리얼


    [Header("SFX Clip")]
    public AudioClip completeClip;
}
