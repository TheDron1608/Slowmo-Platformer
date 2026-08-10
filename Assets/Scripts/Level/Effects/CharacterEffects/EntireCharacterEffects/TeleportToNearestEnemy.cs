
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TeleportToNearestEnemy : AbstractCharacterEffect, IEntireCharacterEffect
{
    const float TEMP_CAMERA_SPEED_MULT_PER_DISTANCE = 0.25f;

    public float Delay = 5f;
    public List<AbstractEffect> EffectsOnTeleportedToCharacter = new();
    public List<AbstractEffect> EffectsOnNoValidTeleportToCharacters = new();

    protected override void OnApply()
    {
        base.OnApply();

        float nearestCharacterDistance = float.MaxValue;
        AbstractCharacterComponent nearestCharacter = null;
        foreach (Transform characterT in AffectedCharacter.CharacterCollision.CurrentZLayer.CharactersContainer)
        {
            float distance = Vector2.Distance(AffectedCharacter.transform.position, characterT.position);
            if (
                !characterT.IsDestroyed() &&
                distance < nearestCharacterDistance &&
                characterT.TryGetComponent(out AbstractCharacterComponent character) &&
                character.CharComponents != AffectedCharacter &&
                !AffectedCharacter.CharacterTeam.GetIsAllyToAnotherTeam(character.CharComponents.CharacterTeam)
                )
            {
                nearestCharacterDistance = distance;
                nearestCharacter = character;
            }
        }

        if (nearestCharacter != null)
        {
            AffectedCharacter.transform.position = nearestCharacter.transform.position;
            nearestCharacter.CharComponents.CharacterEffectsReceiver.ApplyEffect(EffectsOnTeleportedToCharacter, AffectedCharacter);

            if (AffectedCharacter.TryGetComponent(out CharacterSlowmoActions slowmo))
            {
                TimeManager.Instance.TryTemporalSlowTime(slowmo.SlowmoOnTeleGib);
            }
            if (Camera.main.TryGetComponent(out CameraTrack cameraTrack) && cameraTrack.TrackTargets.Contains(AffectedCharacter.transform))
            {
                cameraTrack.SpeedUpTrackSpeedTemporaly(nearestCharacterDistance * TEMP_CAMERA_SPEED_MULT_PER_DISTANCE);
            }
        }
        else
        {
            AffectedCharacter.CharacterEffectsReceiver.ApplyEffect(EffectsOnNoValidTeleportToCharacters, null);
        }

        RemoveSelf();
    }

    protected override void OnRemove()
    {
        base.OnRemove();

        if (TryGetComponent(out DisableObjectOnDistanceFromCamera disabler))
        {
            disabler.ForceDisable = false;
        }
        else
        {
            AffectedCharacter.gameObject.SetActive(true);
        }
    }

    public override bool ApplyCondition(ObjectEffectsReceiver affectWho, MonoBehaviour sender)
    {
        return base.ApplyCondition(affectWho, sender) && !affectWho.GetHasEffect<TeleportToNearestEnemy>();
    }

    public override bool Equals(AbstractEffect other)
    {
        return
            base.Equals(other) &&
            Delay == (other as TeleportToNearestEnemy).Delay &&
            EffectsOnTeleportedToCharacter == (other as TeleportToNearestEnemy).EffectsOnTeleportedToCharacter;
    }
}