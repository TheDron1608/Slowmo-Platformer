using UnityEngine;

public class LayerOverlayTrackCameraTiled : MonoBehaviour
{
    private SpriteRenderer _spriteRendererComponent;

    private void Awake()
    {
        _spriteRendererComponent = GetComponent<SpriteRenderer>() ?? throw new UnityException("SpriteRenderer component not found");
    }

    private void Update()
    {
        Vector2 spriteSize = _spriteRendererComponent.sprite.bounds.size;
        Vector2 cameraPosition = Camera.main.transform.position;
        transform.position = VectorMath.Vec2ToVec3(cameraPosition - new Vector2(cameraPosition.x % spriteSize.x, cameraPosition.y % spriteSize.y), transform.position.z);
    }
}
