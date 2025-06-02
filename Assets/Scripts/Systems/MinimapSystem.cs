using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using ProjectRaid.EditorTools;

#if UNITY_EDITOR
using UnityEditor;
#endif

public enum BlipType
{
    Enemy,
    NPC,
    Objective,
    Custom
}

[System.Serializable]
public class BlipSettings
{
    public BlipType type;
    public Sprite icon;
    public Color color = Color.white;
    public bool rotateWithTarget = false;
}

public class MinimapSystem : MonoBehaviour
{
    [FoldoutGroup("타겟", ExtendedColor.DeepSkyBlue)]
    // [SerializeField] private Camera minimapCamera;
    [SerializeField] private Transform target;
    [SerializeField] private RectTransform mapRoot; // MapImage + BlipRoot 묶음
    [SerializeField] private RectTransform mapImage; // 실제 지형 이미지
    [SerializeField] private Vector2 worldMin;
    [SerializeField] private Vector2 worldMax;
    // [SerializeField] private float distance = 400f;
    // [SerializeField] private float moveSpeed = 10f;
    [Space(20f)]
    [SerializeField] private bool rotateWithTarget;
    [SerializeField] private Transform rotationTarget;
    [SerializeField] private float rotationSmoothSpeed = 5f;

    [FoldoutGroup("UI", ExtendedColor.Silver)]
    [SerializeField] private AreaUI areaUI;
    [SerializeField] private RectTransform navigationArrow;
    [SerializeField] private RectTransform compass;

    [FoldoutGroup("Blip", ExtendedColor.Crimson)]
    [SerializeField] private RectTransform blipRoot;
    [SerializeField] private float mapScale = 1f;
    [SerializeField] private float maxVisibleDistance = 100f;
    [SerializeField] private List<BlipSettings> blipSettingsList;

    private readonly Dictionary<Transform, MinimapBlip> activeBlips = new();
    private BlipPoolManager blipPool;
    // private Vector3 targetPos;
    private Vector2 worldSize; // (max - min)
    private float targetRot; // 현재 맵 회전값(도)

    public Vector3 MinimapCenter => target.position;
    public float MinimapRotation => targetRot;
    public float MaxVisibleDistance => maxVisibleDistance;
    public bool RotateWithTarget => rotateWithTarget;

    public void SetTarget(Transform t) => target = t;

    private void Awake()
    {
        worldSize = worldMax - worldMin; // 한번만 계산
        SceneManager.sceneLoaded += Init;
    }

    private void Init(Scene scene, LoadSceneMode mode)
    {
        var gameManager = GameManager.Instance;
        if (gameManager != null)
        {
            blipPool = gameManager.BlipPool;
            gameManager.MinimapSystem = this;
        }
    }

    private void Update()
    {
        if (target == null || mapRoot == null) return;

        Vector2 worldXZ = new(target.position.x, target.position.z);
        Vector2 normalized = new(
            (worldXZ.x - worldMin.x) / worldSize.x,   // 0‒1
            (worldXZ.y - worldMin.y) / worldSize.y);  // 0‒1

        // 화면 좌표계 기준 오프셋 (Pivot이 0.5,0.5 라면 -0.5~+0.5 범위)
        Vector2 mapPixel = new(
            -(normalized.x - 0.5f) * mapImage.rect.width,
            (normalized.y - 0.5f) * mapImage.rect.height);

        // 플레이어를 중앙에 고정하기 위해 맵을 반대로 이동
        mapImage.anchoredPosition = mapPixel;

        // targetPos = target.position;
        // targetPos.y = distance;
        // minimapCamera.transform.position = Vector3.Lerp(minimapCamera.transform.position, targetPos, Time.deltaTime * moveSpeed);
    }

    private void LateUpdate()
    {
        if (target == null || navigationArrow == null || compass == null) return;

        if (rotateWithTarget)
        {
            float targetY = rotationTarget ? rotationTarget.eulerAngles.y : target.eulerAngles.y;
            targetRot = Mathf.LerpAngle(targetRot, targetY, Time.deltaTime * rotationSmoothSpeed);

            // 맵과 블립을 같이 회전, 자기자신(플레이어 아이콘/화살표)만 고정
            mapRoot.localEulerAngles = new(0f, 0f, targetRot);
            navigationArrow.localEulerAngles = Vector3.zero;
            compass.localEulerAngles = new(0f, 0f, targetRot);
            blipRoot.localEulerAngles = new(0f, 0f, targetRot);
        }
        else
        {
            targetRot = target.eulerAngles.y;
            mapRoot.localEulerAngles = Vector3.zero;
            navigationArrow.localEulerAngles = new(0f, 0f, -targetRot);
            compass.localEulerAngles = Vector3.zero;
            blipRoot.localEulerAngles = Vector3.zero;
        }

        // if (rotateWithTarget)
        // {
        //     targetRot = Mathf.LerpAngle(targetRot, rotationTarget.eulerAngles.y, Time.deltaTime * rotationSmoothSpeed);

        //     // 미니맵 카메라 자체가 회전
        //     minimapCamera.transform.rotation = Quaternion.Euler(90f, targetRot, 0f);
        //     navigationArrow.localEulerAngles = Vector3.zero;
        //     compass.localEulerAngles = new(0f, 0f, targetRot);
        //     blipRoot.localEulerAngles = new(0f, 0f, targetRot);
        // }
        // else
        // {
        //     targetRot = target.eulerAngles.y;

        //     // 미니맵은 고정, 화살표만 회전
        //     minimapCamera.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        //     navigationArrow.localEulerAngles = new(0f, 0f, -targetRot);
        //     compass.localEulerAngles = Vector3.zero;
        //     blipRoot.localEulerAngles = Vector3.zero;
        // }

        UpdateBlips();
    }

    private void UpdateBlips()
    {
        foreach (var kv in activeBlips)
        {
            var target = kv.Key;
            var blip = kv.Value;

            if (target == null || blip == null)
            {
                activeBlips.Remove(kv.Key);
                continue;
            }

            float distance = Vector3.Distance(target.position, MinimapCenter);
            if (distance > maxVisibleDistance)
            {
                blip.gameObject.SetActive(false);
            }
            else
            {
                blip.gameObject.SetActive(true);
                blip.UpdateBlip(MinimapCenter, mapScale, MinimapRotation, blipRoot);
            }
        }
    }

    public void RegisterBlip(Transform target, BlipSettings settings)
    {
        if (activeBlips.ContainsKey(target)) return;

        var blip = blipPool.Get();
        blip.Initialize(target, this, settings);
        blip.transform.SetParent(blipRoot, false);
        activeBlips[target] = blip;
    }

    public void UnregisterBlip(Transform target)
    {
        if (!activeBlips.ContainsKey(target))
            return;

        var blip = activeBlips[target];
        if (blip != null) blipPool.Return(blip);

        activeBlips.Remove(target);
    }

    public BlipSettings GetBlipSettings(BlipType type)
    {
        return blipSettingsList.Find(s => s.type == type);
    }

    private void OnDisable()
    {
        foreach (var key in activeBlips.Keys.ToList())
            UnregisterBlip(key);
    }

    public void UpdateArea(AreaInfo info)
    {
        areaUI.UpdateArea(info);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Vector3 pBL = new(worldMin.x, 200f, worldMin.y); // Bottom-Left
        Vector3 pTL = new(worldMin.x, 200f, worldMax.y); // Top-Left
        Vector3 pTR = new(worldMax.x, 200f, worldMax.y); // Top-Right
        Vector3 pBR = new(worldMax.x, 200f, worldMin.y); // Bottom-Right

        Debug.DrawLine(pBL, pTL, Color.red); // 왼쪽
        Debug.DrawLine(pTL, pTR, Color.red); // 위
        Debug.DrawLine(pTR, pBR, Color.red); // 오른쪽
        Debug.DrawLine(pBR, pBL, Color.red); // 아래
    }
#endif
}
