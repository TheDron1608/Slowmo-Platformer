using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CityAnimatorParameters : MonoBehaviour
{
    private const string ANIMATOR_CURRENT_LEVEL_PARAM_NAME = "CurrentLevel";
    private const string ANIMATOR_BREAK_INTRO_TRIGGER_NAME = "BreakIntro";

    [SerializeField]
    private Animator _animator;
    [SerializeField]
    private List<InputActionReference> _skipInputs = new();

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

    public void BreakIntro()
    {
        _animator.SetTrigger(ANIMATOR_BREAK_INTRO_TRIGGER_NAME);
    }

    private void Awake()
    {
        foreach (var skipInput in _skipInputs)
        {
            skipInput.action.performed += SkipAction_performed;
        }
    }

    private void SkipAction_performed(InputAction.CallbackContext obj)
    {
        BreakIntro();
    }

    private void OnDestroy()
    {
        foreach (var skipInput in _skipInputs)
        {
            skipInput.action.performed -= SkipAction_performed;
        }
    }
}
