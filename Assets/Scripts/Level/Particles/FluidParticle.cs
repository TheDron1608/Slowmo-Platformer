using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using static UnityEngine.UI.Image;

public class FluidParticle : AbstractParticle
{
    const float LIMIT_FLUID_PARTICLE_LIFETIME_SECONDS = 10f;
    const float FLUID_GRAVITY_MULTIPLIER = 0.5f;
    const int FLYING_SPRITE_SORTING_ORDER_ADD = 99;
    const float BASE_FLUID_SPREAD_ITERATIONS_PER_VELOCITY = 3f;
    const float MIN_DRAW_SKIP_CHANCE = 0.3f;
    const float MAX_DRAW_SKIP_CHANCE = 0.9f;
    const int SPREAD_FPS = 10;

    public float MinLifeTime = 0.05f;
    public float MaxLifeTime = 0.25f;
    public float MaxThickness = 3f;
    public float MinThickness = 1f;
    public Sprite FlyingSprite;

    private Vector2  _velocity;
    private float _lifeTime;
    private float _currentLifeTime = 0f;
    private Coroutine _flyCoroutine;
    private Coroutine _spreadCoroutine;
    private int _currentEnviromentLayerMask;
    private bool _addedExtraFlyingSortingOrder = false;
    private SpriteRenderer _spriteRenderer;

    private void SetAddedExtraFlyingSortingOrder(bool value)
    {
        if (_addedExtraFlyingSortingOrder == value) return;
        _spriteRenderer.sortingOrder += FLYING_SPRITE_SORTING_ORDER_ADD * (value ? 1 : -1);
        _addedExtraFlyingSortingOrder = value;
    }

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public override void SetParticleAttrs(
        Vector2 position, 
        Vector2 direction, 
        float angle,
        float velocity,
        float angularVelocity, 
        Material material, 
        ZIndexLayer layer, 
        Sprite sprite = null, 
        Animator animator = null, 
        BoxCollider2D collider = null,
        string particleName = "untitled"
        )
    {
        base.SetParticleAttrs(position, direction, angle, velocity, angularVelocity, material, layer, sprite, animator, collider, particleName);

        _currentEnviromentLayerMask = 1 << layer.EnviromentLayer;
        _velocity = direction * velocity;
        _lifeTime = NumberMath.PickRandomInRangeNoSeed(MinLifeTime, MaxLifeTime);
        _currentLifeTime = 0f;

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
        SetAddedExtraFlyingSortingOrder(true);

        _currentLifeTime = 0f;
        while (true)
        {
            transform.position += VectorMath.Vec2ToVec3(_velocity) * Time.fixedDeltaTime;
            _velocity += Physics2D.gravity * Time.fixedDeltaTime;
            transform.rotation = VectorMath.Vec2ToQuarterninon2D(_velocity);
            yield return new WaitForFixedUpdate();
            _currentLifeTime += Time.fixedDeltaTime;

            bool isOnBackgound = LayerManager.Instance.GetZLayerOfGameObject(gameObject).TileManager.GetHasTileBehaviourAt(
                    transform.position, TileBehaviour.TileBehaviourType.BACKGROUND
                    );

            if (_currentLifeTime > _lifeTime && isOnBackgound)
            {
                DripOnBackground();
                break;
            }
            else if (_currentLifeTime > LIMIT_FLUID_PARTICLE_LIFETIME_SECONDS)
            {
                RemoveParticle();
                break;
            }

            if (Physics2D.OverlapPoint(transform.position, 1 << _currentEnviromentLayerMask) != null)
            {
                if (isOnBackgound)
                {
                    DripOnBackground();
                }
                else
                {
                    RemoveParticle();
                }
                break;
            }
        }
    }

    private void DripOnBackground()
    {
        if (_flyCoroutine != null)
        {
            StopCoroutine(_flyCoroutine);
            _flyCoroutine = null;
        }

        SetAddedExtraFlyingSortingOrder(false);

        transform.rotation = LayerManager.Instance.GetZLayerOfGameObject(gameObject).transform.rotation;

        int targetResolution = (int)math.ceil(BASE_FLUID_SPREAD_ITERATIONS_PER_VELOCITY * _velocity.magnitude * 2f);
        Texture2D dynamicTexture = new(targetResolution, targetResolution);
        dynamicTexture.filterMode = FilterMode.Point;
        FillTexture(dynamicTexture, new Color(0, 0, 0, 0));

        Vector2 velocityNormalizedPosition = VectorMath.PositionToPixelPosition(_velocity.normalized);
        _spriteRenderer.sprite = Sprite.Create(
            dynamicTexture,
            new Rect(0, 0, dynamicTexture.width, dynamicTexture.height),
            new Vector2(0.5f, 0.5f),
            16
            );

        transform.position = VectorMath.PositionToPixelPosition(transform.position);

        if (_spreadCoroutine != null)
        {
            StopCoroutine(_spreadCoroutine);
        }
        _spreadCoroutine = StartCoroutine(SpreadCoroutine(dynamicTexture, _velocity, math.lerp(MaxThickness, MinThickness, _currentLifeTime / (MaxLifeTime + MinLifeTime))));
    }

    private IEnumerator SpreadCoroutine(Texture2D targetTexture, Vector2 velocity, float thickness)
    {
        int spreadLength = (int)math.ceil(BASE_FLUID_SPREAD_ITERATIONS_PER_VELOCITY * velocity.magnitude);
        int currentLength = 0;
        Vector2Int startPosition = new(
            targetTexture.width / 2, 
            targetTexture.height / 2 
            );

        while (currentLength < spreadLength)
        {
            currentLength += (int)math.ceil((spreadLength - currentLength) / 2f);

            for (int i = 0; i < currentLength; i++)
            {
                if (UnityEngine.Random.value < math.lerp(MIN_DRAW_SKIP_CHANCE, MAX_DRAW_SKIP_CHANCE, i / spreadLength)) continue;

                Vector2Int targetPosition = startPosition + VectorMath.Vec2ToVec2Int(velocity.normalized * i);
                DrawCircle(targetTexture, targetPosition, (int)math.floor(thickness * (currentLength - i) / spreadLength), Color.white);
            }
            targetTexture.Apply();
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

        transform.parent = ParticlesManager.Instance.UnusedFluidParticleContainer;
    }

    private void FillTexture(Texture2D texture, Color color)
    {
        for (int i = 0; i < texture.width; i++)
        {
            for (int j = 0; j < texture.height; j++)
            {
                texture.SetPixel(i, j, color);
            }
        }
        texture.Apply();
    }

    private void DrawCircle(Texture2D texture, Vector2Int position, int radius, Color color)
    {
        for (int y = -radius; y <= radius; y++)
        {
            for (int x = -radius; x <= radius; x++)
            {
                if (x * x + y * y <= radius * radius)
                {
                    texture.SetPixel(position.x + x, position.y + y, color);
                }
            }
        }
    }
}
