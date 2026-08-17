using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

[DefaultExecutionOrder(-1)]
public class GameplayUIManager : MonoBehaviour
{
    const float HOLD_OBJECT_INFO_VISIBILITY_CHANGE_SPEED_MULTIPLIER = 8f;
    const float HOLD_OBJECT_HIDE_POS_MULTIPLIER_X = 1.5f;
    const float HOLD_OBJECT_HIDE_POS_MULTIPLIER_Y = -1.5f;
    const float DAMAGED_OVERLAY_FILL_SPEED_MULTIPLIER = 5f;
    const float DYING_DAMAGED_OVERLAY_FILL_AMOUNT = 2f;
    const float UNLIMITED_HEALTH_DAMAGE_OVERLAY_FAKE_MAX_HEALTH = 2f;

    public MultiHealthbarsManager MultiHealthbarsManager;
    public HoldObjectInfo HoldObjectInfo;
    public PauseMenu Pause;
    public ComboEncounter Combo;
    public InputActionReference PauseAction;

    private List<CharacterUITrack> _trackedCharacters = new();
    private RectTransform _holdObjectInfoRectTransform;

    public static GameplayUIManager GetInstance()
    {
        return UIManager.Instance?.GameplayScreenOverlay?.GetGameplayUI();
    }

    private void Awake()
    {
        _holdObjectInfoRectTransform = HoldObjectInfo.GetComponent<RectTransform>();
    }
    private void Start()
    {
        PauseAction.action.started += PauseActionReference_OnActionStarted;
    }

    private void OnDestroy()
    {
        PauseAction.action.started -= PauseActionReference_OnActionStarted;
        UIManager.Instance?.DamagedScreenOverlay?.Hide();
        UIManager.Instance?.LivingTimeLeftScreenOverlay?.Hide();
    }

    public void AddTrackedCharacter(CharacterUITrack character)
    {
        _trackedCharacters.Add(character);
        if (character.TrackHealth)
        {
            MultiHealthbarsManager.AddHealthbar(character);
        }
        if (character.TrackHoldable && _trackedCharacters.Count == 1)
        {
            HoldObjectInfo.TrackedHolder = character.CharComponents.CharacterHolding;
        }
    }

    public void RemoveTrackedCharacter(CharacterUITrack character)
    {
        _trackedCharacters.Remove(character);
        MultiHealthbarsManager.RemoveHealthbar(character);
        HoldObjectInfo.TrackedHolder = _trackedCharacters.Where(e => e.TrackHoldable).FirstOrDefault()?.CharComponents.CharacterHolding;
    }

    private bool ShowHoldObjectInfoCondition()
    {
        return _trackedCharacters.Any(e => e.TrackHoldable && e.CharComponents.CharacterHolding.CurrentHoldObject != null);
    }

    private void FixedUpdate()
    {
        bool enableHoldObjectInfo = ShowHoldObjectInfoCondition();
        _holdObjectInfoRectTransform.anchoredPosition = math.lerp(
            _holdObjectInfoRectTransform.anchoredPosition,
            enableHoldObjectInfo ? Vector2.zero : new Vector2(_holdObjectInfoRectTransform.rect.width * HOLD_OBJECT_HIDE_POS_MULTIPLIER_X, _holdObjectInfoRectTransform.rect.height * HOLD_OBJECT_HIDE_POS_MULTIPLIER_Y),
            Time.deltaTime * HOLD_OBJECT_INFO_VISIBILITY_CHANGE_SPEED_MULTIPLIER
            );

        if (_trackedCharacters.Count > 0 && !UIManager.Instance.GameOverScreenOverlay.IsShown())
        {
            ILethalEffect dyingEffect = null;
            AbstractEffect dyingEffectOwner = null;
            CharacterUITrack dyingCharacter = _trackedCharacters.Find(
                e => e.TrackIsDying && e.GetTrackedIsDying(out dyingEffect, out dyingEffectOwner)
                );

            UIManager.Instance.DamagedScreenOverlay.Show();
            UIManager.Instance.DamagedScreenOverlay.FillAmount = math.lerp(
                UIManager.Instance.DamagedScreenOverlay.FillAmount,
                1f - math.sin(PickAvgPlayersHealthRelative() * math.PI / 2),
                Time.deltaTime * DAMAGED_OVERLAY_FILL_SPEED_MULTIPLIER
                );

            if (dyingCharacter != null && dyingEffectOwner is TimeDelayedEffect timeDelayedDyingEffect)
            {
                UIManager.Instance.GetLiveTimeLeftScreenOverlayByType(dyingCharacter.LiveTimeLeftType).Show(
                    timeDelayedDyingEffect.TimeLeft.ToString("0.00")
                    );
            }
            else
            {
                UIManager.Instance.LivingTimeLeftScreenOverlay.Hide();
                UIManager.Instance.SwordPlayerLiveTimeLeftScreenOverlay.Hide();
            }
        }
        else
        {
            UIManager.Instance.DamagedScreenOverlay.Hide();
            UIManager.Instance.LivingTimeLeftScreenOverlay.Hide();
            UIManager.Instance.SwordPlayerLiveTimeLeftScreenOverlay.Hide();
        }
    }

    private float PickAvgPlayersHealthRelative()
    {
        float result = 0;
        int playersCount = 0;
        foreach (CharacterUITrack character in _trackedCharacters)
        {
            if (character.CharComponents.CharacterTeam.Team != TeamManager.Teams.PLAYER) continue;

            CharacterHealth characterHealth = character.CharComponents.CharacterHealth;
            result +=
                characterHealth.CurrentHealth / 
                (
                    characterHealth.UnlimitedHealth ? 
                    math.max(UNLIMITED_HEALTH_DAMAGE_OVERLAY_FAKE_MAX_HEALTH, characterHealth.CurrentHealth):
                    characterHealth.MaxHealth
                );
            playersCount++;
        }
        result /= math.max(playersCount, .001f);
        return result;
    }

    private void PauseActionReference_OnActionStarted(InputAction.CallbackContext context)
    {
        if (
            !UIManager.Instance.GameOverScreenOverlay.IsShown() &&
            !UIManager.Instance.DifficultyCurseChoiseScreenOverlay.IsShown()
            )
        {
            Pause.Paused = !Pause.Paused;
        }
    }
}