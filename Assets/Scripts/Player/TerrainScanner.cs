using UnityEngine;
using DG.Tweening;
using System;
// using ProjectRaid.EditorTools;
using UnityEngine.UI;
using ProjectRaid.EditorTools;

public class TerrainScanner : MonoBehaviour
{
    [SerializeField] private Material agentSilhouette;
    [SerializeField] private Material scannerMaterial;
    [SerializeField] private Transform scanStartPoint;
    [SerializeField] private Vector3 defaultPosition;
    [SerializeField] private float maxRange = 25f;
    [SerializeField] private Ease ease;
    [SerializeField] private float duration = 2f;
    [SerializeField] private AudioSource source;
    [SerializeField] private AudioClip sfx;
    [SerializeField] private float volume = 0.5f;

    [SerializeField] private LayerMask scanLayerMask;
    [SerializeField] private bool isTest = false;
    [SerializeField] private float coolTime = 5f;
    private float lastScanTime;

    private Collider[] outLineColliders;
    [SerializeField] bool isDelayScan;
    [SerializeField] float outLineActiveTime = 2f;

    private Tween cancleOutLine;

    [FoldoutGroup("ScanUI", ExtendedColor.White)]
    [SerializeField] private Image coolTimeDim;

    private PlayerController player;

    public bool CheckCoolTime() => Time.time - lastScanTime >= coolTime;

    private void Awake()
    {
        InitScanner();
        player = GetComponentInParent<PlayerController>();
    }

    public void InitScanner()
    {
        Vector3 position = scanStartPoint != null ? scanStartPoint.position : transform.position;
        scannerMaterial.SetVector("_Position", position);

        if (scannerMaterial != null)
        {
            scannerMaterial.SetFloat("_Range", 0.01f);
            scannerMaterial.SetFloat("_Opacity", 0f);
        }

        if (agentSilhouette != null)
        {
            agentSilhouette.SetFloat("_Dissolve", 0f);
            agentSilhouette.SetFloat("_SilhouetteAlpha", 0f);
        }

        coolTime = isTest ? 0 : coolTime;

        lastScanTime = float.MinValue;
    }

    private void Update()
    {
        if (player.InputHandler.IsScanPressed)
        {
            Debug.Log("[TerrainScanner] 'T'키 입력됨");
            StartSceneScanning();
        }
    }

    public void StartSceneScanning()
    {
        if (!CheckCoolTime())
            return;

        Debug.Log("[TerrainScanner] 'CheckCoolTime()' 통과");

        lastScanTime = Time.time;

        if (scannerMaterial != null)
        {
            Vector3 position = scanStartPoint != null ? scanStartPoint.position : transform.position;
            scannerMaterial.SetVector("_Position", position);

            DOTween.Kill(scannerMaterial);
            DOTween.To(() => 0.01f, x => scannerMaterial.SetFloat("_Range", x), maxRange, duration).SetEase(ease); // scanRange를 0에서 100으로 애니메이션
            DOTween.To(() => 1f, x => scannerMaterial.SetFloat("_Opacity", x), 0.01f, duration).SetEase(ease); // opacity를 1에서 0으로 애니메이션
        }

        if (source != null && sfx != null)
        {
            source.PlayOneShot(sfx, volume);
        }

        float delay = isDelayScan ? duration : 0f;
        DOVirtual.DelayedCall(delay, ActiveOutLine);

        if (coolTimeDim != null)
        {
            coolTimeDim.DOFillAmount(0, coolTime)
                .SetEase(Ease.Linear)
                .From(1f);
        }
    }

    public void ActiveOutLine()
    {
        outLineColliders = Physics.OverlapSphere(transform.position, maxRange, scanLayerMask);

        for (int i = 0; i < outLineColliders.Length; i++)
        {
            BaseScanable scanable;
            if (outLineColliders[i].TryGetComponent(out scanable))
            {
                scanable.ActiveOutLine();
            }
        }

        if (cancleOutLine != null)
        {
            cancleOutLine.Kill();
            cancleOutLine = null;
        }

        cancleOutLine = DOVirtual.DelayedCall(outLineActiveTime, CancleOutLine);
    }

    public void CancleOutLine()
    {
        Debug.Log("아웃라인 취소");

        for (int i = 0; i < outLineColliders.Length; i++)
        {
            BaseScanable scanable;
            if (outLineColliders[i].TryGetComponent(out scanable))
            {
                scanable.CancleOutLine();
            }
        }

        cancleOutLine = null;
        Array.Clear(outLineColliders, 0, outLineColliders.Length);
    }

    public void SetSilhouetteAlpha(float alpha)
    {
        DOTween.Kill(agentSilhouette);
        DOTween.To(() => agentSilhouette.GetFloat("_SilhouetteAlpha"), x => agentSilhouette.SetFloat("_SilhouetteAlpha", x), alpha, duration * 0.2f).SetEase(ease);
    }

    private void OnApplicationQuit()
    {
        if (scannerMaterial != null)
        {
            Vector3 position = defaultPosition;
            scannerMaterial.SetVector("_Position", position);

            scannerMaterial.SetFloat("_Range", 0.01f);
            scannerMaterial.SetFloat("_Opacity", 0f);
        }

        if (agentSilhouette != null)
        {
            agentSilhouette.SetFloat("_Dissolve", 0f);
            agentSilhouette.SetFloat("_SilhouetteAlpha", 0f);
        }
    }
}
