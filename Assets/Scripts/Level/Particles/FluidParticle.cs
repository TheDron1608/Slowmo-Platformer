using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditor.Localization.Plugins.XLIFF.V12;
using UnityEngine;
using static UnityEngine.UI.Image;

public class FluidParticle : AbstractSpriteParticle
{
    const float LIMIT_FLUID_PARTICLE_LIFETIME_SECONDS = 10f;
    const float FLUID_GRAVITY_MULTIPLIER = 0.5f;
    const int BACKGROUND_SORTING_OREDER_ADD = 0;
    const int FLYING_SPRITE_SORTING_ORDER_ADD = 50;
    const int FOREGROUND_SORTING_ORDER_ADD = 700;
    const float BASE_FLUID_SPREAD_ITERATIONS = 8f;
    const float MIN_DRAW_SKIP_CHANCE = 0.3f;
    const float MAX_DRAW_SKIP_CHANCE = 0.9f;
    const int SPREAD_FPS = 30;
    const int DRIP_TEXTURE_RESOLUTION = 32;
    const float DRIP_OVERLAY_CLOSEST_PARTICLE_MAX_DISTANCE = 1.05f;
    const float DRIP_ON_FOREGROUND_PARTICLE_LENGTH_MULTIPLIER = 0.1f;

    public float MinLifeTime = 0.05f;
    public float MaxLifeTime = 0.25f;
    public float MaxAmount = 3f;
    public float MinAmount = 1f;
    public float LengthMultiplier = 1f;
    public Sprite FlyingSprite;

    private Vector2  _velocity;
    private Sprite _dripSprite;
    private float _lifeTime;
    private float _currentLifeTime = 0f;
    private Coroutine _flyCoroutine;
    private Coroutine _spreadCoroutine;
    private int _currentEnviromentLayerMask;
    private int _addedExtraFlyingSortingOrder = 0;
    private SpriteRenderer _spriteRenderer;
    private ZIndexLayer _layer;

    private void SetAddedExtraFlyingSortingOrder(int value)
    {
        if (_addedExtraFlyingSortingOrder == value) return;
        _spriteRenderer.sortingOrder -= _addedExtraFlyingSortingOrder;
        _spriteRenderer.sortingOrder += value;
        _addedExtraFlyingSortingOrder = value;

        _spriteRenderer.sortingLayerID = GetCurrentLayerSotringOrder(_layer);
    }

    public int GetCurrentLayerSotringOrder(ZIndexLayer layer)
    {
        switch (_addedExtraFlyingSortingOrder)
        {
            case FLYING_SPRITE_SORTING_ORDER_ADD:
                return layer.ObjectsSortingLayer;
            case BACKGROUND_SORTING_OREDER_ADD:
                return layer.BackgroundSortingLayer;
            case FOREGROUND_SORTING_ORDER_ADD:
                return layer.EnviromentSortingLayer;
        }
        throw new UnityException("not found valid value for current added extra soring order: " + _addedExtraFlyingSortingOrder);
    }

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        Texture2D dripTexture = new(DRIP_TEXTURE_RESOLUTION, DRIP_TEXTURE_RESOLUTION);
        dripTexture.filterMode = FilterMode.Point;
        _dripSprite = Sprite.Create(
            dripTexture,
            new Rect(0, 0, dripTexture.width, dripTexture.height),
            new Vector2(0.5f, 0.5f),
            16
            );
    }

    public override void SetParticleAttrs(
        AbstractParticle original,
        Vector2 position,
        Vector2 direction,
        float angle,
        float velocity,
        float angularVelocity,
        Material material,
        ZIndexLayer layer
        )
    {
        base.SetParticleAttrs(original, position, direction, angle, velocity, angularVelocity, material, layer);

        FluidParticle originalFluidParticle = original.GetComponent<FluidParticle>();
        MinLifeTime = originalFluidParticle.MinLifeTime;
        MaxLifeTime = originalFluidParticle.MaxLifeTime;
        MaxAmount = originalFluidParticle.MaxAmount;
        MinAmount = originalFluidParticle.MinAmount;
        LengthMultiplier = originalFluidParticle.LengthMultiplier;
        FlyingSprite = originalFluidParticle.FlyingSprite;

        _layer = LayerManager.Instance.GetZLayerOfGameObject(gameObject);

        _currentEnviromentLayerMask = 1 << layer.EnviromentLayer;
        _velocity = direction * velocity;
        _lifeTime = NumberMath.PickRandomInRangeNoSeed(MinLifeTime, MaxLifeTime);
        _currentLifeTime = 0f;
        _spriteRenderer.sprite = FlyingSprite;

        if (material != null && TryGetComponent(out SpriteRenderer newParticleSpriteRenderer))
        {
            newParticleSpriteRenderer.material = material;
        }

        if (_flyCoroutine != null)
        {
            StopCoroutine(_flyCoroutine);
        }
        _flyCoroutine = StartCoroutine(FlyCoroutine());
    }

    private IEnumerator FlyCoroutine()
    {
        if (_spreadCoroutine != null)
        {
            StopCoroutine(_spreadCoroutine);
            _spreadCoroutine = null;
        }

        _spriteRenderer.sprite = FlyingSprite;
        SetAddedExtraFlyingSortingOrder(FLYING_SPRITE_SORTING_ORDER_ADD);

        _currentLifeTime = 0f;
        while (true)
        {
            transform.position += VectorMath.Vec2ToVec3(_velocity) * Time.fixedDeltaTime;
            _velocity += Physics2D.gravity * Time.fixedDeltaTime;
            transform.rotation = VectorMath.Vec2ToQuarterninon2D(_velocity);
            yield return new WaitForFixedUpdate();
            _currentLifeTime += Time.fixedDeltaTime;

            if (_currentLifeTime > _lifeTime && GetIsOnBackground())
            {
                DripOnBackground();
                break;
            }
            else if (_currentLifeTime > LIMIT_FLUID_PARTICLE_LIFETIME_SECONDS)
            {
                RemoveParticle();
                break;
            }

            Collider2D evniromentHit = Physics2D.OverlapPoint(transform.position, _currentEnviromentLayerMask);
            if (evniromentHit != null)
            {
                if (evniromentHit.gameObject.TryGetComponent(out TileBehaviour hitTileBehaviour) && hitTileBehaviour.ValidAsPlatform)
                {
                    DripOnForeground();
                }
                else
                {
                    RemoveParticle();
                }
                break;
            }
        }
    }

    private bool GetIsOnBackground()
    {
        return 
            _layer.MultiTileMapsContainer.GetTileMapByBehaviourType(
            TileBehaviour.TileBehaviourType.BACKGROUND
            ).GetTile<BackgroundRuleTile>(new Vector3Int((int)math.floor(transform.position.x), (int)math.floor(transform.position.y), 0))?.CanBeSpilledByFluidParticles ?? false;
    }

    private void DripOnBackground()
    {
        if (_flyCoroutine != null)
        {
            StopCoroutine(_flyCoroutine);
            _flyCoroutine = null;
        }

        SetAddedExtraFlyingSortingOrder(BACKGROUND_SORTING_OREDER_ADD);
        UpdateOverlayingClosestParticles();

        transform.position = VectorMath.PositionToPixelPosition(transform.position);
        transform.rotation = _layer.transform.rotation;

        if (_spreadCoroutine != null)
        {
            StopCoroutine(_spreadCoroutine);
        }
        _spreadCoroutine = StartCoroutine(SpreadCoroutine(
            _velocity, 
            math.lerp(MaxAmount, MinAmount, NumberMath.LimitFloatBetweenZeroAndOne(_currentLifeTime / (MaxLifeTime + MinLifeTime))),
            LengthMultiplier,
            true
            ));
    }


    private void DripOnForeground()
    {
        if (_flyCoroutine != null)
        {
            StopCoroutine(_flyCoroutine);
            _flyCoroutine = null;
        }

        SetAddedExtraFlyingSortingOrder(FOREGROUND_SORTING_ORDER_ADD);
        UpdateOverlayingClosestParticles();

        transform.position = VectorMath.PositionToPixelPosition(transform.position);
        transform.rotation = _layer.transform.rotation;

        if (_spreadCoroutine != null)
        {
            StopCoroutine(_spreadCoroutine);
        }
        _spreadCoroutine = StartCoroutine(SpreadCoroutine(
            _velocity,
            math.lerp(MaxAmount, MinAmount, NumberMath.LimitFloatBetweenZeroAndOne(_currentLifeTime / (MaxLifeTime + MinLifeTime))),
            LengthMultiplier * DRIP_ON_FOREGROUND_PARTICLE_LENGTH_MULTIPLIER,
            false
            ));
    }

    private IEnumerator SpreadCoroutine(Vector2 velocity, float amount, float lengthMultiplier, bool backgroundOrForeground)
    {
        ClearPixels(_dripSprite.texture, new Color(1, 1, 1, 0));
        _spriteRenderer.sprite = _dripSprite;

        int spreadLength = math.max(math.min((int)math.ceil(BASE_FLUID_SPREAD_ITERATIONS * amount * lengthMultiplier), DRIP_TEXTURE_RESOLUTION / 2), 1);
        int currentLength = 0;

        Vector2Int startPosition = new(
            _dripSprite.texture.width / 2,
            _dripSprite.texture.height / 2
            );

        while (currentLength < spreadLength)
        {
            currentLength += (int)math.ceil((spreadLength - currentLength) / 2f);

            for (int i = 0; i < currentLength; i++)
            {
                if (UnityEngine.Random.value < math.lerp(MIN_DRAW_SKIP_CHANCE, MAX_DRAW_SKIP_CHANCE, i / spreadLength)) continue;

                Vector2Int targetPosition = startPosition + VectorMath.Vec2ToVec2Int(velocity.normalized * i);
                DrawFluidPoint(_dripSprite.texture, targetPosition, (int)math.floor(amount * (currentLength - i) / spreadLength), Color.white, backgroundOrForeground);
            }
            _dripSprite.texture.Apply();
            yield return new WaitForSeconds(1f / SPREAD_FPS);
        }
    }

    public override void RemoveParticle()
    {
        base.RemoveParticle();

        if (_flyCoroutine != null)
        {
            StopCoroutine(_flyCoroutine);
            _flyCoroutine = null;
        }
        if (_spreadCoroutine != null)
        {
            StopCoroutine(_spreadCoroutine);
            _spreadCoroutine = null;
        }

        _spriteRenderer.sortingOrder -= _spriteRenderer.sortingOrder % 100;

        transform.parent = ParticlesManager.Instance.UnusedFluidParticleContainer;
    }

    private void ClearSemiTransparentPixels(Texture2D texture, Color color)
    {
        for (int x = 0; x < texture.width; x++)
        {
            for (int y = 0; y < texture.height; y++)
            {
                if (texture.GetPixel(x, y).a < 1)
                {
                    texture.SetPixel(x, y, color);
                }
            }
        }
    }

    private void ClearPixels(Texture2D texture, Color color)
    {
        for (int x = 0; x < texture.width; x++)
        {
            for (int y = 0; y < texture.height; y++)
            {
                texture.SetPixel(x, y, color);
            }
        }
    }

    private void DrawFluidPoint(Texture2D texture, Vector2Int position, int radius, Color color, bool backgroundOrForeground)
    {
        for (int y = -radius; y <= radius; y++)
        {
            for (int x = -radius; x <= radius; x++)
            {
                if (
                    x * x + y * y <= radius * radius &&
                    GetPixelPositionIsValid(texture, position.x + x, position.y + y, backgroundOrForeground)
                    )
                {
                    texture.SetPixel(position.x + x, position.y + y, color);
                }
            }
        }
    }

    private bool GetPixelPositionIsValid(Texture2D texture, int x, int y, bool backgroundOrForeground)
    {
        if (backgroundOrForeground)
        {
            return _layer.MultiTileMapsContainer.GetTileMapByBehaviourType(TileBehaviour.TileBehaviourType.BACKGROUND)
                .GetTile<BackgroundRuleTile>(new Vector3Int((int)math.floor(transform.position.x + (x - texture.width / 2) / 16f), (int)math.floor(transform.position.y + (y - texture.height / 2) / 16f), 0))
                ?.CanBeSpilledByFluidParticles ?? false;
        }
        else
        {
            return _layer.TileManager.GetHasValidAsPlatformAt(
                VectorMath.Vec3ToVec2(transform.position) + new Vector2(x - texture.width / 2, y - texture.height / 2) / 16
                );
        }
    }

    private void UpdateOverlayingClosestParticles()
    {
        foreach (Transform particle in _layer.FluidParticlesContainer)
        {
            if (
                particle.TryGetComponent(out FluidParticle fluidParticle) &&
                fluidParticle._spriteRenderer.sharedMaterial == _spriteRenderer.sharedMaterial &&
                fluidParticle._spriteRenderer.sortingLayerID == _spriteRenderer.sortingLayerID &&
                fluidParticle._spriteRenderer.sortingOrder >= _spriteRenderer.sortingOrder &&
                Vector2.Distance(fluidParticle.transform.position, transform.position) < DRIP_OVERLAY_CLOSEST_PARTICLE_MAX_DISTANCE
                )
            {
                _spriteRenderer.sortingOrder = _spriteRenderer.sortingOrder - (_spriteRenderer.sortingOrder % 100) + ((fluidParticle._spriteRenderer.sortingOrder + 1) % 100);
            }
        }
    }
}
