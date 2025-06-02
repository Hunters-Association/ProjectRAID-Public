using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
[CreateAssetMenu(fileName = "NewClipDatas", menuName = "Data/Audio/ClipDatas")]
public class AudioClipDatas : ScriptableObject
{
    public List<ClipData> clipDatas;
    private Dictionary<string, ClipData> clipDic;

    public void Init()
    {
        clipDic = new Dictionary<string, ClipData>();

        for (int i = 0; i < clipDatas.Count; i++)
        {
            clipDic.Add(clipDatas[i].clipName, clipDatas[i]);
        }
    }

    public ClipData GetClipData(string name)
    {
        if(clipDic.TryGetValue(name, out ClipData clipData))
        {
            return clipData;
        }

        Debug.Log($"{name}이 ClipDatas에 없습니다.");
        return null;
    }

    public AudioClip GetAudioClip(string name)
    {
        AudioClip audioClip = null;

        ClipData clipData = GetClipData(name);

        if (clipData.audioClips == null)
        {
            Debug.Log($"{name}에 연결된 AudioClip이 없습니다");
            return null;
        }

        int clipIndex = Random.Range(0, clipData.audioClips.Length);

        audioClip = clipData.audioClips[clipIndex];

        return audioClip;
    }
}
