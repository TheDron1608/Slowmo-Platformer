using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class Healthbar : MonoBehaviour
{
    const float CHARACTER_HEALTH_AMOUNT_TO_REACH_MAX_HEALTHBAR_WIDTH = 6f;
    const float MIN_HEALTHBAR_WIDTH = 200f;
    const float MAX_HEALTHBAR_WIDTH = 800f;
    const float MIN_HEALTHBAR_WIDTH_TO_SHOW_TEXT = 100f;
    const float HEALTH_CHANGE_SPEED_MULTIPLIER = 2f;

    public bool ShowHealthNumber = false;
    public float CameraShakeOnDamageForce = 0.1f;
    public float DamagedScreenOverlayFillOnDamage = 0.5f;
    public Color DefaultTextColor = Color.white;
    public Color DyingTextColor = Color.red;

    [Header("const references")]
    public Image HealthbarHealth;
    public Image HealthbarHealthLoseChange;
    public Image HealthbarHealthAddChange;
    public TextMeshProUGUI HealthbarText;

    private CharacterHealth _healthTrackedCharacter;
    private CharacterUITrack _uiTrackSource;
    private string _deathText = "DEAD";
    private RectTransform _selfRectTransform;

    public CharacterHealth HealthTrackedCharacter
    {
        get => _healthTrackedCharacter;
        private set
        {
            if (_healthTrackedCharacter != null) _healthTrackedCharacter.OnHitByProjectile -= HealthTrackedCharacter_OnHitByProjectile;
            if (value != null) value.OnHitByProjectile += HealthTrackedCharacter_OnHitByProjectile;

            _healthTrackedCharacter = value;
        }
    }

    public CharacterUITrack UITrackSource
    {
        get => _uiTrackSource;
        set
        {
            if (_uiTrackSource == value) return;
            _uiTrackSource = value;
            HealthTrackedCharacter = _uiTrackSource.CharComponents.CharacterHealth;
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

        float targetFillAmount = NumberMath.LimitFloatBetweenZeroAndOne(HealthTrackedCharacter.CurrentHealth / HealthTrackedCharacter.MaxHealth);

        HealthbarHealth.fillAmount =
            HealthbarHealth.fillAmount > targetFillAmount ?
            targetFillAmount :
            math.lerp(HealthbarHealth.fillAmount, targetFillAmount, Time.deltaTime * HEALTH_CHANGE_SPEED_MULTIPLIER
            );
        HealthbarHealthLoseChange.fillAmount = math.lerp(
            HealthbarHealthLoseChange.fillAmount, 
            math.max(HealthbarHealth.fillAmount, HealthbarHealthAddChange.fillAmount), 
            Time.deltaTime * HEALTH_CHANGE_SPEED_MULTIPLIER
            );
        HealthbarHealthAddChange.fillAmount = targetFillAmount;

        if (_selfRectTransform.sizeDelta.x < MIN_HEALTHBAR_WIDTH_TO_SHOW_TEXT)
        {
            HealthbarText.text = "";
        }
        else if (UITrackSource.GetTrackedIsDead())
        {
            HealthbarText.text = _deathText;
            HealthbarText.color = DyingTextColor;
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

    public float GetFillAmount()
    {
        return HealthbarHealth.fillAmount;
    }

    private void OnDestroy()
    {
        HealthTrackedCharacter = null;
    }
}