using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-1)]
public class ModificatorDebugManager : MonoBehaviour
{
    public KeyCode OpenDebugKeyCode = KeyCode.Tilde;
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
        if (Input.GetKeyDown(OpenDebugKeyCode) && SceneList.GetCurrentSceneIsGameplay())
        {
            SetOpenModDebug(true);
        }
        else if (Input.GetKeyUp(OpenDebugKeyCode))
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
