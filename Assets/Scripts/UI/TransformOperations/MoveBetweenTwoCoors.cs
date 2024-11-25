using System;
using Unity.Mathematics;
using UnityEngine;

public class MoveBetweenTwoCoors : MonoBehaviour
{
    public float MoveSpeed = 10f;

    private GameObject _currentMoveTarget;



    public void StartMoving(GameObject target)
    {
        _currentMoveTarget = target;
    }

    private void Update()
    {
        if (_currentMoveTarget != null)
        {
            MoveTo(_currentMoveTarget.transform.position);
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
            _currentMoveTarget = null;
        }
    }
}
