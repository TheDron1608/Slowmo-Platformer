using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "ModificatorPrices", menuName = "ModificatorPrices")]
public class ModificatorPrices : ScriptableObject
{
    [CustomEditor(typeof(ModificatorPrices))]
    public class ModificatorPricesEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            ModificatorPrices pricesTarget = target as ModificatorPrices;

            if (GUILayout.Button("Update info"))
            {
                pricesTarget.LoadPrices();
            }
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Sort by name"))
            {
                pricesTarget.SortByName();
            }
            if (GUILayout.Button("Sort by price"))
            {
                pricesTarget.SortByPrice();
            }
            GUILayout.EndHorizontal();

            var headerStyle = new GUIStyle();
            headerStyle.fontSize = 18;
            headerStyle.normal.textColor = Color.white;

            string prevTypeTitle = null;
            foreach (ModificatorPriceInfo priceInfo in pricesTarget._prices)
            {
                if (prevTypeTitle != priceInfo.Modificator.ModificatorType.ToString())
                {
                    prevTypeTitle = priceInfo.Modificator.ModificatorType.ToString();
                    GUILayout.Label(prevTypeTitle, headerStyle);
                }
                EditorGUILayout.LabelField(priceInfo.Modificator.gameObject.name);

                float inputVal = EditorGUILayout.FloatField(priceInfo.Price);
                if (inputVal != priceInfo.Price)
                {
                    priceInfo.Price = inputVal;
                    priceInfo.Modificator.ModificatorPrice = priceInfo.Price;
                    EditorUtility.SetDirty(priceInfo.Modificator);
                    Repaint();
                }
            }
        }
    }

    private void OnValidate()
    {
        if (_prices == null)
        {
            LoadPrices();
        }
    }

    public class ModificatorPriceInfo
    {
        public AbstractModificator Modificator;
        public float Price = 0;
    }

    public string ModificatorsPath = "Assets\\Prefabs\\Modificators";

    private List<ModificatorPriceInfo> _prices = null;

    private void LoadPrices()
    {
        List<ModificatorPriceInfo> newPrices = new();
        foreach (var assetGUID in AssetDatabase.FindAssets("t:GameObject", new string[] { ModificatorsPath }))
        {
            AbstractModificator modificatorAsset = AssetDatabase.LoadAssetAtPath<AbstractModificator>(AssetDatabase.GUIDToAssetPath(assetGUID));
            if (modificatorAsset == null) continue;

            ModificatorPriceInfo newInfo = new();
            newInfo.Modificator = modificatorAsset;
            newInfo.Price = modificatorAsset.ModificatorPrice;
            newPrices.Add(newInfo);
        }
        _prices = newPrices;
        SortByName();
    }

    private void SortByName()
    {
        _prices = _prices.OrderBy(e => (int)e.Modificator.ModificatorType + e.Modificator.gameObject.name).ToList();
    }

    private void SortByPrice()
    {
        _prices = _prices.OrderBy(e => (int)e.Modificator.ModificatorType * 1_000_000f + e.Price).ToList();
    }
}