using UnityEngine;
using ProjectRaid.EditorTools;

public enum InteractableType
{
    Object,
    NPC,
    Corpse,
    LostArticle,
}

[CreateAssetMenu(fileName = "Interactable", menuName = "Data/Interactable")]
public class InteractableData : ScriptableObject
{
    [FoldoutGroup("상호작용 데이터", ExtendedColor.White)]
    public InteractableType Type;
    public string NameText;
    [Tooltip("비워두면 타입에 맞는 기본 텍스트 선택됨")] public string ActionText;

    // TODO: 상호작용에 필요한 내용이 더 있다면 추가
}
