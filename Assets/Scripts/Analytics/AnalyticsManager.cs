using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Analytics;
using System.Collections.Generic;

public class AnalyticsManager : MonoBehaviour
{
    async void Start()
    {
        try
        {
            // Unity Services 초기화
            await UnityServices.InitializeAsync();
            AnalyticsService.Instance.StartDataCollection();

            Debug.Log("Unity Services Initialized");

            // (1) 퍼널 이벤트 전송 예시
            // SendFunnelStep(1);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Unity Services failed to initialize: " + e.Message);
        }
    }

    public static void CallLoadSceneEvent(int NowScene, int CallScene, int QuestID) // 씬 로드 시
    {
        var analyticsEvent = new CustomEvent("Load_Scene") // event
        {
            ["Now_Scene_ID"] = NowScene,
            ["Call_Scene_ID"] = CallScene,
            ["Quest_ID"] = QuestID
        };

        //로딩 시간 추가

        AnalyticsService.Instance.RecordEvent(analyticsEvent); // custom event
    }

    public static void TryAttackEvent(int QuestID, int WeaponID, int WeaponClass, int AttackID, float CharacterPositionX, float CharacterPositionY, int Enemy_ID, float Enemy_Position_X, float Enemy_Position_Y, string Enemy_Pattern_ID, float Damage)
    {
        var analyticsEvent = new CustomEvent("Try_Attack") // event
        {
            ["Quest_ID"] = QuestID,
            ["Weapon_ID"] = WeaponID,
            ["Weapon_Class"] = WeaponClass,
            ["Attack_ID"] = AttackID,
            ["Character_Position_X"] = QuestID,
            ["Character_Position_Y"] = QuestID,
            ["Enemy_ID"] = QuestID,
            ["Enemy_Position_X"] = QuestID,
            ["Enemy_Position_Y"] = QuestID,
            ["Enemy_Pattern_ID"] = QuestID,
            ["Damage"] = QuestID
        };

        AnalyticsService.Instance.RecordEvent(analyticsEvent); // custom event
    }

    public static void GetDamagedEvent(int QuestID, int WeaponID, int WeaponClass, string EnemyPatternID, float DamageValue)
    {
        var analyticsEvent = new CustomEvent("Get_Damaged") // event
        {
            ["Quest_ID"] = QuestID,
            ["Weapon_ID"] = WeaponID,
            ["Weapon_Class"] = WeaponClass,
            ["Enemy_Pattern_ID"] = EnemyPatternID,
            ["Damage"] = DamageValue
        };

        AnalyticsService.Instance.RecordEvent(analyticsEvent); // custom event
    }

    public static void CallInteractEvent(string interactTarget, float TargetPosition_X, float TargetPositionY, string Interacttype)
    {
        var analyticsEvent = new CustomEvent("Call_Interact") // event
        {
            ["Interact_Target"] = interactTarget,
            ["Character_Position_X"] = TargetPosition_X,
            ["Character_Position_Y"] = TargetPositionY,
            ["Interact_type"] = Interacttype
        };

        AnalyticsService.Instance.RecordEvent(analyticsEvent); // custom event
    }

    public static void TryGathering(int EnemyID, int ItemID, int ItemValue)
    {
        var analyticsEvent = new CustomEvent("Try_Gathering") // event
        {
            ["Enemy_ID"] = EnemyID,
            ["Item_ID"] = ItemID,
            ["Item_Value"] = ItemValue
        };

        AnalyticsService.Instance.RecordEvent(analyticsEvent); // custom event
    }

    public static void StartBattle(int QuestID, float StartTime)
    {
        var analyticsEvent = new CustomEvent("Start_Battle")
        {
            ["Quest_ID"] = QuestID,
            ["Start_Time"] = StartTime
        };

        AnalyticsService.Instance.RecordEvent(analyticsEvent); // custom event
    }

    public static void EndBattle(int QuestID, float EndTime)
    {
        var analyticsEvent = new CustomEvent("End_Battle") // event
        {
            ["Quest_ID"] = QuestID,
            ["End_Time"] = EndTime
        };

        AnalyticsService.Instance.RecordEvent(analyticsEvent); // custom event
    }

    public static void GetQuest(int QuestID, float QuestStartTime) // event
    {
        var analyticsEvent = new CustomEvent("Get_Quest")
        {
            ["Quest_ID"] = QuestID,
            ["Start_Time"] = QuestStartTime
        };

        AnalyticsService.Instance.RecordEvent(analyticsEvent); // custom event
    }
    public static void EndQuest(int QuestID, float QuestEndTime)
    {
        var analyticsEvent = new CustomEvent("End_Quest") // event
        {
            ["Quest_ID"] = QuestID,
            ["End_Time"] = QuestEndTime
        };

        AnalyticsService.Instance.RecordEvent(analyticsEvent); // custom event
    }

    public static void CharacterDie(int QuestID, int EnemyID, float CharacterPositionX, float CharacterPositionY)
    {
        var analyticsEvent = new CustomEvent("Get_Quest") // event
        {
            ["Quest_ID"] = QuestID,
            ["Enemy_ID"] = EnemyID,
            ["Character_Position_X"] = CharacterPositionX,
            ["Character_Position_Y"] = CharacterPositionY
        };

        AnalyticsService.Instance.RecordEvent(analyticsEvent); // custom event
    }


    private static readonly HashSet<int> sentSteps = new();

    // [1]퍼널
    public static void SendFunnelStep(int stepNumber)
    {
        if (sentSteps.Contains(stepNumber)) return;

        sentSteps.Add(stepNumber);

        var funnelEvent = new CustomEvent("Funnel_Step") // event
        {
            ["Funnel_Step_Number"] = stepNumber // parameter
        };

        AnalyticsService.Instance.RecordEvent(funnelEvent); // custom event
    }
}
