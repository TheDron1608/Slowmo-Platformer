using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class LogoCharacter : MonoBehaviour
{
    public List<Sprite> RandomRegularSprites = new();
    public List<Sprite> RandomDistortedSprites = new();
    public List<Material> RandomMaterials = new();
    public float DistoreChance = 0.25f;
    public float StartOffset = 0f;
    public float ChangeSpriteDuration = 0.333f;
    public float MoveDuration = 3f;
    public float MoveDistance = 250f;

    private Image _img;
    private float _timeSinceLastSpriteUpdate = 0f;
    private float _time = 0f;

    private void Awake()
    {
        if (!TryGetComponent(out _img)) throw new UnityException("Image component not found");

        _timeSinceLastSpriteUpdate = UnityEngine.Random.value * ChangeSpriteDuration;
        _time = StartOffset;

        _img.sprite = NumberMath.PickRandomItem(RandomRegularSprites);
        //_img.material = NumberMath.PickRandomItem(RandomMaterials);
    }

    private void Update()
    {
        _timeSinceLastSpriteUpdate += Time.deltaTime;
        _time += Time.deltaTime;

        if (_timeSinceLastSpriteUpdate >= ChangeSpriteDuration)
        {
            _img.sprite = NumberMath.PickRandomItem(UnityEngine.Random.value > DistoreChance ? RandomRegularSprites : RandomDistortedSprites, _img.sprite);
            _timeSinceLastSpriteUpdate -= ChangeSpriteDuration;
        }

        transform.position = new Vector3(
            transform.position.x,
            transform.position.y + (GetDeltaPosition(_time) - GetDeltaPosition(_time - Time.deltaTime)) * MoveDistance,
            transform.position.z
            );
    }

    private float GetDeltaPosition(float time)
    {
        return math.sin(time / MoveDuration * math.PI2 - math.PIHALF);
    }
}