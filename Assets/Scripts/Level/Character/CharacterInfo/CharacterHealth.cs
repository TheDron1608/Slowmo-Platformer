using UnityEngine;

public class CharacterHealth : DamagableObject
{
    private CharacterComponentsManager _charComponents;
    private CharacterPart _lethallyAffectedCharacterPart = null;

    public CharacterComponentsManager CharComponents
    {
        get => _charComponents;
        private set => _charComponents = value;
    }

    protected override void OnAwake()
    {
        base.OnAwake();
        GameObject curGameObject = gameObject;
        do
        {
            if (curGameObject.TryGetComponent(out CharacterComponentsManager charComponents))
            {
                CharComponents = charComponents;
                return;
            }
            curGameObject = curGameObject.transform.parent.gameObject;
        }
        while (curGameObject.tag == LayerManager.CHARACTER_TAG_NAME);
        throw new UnityException("not found CharacterComponentsManager component in " + gameObject.name + " or in the same tagged child gameObjects");
    }


    public void ApplyDamage(float damage, MonoBehaviour damager, CharacterPart damagedPart)
    {
        CurrentHealth -= damage;
        if (damage > 0 && CurrentHealth <= MinHealth && !CharComponents.CharacterEffectsReceiver.GetHasEffect<ILethalEffect>())
        {
            Die(damager, damagedPart);
        }
        else if (damage < 0 && CurrentHealth > MinHealth && CharComponents.CharacterEffectsReceiver.GetHasEffect(EffectsOnLethal))
        {
            Ressurect();
        }
    }

    public override void Die(MonoBehaviour killer)
    {
        if (_lethallyAffectedCharacterPart == null)
        {
            base.Die(killer);
        }
    }

    public void Die(MonoBehaviour killer, CharacterPart lethallyDamagedPart)
    {
        if (lethallyDamagedPart == null)
        {
            Die(killer);
        }
        else if (!lethallyDamagedPart.CharPartEffectsReceiver.GetHasEffect(EffectsOnLethal))
        {
            CharComponents.CharacterEffectsReceiver.ApplyEffect(EffectsOnLethal, killer, lethallyDamagedPart);
            _lethallyAffectedCharacterPart = lethallyDamagedPart;
        }
    }

    public override void Ressurect()
    {
        base.Ressurect();

        if (_lethallyAffectedCharacterPart != null)
        {
            _lethallyAffectedCharacterPart.CharPartEffectsReceiver.RemoveEffect(EffectsOnLethal);
            _lethallyAffectedCharacterPart.CharPartEffectsReceiver.RemoveEffect<ILethalEffect>();
            _lethallyAffectedCharacterPart = null;
        }
    }
}
