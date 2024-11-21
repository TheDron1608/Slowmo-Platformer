using System;
using Unity.Mathematics;
using UnityEngine;

public class MoveBetweenTwoCoors : MonoBehaviour
{
    public enum MoveMode
    {
        NO_MOVING,
        MOVE_TO_TARGET,
        MOVE_TO_START,
    }

    public float MoveSpeed = 10f;

    public GameObject TargetPositionGameObject;

    private Vector3 _targetPosition;
    private Vector3 _startPosition;

    private MoveMode _currentMoveUIMode = MoveMode.NO_MOVING;



    public void StartMoving(MoveMode mode)
    {
        _currentMoveUIMode = mode;
    }


    private void Awake()
    {
        _targetPosition = TargetPositionGameObject.transform.position;
        _startPosition = transform.position;
    }

    private void Update()
    {
        switch (_currentMoveUIMode)
        {
            case MoveMode.MOVE_TO_TARGET:        //moves to _endPositoin
                MoveTo(_targetPosition);
                break;

            case MoveMode.MOVE_TO_START:      //does same but to _startPosition
                MoveTo(_startPosition);
                break;
        }
    }

    private void MoveTo (Vector3 targetPos)
    {
        Vector3 newPos = new Vector3(
            Mathf.Lerp(transform.position.x, targetPos.x, Time.deltaTime * MoveSpeed),
            Mathf.Lerp(transform.position.y, targetPos.y, Time.deltaTime * MoveSpeed),
            Mathf.Lerp(transform.position.z, targetPos.z, Time.deltaTime * MoveSpeed)
            );

        transform.position = newPos;

        if (VectorMath.GetVectorsEqual(transform.position, targetPos, 0.05f))
        {
            _currentMoveUIMode = MoveMode.NO_MOVING;
        }
    }
}
