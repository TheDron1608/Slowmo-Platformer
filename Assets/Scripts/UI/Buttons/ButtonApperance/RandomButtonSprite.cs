using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RandomButtonSprite : MonoBehaviour
{
    [SerializeField]
    private List<Sprite> _sprites = new List<Sprite>();
    [SerializeField]
    private Image _imageContainer;

    private void Awake()
    {
        RandomizeSprite();
    }

    public void RandomizeSprite()
    {
        _imageContainer.sprite = _sprites[Random.Range(0, _sprites.Count)];
    }
}
