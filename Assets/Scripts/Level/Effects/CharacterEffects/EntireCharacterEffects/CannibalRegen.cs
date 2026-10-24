
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class CannibalRegen : AbstractCharacterEffect, IEntireCharacterEffect
{
    public float HealthPerBite = 0.5f;
    public float DamagePerBite = 1f;
    public float BitesDelaySeconds = 0.5f;
    public float ConsumeRadius = 1f;
    public float AttractRadius = 3f;
    public float MaxBitesPerCharacterScale = 0.35f;
    public float CameraShakeOnBite = 0.1f;
    public float CorpseAttractForce = 5f;
    public List<OrganInfo> Organs = new();
    public SoundPlayer BiteSound;
    public SoundPlayer CollectOrganSound;

    private float _timeSinceLastBite = 0f;
    private Healthbar _currentHealthBar = null;

    protected override void OnApply()
    {
        base.OnApply();

        _currentHealthBar = GameplayUIManager.GetInstance()?.MultiHealthbarsManager.FindHealthbarByCharacter(AffectedCharacter);
        if (_currentHealthBar != null)
        {
            _currentHealthBar.ShowHealthLine = false;
            _currentHealthBar.SpriteItemPerHealth = HealthPerBite;

            while (_currentHealthBar.Spriteitems.Count > 0)
            {
                _currentHealthBar.RemoveSpriteItem();
            }

            List<CharacterLimbPart> limbParts = AffectedCharacter.CharacterPartsManager.GetAllLimbs();
            for (float i = 0f; i < AffectedCharacter.CharacterHealth.CurrentHealth; i += HealthPerBite)
            {
                AddBittenLimbSpriteToHealthbar(NumberMath.PickRandomItem(limbParts));
            }
        }

        AffectedCharacter.CharacterHealth.HealMultiplier = 0f;
    }

    private void FixedUpdate()
    {
        if (
            !AffectedCharacter.CharacterRolling.IsRolling &&
            !AffectedCharacter.CharacterEffectsReceiver.GetHasEffect<AbstractStun>()
            )
        {
            AbstractCharacterComponent nearestCorpse = null;
            float nearestCorpseDistance = AttractRadius;

            foreach (Transform characterT in LayerManager.Instance.GetZLayerOfGameObject(AffectedObject.gameObject).CharactersContainer)
            {
                if (characterT.IsDestroyed() || !characterT.gameObject.activeSelf) continue;
                float distanceToCharacter = Vector2.Distance(characterT.position, AffectedCharacter.transform.position);
                if (
                    !characterT.IsDestroyed() &&
                    characterT.gameObject.activeSelf &&
                    distanceToCharacter < nearestCorpseDistance &&
                    characterT.TryGetComponent(out AbstractCharacterComponent character) &&
                    character.CharComponents != AffectedCharacter &&
                    character.CharComponents.CharacterEffectsReceiver.GetHasEffect<AbstractStun>()
                    )
                {
                    nearestCorpse = character;
                    nearestCorpseDistance = distanceToCharacter;
                }
            }

            if (nearestCorpse != null)
            {
                _timeSinceLastBite += Time.fixedDeltaTime;

                nearestCorpse.CharComponents.CharacterRigidBody.linearVelocity += 
                    VectorMath.Vec3ToVec2((AffectedCharacter.transform.position - nearestCorpse.transform.position).normalized * CorpseAttractForce * Time.fixedDeltaTime);

                if (_timeSinceLastBite > BitesDelaySeconds && nearestCorpseDistance <= ConsumeRadius)
                {
                    CharacterLimbPart bittenLimb = NumberMath.PickRandomItem(nearestCorpse.CharComponents.CharacterPartsManager.GetAllLimbs());
                    if (bittenLimb != null)
                    {
                        foreach (CharacterPart bittenSubPart in nearestCorpse.CharComponents.CharacterPartsManager.CharacterParts)
                        {
                            if (bittenSubPart == bittenLimb || (bittenSubPart as CharacterEquipmentPart)?.EquipAtType == bittenLimb.PartType)
                            {
                                CharacterPart selfPart = AffectedCharacter.CharacterPartsManager.GetCharacterPart(bittenSubPart.PartType);
                                if (selfPart != null && selfPart.TryGetComponent(out DynamicMaterial selfDM) && bittenSubPart.TryGetComponent(out DynamicMaterial bittenDM))
                                {
                                    selfDM.DefaultMaterial = bittenDM.GetCurrentMaterial();
                                }
                            }
                        }

                        bittenLimb.CharPartHealth.SoundOnLethalDamage.PlaySound(false, bittenLimb.transform.position);
                        Camera.main.GetComponent<ShakableObject>()?.Shake(CameraShakeOnBite);

                        AddBittenLimbSpriteToHealthbar(bittenLimb);

                        BiteSound.PlaySound();

                        bittenLimb.CharPartHealth.ApplyDamage(DamagePerBite, AffectedCharacter);
                        bittenLimb.CharPartHealth.TryGib(AffectedCharacter);
                    }

                    nearestCorpse.CharComponents.CharacterHealth.Bites++;
                    if (nearestCorpse.CharComponents.transform.localScale.x < nearestCorpse.CharComponents.CharacterHealth.Bites * MaxBitesPerCharacterScale)
                    {
                        nearestCorpse.CharComponents.CharacterHealth.Gib(AffectedCharacter);
                    }
                    AffectedCharacter.CharacterHealth.ApplyDamage(-HealthPerBite, null, false);

                    if (ComboMoneyModificator.Instance != null)
                    {
                        OrganInfo randomOrgan = NumberMath.PickRandomItem(Organs);
                        if (RandomManager.Instance.ProcRandomGoodChance(randomOrgan?.DropChance ?? 0f))
                        {
                            AffectedCharacter.CharacterVisual.PopupText(
                                randomOrgan.LocalizedName.GetLocalizedString() + " +" + randomOrgan.Price.ToString() + "$",
                                true,
                                randomOrgan.TextColor
                                );
                            CollectOrganSound.PlaySound(randomOrgan.DropSound);
                            ComboMoneyModificator.Instance.CurrentMoney += randomOrgan.Price;
                        }
                    }

                    _timeSinceLastBite = 0f;
                }
            }
        }
    }

    private void AddBittenLimbSpriteToHealthbar(CharacterLimbPart characterLimb)
    {
        Sprite[] randomMultiSprites = NumberMath.PickRandomItem(characterLimb.CharComponents.CharacterVisual.MultiSpritesSO.SampleSprites)?.Value;
        if (_currentHealthBar != null && characterLimb.TryGetComponent(out SpriteRenderer bittenPartSR) && randomMultiSprites != null)
        {
            Sprite randomSprite = randomMultiSprites[(int)characterLimb.CharPartVisual.VisualType];

            GameObject newHBItem = new GameObject("BittenPart");
            newHBItem.transform.position = characterLimb.transform.position;

            Image newHBItemSR = newHBItem.AddComponent<Image>();
            newHBItemSR.sprite = randomSprite;
            newHBItemSR.material = bittenPartSR.sharedMaterial;

            _currentHealthBar.AddSpriteItem(newHBItemSR);
        }
    }

    protected override void OnRemove()
    {
        base.OnRemove();

        if (_currentHealthBar != null) _currentHealthBar.ShowHealthLine = true;

        AffectedCharacter.CharacterHealth.HealMultiplier = 1f;
    }
}