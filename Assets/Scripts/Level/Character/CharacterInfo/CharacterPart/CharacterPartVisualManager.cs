using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.WSA;

public class CharacterPartVisualManager : MonoBehaviour
{
    public enum CharPartsAnimation : int
    {
        Aim,
        AimStart,
        ClumsyMoveAlignChange,
        Fall,
        Fallen,
        FallenFront,
        FallOnFloor,
        FallOnKnees,
        Idle,
        Jump,
        LookBack,
        LookForward,
        MinorStun,
        Move,
        Roll,
        SlideOnWall,
        WakeUpFront,
        WakeUp
    }

    public enum AnimatedCharacterParts
    {
        Body,
        BodyArmor,
        BodyCape,
        BodyHeavyArmor,
        EyesDefault,
        EyesGlasses,
        Head,
        HeadHeavyHelmet,
        HeadHelmet,
        LegsArmor
    }

    [Serializable] private class CharacterPartClipsContent
    {
        public string Title;
        public AnimationClip[] AnimationClips;
    }

    public static CharacterPartVisualManager Instance;

    private void Awake()
    {
        if (Instance != null) throw new UnityException("limit of 1 CharacterPartVisualManager instance per scene");
        Instance = this;

        _characterPartAnimationClipsDict = new();
        foreach (var characterAnimationClip in _characterPartAnimationClips)
        {
            _characterPartAnimationClipsDict.Add(characterAnimationClip.AnimationClips[0], characterAnimationClip.AnimationClips);
        }
    }

    [SerializeField] private CharacterPartClipsContent[] _characterPartAnimationClips;
    private Dictionary<AnimationClip, AnimationClip[]> _characterPartAnimationClipsDict;

    public AnimationClip GetCharacterPartClip(AnimationClip sampleClip, AnimatedCharacterParts animatedPart)
    {
        return _characterPartAnimationClipsDict[sampleClip][(int)animatedPart];
    }

    private static List<Sprite> GetSpritesFromClip(AnimationClip clip)
    {
        var sprites = new List<Sprite>();
        if (clip != null)
        {
            foreach (var binding in AnimationUtility.GetObjectReferenceCurveBindings(clip))
            {
                var keyframes = AnimationUtility.GetObjectReferenceCurve(clip, binding);
                foreach (var frame in keyframes)
                {
                    sprites.Add((Sprite)frame.value);
                }
            }
        }
        return sprites;
    }
}
