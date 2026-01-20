using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public abstract class AbstractHoldablesModificator : AbstractMultiplierableModificator
{
    public TeamManager.Teams Team = TeamManager.Teams.PLAYER;
    public float AffectChance = 1f;
    public RandomManager.ProcChanceTypes ChanceType;

    private List<Holdable> _affectedHoldables = new();

    protected override void OnObjectSpawned(object sender, GameObject e)
    {
        base.OnObjectSpawned(sender, e);

        if (
            e.TryGetComponent(out Holdable holdable) &&
            RandomManager.Instance.ProcRandomChance(AffectChance, ChanceType)
            )
        {
            _affectedHoldables.Add(holdable);
            holdable.OnGiven += AffectedHoldable_OnGiven;
            holdable.OnThrown += AffectedHoldable_OnThrown;
            OnHoldableAffected(holdable);
        }
    }

    private void AffectedHoldable_OnGiven(object sender, CharacterHoldingObjects e)
    {
        if (e.CharComponents.CharacterTeam.Team == Team)
        {
            OnAffectedHoldablePickedUp((Holdable)sender, e);
        }
    }
    private void AffectedHoldable_OnThrown(object sender, Holdable.OnThrownEventArgs e)
    {
        if (e.Thrower.CharComponents.CharacterTeam.Team == Team)
        {
            OnAffectedHoldableThrown((Holdable)sender, e.Thrower);
        }
    }

    public override void OnModificatorRemoved()
    {
        base.OnModificatorRemoved();

        foreach (Holdable holdable in _affectedHoldables)
        {
            if (holdable != null && !holdable.IsDestroyed())
            {
                holdable.OnGiven -= AffectedHoldable_OnGiven;
                holdable.OnThrown -= AffectedHoldable_OnThrown;
                OnHoldableRemovedAffect(holdable);
            }
        }

        _affectedHoldables = new();
    }

    protected abstract void OnHoldableAffected(Holdable holdable);

    protected abstract void OnHoldableRemovedAffect(Holdable holdable);

    protected abstract void OnAffectedHoldablePickedUp(Holdable holdable, CharacterHoldingObjects holder);

    protected abstract void OnAffectedHoldableThrown(Holdable holdable, CharacterHoldingObjects thrower);
}