using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DialogueLineData
{
    public string speakerName;        // 이 대사를 말하는 NPC 이름
    [TextArea(2, 4)]
    public string dialogueText;       // 텍스트 본문
}


[CreateAssetMenu(menuName = "Game/Dialogue Sequence")]
public class NPCDialogData : ScriptableObject
{
    public List<DialogueLineData> lines;      // 대화 시퀀스
    public int questToAcceptID;           // 대화 종료 시 강제 수주할 퀘스트 (nullable)
}