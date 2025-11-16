using UnityEngine;

public class ButtonOnClickResetSelectedCharacter : MonoBehaviour
{
    [SerializeField]
    private CityAnimatorParameters _cityAnimator;
    [SerializeField]
    private CharacterSelectContainer _characterSelectContainer;

    public void ResetFloor()
    {
        _cityAnimator.CurrentLevel = 0;
        _characterSelectContainer.SetCharacterInfoVisibility(false);
    }
}
