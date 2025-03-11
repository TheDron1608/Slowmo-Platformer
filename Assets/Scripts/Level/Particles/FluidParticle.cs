using System.Collections;
using UnityEngine;

public class FluidParticle : MonoBehaviour
{
    const float FLUID_GRAVITY_MULTIPLIER = 0.5f;

    private Vector2  _velocity;
    private float _lifeTime;
    private Coroutine _moveCoroutine;

    public void SetProperties(Vector2 velocity, float lifeTime)
    {
        _velocity = velocity;
        _lifeTime = lifeTime;
        if (_moveCoroutine != null)
        {
            StopCoroutine(_moveCoroutine);
        }
        _moveCoroutine = StartCoroutine(MoveCoroutine());
    }

    private IEnumerator MoveCoroutine()
    {
        while (_lifeTime > 0f)
        {
            transform.position += VectorMath.Vec2ToVec3(_velocity) * Time.fixedDeltaTime;
            _velocity += Physics2D.gravity * Time.fixedDeltaTime * FLUID_GRAVITY_MULTIPLIER;
            yield return new WaitForFixedUpdate();
            _lifeTime -= Time.fixedDeltaTime;
        }
        GetComponent<Animator>().enabled = true;
    }
}
