using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewSFX", menuName = "Data/Audio/SFX")]
public class ClipData : ScriptableObject
{
    public string clipName;
    public float volume;
    public AudioClip[] audioClips;
}
