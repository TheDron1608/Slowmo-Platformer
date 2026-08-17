
using Unity.Mathematics;
using UnityEngine;

public class OnInteractSit : Interactable, INavPointersScreenOverlayTrackableObject
{
    const float SIT_TRANSITION_SPEED_MULT = 5f;

    [SerializeField] private Transform _sitPosition;
    [SerializeField] private CharacterVisual.CharacterPartBusyStates _sitAnimation;
    [SerializeField] private float _offsetForPointerPosition;

    private AbstractCharacterComponent _sittingCharacter = null;

    public AbstractCharacterComponent GetCurrentSitter()
    {
        return _sittingCharacter;
    }

    protected override bool StartInteractCondition(GameObject interactor)
    {
        return base.StartInteractCondition(interactor) && _sittingCharacter == null && interactor.TryGetComponent(out AbstractCharacterComponent character);
    }

    protected override void OnStartInteact(GameObject interactor)
    {
        base.OnStartInteact(interactor);

        if (!interactor.TryGetComponent(out _sittingCharacter)) Debug.LogWarning("could not find Character in " + interactor.name);

        _sittingCharacter.CharComponents.CharacterRigidBody.simulated = false;
        _sittingCharacter.CharComponents.CharacterVisual.CurrentBusyAnimation = _sitAnimation;

        NavPointersScreenOverlay.Instance?.UpdateNavTargets();
    }

    public void RemoveSitter()
    {
        if (_sittingCharacter != null)
        {
            _sittingCharacter.CharComponents.CharacterRigidBody.simulated = true;
            if (_sittingCharacter.CharComponents.CharacterVisual.CurrentBusyAnimation == _sitAnimation)
            {
                _sittingCharacter.CharComponents.CharacterVisual.BreakBusyAnimation();
            }
            _sittingCharacter = null;
        }

        NavPointersScreenOverlay.Instance?.UpdateNavTargets();
    }

    private void FixedUpdate()
    {
        if (_sittingCharacter != null)
        {
            _sittingCharacter.transform.position = math.lerp(
                _sittingCharacter.transform.position,
                _sitPosition.position,
                Time.fixedDeltaTime * SIT_TRANSITION_SPEED_MULT
                );
        }
    }

    public bool PointingCondition()
    {
        return enabled && _sittingCharacter == null;
    }

    public float GetOffsetForPointerPosition()
    {
        return _offsetForPointerPosition;
    }

    private void OnDisable()
    {
        RemoveSitter();
    }
}