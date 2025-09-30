using UnityEngine;

public class LayerOverlayTrackCamera : MonoBehaviour
{
    private void Update()
    {
        transform.position = VectorMath.Vec2ToVec3(Camera.main.transform.position, transform.position.z);
    }
}
