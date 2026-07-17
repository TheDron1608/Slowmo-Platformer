
using UnityEngine;

public class ExtraHealthForNearCharacters : AbstractDamagableObjectEffect, IMultiplierableEffect
{
    public float HealthConversion = 0.05f;

    private float _effectMultiplier = 1f;
    private float _addedHealthThisFrame = 0f;

    public float EffectMultiplier
    {
        get => _effectMultiplier;
        set => _effectMultiplier = value;
    }

    private void FixedUpdate()
    {
        ZIndexLayer currentLayer = LayerManager.Instance.GetZLayerOfGameObject(AffectedObject.gameObject);
        float newAddedHealth = 0f;
        foreach (Transform characterTransform in currentLayer.CharactersContainer)
        {
            if (
                characterTransform.gameObject.activeSelf &&
                characterTransform.gameObject != AffectedObject.gameObject &&
                characterTransform.TryGetComponent(out AbstractCharacterComponent character) &&
                !character.CharComponents.CharacterEffectsReceiver.GetHasEffect<ILethalEffect>()
                )
            {
                newAddedHealth += Mathf.Max(character.CharComponents.CharacterHealth.CurrentHealth, 0f) * HealthConversion;
            }
        }

        AffectedDamagableObject.ApplyMaxHealth(AffectedDamagableObject.MaxHealth - _addedHealthThisFrame + newAddedHealth, null);
        _addedHealthThisFrame = newAddedHealth;
    }

    public override bool Equals(AbstractEffect other)
    {
        return base.Equals(other) && HealthConversion == (other as ExtraHealthForNearCharacters).HealthConversion;
    }
}
