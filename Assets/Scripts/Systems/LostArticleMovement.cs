using UnityEngine;
using ProjectRaid.EditorTools;

public class ItemBase : MonoBehaviour
{
    [FoldoutGroup("아이템 설정 (효과음)", ExtendedColor.Orange)]
    [SerializeField] [Range(0, 1)] private float volume = 0.5f;
    [SerializeField] private AudioClip audioClip;

    [FoldoutGroup("아이템 설정 (애니메이션)", ExtendedColor.Cyan)]
    [SerializeField] private float rotationSpeed = 50f; // 초당 회전 속도
    [SerializeField] private float floatAmplitude = 0.05f; // 상하 이동 거리
    [SerializeField] private float floatFrequency = 2f; // 상하 이동 주기

    [FoldoutGroup("아이템 설정 (자석)", ExtendedColor.GreenYellow)]
    [SerializeField] private bool autoPickup;
    [SerializeField] private bool useMagnet = false; // 자석 효과 활성화 여부
    [SerializeField] private float magnetRange = 5f; // 플레이어와의 자석 거리
    [SerializeField] private float minMagnetSpeed = 0f; // 최소 이동 속도
    [SerializeField] private float maxMagnetSpeed = 10f; // 최대 이동 속도

    private Vector3 startPosition;

#region Setup
    private void Start()
    {
        startPosition = transform.position;
    }
#endregion

#region Animation
    private void Update()
    {
        PerformRotation();

        if (useMagnet && magnetRange > 0 && IsPlayerInMagnetRange(out GameObject player))
        {
            PerformMagnetMovement(player);
        }
        else
        {
            PerformFloating();
        }
    }

    private void PerformRotation()
    {
        // 제자리에서 회전
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
    }

    private void PerformFloating()
    {
        // 상하 왕복 이동
        float offsetY = Mathf.Sin(Time.time * floatFrequency) * floatAmplitude;
        transform.position = startPosition + new Vector3(0, offsetY, 0);
    }

    private void PerformMagnetMovement(GameObject player)
    {
        // 플레이어와의 거리 계산
        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);

        // 거리에 비례하여 이동 속도 조정
        float speed = Mathf.Lerp(maxMagnetSpeed, minMagnetSpeed, distanceToPlayer / magnetRange);
        speed = Mathf.Clamp(speed, minMagnetSpeed, maxMagnetSpeed);

        // 플레이어 방향으로 이동
        transform.position = Vector3.MoveTowards(transform.position, player.transform.position + new Vector3(0, 1.2f, 0), speed * Time.deltaTime);
    }
#endregion

#region Magnet
    private bool IsPlayerInMagnetRange(out GameObject player)
    {
        player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return false;

        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
        return distanceToPlayer <= magnetRange;
    }
#endregion

#region Pickup
    private void OnTriggerEnter(Collider other)
    {
        if (!autoPickup) return;
        if (other.CompareTag("Player"))
        {
            PlayPickupSound();
            // OnPickup(other.gameObject);
            Destroy(gameObject); // 아이템 제거
        }
    }

    private void PlayPickupSound()
    {
        if (audioClip != null)
        {
            AudioSource.PlayClipAtPoint(audioClip, transform.position, volume);
        }
    }

    // 아이템 획득 동작은 자식 클래스에서 구현
    // public abstract void OnPickup(GameObject player);
#endregion
}