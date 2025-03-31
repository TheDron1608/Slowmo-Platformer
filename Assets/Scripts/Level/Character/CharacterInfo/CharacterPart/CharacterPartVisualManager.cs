using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.WSA;

public class CharacterPartVisualManager : MonoBehaviour
{
    public const AnimatedCharacterParts SAMPLE_ANIMATION = AnimatedCharacterParts.Body;

    const string CHARACTER_SPRITES_DIR = "\\Assets\\Sprites\\Character";

    [Serializable]
    public class SerializableSampleSpritesDictionaryItem
    {
        public Sprite Key;
        public Sprite[] Value;

        public SerializableSampleSpritesDictionaryItem (Sprite key, Sprite[] value)
        {
            Key = key;
            Value = value;
        }
    }

    public static CharacterPartVisualManager Instance;

    /// <summary>
    /// Note: if you will add new item, they MUST be sorted by name alphabetically and be same as sprite name
    /// </summary>
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

    /// <summary>
    /// Note: if you will add new item, they MUST be sorted by name alphabetically and be same as sprite name
    /// </summary>
    public enum AnimatedCharacterParts : int
    {
        BodyArmor,
        BodyCape,
        BodyHeavyArmor,
        Body,
        EyesDefault,
        EyesGlasses,
        HeadHeavyHelmet,
        HeadHelmet,
        Head,
        LegsArmor
    }

    public Dictionary<Sprite, Sprite[]> SampleSprites = new();

    //unity 6.0.026f1 not supports serialized dictionaries
    public List<SerializableSampleSpritesDictionaryItem> SerializedSampleSprites = new();


    private void Awake()
    {
        if (Instance != null) throw new UnityException("limit of 1 CharacterPartVisualManager per scene");
        Instance = this;

        for (int i = 0; i < SerializedSampleSprites.Count; i++)
        {
            SampleSprites.Add(SerializedSampleSprites[i].Key, SerializedSampleSprites[i].Value);
        }
    }

    private void OnDestroy()
    {
        Instance = null;
    }
}