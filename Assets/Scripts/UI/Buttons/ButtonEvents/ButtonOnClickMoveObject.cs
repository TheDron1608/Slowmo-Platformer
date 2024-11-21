using Unity.Mathematics;
using Unity.Mathematics.Geometry;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ButtonOnClickMoveObject : MonoBehaviour
{
    [SerializeField]
    private MoveBetweenTwoCoors _moveObject;
    [SerializeField]
    private MoveBetweenTwoCoors.MoveMode _moveMode;

    public void MoveObject()
    {
        _moveObject.StartMoving(_moveMode);
    }
}
