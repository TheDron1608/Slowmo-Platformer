using UnityEngine;

[RequireComponent(typeof(CharacterComponentsManager))]
public class CharacterDisableObjectOnDistanceFromCamera : DisableObjectOnDistanceFromCamera
{
    private CharacterComponentsManager _charComponents;

    private void Awake()
    {
        _charComponents = GetComponent<CharacterComponentsManager>();
    }

    public override bool DisableCondition()
    {
        return
            base.DisableCondition() &&
            _charComponents.CharacterCollision.IsCollidingFloor();
    }
}
