using UnityEngine;
using UnityEditor;
using System;
using UnityEngine.UIElements;
using System.IO;
using Unity.VisualScripting;

public class UpdateMultiSprites : Editor
{
    const string CHARACTER_SPRITES_DIR = "\\Assets\\Sprites\\Character";

    [MenuItem("Tools/Update character textures")]
    private static void UpdateCharacterTextures()
    {
        string[] files = Directory.GetFiles(Directory.GetCurrentDirectory() + CHARACTER_SPRITES_DIR, "*png", SearchOption.AllDirectories);
        foreach (var file in files)
        {
            Debug.Log(file);
            Debug.Log(Resources.Load(file));
        }
    }
}
