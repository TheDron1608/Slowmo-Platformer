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

    public void MoveObject()
    {
        MovingObject.StartMoving(TargetObject);
    }
}
