using System;
using System.Collections.Generic;
using System.Reflection;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelColorset", menuName = "WorldGeneration/LevelColorset")]
public class LevelColorset : ScriptableObject
{
    public class ColorTypeAttr : Attribute
    {
        public ColorType Type;

        public ColorTypeAttr(ColorType type)
        {
            Type = type;
        }
    }

    public enum ColorType
    {
        //enviroment
        NORMAL_TILES = 0,
        STICKY_TILES = 1,
        BACKGROUND = 2,
        BACKGROUND_DECORATIONS = 3,
        OVERGROUND = 4,
        OVERGROUND_DECATIONS = 5,
        LAYER_OVERLAY = 6,

        //furniture
        INTERACTABLE_FURNITURE = 100,
        LOOT_FURNITURE = 101,
        DECORATIVE_FURNITURE = 102,
        DOOR = 103,

        //holdables
        DEFAULT_HOLDABLE = 200,
        TIER1_WEAPON = 201,
        TIER2_WEAPON = 202,
        TIER3_WEAPON = 203,

        //characters
        PLAYER_CHARACTER = 300,
        PLAYER_CHARACTER_EYES = 301,
        PLAYER_CHARACTER_CAPE = 302,
        CLUMSY_ENEMY = 303,
        CLUMSY_ENEMY_EYES = 304,
        NORMAL_ENEMY = 305,
        NORMAL_ENEMY_EYES = 306,
        FAST_ENEMY = 307,
        FAST_ENEMY_EYES = 308,
        THROWER_ENEMY = 309,
        THROWER_ENEMY_EYES = 310,
        DEFLECTOR_ENEMY = 311,
        DEFLECTOR_ENEMY_EYES = 312, 

        //equipment
        DEFAULT_EQUIPMENT = 400,
        MINOR_ARMOR_EQUIPMENT = 401,
        HEAVY_ARMOR_EQUIPMENT = 402,
        DEFAULT_GLASSES = 403,

        //projectiles
        DEFAULT_MELEE_PROJECTILE = 500,
        DEFAULT_ENEMY_MELEE_PROJECTILE = 501,
        DEFAULT_RANGED_PROJECTILE = 502,
        DEFAULT_ENEMY_RANGED_PROJECTILE = 503
    }

    public Material GetMaterialByType(ColorType type)
    {
        foreach (var prop in GetType().GetFields())
        {
            if (prop.GetAttribute<ColorTypeAttr>()?.Type == type)
            {
                if (prop.GetValue(this) is Material result)
                {
                    return result;
                }
                else
                {
                    throw new UnityException("Color manager's values with ColorTypeAttr attribute must be of Material type");
                }
            }
        }

        throw new UnityException("Color manager not found property with ColorType: " + type);
    }

    [Header("Enviroment")]
    [ColorTypeAttr(ColorType.NORMAL_TILES)] public Material NormalTiles;
    [ColorTypeAttr(ColorType.STICKY_TILES)] public Material StickyTiles;
    [ColorTypeAttr(ColorType.BACKGROUND)] public Material Backgound;
    [ColorTypeAttr(ColorType.BACKGROUND_DECORATIONS)] public Material BackgroundDecations;
    [ColorTypeAttr(ColorType.OVERGROUND)] public Material Overgound;
    [ColorTypeAttr(ColorType.OVERGROUND_DECATIONS)] public Material OvergroundDecorations;
    [ColorTypeAttr(ColorType.LAYER_OVERLAY)] public Material LayerOverlay;
    [Header("Furniture")]
    [ColorTypeAttr(ColorType.INTERACTABLE_FURNITURE)] public Material Furniture;
    [ColorTypeAttr(ColorType.LOOT_FURNITURE)] public Material LootFurniture;
    [ColorTypeAttr(ColorType.DECORATIVE_FURNITURE)] public Material DecorativeFurniture;
    [ColorTypeAttr(ColorType.DOOR)] public Material Door;
    [Header("Holdables")]
    [ColorTypeAttr(ColorType.DEFAULT_HOLDABLE)] public Material DefaultHoldable;
    [ColorTypeAttr(ColorType.TIER1_WEAPON)] public Material Tier1Weapon;
    [ColorTypeAttr(ColorType.TIER2_WEAPON)] public Material Tier2Weapon;
    [ColorTypeAttr(ColorType.TIER3_WEAPON)] public Material Tier3Weapon;
    [Header("Characters")]
    [ColorTypeAttr(ColorType.PLAYER_CHARACTER)] public Material PlayerCharacter;
    [ColorTypeAttr(ColorType.PLAYER_CHARACTER_EYES)] public Material PlayerCharacterEyes;
    [ColorTypeAttr(ColorType.PLAYER_CHARACTER_CAPE)] public Material PlayerCharacterCape;
    [ColorTypeAttr(ColorType.CLUMSY_ENEMY)] public Material ClumsyEnemy;
    [ColorTypeAttr(ColorType.CLUMSY_ENEMY_EYES)] public Material ClumsyEnemyEyes;
    [ColorTypeAttr(ColorType.NORMAL_ENEMY)] public Material NormalEnemy;
    [ColorTypeAttr(ColorType.NORMAL_ENEMY_EYES)] public Material NormalEnemyEyes;
    [ColorTypeAttr(ColorType.FAST_ENEMY)] public Material FastEnemy;
    [ColorTypeAttr(ColorType.FAST_ENEMY_EYES)] public Material FastEnemyEyes;
    [ColorTypeAttr(ColorType.THROWER_ENEMY)] public Material ThrowerEnemy;
    [ColorTypeAttr(ColorType.THROWER_ENEMY_EYES)] public Material ThrowerEnemyEyes;
    [ColorTypeAttr(ColorType.DEFLECTOR_ENEMY)] public Material DeflectorEnemy;
    [ColorTypeAttr(ColorType.DEFLECTOR_ENEMY_EYES)] public Material DeflectorEnemyEyes;
    [Header("Equipment")]
    [ColorTypeAttr(ColorType.DEFAULT_EQUIPMENT)] public Material DefaultEquipment;
    [ColorTypeAttr(ColorType.MINOR_ARMOR_EQUIPMENT)] public Material MinorArmorEquipment;
    [ColorTypeAttr(ColorType.HEAVY_ARMOR_EQUIPMENT)] public Material HeavyArmorEquipment;
    [ColorTypeAttr(ColorType.DEFAULT_GLASSES)] public Material DefaultGlasses;
    [Header("Projectiles")]
    [ColorTypeAttr(ColorType.DEFAULT_MELEE_PROJECTILE)] public Material DefaultMeleeProjectile;
    [ColorTypeAttr(ColorType.DEFAULT_ENEMY_MELEE_PROJECTILE)] public Material DefaultEnemyMeleeProjectile;
    [ColorTypeAttr(ColorType.DEFAULT_RANGED_PROJECTILE)] public Material DefaultRangedProjectile;
    [ColorTypeAttr(ColorType.DEFAULT_ENEMY_RANGED_PROJECTILE)] public Material DefaultEnemyRangedProjectile;
}
