using System.Collections;
using Unity.Mathematics;
using UnityEngine;

public class FluidParticle : AbstractParticle
{
    const float MIN_FLUID_PARTICLE_LIFETIME_SECONDS = 0.25f;
    const float MAX_FLUID_PARTCIEL_LIFETIME_SECONDS = 1f;
    const float LIMIT_FLUID_PARTICLE_LIFETIME_SECONDS = 10f;
    const float FLUID_STOP_VELOCITY_MULTIPLIER = 10f;
    const float FLUID_GRAVITY_MULTIPLIER = 0.5f;
    const float MIN_APPEAR_SPEED_LIFETIME_REQUIRED = 0.125f;
    const float MAX_DISTANCE_TO_INCREASE_SPRITE_SORTING_ORDER = 1.5f;
    const float MAX_DISTANCE_TO_REMOVE_NEAREST_PARTICLE = 0.15f;
    const float REMOVE_DURATION_SECONDS = 0.5f;
    const string ANIMATOR_APPEAR_SPEED_PARAM_NAME = "AppearSpeed";
    const int FLYING_SPRITE_SORTING_ORDER_ADD = 99;

    private Vector2  _velocity;
    private float _lifeTime;
    private Coroutine _moveCoroutine;
    private Vector3 _previousPosition;
    private int _currentEnviromentLayerMask;
    private bool _addedExtraFlyingSortingOrder = false;
    private Sprite _flyingSprite;

    private void SetAddedExtraFlyingSortingOrder(bool value)
    {
        if (_addedExtraFlyingSortingOrder == value) return;
        GetComponent<SpriteRenderer>().sortingOrder += FLYING_SPRITE_SORTING_ORDER_ADD * (value ? 1 : -1);
        _addedExtraFlyingSortingOrder = value;
    }

    [SerializeField] private int _size;

    public int Size
    {
        get => _size; 
        set => _size = value;
    }

    private void Awake()
    {
        _flyingSprite = GetComponent<SpriteRenderer>().sprite;
    }

    public override void SetParticleAttrs(
        Vector2 position, 
        Vector2 direction, 
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
        base.SetParticleAttrs(position, direction, velocity, angularVelocity, material, layer, sprite, animator, collider, particleName);

        _currentEnviromentLayerMask = 1 << layer.EnviromentLayer;
        _velocity = direction * velocity;
        _lifeTime = NumberMath.PickRandomInRangeNoSeed(MIN_FLUID_PARTICLE_LIFETIME_SECONDS, MAX_FLUID_PARTCIEL_LIFETIME_SECONDS);

        if (material != null && TryGetComponent(out SpriteRenderer newParticleSpriteRenderer))
        {
            newParticleSpriteRenderer.material = material;
        }

        if (_moveCoroutine != null)
        {
            StopCoroutine(_moveCoroutine);
        }
        _moveCoroutine = StartCoroutine(MoveCoroutine());

        GetComponent<Animator>().SetFloat(ANIMATOR_APPEAR_SPEED_PARAM_NAME, Mathf.Max(1f, MIN_APPEAR_SPEED_LIFETIME_REQUIRED / _lifeTime > 0 ? _lifeTime : 1f));
    }

    private IEnumerator MoveCoroutine()
    {
        GetComponent<Animator>().enabled = false;
        GetComponent<SpriteRenderer>().sprite = _flyingSprite;
        SetAddedExtraFlyingSortingOrder(true);

        _previousPosition = transform.position;

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
                StartCoroutine(SmoothStopCoroutine());
                break;
            }

            RaycastHit2D hit = Physics2D.Linecast(_previousPosition, transform.position, _currentEnviromentLayerMask);
            if (hit.collider != null)
            {
                transform.position = VectorMath.Vec2ToVec3(hit.point, transform.position.z);
                if (isOnBackgound)
                {
                    InstantStop();
                }
                else
                {
                    RemoveParticle();
                }
                break;
            }

            _previousPosition = transform.position;
        }
    }

    private IEnumerator SmoothStopCoroutine()
    {
        GetComponent<Animator>().enabled = true;

        while (true)
        {
            yield return new WaitForFixedUpdate();

            transform.position += VectorMath.Vec2ToVec3(_velocity) * Time.fixedDeltaTime;
            _velocity = math.lerp(_velocity, 0, Time.fixedDeltaTime * FLUID_STOP_VELOCITY_MULTIPLIER);

            if (
                !LayerManager.Instance.GetZLayerOfGameObject(gameObject).TileManager.GetHasTileBehaviourAt(
                    transform.position, TileBehaviour.TileBehaviourType.BACKGROUND
                    )
                )
            {
                StartCoroutine(MoveCoroutine());
                break;
            }

            if (VectorMath.Vec2ToDistance(_velocity) <= 0.05f)
            {
                InstantStop();
                break;
            }
        }
    }

    private void InstantStop()
    {
        GetComponent<Animator>().enabled = true;
        SetAddedExtraFlyingSortingOrder(false);
        _velocity = Vector2.zero;
    }

    public bool GetIsStatic()
    {
        return _velocity == Vector2.zero;
    }

    public override void RemoveParticle()
    {
        base.RemoveParticle();

        transform.parent = ParticlesManager.Instance.UnusedFluidParticleContainer;
    }
}
