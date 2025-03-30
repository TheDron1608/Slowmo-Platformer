using UnityEngine;
using UnityEditor;
using System;
using UnityEngine.UIElements;
using System.IO;
using Unity.VisualScripting;
using System.Linq;

public class UpdateMultiSprites : Editor
{
    const string CHARACTER_SPRITES_DIR = "\\Assets\\Sprites\\Character";

    public CharacterPartVisualManager TargetVisual;
    public static CharacterPartVisualManager StaticTargetVisual;

    [MenuItem("Tools/Update character textures")]
    private static void UpdateCharacterTextures()
    {
        string[] files = Directory.GetFiles(Directory.GetCurrentDirectory() + CHARACTER_SPRITES_DIR, "*png", SearchOption.AllDirectories);
        foreach (var file in files)
        {
            string currentAnimationShortTitle = null;
            Sprite[] currentSprites = new Sprite[Enum.GetNames(typeof(CharacterPartVisualManager.CharPartsAnimation)).Length];
            Sprite currentSampleSprite = null;
            int currentAnimationIndex = 0;

            UnityEngine.Object[] loadedItems = AssetDatabase.LoadAllAssetsAtPath(file.Substring(Directory.GetCurrentDirectory().Length + 1));
            foreach (var item in loadedItems)
            {
                string newAnimationShortTitle = null;

                if (item is Sprite spriteItem)
                {
                    currentSprites[currentAnimationIndex] = spriteItem;
                    if (currentAnimationIndex == (int)CharacterPartVisualManager.SAMPLE_ANIMATION)
                    {
                        currentSampleSprite = spriteItem;
                    }

                    newAnimationShortTitle = spriteItem.name.Substring(0, spriteItem.name.IndexOf('.')) + (spriteItem.name.IndexOf('_') != -1 ? spriteItem.name.Substring(spriteItem.name.IndexOf('_')) : "");
                    currentAnimationIndex++;

                    if (newAnimationShortTitle != currentAnimationShortTitle)
                    {
                        StaticTargetVisual.SampleSprites.Add(currentSampleSprite, currentSprites);
                        Debug.Log(currentSampleSprite + " : ");
                        foreach (Sprite sprite in currentSprites)
                        {
                            Debug.Log(sprite);
                        }

                        currentSprites = new Sprite[Enum.GetNames(typeof(CharacterPartVisualManager.CharPartsAnimation)).Length];
                        currentSampleSprite = null;
                        currentAnimationIndex = 0;
                    }

                    currentAnimationShortTitle = newAnimationShortTitle;
                }
            }
        }
        

    }

    void OnGUI()
    {
        TargetVisual = EditorGUILayout.ObjectField(TargetVisual, typeof(CharacterPartVisualManager), true) as CharacterPartVisualManager;
        StaticTargetVisual = TargetVisual;
    }
}