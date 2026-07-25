using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[DefaultExecutionOrder(-10)]
public class TutorialInitializer : MonoBehaviour
{
    public Transform PlayerSpawnPosition;
    public ZIndexLayer PlayerSpawnLayer;
    public AbstractCharacterComponent AttackTutorialCharacter;
    public OnInteractToggleOpenDoor AttackTutorialDoor;
    public AbstractCharacterComponent RollTutorialCharacter;
    public OnInteractToggleOpenDoor RollTutorialDoor;
    public InputActionReference PauseButton;
    public OnInteractToggleOpenDoor PauseTutorialDoor;

    private CharacterComponentsManager _spawnedPlayer = null;

    private void Start()
    {
        while (DifficultyManager.Instance.Difficulties.Count > 1)
        {
            DifficultyManager.Instance.Difficulties.RemoveLast();
        }

        UIManager.Instance.GameplayScreenOverlay.Show();
        UIManager.Instance.ModificatorsScreenOverlay.Hide();
        UIManager.Instance.ArtifactModificatorsScreenOverlay.Hide();
        UIManager.Instance.DifficultyScreenOverlay.Show();

        _spawnedPlayer = SpawnManager.Instance?.SpawnPlayerCharacterAt(
            PlayerSpawnPosition.transform.position,
            PlayerSpawnLayer
            );

        MetaProgressManager.Instance.enabled = false;

        if (PauseButton != null)
        {
            PauseButton.action.performed += Pause_performed;
        }
    }

    private void OnDestroy()
    {
        if (PauseButton != null)
        {
            PauseButton.action.performed -= Pause_performed;
        }
    }

    private void FixedUpdate()
    {
        if (AttackTutorialCharacter?.CharComponents.CharacterEffectsReceiver.GetHasEffect<ILethalEffect>() ?? false)
        {
            AttackTutorialDoor?.Open();
        }

        if (RollTutorialCharacter?.CharComponents.CharacterEffectsReceiver.GetHasEffect<HardStun>() ?? false)
        {
            RollTutorialDoor?.Open();
        }
    }

    private void Pause_performed(InputAction.CallbackContext obj)
    {
        PauseTutorialDoor?.Open();
    }
}
