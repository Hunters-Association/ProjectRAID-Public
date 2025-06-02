using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    public TMP_Text nameUI;
    public TMP_Text textUI;

    private NPCDialogData currentDialogue;
    private int currentIndex;

    public void StartDialogue(NPCDialogData data)
    {
        currentDialogue = data;
        currentIndex = 0;
        ShowCurrentLine();
    }

    public void OnNext()
    {
        currentIndex++;

        if (currentIndex >= currentDialogue.lines.Count)
        {
            EndDialogue();
        }
        else
        {
            ShowCurrentLine();
        }
    }

    void ShowCurrentLine()
    {
        var line = currentDialogue.lines[currentIndex];
        nameUI.text = line.speakerName;
        textUI.text = line.dialogueText;
    }

    void EndDialogue()
    {
        QuestManager.Instance.AcceptQuest(currentDialogue.questToAcceptID);

        currentDialogue = null;
        nameUI.text = "";
        textUI.text = "";
    }
}
