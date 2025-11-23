using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class HoldObjectInfo : MonoBehaviour
{
    const float MELEE_DURABILITY_BAR_SPEED_MULTIPLIER = 15f;

    public CharacterHoldingObjects TrackedHolder = null;

    private Holdable _currentHoldObject = null;
    private string _unarmedText = "UNARMED";

    [Header("const references")]
    [SerializeField] private GameObject _holdObjectImageContainer;
    [SerializeField] private Image _holdObjectImage;
    [SerializeField] private TextMeshProUGUI _holdObjectName;
    [SerializeField] private HoldObjectAmmoList _loadedBulletsList;
    [SerializeField] private HoldObjectAmmoList _magsList;
    [SerializeField] private GameObject _meleeDurabilityContainer;
    [SerializeField] private Image _meleeDurabilityProgress;

    private void FixedUpdate()
    {
        if (TrackedHolder == null) return;

        if (_currentHoldObject != TrackedHolder.CurrentHoldObject)
        {
            _currentHoldObject = TrackedHolder.CurrentHoldObject;
            _loadedBulletsList.RemoveAllAmmo();
            _magsList.RemoveAllAmmo();
        }

        if (_currentHoldObject != null)
        {
            //update holdable icon
            _holdObjectImageContainer.gameObject.SetActive(true);
            SpriteRenderer holdObjectSpriteRenderer = _currentHoldObject.GetComponent<SpriteRenderer>();
            _holdObjectImage.sprite = holdObjectSpriteRenderer.sprite;
            _holdObjectImage.SetNativeSize();

            //update holdable name
            _holdObjectName.text = _currentHoldObject.GetLocalizedName();

            //update ranged weapon loaded ammo info
            if (_currentHoldObject.TryGetComponent(out RangedWeapon rangedWeapon))
            {
                _loadedBulletsList.AmmoSprite = rangedWeapon.Projectile.GameplayUISprite;
                _loadedBulletsList.AmmoAmount = rangedWeapon.LoadedLivingAmmoLeft;
            }
            else
            {
                _loadedBulletsList.RemoveAllAmmo();
            }

            //update ranged weapon unloaded ammo info
            if (_currentHoldObject.TryGetComponent(out MagReloadingWeapon magReloadingWeapon))
            {
                _magsList.AmmoSprite = magReloadingWeapon.GameplayUIMagSprite;
                _magsList.AmmoAmount = magReloadingWeapon.Mags;
            }
            else if (_currentHoldObject.TryGetComponent(out BulletReloadingWeapon bulletReloadingWeapon))
            {
                _magsList.AmmoSprite = bulletReloadingWeapon.Projectile.GameplayUISprite;
                _magsList.AmmoAmount = bulletReloadingWeapon.AmmoLeft;
            }
            else
            {
                _magsList.RemoveAllAmmo();
            }


            //update melee durability info
            if (_currentHoldObject.GetComponent<MeleeWeapon>() != null)
            {
                if (_currentHoldObject.TryGetComponent(out Chainsaw chainsaw))
                {
                    _meleeDurabilityContainer.SetActive(true);
                    _meleeDurabilityProgress.fillAmount = chainsaw.FuelLeft / chainsaw.MaxFuel;
                }
                else if (_currentHoldObject.TryGetComponent(out BreakableHoldable breakableHoldable))
                {
                    _meleeDurabilityContainer.SetActive(true);
                    _meleeDurabilityProgress.fillAmount = math.lerp(
                        _meleeDurabilityProgress.fillAmount,
                        breakableHoldable.UnlimitedUses ? 1f : (float)breakableHoldable.UsesLeft / breakableHoldable.MaxUses,
                        Time.fixedDeltaTime * MELEE_DURABILITY_BAR_SPEED_MULTIPLIER
                        );
                }
                else
                {
                    _meleeDurabilityContainer.SetActive(false);
                }
            }
            else
            {
                _meleeDurabilityContainer.SetActive(false);
            }
        }
        else
        {
            //hide hold object info
            _holdObjectImageContainer.gameObject.SetActive(false);
            _holdObjectName.text = _unarmedText;
        }
    }
}