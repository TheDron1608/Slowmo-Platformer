using Unity.Mathematics;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    const float RANDOMIZE_DIRECTION_ACCURACY = 0.5f;

    public float ContantShakingForce = 0f;
    public float DecayingShakingForce = 0f;
    public float ShakingAmplitude = 10f;
    public float ShakingDecayingMultiplier = 5f;

    private Vector2 _currentShakeDirection;
    private float _currentOffset = 0f;

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
        if (_currentOffset > 0f ^ math.sin(Time.time * math.PI * ShakingAmplitude) > 0f) 
        { 
            _currentShakeDirection = -VectorMath.RotateVec2(_currentShakeDirection, UnityEngine.Random.value * RANDOMIZE_DIRECTION_ACCURACY);
        }

        _currentOffset = math.abs(math.sin(Time.time * math.PI * ShakingAmplitude));

        transform.position = 
            transform.position + VectorMath.Vec2ToVec3(_currentShakeDirection * _currentOffset * (ContantShakingForce + DecayingShakingForce));

        DecayingShakingForce = math.lerp(DecayingShakingForce, 0f, ShakingDecayingMultiplier * Time.deltaTime);
    }
}
