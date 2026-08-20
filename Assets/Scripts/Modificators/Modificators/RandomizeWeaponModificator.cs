
using System.Collections.Generic;
using UnityEngine;

public class RandomizeWeaponModificator : AbstractModificator
{
    public float DelayBetweenRandomization = 5f;
    public List<TeamManager.Teams> AffectTeams = new();

    private float _timeSinceLastrandomization = 0f;

    private void FixedUpdate()
    {
        if (DisabledModificator) return;

        _timeSinceLastrandomization += Time.deltaTime;

        if (_timeSinceLastrandomization > DelayBetweenRandomization / ModificatorMultiplier)
        {
            List<Holdable> avaibleRangedHoldables = new();
            List<Holdable> avaibleMeleeHoldables = new();
            foreach (var enemyInfo in SpawnManager.Instance.EnemyPool)
            {
                foreach (Holdable possibleWeapon in enemyInfo.Weapon.PossibleWeapon)
                {
                    if (possibleWeapon.TryGetComponent(out RangedWeapon rw))
                    {
                        avaibleRangedHoldables.Add(possibleWeapon);
                    }
                    else if (possibleWeapon.TryGetComponent(out MeleeWeapon mw))
                    {
                        avaibleMeleeHoldables.Add(possibleWeapon);
                    }
                }
            }
            foreach (var lootInfo in SpawnManager.Instance.LootDrops)
            {
                foreach (GameObject possibleLoot in lootInfo.PossibleLoot)
                {
                    if (possibleLoot.TryGetComponent(out Holdable holdaleLoot))
                    {
                        if (possibleLoot.TryGetComponent(out RangedWeapon rw))
                        {
                            avaibleRangedHoldables.Add(holdaleLoot);
                        }
                        else if (possibleLoot.TryGetComponent(out MeleeWeapon mw))
                        {
                            avaibleMeleeHoldables.Add(holdaleLoot);
                        }
                    }
                }
            }

            List<Holdable> filteredHoldables = new();
            if (LayerManager.Instance != null && Camera.main.TryGetComponent(out MultiZLayerCamera zLayerCamera))
            {
                foreach (Transform holdableT in zLayerCamera.CurrentZLayer.HoldablesContainer)
                {
                    if (
                        holdableT.gameObject.activeSelf && 
                        holdableT.TryGetComponent(out Holdable holdable) &&
                        holdable.CurrentHolder != null &&
                        AffectTeams.Contains(holdable.CurrentHolder.CharComponents.CharacterTeam.Team)
                        )
                    {
                        filteredHoldables.Add(holdable);
                    }
                }
            }

            foreach (Holdable filteredHoldable in filteredHoldables)
            {
                if (filteredHoldable.TryGetComponent(out RangedWeapon rw) && avaibleRangedHoldables.Count > 1)
                {
                    filteredHoldable.TransformToAnotherObject(NumberMath.PickRandomItem(avaibleRangedHoldables, filteredHoldable.OriginalPrefab.GetComponent<Holdable>()));
                }
                else if (filteredHoldable.TryGetComponent(out MeleeWeapon mw) && avaibleMeleeHoldables.Count > 1)
                {
                    filteredHoldable.TransformToAnotherObject(NumberMath.PickRandomItem(avaibleMeleeHoldables, filteredHoldable.OriginalPrefab.GetComponent<Holdable>()));
                }
            }

            _timeSinceLastrandomization = 0f;
        }
    }
}