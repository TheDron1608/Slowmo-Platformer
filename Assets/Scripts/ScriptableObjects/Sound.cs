using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Sound", menuName = "Sound")]
public class Sound : ScriptableObject
{
    public List<AudioClip> AudioClips = new List<AudioClip>();

    public float RandomPitchSpread = 0.2f;
}
