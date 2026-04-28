using Unity.VisualScripting;
using UnityEngine;

[DefaultExecutionOrder(10)]
public class CharacterUITrack : AbstractCharacterComponent
{
    public bool TrackHealth = true;
    public bool TrackHoldable = true;
    public bool TrackCamera = true;
    public bool TrackIfDead = true;
    public bool TrackIsDying = true;
    public UIManager.LiveTimeLeftTypes LiveTimeLeftType = UIManager.LiveTimeLeftTypes.DEFAULT;

    private bool _tracked = false;

    private void SetTracked(bool value)
    {
        if (_tracked == value) return;
        if (value)
        {
            GameplayUIManager.GetInstance()?.AddTrackedCharacter(this);
            if (TrackCamera) Camera.main?.GetComponent<CameraTrack>().TrackTargets.Add(CharComponents.transform);
        }
        else
        {
            GameplayUIManager.GetInstance()?.RemoveTrackedCharacter(this);
            Camera.main?.GetComponent<CameraTrack>().TrackTargets.Remove(CharComponents.transform);
        }
        _tracked = value;
    }

    public bool GetTrackedIsDead()
    {
        return CharComponents.CharacterEffectsReceiver.GetHasEffect<ILethalEffect>();
    }

    public bool GetTrackedIsDying()
    {
        return CharComponents.CharacterEffectsReceiver.GetHasEffect<ILethalEffect>(true) && !GetTrackedIsDead();
    }

    public bool GetTrackedIsDead(out ILethalEffect deathEffect)
    {
        return CharComponents.CharacterEffectsReceiver.TryGetEffect(out deathEffect);
    }

    public bool GetTrackedIsDying(out ILethalEffect deathEffect, out AbstractEffect deathEffectOwner)
    {
        return CharComponents.CharacterEffectsReceiver.TryGetEffect(out deathEffect, out deathEffectOwner, true) && !GetTrackedIsDead();
    }

    private void OnEnable()
    {
        SetTracked(TrackIfDead || !CharComponents.CharacterEffectsReceiver.GetHasEffect<ILethalEffect>());

        if (!TrackIfDead)
        {
            CharComponents.CharacterEffectsReceiver.OnEffectAdded += CharacterEffectsReceiver_OnEffectAdded;
            CharComponents.CharacterEffectsReceiver.OnEffectRemoved += CharacterEffectsReceiver_OnEffectRemoved;
        }
    }

    private void OnDisable()
    {
        if (ExcludeDisableConditions()) return;

        if (CharComponents?.CharacterEffectsReceiver != null)
        {
            CharComponents.CharacterEffectsReceiver.OnEffectAdded -= CharacterEffectsReceiver_OnEffectAdded;
            CharComponents.CharacterEffectsReceiver.OnEffectRemoved -= CharacterEffectsReceiver_OnEffectRemoved;
        }

        SetTracked(false);
    }

    private void CharacterEffectsReceiver_OnEffectAdded(object sender, ObjectEffectsReceiver.EffectAddedEventArgs e)
    {
        if (e.Effect is ILethalEffect)
        {
            SetTracked(false);
        }
    }

    private void CharacterEffectsReceiver_OnEffectRemoved(object sender, AbstractEffect e)
    {
        if (e is ILethalEffect)
        {
            SetTracked(true);
        }
    }

    private bool ExcludeDisableConditions()
    {
        return
            !(//exclude if destroyed
                (CharComponents?.IsDestroyed() ?? false) || 
                (CharComponents?.CharacterSpecial.IsDestroyed() ?? false)
            ) && 
            (CharComponents.CharacterSpecial?.GetComponent<CharacterBleedTeleportation>()?.IsTeleporting ?? false); //exclude if is teleporting
    }
}
