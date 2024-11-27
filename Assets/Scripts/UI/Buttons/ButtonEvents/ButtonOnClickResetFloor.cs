using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonOnClickResetFloor : MonoBehaviour
{
    [SerializeField]
    private CityAnimatorParameters _cityAnimator;

    [SerializeField]
    private GameObject _levelButtonsContainer;

    public void ResetFloor()
    {
        UpdateCityAnimator();
        UpdateLevelButtonsContainer();
    }

    private void UpdateCityAnimator()
    {
        _cityAnimator.CurrentLevel = 0;
    }

    private void UpdateLevelButtonsContainer()
    {
        foreach (Transform child in _levelButtonsContainer.transform)
        {
            Destroy(child.gameObject);
        }
    }
}
