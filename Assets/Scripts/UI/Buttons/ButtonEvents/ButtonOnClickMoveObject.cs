using Unity.Mathematics;
using Unity.Mathematics.Geometry;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

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
