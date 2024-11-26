using UnityEngine;

public class CityAnimatorParameters : MonoBehaviour
{
    private const string ANIMATOR_CURRENT_LEVEL_PARAM_NAME = "CurrentLevel";

    [SerializeField]
    private Animator _animator;

    public int CurrentLevel
    {
        get
        {
            return _animator.GetInteger(ANIMATOR_CURRENT_LEVEL_PARAM_NAME);
        }
        set
        {
            _animator.SetInteger(ANIMATOR_CURRENT_LEVEL_PARAM_NAME, value);
        }
    }
}
