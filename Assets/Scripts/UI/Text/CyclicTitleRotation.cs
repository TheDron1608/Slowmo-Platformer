using Unity.Mathematics;
using UnityEngine;

public class CyclicTitleRotation : MonoBehaviour
{
    [SerializeField]
    private float _rotationAngle = 2f;

    [SerializeField]
    private float _rotationSpeed = 0.15f;

    private void Update()
    {
        float rotation = math.sin(Time.timeSinceLevelLoad * math.PI * _rotationSpeed) * _rotationAngle;

        Vector3 eulerAgnleTransform = transform.localEulerAngles;

        transform.localEulerAngles = new Vector3(eulerAgnleTransform.x, eulerAgnleTransform.y, rotation);
    }
}
