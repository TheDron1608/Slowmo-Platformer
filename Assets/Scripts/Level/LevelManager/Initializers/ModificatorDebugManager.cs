using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[DefaultExecutionOrder(-1)]
public class ModificatorDebugManager : MonoBehaviour
{
    public Key OpenDebugKey = Key.Tab;
    public List<AbstractModificator> DebugModificators = new();

    public static ModificatorDebugManager Instance = null;

    private void Awake()
    {
        if (Instance != null) throw new UnityException("limit of 1 ModificatorDebugManager per scene");
        transform.SetParent(ModificatorsManager.Instance.transform);
        Instance = this;
    }

    private void Update()
    {
        if (Keyboard.current[OpenDebugKey].wasPressedThisFrame && SceneList.GetCurrentSceneIsGameplay())
        {
            SetOpenModDebug(true);
        }
        else if (Keyboard.current[OpenDebugKey].wasReleasedThisFrame)
        {
            SetOpenModDebug(false);
        }
    }

    private void SetOpenModDebug(bool open)
    {
        if (open)
        {
            UIManager.Instance.DifficultyCurseChoiseScreenOverlay.DebugShow();
        }
        else
        {
            if (UIManager.Instance.DifficultyCurseChoiseScreenOverlay.IsShown())
            {
                UIManager.Instance.DifficultyCurseChoiseScreenOverlay.DifficultyCurseChoiseUI.EnemiesEffectOnFinish = new() { };
                UIManager.Instance.DifficultyCurseChoiseScreenOverlay.DifficultyCurseChoiseUI.FinishTrade();
            }
            UIManager.Instance.DifficultyCurseChoiseScreenOverlay.Hide();
        }
    }

    private void OnDestroy()
    {
        Instance = null;
    }
}
