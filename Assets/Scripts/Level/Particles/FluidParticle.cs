using System.Collections;
using Unity.Mathematics;
using UnityEngine;

public class FluidParticle : MonoBehaviour
{
    const float MAX_FLUID_PARTICLE_LIFETIME_SECONDS = 10f;
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
    private Coroutine _removeCoroutine;
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
        ZIndexLayer layer = LayerManager.Instance.GetZLayerOfGameObject(gameObject);
        LayerManager.Instance.ChangeZIndexForGameObject(layer, gameObject);
        _currentEnviromentLayerMask = 1 << layer.EnviromentLayer;
        _flyingSprite = GetComponent<SpriteRenderer>().sprite;
    }

    public void SetProperties(Vector2 velocity, float lifeTime, Material material)
    {
        _velocity = velocity;
        _lifeTime = lifeTime;

        if (material != null && TryGetComponent(out SpriteRenderer newParticleSpriteRenderer))
        {
            newParticleSpriteRenderer.material = material;
        }

        if (_moveCoroutine != null)
        {
            StopCoroutine(_moveCoroutine);
        }
        _moveCoroutine = StartCoroutine(MoveCoroutine());

        GetComponent<Animator>().SetFloat(ANIMATOR_APPEAR_SPEED_PARAM_NAME, Mathf.Max(1f, MIN_APPEAR_SPEED_LIFETIME_REQUIRED / lifeTime > 0 ? lifeTime : 1f));
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
                    InstantRemoveFluidParticle();
                }
                break;
            }

            if (_awaitTime > MAX_FLUID_PARTICLE_LIFETIME_SECONDS)
            {
                RemoveFluidParticle();
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
        RemoveNeighbourFluidParticlesAndSetSortingOrder();
    }

    private void RemoveNeighbourFluidParticlesAndSetSortingOrder()
    {
        int highestRemovedParticleSortingOrder = 0;
        Transform particlesContainer = LayerManager.Instance.GetZLayerOfGameObject(gameObject).FluidParticlesContainer;
        for (int i = 0; i < particlesContainer.childCount; i++)
        {
            Transform particle = particlesContainer.GetChild(i);
            if (
                particle != null &&
                particle.TryGetComponent(out FluidParticle fluidparticle) &&
                fluidparticle.GetIsStatic() &&
                fluidparticle != this
                )
            {
                float distanceToFluidParticle = Vector2.Distance(transform.position, particle.position);

                if (
                    distanceToFluidParticle <= MAX_DISTANCE_TO_REMOVE_NEAREST_PARTICLE &&
                    (
                        (GetComponent<SpriteRenderer>().material == fluidparticle.GetComponent<SpriteRenderer>().material && Size >= fluidparticle.Size) ||
                        Size > fluidparticle.Size
                    )
                    )
                {
                    fluidparticle.RemoveFluidParticle();
                }
                else if (distanceToFluidParticle <= MAX_DISTANCE_TO_INCREASE_SPRITE_SORTING_ORDER)
                {
                    highestRemovedParticleSortingOrder = math.max(highestRemovedParticleSortingOrder, (fluidparticle.GetComponent<SpriteRenderer>().sortingOrder + 1) % 100);
                }
            }
        }
        GetComponent<SpriteRenderer>().sortingOrder += highestRemovedParticleSortingOrder;
    }

    public bool GetIsStatic()
    {
        return _velocity == Vector2.zero;
    }

    public bool GetIsRemoving()
    {
        return _removeCoroutine != null;
    }

    public void RemoveFluidParticle()
    {
        if (_removeCoroutine == null)
        {
            _removeCoroutine = StartCoroutine(RemoveFluidParticleProcess());
        }
    }

    public void InstantRemoveFluidParticle()
    {
        GameObject.Destroy(gameObject);
    }

    private IEnumerator RemoveFluidParticleProcess()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        while (spriteRenderer.color.a > 0f)
        {
            spriteRenderer.color = new Color(
                spriteRenderer.color.r,
                spriteRenderer.color.g,
                spriteRenderer.color.b,
                spriteRenderer.color.a - Time.deltaTime / REMOVE_DURATION_SECONDS
                );
            yield return new WaitForEndOfFrame();
        }

        GameObject.Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (FluidParticleManager.Instance != null)
        {
            FluidParticleManager.Instance.OnRemoveFluidParticle(this);
        }
    }
}
