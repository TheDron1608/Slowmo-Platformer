using System.Collections;
using UnityEngine;

public class CharacterPositionPoint : AbstractCharacterComponent
{
    private Vector3 _positionThisFrame;
    private Vector3 _positionPreviousFrame;

    public Vector3 PositionPreviousFrame
    {
        get => _positionPreviousFrame;
        private set => _positionPreviousFrame = value;
    }

    private void Awake()
    {
        _positionThisFrame = transform.position;
        _positionPreviousFrame = transform.position;
    }

    private void FixedUpdate()
    {
        _positionPreviousFrame = _positionThisFrame;
        _positionThisFrame = transform.position;
    }
}
