using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-10)]
public class WinRoomInitializer : MonoBehaviour
{
    public Transform PlayerSpawnPosition;
    public ZIndexLayer PlayerSpawnLayer;
    public float KillPlayerDelay = 10f;
    public List<AbstractEffect> KillEffects = new();
    public Holdable KillPlayerOnHoldedObj;

    private CharacterComponentsManager _spawnedPlayer = null;
    private Coroutine _killAndFinishGameCoroutine = null;

    private void Awake()
    {
        foreach (AbstractModificator modificator in ModificatorsManager.Instance.CurrentModificators)
        {
            modificator.DisabledModificator = true;
        }
    }

    private void Start()
    {
        UIManager.Instance.GameplayScreenOverlay.Show();
        UIManager.Instance.ModificatorsScreenOverlay.Hide();
        UIManager.Instance.ArtifactModificatorsScreenOverlay.Hide();
        UIManager.Instance.DifficultyScreenOverlay.Show();

        _spawnedPlayer = SpawnManager.Instance?.SpawnPlayerCharacterAt(
            PlayerSpawnPosition.transform.position,
            PlayerSpawnLayer
            );

        if (_spawnedPlayer != null)
        {
            KillPlayerOnHoldedObj.OnGiven += KillPlayerOnHoldedObj_OnGiven;
        }
        else
        {
            _killAndFinishGameCoroutine = StartCoroutine(DelayedKillAndFinishGame());
        }
    }

    private void KillPlayerOnHoldedObj_OnGiven(object sender, CharacterHoldingObjects e)
    {
        if (e == _spawnedPlayer.CharacterHolding && _killAndFinishGameCoroutine == null)
        {
            _killAndFinishGameCoroutine = StartCoroutine(DelayedKillAndFinishGame());
        }
    }

    private IEnumerator DelayedKillAndFinishGame()
    {
        yield return new WaitForSeconds(KillPlayerDelay);

        GameOverManager.Instance.ForceFinishGame(GameOverUIManager.GameOverReasons.FINISHED_GAME);

        if (_spawnedPlayer != null)
        {
            _spawnedPlayer.CharacterEffectsReceiver.ApplyEffect(KillEffects, null);
        }

        KillPlayerOnHoldedObj.OnGiven -= KillPlayerOnHoldedObj_OnGiven;
    }
}
