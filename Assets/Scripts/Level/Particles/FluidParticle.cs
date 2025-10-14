using System.Collections;
using Unity.Mathematics;
using UnityEngine;

public class FluidParticle : AbstractParticle
{
    const float LIMIT_FLUID_PARTICLE_LIFETIME_SECONDS = 10f;
    const float FLUID_GRAVITY_MULTIPLIER = 0.5f;
    const int FLYING_SPRITE_SORTING_ORDER_ADD = 99;
    const int BASE_FLUID_SPREAD_ITERATIONS = 8;
    const int SPREAD_FPS = 10;

    public float MinLifeTime = 0.05f;
    public float MaxLifeTime = 0.25f;
    public float FluidAmount = 1f;
    public int MaxResolution = 16;
    public float SpreadDurationSeconds = 0.25f;
    public Sprite FlyingSprite;

    private Vector2  _velocity;
    private float _lifeTime;
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

        float _awaitTime = 0f;
        while (true)
        {
            transform.position += VectorMath.Vec2ToVec3(_velocity) * Time.fixedDeltaTime;
            _velocity += Physics2D.gravity * Time.fixedDeltaTime;
            transform.rotation = VectorMath.Vec2ToQuarterninon2D(_velocity);
            yield return new WaitForFixedUpdate();
            _lifeTime -= Time.fixedDeltaTime;
            _awaitTime += Time.fixedDeltaTime;

            bool isOnBackgound = LayerManager.Instance.GetZLayerOfGameObject(gameObject).TileManager.GetHasTileBehaviourAt(
                    transform.position, TileBehaviour.TileBehaviourType.BACKGROUND
                    );

            if (_lifeTime <= 0f && isOnBackgound)
            {
                DripOnBackground();
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
        LayerManager.Instance.GetZLayerOfGameObject(gameObject).TileManager.Debug_MarkTile(transform.position, Color.red, 999f);
        if (_flyCoroutine != null)
        {
            StopCoroutine(_flyCoroutine);
            _flyCoroutine = null;
        }

        SetAddedExtraFlyingSortingOrder(false);

        transform.rotation = LayerManager.Instance.GetZLayerOfGameObject(gameObject).transform.rotation;

        Texture2D dynamicTexture = new(MaxResolution, MaxResolution);
        dynamicTexture.filterMode = FilterMode.Point;
        FillTexture(dynamicTexture, new Color(0, 0, 0, 0));

        Vector2 velocityNormalizedPosition = VectorMath.PositionToPixelPosition(_velocity.normalized);
        _spriteRenderer.sprite = Sprite.Create(
            dynamicTexture,
            new Rect(0, 0, dynamicTexture.width, dynamicTexture.height),
            new Vector2(velocityNormalizedPosition.x, velocityNormalizedPosition.y),
            16
            );

        transform.position = VectorMath.PositionToPixelPosition(new Vector3(
            transform.position.x + dynamicTexture.width / 16 * _velocity.normalized.x,
            transform.position.y + dynamicTexture.height / 16 * _velocity.normalized.y, 
            transform.position.z)
            );

        if (_spreadCoroutine != null)
        {
            StopCoroutine(_spreadCoroutine);
        }
        _spreadCoroutine = StartCoroutine(SpreadCoroutine(dynamicTexture));
    }

    private IEnumerator SpreadCoroutine(Texture2D targetTexture)
    {
        Vector2Int startPosition = new(
            targetTexture.width / 2 - (int)math.round(_velocity.normalized.x * targetTexture.width / 2), 
            targetTexture.height / 2 - (int)math.round(_velocity.normalized.y * targetTexture.height / 2)
            );

        for (int i = 0; i < math.ceil(BASE_FLUID_SPREAD_ITERATIONS * FluidAmount); i++)
        {
            Vector2Int targetPosition = startPosition + VectorMath.Vec2ToVec2Int(_velocity.normalized * i);
            targetTexture.SetPixel(targetPosition.x, targetPosition.y, Color.white / i);
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
}
