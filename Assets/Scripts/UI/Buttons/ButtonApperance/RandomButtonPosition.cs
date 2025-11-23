using UnityEngine;

public class RandomButtonPosition : MonoBehaviour
{
    [SerializeField]
    private Vector2 _randomPosRange = Vector2.zero;


    private void Awake()
    {
        transform.localPosition = new Vector3(
            Random.Range(-_randomPosRange.x, _randomPosRange.x),
            Random.Range(-_randomPosRange.y, _randomPosRange.y),
            transform.localPosition.z
            );
    }
}
