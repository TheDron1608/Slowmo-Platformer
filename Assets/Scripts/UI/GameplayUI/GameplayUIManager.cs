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

    public static GameplayUIManager Instance = null;

    public MultiHealthbarsManager MultiHealthbarsManager;
    public HoldObjectInfo HoldObjectInfo;
    public PauseMenu Pause;
    public InputActionReference PauseAction;

    private List<CharacterComponentsManager> _trackedCharacters = new();
    private RectTransform _holdObjectInfoRectTransform;

    public static bool GamePaused()
    {
        return Instance.Pause.Paused;
    }

    private void Awake()
    {
        if (Instance != null) throw new UnityException("limit of 1 GameplayUIManager instance per scene");
        Instance = this;
        _holdObjectInfoRectTransform = HoldObjectInfo.GetComponent<RectTransform>();
    }
    private void Start()
    {
        PauseAction.action.started += PauseActionReference_OnActionStarted;
    }

    private void OnDestroy()
    {
        PauseAction.action.started -= PauseActionReference_OnActionStarted;
        Instance = null;
    }

    public void AddTrackedCharacter(CharacterComponentsManager character)
    {
        _trackedCharacters.Add(character);
        MultiHealthbarsManager.AddHealthbar(character.CharacterHealth);
    }

    public void RemoveTrackedCharacter(CharacterComponentsManager character)
    {
        _trackedCharacters.Remove(character);
        MultiHealthbarsManager.RemoveHealthbar(character.CharacterHealth);
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
        Pause.Paused = !Pause.Paused;
    }
}