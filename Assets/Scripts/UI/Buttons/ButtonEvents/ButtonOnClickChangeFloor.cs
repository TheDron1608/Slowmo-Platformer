using UnityEngine;

public class ButtonOnClickChangeFloor : MonoBehaviour
{
    [SerializeField]
    private int _floor;

    [SerializeField]
    private CityAnimatorParameters _cityAnimator;


    //called when clicked
    public void ChangeFloor()
    {
        _cityAnimator.CurrentLevel = _floor;
    }
}
