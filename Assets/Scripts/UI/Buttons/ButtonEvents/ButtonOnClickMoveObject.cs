using UnityEngine;

public class ButtonOnClickMoveObject : MonoBehaviour
{
    public MoveBetweenTwoCoors MovingObject;
    public GameObject TargetObject;
    public bool IgnoreDeleteSavesMode = false;

    public void MoveObject()
    {
        if (ButtonOnClickToggleDeleteSaves.DeleteSaves && !IgnoreDeleteSavesMode) return;

        MovingObject.StartMoving(TargetObject);
    }
}
