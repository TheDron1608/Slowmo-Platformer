using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

[DefaultExecutionOrder(-1)]
public abstract class AbstractMultiSpriteSO : ScriptableObject
{
    public string TargetSpritesDir = "\\Assets\\Sprites";
    public string SampleSpritePrefix;

    [Serializable]
    public class SerializableSampleSpritesDictionaryItem
    {
        public Sprite Key;
        public Sprite[] Value;

        public SerializableSampleSpritesDictionaryItem(Sprite key, Sprite[] value)
        {
            Key = key;
            Value = value;
        }
    }

    public Sprite[] GetSampleSprites(Sprite keySprite)
    {
        foreach (var sampleSprites in _serializedSampleSprites)
        {
            if (sampleSprites.Key == keySprite) return sampleSprites.Value;
        }
        throw new UnityException("not found key sprite with value: " + keySprite);
    }

    //unity 6.0.026f1 not supports serialized dictionaries
    [SerializeField] private List<SerializableSampleSpritesDictionaryItem> _serializedSampleSprites = new();
#if UNITY_EDITOR
    public void UpdateCharacterTextures()
    {
        _serializedSampleSprites.Clear();

        string[] files = Directory.GetFiles(Directory.GetCurrentDirectory() + TargetSpritesDir, "*png", SearchOption.AllDirectories);
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

                    if ((newSpriteName != currentSpriteName && currentSpriteName != null) || (i == files.Length - 1 && j == loadedItems.Length - 1))
                    {
                        foreach (string key in currentSprites.Keys)
                        {
                            Sprite mainSampleSpritePrefix = currentSprites[key].Find(sprite => sprite.name.Contains("." + SampleSpritePrefix + "_"));
                            _serializedSampleSprites.Add(
                                new AbstractMultiSpriteSO.SerializableSampleSpritesDictionaryItem(mainSampleSpritePrefix, currentSprites[key].ToArray())
                                );
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

        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        //AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(this)).SaveAndReimport();

        Debug.Log("Updated at " + AssetDatabase.GetAssetPath(this));
    }
#endif
}