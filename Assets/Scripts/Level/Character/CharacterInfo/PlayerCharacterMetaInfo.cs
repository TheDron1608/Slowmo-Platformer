using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

[CreateAssetMenu(fileName = "PlayreCharacterInfo", menuName = "PlayreCharacterInfo")]
public class PlayerCharacterInfo : ScriptableObject
{
    public CharacterComponentsManager PlayerCharacter;
    public List<AbstractModificator> StartModificators;
    public LocalizedString LocalizedName;
    public LocalizedString LocalizedDesc;
    public LocalizedString LocalizedUnlockCondition;
    public GameObject CustomUnlockConditionInfo = null;
    public Sprite CharacterIconSprite;
    public Color InfoTextColor;
    public Material InfoIconMaterial;
    public Material InfoBgMaterial;

    public string GetUnlockCharacterJSONName()
    {
        return PlayerCharacter.gameObject.name;
    }
}
