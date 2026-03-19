using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class CursePickManager : AbstractModificatorCardsManager
{
    const float CHANGE_SCENE_DELAY_AFTER_SPEND_ALL_PICKS = 0.5f;
    const float SHOW_CARDS_DELAY = 0.5f;

    [SerializeField] private ModificatorCardsCluster _clusterInstance;
    [SerializeField] private ModificatorVisualInfo _cardInfoInstance;

    private int _picksLeft = 1;
    private Coroutine _changeSceneDelayAfterSpendAllPicksCoroutine = null;

    public static CursePickManager Instance;

    private void Awake()
    {
        _picksLeft = ModificatorsManager.Instance?.ModifiactorsPickAmount ?? 1;
        StartCoroutine(ShowCardsAfterDelay());
        if (Instance != null) throw new UnityException("Limit of 1 ModificatorsContainer instance per scene");
        Instance = this;
    }

    private IEnumerator ShowCardsAfterDelay()
    {
        if (ModificatorsManager.Instance != null)
        {
            yield return new WaitForSeconds(SHOW_CARDS_DELAY);
            AddModificatorCardsCluster(ModificatorsManager.Instance.PickRandomModifcators(new AbstractModificator.ModificatorTypes[] { AbstractModificator.ModificatorTypes.NEGATIVE }));
        }
    }

    public void SpendPicksLeft(int amount = 1)
    {
        _picksLeft -= amount;
        if (_picksLeft <= 0)
        {
            while (ModificatorCardsClusters.Count > 0)
            {
                RemoveModificatorCardsCluster(ModificatorCardsClusters.First());
            }

            foreach (AbstractModificator modificator in ModificatorsManager.Instance.CurrentModificators)
            {
                if (!modificator.DisabledModificator)
                {
                    modificator.OnModificatorChoiseFinished();
                }
            }
            _changeSceneDelayAfterSpendAllPicksCoroutine = StartCoroutine(ChangeSceneDelayAfterSpendAllPicks());
        }
        else
        {
            if (_changeSceneDelayAfterSpendAllPicksCoroutine != null)
            {
                StopCoroutine(_changeSceneDelayAfterSpendAllPicksCoroutine);
                _changeSceneDelayAfterSpendAllPicksCoroutine = null;
            }
            SetAllCardsInteractable(true);
        }
    }

    private IEnumerator ChangeSceneDelayAfterSpendAllPicks()
    {
        yield return new WaitForSeconds(CHANGE_SCENE_DELAY_AFTER_SPEND_ALL_PICKS);
        UIManager.Instance.LoadSceneWithEffect(SceneList.GAMEPLAY);
    }

    private void OnDestroy()
    {
        Instance = null;
    }
}