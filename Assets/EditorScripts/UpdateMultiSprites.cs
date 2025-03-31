using UnityEngine;
using UnityEditor;
using System;
using UnityEngine.UIElements;
using System.IO;
using Unity.VisualScripting;
using System.Linq;
using System.Collections.Generic;

[CustomEditor(typeof(CharacterPartVisualManager))]
public class UpdateMultiSprites : Editor
{
    const string CHARACTER_SPRITES_DIR = "\\Assets\\Sprites\\Character";

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        CharacterPartVisualManager myTarget = (CharacterPartVisualManager)target;

        if (GUILayout.Button("UpdateCharacterTextures"))
        {
            UpdateCharacterTextures(myTarget);
        }
    }

    private static void UpdateCharacterTextures(CharacterPartVisualManager targetVisual)
    {
        targetVisual.SerializedSampleSprites.Clear();

        string[] files = Directory.GetFiles(Directory.GetCurrentDirectory() + CHARACTER_SPRITES_DIR, "*png", SearchOption.AllDirectories);
        string currentSpriteName = null;
        Dictionary<string, List<Sprite>> currentSprites = new();

        for (int i = 0; i < files.Length; i++)
        {
            UnityEngine.Object[] loadedItems = AssetDatabase.LoadAllAssetsAtPath(files[i].Substring(Directory.GetCurrentDirectory().Length + 1));
            for (int j = 0; j < loadedItems.Length; j++)
            {
                if (loadedItems[j] is Sprite spriteItem)
                {
                    string newSpriteName = loadedItems[j].name.Substring(0, loadedItems[j].name.IndexOf('.'));

                    if ((newSpriteName != currentSpriteName && currentSpriteName != null) || (i == files.Length-1 && j == loadedItems.Length-1))
                    {
                        foreach (string key in currentSprites.Keys)
                        {
                            Sprite mainSampleSprite = currentSprites[key][(int)CharacterPartVisualManager.SAMPLE_ANIMATION];
                            targetVisual.SerializedSampleSprites.Add(
                                new CharacterPartVisualManager.SerializableSampleSpritesDictionaryItem( mainSampleSprite, currentSprites[key].ToArray()));
                        }

                        currentSprites.Clear();
                    }
                    currentSpriteName = newSpriteName;

                    string currentKey = spriteItem.name.Substring(spriteItem.name.IndexOf('_') + 1);

                    if (!currentSprites.ContainsKey(currentKey))
                    {
                        currentSprites.Add(currentKey, new List<Sprite>());
                    }
                    currentSprites[currentKey].Add(spriteItem);
                }
            }
        }

        PrefabUtility.RecordPrefabInstancePropertyModifications(targetVisual);
        Debug.Log("Updated");
    }
}