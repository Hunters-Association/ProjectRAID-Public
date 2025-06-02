using System.Collections.Generic; // List 사용
using ProjectRaid.Data;          // ItemData, RecipeData 등이 이 네임스페이스에 있다고 가정
using UnityEngine;               // Sprite 등 Unity 타입 사용

public class RecipeViewModel
{
    public RecipeData Recipe; // 실제 사용하는 RecipeData 클래스명으로 통일
    public string DisplayName;
    public Sprite Icon;
    public bool CanCraft;
}

public class RecipeDetailsViewModel
{
    public string Name;
    public string Description;
    public Sprite Icon;
    // public Sprite PreviewImage; // 필요하다면 주석 해제
    public List<MaterialViewModel> RequiredMaterials;
    public List<StatViewModel> Stats;
}

public class MaterialViewModel
{
    public ItemData MaterialItem; // 실제 사용하는 ItemData 클래스명으로 통일
    public int RequiredCount;
    public int OwnedCount;
    public bool IsSufficient => OwnedCount >= RequiredCount;
}

public class StatViewModel
{
    public string Name;
    public string ValueString;
}
