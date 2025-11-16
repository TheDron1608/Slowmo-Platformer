using UnityEngine;

public class ButtonOnClickSelectCharacter : MonoBehaviour
{
    public CharacterComponentsManager Character;

    public void SelectCharacter()
    {
        SpawnManager.Instance.PlayerCharacter = Character;
    }
}
