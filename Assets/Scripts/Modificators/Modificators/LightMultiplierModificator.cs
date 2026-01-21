
public class LightMultiplierModificator : AbstractModificator
{
    public float FurnitureLightMultiplier = 1f;
    public float CharacterLightMultiplier = 0f;
    public float GlobalLightMultiplier = 1f;
    public float WeaponLightMultiplier = 0f;

    public override void OnModificatorAdded()
    {
        base.OnModificatorAdded();

        LightManager.Instance.FurnitureLightIntensityMultiplier = FurnitureLightMultiplier;
        LightManager.Instance.CharacterLightIntensityMultiplier = CharacterLightMultiplier;
        LightManager.Instance.GlobalLightIntensityMultiplier = GlobalLightMultiplier;
        LightManager.Instance.WeaponLightIntensityMultiplier = WeaponLightMultiplier;
    }

    public override void OnModificatorRemoved()
    {
        base.OnModificatorRemoved();

        LightManager.Instance.FurnitureLightIntensityMultiplier = 1f;
        LightManager.Instance.CharacterLightIntensityMultiplier = 0f;
        LightManager.Instance.GlobalLightIntensityMultiplier = 1f;
        LightManager.Instance.WeaponLightIntensityMultiplier = 0f;
    }
}