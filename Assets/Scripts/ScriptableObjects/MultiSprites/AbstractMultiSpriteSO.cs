using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.WSA;

public abstract class AbstractMultiSpriteSO : ScriptableObject
{
    public string TargetSpritesDir = "\\Assets\\Sprites";
    public string SampleSpritePrefix;

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

    private Dictionary<Sprite, Sprite[]> _sampleSprites = new();
    public Dictionary<Sprite, Sprite[]> SampleSprites
    {
        get => _sampleSprites;
    }

    private void OnValidate()
    {
        OnVirtualValidate();
    }

    protected virtual void OnVirtualValidate()
    {
        foreach (var listItem in _serializedSampleSprites)
        {
            SampleSprites.Add(listItem.Key, listItem.Value);
        }
    }

    //unity 6.0.026f1 not supports serialized dictionaries
    [SerializeField] private List<SerializableSampleSpritesDictionaryItem> _serializedSampleSprites = new();

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
        AssetDatabase.SaveAssetIfDirty(this);

        Debug.Log("Updated");
    }
}