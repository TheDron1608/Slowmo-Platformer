using Unity.Mathematics;
using UnityEngine;

public class AdrenalineSlowmo : AbstractCharacterEffect, IEntireCharacterEffect
{
    public float MaxSlowmo = 0.5f;

    private static float _oldSlowmo = 1f;
    private static float _currentSlowmo = 1f;

    private void Update()
    {
        _currentSlowmo = math.min(_currentSlowmo, math.lerp(
            MaxSlowmo, 1f,
            AffectedCharacter.CharacterHealth.MaxHealth > 0 ?
                math.min(AffectedCharacter.CharacterHealth.CurrentHealth, AffectedCharacter.CharacterHealth.MaxHealth) / AffectedCharacter.CharacterHealth.MaxHealth : 0f
            ));
    }

    private void LateUpdate()
    {
        TimeManager.Instance.CurrentTimeScale = TimeManager.Instance.CurrentTimeScale / _oldSlowmo * _currentSlowmo;
        _oldSlowmo = _currentSlowmo;
        _currentSlowmo = 1f;
    }

    protected override void OnRemove()
    {
        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.CurrentTimeScale /= _currentSlowmo;
        }
        _currentSlowmo = 1f;
    }
}
