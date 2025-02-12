using UnityEngine;

public abstract class AbstractCharacterComponent : MonoBehaviour
{
    private CharacterComponentsManager _charComponents;

    public CharacterComponentsManager CharComponents
    {
        get => _charComponents;
        private set => _charComponents = value;
    }

    private void Awake()
    {
        OnAwake();
    }

    protected virtual void OnAwake()
    {
        GameObject curGameObject = gameObject;
        do
        {
            if (curGameObject.TryGetComponent(out CharacterComponentsManager charComponents))
            {
                CharComponents = charComponents;
                return;
            }
            curGameObject = curGameObject.transform.parent.gameObject;
        }
        while (curGameObject.tag == LayerManager.CHARACTER_TAG_NAME);
        throw new UnityException("not found CharacterComponentsManager component in " + gameObject.name + " or in the same tagged child gameObjects");
    }
}
