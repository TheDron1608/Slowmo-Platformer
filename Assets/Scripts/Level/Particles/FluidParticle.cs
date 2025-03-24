using System.Collections;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class FluidParticle : MonoBehaviour
{
    const float FLUID_STOP_VELOCITY_MULTIPLIER = 10f;
    const float FLUID_GRAVITY_MULTIPLIER = 0.5f;
    const float MIN_APPEAR_SPEED_LIFETIME_REQUIRED = 0.125f;
    const float MAX_DISTANCE_TO_REMOVE_NEAREST_PARTICLE = 0.15f;
    const float REMOVE_DURATION_SECONDS = 0.5f;
    const string ANIMATOR_APPEAR_SPEED_PARAM_NAME = "AppearSpeed";

    private Vector2  _velocity;
    private float _lifeTime;
    private Coroutine _moveCoroutine;
    private Vector3 _previousPosition;
    private int _currentEnviromentLayerMask;
    private Coroutine _removeCoroutine;

    [SerializeField] private int _size;

    public int Size
    {
        get => _size; 
        set => _size = value;
    }

    private void Awake()
    {
        _previousPosition = transform.position;
        _currentEnviromentLayerMask = 1 << LayerManager.Instance.GetZLayerOfGameObject(gameObject).EnviromentLayer;
    }

    public void SetProperties(Vector2 velocity, float lifeTime)
    {
        _velocity = velocity;
        _lifeTime = lifeTime;
        if (_moveCoroutine != null)
        {
            StopCoroutine(_moveCoroutine);
        }
        _moveCoroutine = StartCoroutine(MoveCoroutine());
        GetComponent<Animator>().SetFloat(ANIMATOR_APPEAR_SPEED_PARAM_NAME, Mathf.Max(1f, MIN_APPEAR_SPEED_LIFETIME_REQUIRED / lifeTime > 0 ? lifeTime : 1f));
    }

    private IEnumerator MoveCoroutine()
    {
        bool stopParticleSmoothly = true;
        while (_lifeTime > 0f)
        {
            transform.position += VectorMath.Vec2ToVec3(_velocity) * Time.fixedDeltaTime;
            _velocity += Physics2D.gravity * Time.fixedDeltaTime * FLUID_GRAVITY_MULTIPLIER;
            transform.rotation = VectorMath.Vec2ToQuarterninon2D(_velocity);
            yield return new WaitForFixedUpdate();
            _lifeTime -= Time.fixedDeltaTime;

            RaycastHit2D hit = Physics2D.Linecast(_previousPosition, transform.position, _currentEnviromentLayerMask);
            if (hit.point != Vector2.zero)
            {
                transform.position = VectorMath.Vec2ToVec3(hit.point, transform.position.z);
                stopParticleSmoothly = false;
                break;
            }
        }
        _velocity = Vector2.zero;

        GetComponent<Animator>().enabled = true;

        if (stopParticleSmoothly)
        {
            while (VectorMath.Vec2ToDistance(_velocity) > 0.05f)
            {
                transform.position += VectorMath.Vec2ToVec3(_velocity) * Time.fixedDeltaTime;
                _velocity = math.lerp(_velocity, 0, Time.fixedDeltaTime * FLUID_STOP_VELOCITY_MULTIPLIER);
                yield return new WaitForFixedUpdate();
            }
        }

        Transform particlesContainer = LayerManager.Instance.GetZLayerOfGameObject(gameObject).FluidParticlesContainer;
        for (int i = 0; i < particlesContainer.childCount; i++)
        {
            Transform particle = particlesContainer.GetChild(i);
            if (
                particle != null &&
                particle.TryGetComponent(out FluidParticle fluidparticle) &&
                fluidparticle.GetIsStatic() && 
                Vector2.Distance(transform.position, particle.position) <= MAX_DISTANCE_TO_REMOVE_NEAREST_PARTICLE &&
                fluidparticle != this
                ) 
            {
                if (Size > fluidparticle.Size)
                {
                    fluidparticle.RemoveFluidParticle();
                }
                else if (Size != fluidparticle.Size)
                {
                    this.RemoveFluidParticle();
                }
            }
        }
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
