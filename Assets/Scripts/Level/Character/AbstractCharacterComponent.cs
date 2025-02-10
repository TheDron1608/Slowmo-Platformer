using UnityEngine;

public abstract class AbstractCharacterComponent : MonoBehaviour
{
    protected CharacterComponentsManager _charComponents;

    private void Awake()
    {
        OnAwake();
    }

    protected virtual void OnAwake()
    {
        _charComponents = GetComponent<CharacterComponentsManager>();
    }
}
