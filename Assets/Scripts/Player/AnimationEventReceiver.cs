using UnityEngine;

public class AnimationEventReceiver : MonoBehaviour
{
    private PlayerController player;

    private void Awake()
    {
        player = GetComponent<PlayerController>();
    }

    /// <summary>
    /// 애니메이션 이벤트: 무기 보관 애니메이션이 거의 끝났을 때 호출 (아직 필요 없음)
    /// </summary>
    // public void OnSheatheWeaponFinished()
    // {
    //     if (player != null) player.OnSheatheWeaponFinished();
    // }

    // TODO: 다른 애니메이션 이벤트도 여기로 이동시켜서 관리
}
