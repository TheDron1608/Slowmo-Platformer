using NUnit.Framework;
using System.Collections.Generic;
using UnityEditor.SearchService;
using UnityEngine;

public class ZIndexLayer : MonoBehaviour
{
    public int ZIndex = 1;

    private GameObject _enviromentContainer;
    public GameObject EnviromentContainer
    {
        get => _enviromentContainer;
        private set => _enviromentContainer = value;
    }

    private GameObject _charactersContainer;
    public GameObject CharacterContainer
    {
        get => _charactersContainer;
        private set => _charactersContainer = value;
    }

    private GameObject _holdablesContainer;
    public GameObject HoldablesContainer
    {
        get => _holdablesContainer;
        private set => _holdablesContainer = value;
    }

    private GameObject _furnitureContainer;
    public GameObject FurnitureContainer
    {
        get => _furnitureContainer; 
        private set => _furnitureContainer = value;
    }

    private void Awake()
    {
        foreach (Transform t in transform)
        {
            switch (t.gameObject.name)
            {
                case LayerManager.ENVIROMENT_TAG_NAME:
                    EnviromentContainer = t.gameObject;
                    break;
                case LayerManager.CHARACTER_TAG_NAME:
                    CharacterContainer = t.gameObject;
                    break;
                case LayerManager.HOLDABLE_TAG_NAME:
                    HoldablesContainer = t.gameObject;
                    break;
                case LayerManager.FURNITURE_TAG_NAME:
                    FurnitureContainer = t.gameObject;
                    break;
            }
        }
    }
}
