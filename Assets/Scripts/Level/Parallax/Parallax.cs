using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

[DefaultExecutionOrder(1)]
public class Parallax : MonoBehaviour
{
    public List<Sprite> BgSprite;
    public int SpritesAmount = 10;
    public float ParallaxScale = 1f;
    public int ParallaxSortingOrder = 1;

    [SerializeField] private Material _parallaxMaterial;
    [SerializeField] private Color _parallaxColor = Color.white;

    private List<SpriteRenderer> _currentParallaxSprites = new();
    private Vector2 _spriteSizeUnits;
    private float _randomPatternSeed;

    public Material ParallaxMaterial
    {
        get => _parallaxMaterial;
        set
        {
            foreach (SpriteRenderer parallaxSprite in _currentParallaxSprites)
            {
                if (parallaxSprite.IsDestroyed()) continue;
                parallaxSprite.sharedMaterial = value;
            }
            _parallaxMaterial = value;
        }
    }

    private void Awake()
    {
        _spriteSizeUnits = BgSprite.First().bounds.size;
        _randomPatternSeed = NumberMath.PickRandomInRangeNoSeed(100f, 500f);

        for (int i = 0; i < SpritesAmount; i++)
        {
            GameObject newParallaxGO = new GameObject("ParallaxSprite");
            newParallaxGO.transform.SetParent(transform, false);
            newParallaxGO.transform.localScale = Vector3.one * ParallaxScale;

            SpriteRenderer newParallaxRenderer = newParallaxGO.AddComponent<SpriteRenderer>();
            newParallaxRenderer.sharedMaterial = _parallaxMaterial;
            newParallaxRenderer.color = _parallaxColor;
            newParallaxRenderer.sprite = NumberMath.PickRandomItem(BgSprite);
            newParallaxRenderer.sortingOrder = ParallaxSortingOrder;

            _currentParallaxSprites.Add(newParallaxRenderer);
        }
    }

    private void Update()
    {
        float centerXPosition = Camera.main.transform.position.x;
        for (int i = 0; i < _currentParallaxSprites.Count; i++)
        {
            float xPosition = 
                centerXPosition - (centerXPosition % (_spriteSizeUnits.x * ParallaxScale)) + 
                (-_currentParallaxSprites.Count / 2 + i) * (_spriteSizeUnits.x * ParallaxScale);

            _currentParallaxSprites[i].transform.position = new Vector3(
                xPosition,
                LayerManager.Instance.GetLevelBottom(),
                transform.position.z
                );

            _currentParallaxSprites[i].sprite = BgSprite[math.abs((int)(xPosition * _randomPatternSeed)) % BgSprite.Count];
        }
    }
}