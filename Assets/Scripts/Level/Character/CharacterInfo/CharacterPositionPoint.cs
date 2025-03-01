using System.Collections;
using UnityEngine;

public class CharacterPositionPoint : AbstractCharacterComponent
{
    private Vector3 _positionPreviousFrame;

    public Vector3 PositionPreviousFrame
    {
        get => _positionPreviousFrame;
        private set => _positionPreviousFrame = value;
    }

    private void FixedUpdate()
    {
        StartCoroutine(SetPositionPrevFrameAfterUpdate(transform.position));
    }

    private IEnumerator SetPositionPrevFrameAfterUpdate(Vector3 value)
    {
        yield return new WaitForFixedUpdate();
        PositionPreviousFrame = value;
    }
}
