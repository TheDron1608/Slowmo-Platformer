using System.Collections.Generic;
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
    const float SPRITE_ITEM_WIDTH = 33f;
    const float SPRITE_ITEMS_WAVE_HEIGHT = 10f;
    const float SPRITE_ITEMS_WAVE_FREQ = 2f;
    const float SPRITE_ITEMS_WAVE_WIDTH = 7f;
    const float SPRITE_ITEM_SCALE_MULT = 0.75f;

    public bool ShowHealthNumber = false;
    public float CameraShakeOnDamageForce = 0.1f;
    public float DamagedScreenOverlayFillOnDamage = 0.5f;
    public float GlitchEffectIntencityOnDamage = 0.1f;
    public Color DefaultTextColor = Color.white;
    public Color DyingTextColor = Color.red;
    public float SpriteItemPerHealth = 1f;

    [Header("const references")]
    public Image HealthbarHealth;
    public Image HealthbarHealthLoseChange;
    public Image HealthbarHealthAddChange;
    public Image HealthbarBg;
    public TextMeshProUGUI HealthbarText;
    public Transform SpriteItemsContainer;
    public Transform SpriteItemsTrackTargetsContainer;

    private CharacterHealth _healthTrackedCharacter;
    private CharacterUITrack _uiTrackSource;
    private string _deathText = "DEAD";
    private RectTransform _selfRectTransform;
    private bool _showHealthLine = true;
    private List<Image> _spriteItems = new();
    private float _waveTime = 0f;

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

    public bool ShowHealthLine
    {
        get => _showHealthLine;
        set
        {
            if (_showHealthLine == value) return;

            HealthbarHealth.enabled = value;
            HealthbarHealthAddChange.enabled = value;
            HealthbarHealthLoseChange.enabled = value;
            HealthbarBg.enabled = value;
            HealthbarText.enabled = value;

            _showHealthLine = value;
        }
    }

    public List<Image> Spriteitems
    {
        get => _spriteItems;
    }

    public void AddSpriteItem(Image sprite)
    {
        GameObject spriteItemContainer = new GameObject("SpriteItemContainer", typeof(RectTransform));
        spriteItemContainer.transform.SetParent(SpriteItemsContainer);
        spriteItemContainer.transform.position = Camera.main.WorldToScreenPoint(sprite.transform.position);

        sprite.transform.SetParent(spriteItemContainer.transform);
        UIElementTrackTarget.CreateTrackTarget(SpriteItemsTrackTargetsContainer, spriteItemContainer.transform);

        _spriteItems.Add(sprite);
        UpdateSpriteItemTrackTargetsContainerScale();
    }

    public void RemoveSpriteItem()
    {
        if (_spriteItems.Count > 0)
        {
            Destroy(_spriteItems[0].transform.parent.gameObject);
            _spriteItems.RemoveAt(0);
            UpdateSpriteItemTrackTargetsContainerScale();
        }
    }

    private void UpdateSpriteItemTrackTargetsContainerScale()
    {
        SpriteItemsTrackTargetsContainer.transform.localScale = new Vector3(NumberMath.LimitFloatBetweenZeroAndOne(_spriteItems.Count * SPRITE_ITEM_WIDTH / MAX_HEALTHBAR_WIDTH), 1f, 1f);
    }

    private void HealthTrackedCharacter_OnHitByProjectile(object sender, AbstractProjectile e)
    {
        if (UITrackSource.CharComponents.CharacterTeam.Team == TeamManager.Teams.PLAYER)
        {
            Camera.main?.GetComponent<ShakableObject>().Shake(CameraShakeOnDamageForce);
            UIManager.Instance.RaiseTemporalGlitchEffectIntencity(GlitchEffectIntencityOnDamage);
            UIManager.Instance.DamagedScreenOverlay.FillAmount += DamagedScreenOverlayFillOnDamage;
        }
    }

    private void Awake()
    {
        _selfRectTransform = GetComponent<RectTransform>() ?? throw new UnityException("RectTransform component not found");
    }

    private void Update()
    {
        if (HealthTrackedCharacter == null) return;

        if (ShowHealthLine)
        {
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

            _selfRectTransform.sizeDelta = new Vector2(
                NumberMath.LimitFloatInRange(HealthTrackedCharacter.MaxHealth * (MAX_HEALTHBAR_WIDTH / CHARACTER_HEALTH_AMOUNT_TO_REACH_MAX_HEALTHBAR_WIDTH), MIN_HEALTHBAR_WIDTH, MAX_HEALTHBAR_WIDTH),
                _selfRectTransform.sizeDelta.y
                );

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
        }
        else
        {
            _selfRectTransform.sizeDelta = new Vector2(MAX_HEALTHBAR_WIDTH, _selfRectTransform.sizeDelta.y);
        }



        while (HealthTrackedCharacter.CurrentHealth < (_spriteItems.Count - 1) * SpriteItemPerHealth && _spriteItems.Count > 0)
        {
            RemoveSpriteItem();
        }

        _waveTime += Time.unscaledDeltaTime;
        for (int i = 0; i < _spriteItems.Count; i++)
        {
            _spriteItems[i].transform.localPosition = new Vector3(
                0f,
                math.sin((_waveTime / SPRITE_ITEMS_WAVE_FREQ + i / SPRITE_ITEMS_WAVE_WIDTH) * math.PI) * SPRITE_ITEMS_WAVE_HEIGHT,
                0f
                );

            _spriteItems[i].transform.localScale =
                _spriteItems[i].sprite.rect.size / _spriteItems[i].sprite.pixelsPerUnit * SPRITE_ITEM_SCALE_MULT *
                (i == 0 ? (HealthTrackedCharacter.CurrentHealth % SpriteItemPerHealth) / SpriteItemPerHealth : 1f);
        }
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