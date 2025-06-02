using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class DecalObjectPool : MonoBehaviour
{
    [SerializeField] private int initCount = 50;
    private readonly Queue<GameObject> decalObjects = new();

    public void Init()
    {
        for (int i = 0; i < initCount; i++)
            decalObjects.Enqueue(CreateNewObject());
    }

    public GameObject GetObject()
    {
        GameObject go = decalObjects.Count > 0
            ? decalObjects.Dequeue()
            : CreateNewObject();

        go.SetActive(true);
        return go;
    }

    public void ReturnObj(GameObject go)
    {
        go.SetActive(false);
        go.transform.SetParent(transform);
        decalObjects.Enqueue(go);
    }

    public GameObject CreateNewObject()
    {
        var obj = new GameObject("DecalObject", typeof(DecalProjector));
        obj.transform.SetParent(transform);
        obj.SetActive(false);
        return obj;
    }
}
