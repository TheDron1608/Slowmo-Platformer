using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.WSA;

[CreateAssetMenu(fileName = "CharacterMultiSpriteSO", menuName = "MultiSprites/CharacterMultiSpritesSO")]
[DefaultExecutionOrder(-1)]
public class CharacterMultiSpritesSO : AbstractMultiSpriteSO
{
    public static CharacterMultiSpritesSO Instance;

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

    private readonly Dictionary<AnimatedCharacterParts, int> _animatedCharacerPartsOrderInLayer = new()
    {
        { AnimatedCharacterParts.BodyArmor, 5 },
        { AnimatedCharacterParts.BodyCape, 4 },
        { AnimatedCharacterParts.BodyHeavyArmor, 5 },
        { AnimatedCharacterParts.Body, 1 },
        { AnimatedCharacterParts.EyesDefault, 5 },
        { AnimatedCharacterParts.EyesGlasses, 6 },
        { AnimatedCharacterParts.HeadHeavyHelmet, 7 },
        { AnimatedCharacterParts.HeadHelmet, 7 },
        { AnimatedCharacterParts.Head, 2 },
        { AnimatedCharacterParts.LegsArmor, 3 }
    };

    public Dictionary<AnimatedCharacterParts, int> AnimatedCharacerPartsOrderInLayer
    {
        get => _animatedCharacerPartsOrderInLayer;
    }

    protected override void OnVirtualValidate()
    {
        base.OnVirtualValidate();
        Instance = this;
    }

    [CustomEditor(typeof(CharacterMultiSpritesSO))]
    public class UpdateMultiSprites : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("UpdateCharacterTextures"))
            {
                ((AbstractMultiSpriteSO)target).UpdateCharacterTextures();
            }
        }
    }
}