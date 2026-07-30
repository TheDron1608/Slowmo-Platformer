using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Holdable))]
public class LaserHoldable : MonoBehaviour
{
    const float LASER_SPRITE_UNIT_SIZE = 0.0625f;
    const float LASER_UPDATE_FRAMERATE = 10;
    const float MAX_LASER_DISTANCE = 30f;

    [SerializeField] private Transform _laserContainer;
    [SerializeField] private SpriteRenderer _laserSprite;
    [SerializeField] private bool _laserEnabled = false;

    private Holdable _holdableComponent;
    private Coroutine _laserDistanceUpdateCoroutine = null;

    public bool LaserEnabled
    {
        get => _laserEnabled;
        set
        {
            if (_laserEnabled == value) return;

            _laserEnabled = value;
            UpdateLaserEnabled();
        }
    }

    public Material LaserMaterial
    {
        get => _laserSprite.sharedMaterial;
        set => _laserSprite.sharedMaterial = value;
    }

    private void Awake()
    {
        if (!TryGetComponent(out _holdableComponent)) throw new UnityException("not found Holdable component at " + gameObject.name);
    }

    private void FixedUpdate()
    {
        LaserEnabled =
            ((!_holdableComponent.CurrentHolder?.CharComponents.CharacterAiming.AimWeaponDown) ?? false) &&
            !_holdableComponent.IsHolstered &&
            (
                !TryGetComponent(out RangedWeapon rangedWeapon) ||
                (!rangedWeapon.IsReloading && !rangedWeapon.IsUnloading)
            );
    }

    private void UpdateLaserEnabled()
    {
        if (_laserEnabled && _laserDistanceUpdateCoroutine == null)
        {
            _laserSprite.enabled = true;
            _laserDistanceUpdateCoroutine = StartCoroutine(LaserDistanceUpdate());
        }
        else if (!_laserEnabled && _laserDistanceUpdateCoroutine != null)
        {
            _laserSprite.enabled = false;
            StopCoroutine(_laserDistanceUpdateCoroutine);
            _laserDistanceUpdateCoroutine = null;
        }
    }

    private IEnumerator LaserDistanceUpdate()
    {
        while (true)
        {
            ZIndexLayer currentLayer = LayerManager.Instance.GetZLayerOfGameObject(gameObject);
            RaycastHit2D hit = Physics2D.Raycast(
                _laserContainer.transform.position,
                VectorMath.Quartenion2DToVec2(_laserContainer.transform.rotation),
                MAX_LASER_DISTANCE,
                (1 << currentLayer.CharactersLayer) | (1 << currentLayer.EnviromentLayer)
                );

            float targetDistance = hit.collider != null ? hit.distance : MAX_LASER_DISTANCE;

            _laserSprite.transform.localScale = new Vector3(
                targetDistance / LASER_SPRITE_UNIT_SIZE,
                _laserSprite.transform.localScale.y,
                _laserSprite.transform.localScale.z
                );

            _laserSprite.transform.localPosition = new Vector3(
                targetDistance / 2f,
                _laserSprite.transform.localPosition.y,
                _laserSprite.transform.localPosition.z
                );

            yield return new WaitForSeconds(1f / LASER_UPDATE_FRAMERATE);
        }
    }
}
