using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DoomedModificator : AbstractModificator
{
    const string TIME_OUT_TEXT = "time out";

    public float TimeSeconds = 300f; //5 mins

    private float _timeLeft = 300f;

    public override void OnModificatorAdded()
    {
        base.OnModificatorAdded();

        _timeLeft = TimeSeconds;

        UpdateIconText();
    }

    private void Update()
    {
        if (
            !DisabledModificator && 
            SceneList.GetCurrentSceneIsGameplay() &&
            !TimeManager.Instance.Paused
            )
        {
            _timeLeft -= Time.unscaledDeltaTime;
            if (_timeLeft < 0f)
            {
                _timeLeft = 0f;
            }
        }

        if (_timeLeft <= 0f && LayerManager.Instance != null)
        {
            foreach (ZIndexLayer layer in LayerManager.Instance.ZLayers)
            {
                foreach (Transform characterTransform in layer.CharactersContainer)
                {
                    if (characterTransform.TryGetComponent(out AbstractCharacterComponent character))
                    {
                        character.CharComponents.CharacterHealth.Gib(null);
                    }
                }
            }
        }

        UpdateIconText();
    }

    private void UpdateIconText()
    {
        if (CurrentIcon != null)
        {
            TextMeshProUGUI iconText = CurrentIcon.GetComponentInChildren<TextMeshProUGUI>();

            if (iconText != null)
            {
                if (_timeLeft > 0f)
                {
                    iconText.text = new TimeSpan(0, 0, (int)_timeLeft).ToString("mm':'ss");
                }
                else
                {
                    iconText.text = TIME_OUT_TEXT;
                }
            }
        }
    }
}