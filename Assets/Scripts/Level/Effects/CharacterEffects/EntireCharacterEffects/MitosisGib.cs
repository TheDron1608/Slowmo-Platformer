using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[AllowEffectWithSenderReceiveNull]
public class MitosisGib : Gib, IEntireCharacterEffect, ILethalEffect
{
    const float MITOSIS_SPAWNED_CHARACTERS_VELOCITY = 12.5f;

    public int MitosisAmount = 2;
    public List<AbstractEffect> EffectsOnMitosisCharacters = new();

    protected override void OnReceivedSender(MonoBehaviour sender)
    {
        Vector2 mitosisSpawnPosition = AffectedCharacter.Center.transform.position;
        ZIndexLayer mitosisLayer = LayerManager.Instance.GetZLayerOfGameObject(AffectedObject.gameObject);

        for (int i = 0; i < MitosisAmount; i++)
        {
            EnemySpawnInfo spawnInfo = SpawnManager.Instance.PickRandomEnemy();
            AbstractCharacterComponent mitosisCharacter = mitosisLayer.TrySpawnObject(
                spawnInfo.Enemy.gameObject,
                mitosisSpawnPosition,
                null,
                null
                ).FirstOrDefault()?.GetComponent<AbstractCharacterComponent>();

            if (mitosisCharacter != null)
            {
                //give weapon
                mitosisCharacter.CharComponents.CharacterHolding.GiveNewHoldable(spawnInfo.Weapon?.PickRandomWeapon());

                //give equipment
                foreach (CharacterEquipmentPart randomEquipment in spawnInfo.Equipment?.PickRandomEquipment() ?? new List<CharacterEquipmentPart>())
                {
                    mitosisCharacter.CharComponents.CharacterPartsManager.GiveNewEquipment(randomEquipment);
                }

                //give random knockback to top direction
                Vector2 randomVelocity = VectorMath.PickRandomDirection();
                randomVelocity.x = Mathf.Abs(randomVelocity.x);
                randomVelocity *= MITOSIS_SPAWNED_CHARACTERS_VELOCITY;
                mitosisCharacter.CharComponents.CharacterRigidBody.linearVelocity = randomVelocity;

                //give extra effects
                mitosisCharacter.CharComponents.CharacterEffectsReceiver.ApplyEffect(EffectsOnMitosisCharacters, null);
            }
        }

        base.OnReceivedSender(sender);
    }
}
