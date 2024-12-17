using NUnit.Framework;
using System.Collections.Generic;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.Tilemaps;
using static UnityEditor.Experimental.GraphView.GraphView;

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

    private float _alpha = 1f;

    public float Alpha
    {
        get => _alpha;
        set
        {
            if (_alpha == value) return;

            _alpha = value;
            SetAlphaForAllChildren(_alpha, transform);
        }
    }

    private void SetAlphaForAllChildren(float alpha, Transform t)
    {
        for (int i = 0; i < t.childCount; i++)
        {
            SetAlphaForAllChildren(alpha, t.GetChild(i));

            if (t.GetChild(i).gameObject.TryGetComponent(out SpriteRenderer spriteRenderer))
            {
                spriteRenderer.color = new Color(
                    spriteRenderer.color.r,
                    spriteRenderer.color.g,
                    spriteRenderer.color.b,
                    alpha
                    );
            }
            else if (t.GetChild(i).gameObject.TryGetComponent(out Tilemap tilemap))
            {
                tilemap.color = new Color(
                    tilemap.color.r,
                    tilemap.color.g,
                    tilemap.color.b,
                    alpha
                    );
            }
        }
    }
}
