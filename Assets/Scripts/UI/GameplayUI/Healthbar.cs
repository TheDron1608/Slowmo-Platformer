using System;
using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class Healthbar : MonoBehaviour
{
    const float MIN_HEALTHBAR_WIDTH = 10f;
    const float MAX_HEALTHBAR_WIDTH = 2000f;
    const float HEALTH_CHANGE_SPEED_MULTIPLIER = 2f;

    public CharacterHealth HealthTrackedCharacter;
    public bool ShowHealthNumber = false;

    [Header("const references")]
    public Image HealthbarBackground;
    public Image HealthbarHealth;
    public Image HealthbarHealthChange;
    public TextMeshProUGUI HealthbarText;

    private string _deathText = "DEAD";

    private void Update()
    {
        if (HealthTrackedCharacter == null) return;

        HealthbarHealth.fillAmount = NumberMath.LimitFloatBetweenZeroAndOne(HealthTrackedCharacter.CurrentHealth / HealthTrackedCharacter.MaxHealth);
        HealthbarHealthChange.fillAmount = math.lerp(HealthbarHealthChange.fillAmount, HealthbarHealth.fillAmount, Time.deltaTime * HEALTH_CHANGE_SPEED_MULTIPLIER);

        if (HealthTrackedCharacter.CharComponents.CharacterEffectsReceiver.GetHasEffect<ILethalEffect>())
        {
            HealthbarText.text = _deathText;
        }
        else if (ShowHealthNumber)
        {
            HealthbarText.text = HealthTrackedCharacter.CurrentHealth.ToString("0");
        }
        else
        {
            HealthbarText.text = "";
        }
    }

    public void SetDeathText(string text)
    {
        _deathText = text;
    }

    public bool GetTrackedIsDead()
    {
        return _deathText == HealthbarText.text;
    }
}