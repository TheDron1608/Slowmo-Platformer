using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class Healthbar : MonoBehaviour
{
    const float CHARACTER_HEALTH_AMOUNT_TO_REACH_MAX_HEALTHBAR_WIDTH = 10f;
    const float MIN_HEALTHBAR_WIDTH = 30f;
    const float MAX_HEALTHBAR_WIDTH = 800f;
    const float MIN_HEALTHBAR_WIDTH_TO_SHOW_TEXT = 150f;
    const float HEALTH_CHANGE_SPEED_MULTIPLIER = 2f;

    public bool ShowHealthNumber = false;
    public float CameraShakeOnDamageForce = 0.1f;
    public float DamagedScreenOverlayFillOnDamage = 0.5f;
    public Color DefaultTextColor = Color.white;
    public Color DyingTextColor = Color.red;
    public Color DeadTextColor = Color.white;

    [Header("const references")]
    public Image HealthbarHealth;
    public Image HealthbarHealthChange;
    public TextMeshProUGUI HealthbarText;

    private CharacterHealth _healthTrackedCharacter;
    private string _deathText = "DEAD";
    private RectTransform _selfRectTransform;

    public CharacterHealth HealthTrackedCharacter
    {
        get => _healthTrackedCharacter;
        set
        {
            if (_healthTrackedCharacter != null) _healthTrackedCharacter.OnHitByProjectile -= HealthTrackedCharacter_OnHitByProjectile;
            if (value != null) value.OnHitByProjectile += HealthTrackedCharacter_OnHitByProjectile;

            _healthTrackedCharacter = value;
        }
    }

    private void HealthTrackedCharacter_OnHitByProjectile(object sender, AbstractProjectile e)
    {
        Camera.main?.GetComponent<ShakableObject>().Shake(CameraShakeOnDamageForce);
        UIManager.Instance.DamagedScreenOverlay.FillAmount += DamagedScreenOverlayFillOnDamage;
    }

    private void Awake()
    {
        _selfRectTransform = GetComponent<RectTransform>() ?? throw new UnityException("RectTransform component not found");
    }

    private void Update()
    {
        if (HealthTrackedCharacter == null) return;


        HealthbarHealth.fillAmount = NumberMath.LimitFloatBetweenZeroAndOne(HealthTrackedCharacter.CurrentHealth / HealthTrackedCharacter.MaxHealth);
        HealthbarHealthChange.fillAmount = math.lerp(HealthbarHealthChange.fillAmount, HealthbarHealth.fillAmount, Time.deltaTime * HEALTH_CHANGE_SPEED_MULTIPLIER);

        if (_selfRectTransform.sizeDelta.x < MIN_HEALTHBAR_WIDTH_TO_SHOW_TEXT)
        {
            HealthbarText.text = "";
        }
        else if (GetTrackedIsDead())
        {
            HealthbarText.text = _deathText;
            HealthbarText.color = DeadTextColor;
        }
        else if (ShowHealthNumber)
        {
            HealthbarText.text = HealthTrackedCharacter.CurrentHealth.ToString("0.00");
            HealthbarText.color = (HealthTrackedCharacter.MinHealth > HealthTrackedCharacter.CurrentHealth ? DyingTextColor : DefaultTextColor);
        }
        else
        {
            HealthbarText.text = "";
        }

        _selfRectTransform.sizeDelta = new Vector2(
            NumberMath.LimitFloatInRange(HealthTrackedCharacter.MaxHealth * (MAX_HEALTHBAR_WIDTH / CHARACTER_HEALTH_AMOUNT_TO_REACH_MAX_HEALTHBAR_WIDTH), MIN_HEALTHBAR_WIDTH, MAX_HEALTHBAR_WIDTH),
            _selfRectTransform.sizeDelta.y
            );
    }

    public void SetDeathText(string text)
    {
        _deathText = text;
    }

    public bool GetTrackedIsDead()
    {
        return HealthTrackedCharacter.CharComponents.CharacterEffectsReceiver.GetHasEffect<ILethalEffect>();
    }

    public bool GetTrackedIsDying()
    {
        return HealthTrackedCharacter.CharComponents.CharacterEffectsReceiver.GetHasEffect<ILethalEffect>(true) && !GetTrackedIsDead();
    }

    public bool GetTrackedIsDead(out ILethalEffect deathEffect)
    {
        return HealthTrackedCharacter.CharComponents.CharacterEffectsReceiver.TryGetEffect(out deathEffect);
    }

    public bool GetTrackedIsDying(out ILethalEffect deathEffect, out AbstractEffect deathEffectOwner)
    {
        return HealthTrackedCharacter.CharComponents.CharacterEffectsReceiver.TryGetEffect(out deathEffect, out deathEffectOwner, true) && !GetTrackedIsDead();
    }

    public void SetHealthbarMaterial(Material material)
    {
        HealthbarHealth.material = material;
        HealthbarHealthChange.material = material;
    }

    public float GetFillAmount()
    {
        return HealthbarHealth.fillAmount;
    }

    private void OnDestroy()
    {
        HealthTrackedCharacter = null;
    }
}