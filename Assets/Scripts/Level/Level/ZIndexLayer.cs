using NUnit.Framework;
using System.Collections.Generic;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.Tilemaps;
using static UnityEditor.Experimental.GraphView.GraphView;

public class ZIndexLayer : MonoBehaviour
{
    public int ZIndex = 1;

    private void Awake()
    {
        UpdateOrderInLayerForAllChildren();
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

            if (t.GetChild(i).TryGetComponent(out SpriteRenderer spriteRenderer))
            {
                spriteRenderer.color = new Color(
                    spriteRenderer.color.r,
                    spriteRenderer.color.g,
                    spriteRenderer.color.b,
                    alpha
                    );
            }
            else if (t.GetChild(i).TryGetComponent(out Tilemap tilemap))
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

    public void UpdateOrderInLayerForAllChildren()
    {
        UpdateOrderInLayerForAllChildren(transform);
    }
    public void UpdateOrderInLayerForAllChildren(Transform t)
    {
        for (int i = 0; i < t.childCount; i++)
        {
            UpdateOrderInLayerForAllChildren(t.GetChild(i));

            UpdateOrderLayerForGameObject(t.GetChild(i).gameObject);
        }
    }

    public void UpdateOrderLayerForGameObject(GameObject gameObject)
    {
        if (gameObject.TryGetComponent(out SpriteRenderer spriteRenderer))
        {
            spriteRenderer.sortingOrder = spriteRenderer.sortingOrder % 100 + ZIndex * 100;
        }
        else if (gameObject.TryGetComponent(out TilemapRenderer tileMapRenderer))
        {
            tileMapRenderer.sortingOrder = tileMapRenderer.sortingOrder % 100 + ZIndex * 100;
        }
    }
}
