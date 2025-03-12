using System.Collections;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class FluidParticle : MonoBehaviour
{
    const float FLUID_STOP_VELOCITY_MULTIPLIER = 10f;
    const float FLUID_GRAVITY_MULTIPLIER = 0.5f;
    const float MIN_APPEAR_SPEED_LIFETIME_REQUIRED = 0.125f;
    const string ANIMATOR_APPEAR_SPEED_PARAM_NAME = "AppearSpeed";

    private Vector2  _velocity;
    private float _lifeTime;
    private Coroutine _moveCoroutine;
    private Vector3 _previousPosition;
    private int _currentEnviromentLayerMask;

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
            yield return new WaitForFixedUpdate();
            _lifeTime -= Time.fixedDeltaTime;

            RaycastHit2D hit = Physics2D.Linecast(_previousPosition, transform.position, _currentEnviromentLayerMask);
            if (hit.point != Vector2.zero)
            {
                transform.position = hit.point;
                stopParticleSmoothly = false;
                break;
            }
        }

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
    }
}
