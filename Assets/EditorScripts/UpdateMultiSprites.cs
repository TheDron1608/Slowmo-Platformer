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

        foreach (var file in files)
        {

            UnityEngine.Object[] loadedItems = AssetDatabase.LoadAllAssetsAtPath(file.Substring(Directory.GetCurrentDirectory().Length + 1));
            foreach (var item in loadedItems)
            {
                if (item is Sprite spriteItem)
                {
                    string newSpriteName = item.name.Substring(0, item.name.IndexOf('.'));

                    if (newSpriteName != currentSpriteName && currentSpriteName != null)
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