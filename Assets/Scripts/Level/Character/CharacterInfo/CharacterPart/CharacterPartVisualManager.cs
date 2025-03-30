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
}