using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[DefaultExecutionOrder(-1)]
public class GameplayUIManager : MonoBehaviour
{
    const float HOLD_OBJECT_INFO_VISIBILITY_CHANGE_SPEED_MULTIPLIER = 8f;
    const float HOLD_OBJECT_HIDE_POS_MULTIPLIER_X = 1.5f;
    const float HOLD_OBJECT_HIDE_POS_MULTIPLIER_Y = -1.5f;

    public MultiHealthbarsManager MultiHealthbarsManager;
    public HoldObjectInfo HoldObjectInfo;
    public PauseMenu Pause;
    public InputActionReference PauseAction;
    public string MainMenuSceneName = "MainMenu";

    private List<CharacterComponentsManager> _trackedCharacters = new();
    private RectTransform _holdObjectInfoRectTransform;

    public static GameplayUIManager GetInstance()
    {
        return UIManager.Instance.GameplayScreenOverlay.GetGameplayUI();
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
    }

    public void AddTrackedCharacter(CharacterComponentsManager character)
    {
        _trackedCharacters.Add(character);
        MultiHealthbarsManager.AddHealthbar(character.CharacterHealth);
        HoldObjectInfo.TrackedHolder = _trackedCharacters.Count == 1 ? character.CharacterHolding : null;
    }

    public void RemoveTrackedCharacter(CharacterComponentsManager character)
    {
        _trackedCharacters.Remove(character);
        MultiHealthbarsManager.RemoveHealthbar(character.CharacterHealth);
        HoldObjectInfo.TrackedHolder = _trackedCharacters.Count == 1 ? _trackedCharacters[0].CharacterHolding : null;
    }

    private bool ShowHoldObjectInfoCondition()
    {
        return
            _trackedCharacters.Count == 1 &&
            _trackedCharacters.First().CharacterHolding.CurrentHoldObject != null;
    }

    private void FixedUpdate()
    {
        bool enableHoldObjectInfo = ShowHoldObjectInfoCondition();
        _holdObjectInfoRectTransform.anchoredPosition = math.lerp(
            _holdObjectInfoRectTransform.anchoredPosition, 
            enableHoldObjectInfo ? Vector2.zero : new Vector2(_holdObjectInfoRectTransform.rect.width * HOLD_OBJECT_HIDE_POS_MULTIPLIER_X, _holdObjectInfoRectTransform.rect.height * HOLD_OBJECT_HIDE_POS_MULTIPLIER_Y), 
            Time.deltaTime * HOLD_OBJECT_INFO_VISIBILITY_CHANGE_SPEED_MULTIPLIER
            );
    }

    private void PauseActionReference_OnActionStarted(InputAction.CallbackContext context)
    {
        if (GameOverUIManager.GetInstance() == null)
        {
            Pause.Paused = !Pause.Paused;
        }
    }
}