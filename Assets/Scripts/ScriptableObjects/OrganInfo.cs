
using UnityEngine;
using UnityEngine.Localization;

[CreateAssetMenu(fileName = "OrganInfo", menuName = "OrganInfo")]
public class OrganInfo : ScriptableObject
{
    public LocalizedString LocalizedName;
    public Color TextColor = Color.greenYellow;
    public int Price;
    public float DropChance;
    public Sound DropSound;
}