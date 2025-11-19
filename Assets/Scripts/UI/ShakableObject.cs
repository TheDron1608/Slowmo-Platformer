using Unity.Mathematics;
using UnityEngine;

public class ShakableObject : MonoBehaviour
{
    const float RANDOMIZE_DIRECTION_ACCURACY = 0.5f;

    public float ContantShakingForce = 0f;
    public float DecayingShakingForce = 0f;
    public float ShakingAmplitude = 10f;
    public float ShakingDecayingMultiplier = 5f;

    private Vector2 _currentShakeDirection;
    private Vector2 _currentOffset;

    public void Shake(float shakeForce)
    {
        DecayingShakingForce = math.max(DecayingShakingForce, shakeForce);
    }

    private void Awake()
    {
        _currentShakeDirection = VectorMath.PickRandomDirection();
    }

    private void Update()
    {
        Vector2 currentMove = _currentShakeDirection * math.sin((Time.time * math.PI * ShakingAmplitude) + math.PIHALF) * (ContantShakingForce + DecayingShakingForce);
        _currentOffset += currentMove;
        transform.position = transform.position + VectorMath.Vec2ToVec3(currentMove);

        if (math.sin(Time.time * math.PI) > 0f ^ math.sin((Time.time - Time.deltaTime) * math.PI) > 0f)
        {
            _currentShakeDirection = VectorMath.RandomizeVec2(_currentShakeDirection, RANDOMIZE_DIRECTION_ACCURACY);
            transform.position -= VectorMath.Vec2ToVec3(_currentOffset);
            _currentOffset = Vector2.zero;
        }

        DecayingShakingForce = math.lerp(DecayingShakingForce, 0f, ShakingDecayingMultiplier * Time.deltaTime);
    }
}
