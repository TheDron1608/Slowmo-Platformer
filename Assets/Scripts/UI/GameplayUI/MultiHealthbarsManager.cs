using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class MultiHealthbarsManager : MonoBehaviour
{
    const int MAX_HEALTHBARS = 2;

    [SerializeField] private Healthbar _spawnHealthbar;
    [SerializeField] private RectTransform _healthbarsSpawnPosition;
    [SerializeField] private RectTransform _healthbarsTrackTargetsContainer;

    private List<Healthbar> _currentHealthbars = new();

    public Healthbar AddHealthbar(CharacterUITrack healthbarOwner)
    {
        if (_currentHealthbars.Count >= MAX_HEALTHBARS)
        {
            if (!TryRemoveDeadHealthbar()) return null;
        }
        Healthbar newHealthbar = Instantiate(_spawnHealthbar, _healthbarsSpawnPosition);
        newHealthbar.UITrackSource = healthbarOwner;
        _currentHealthbars.Add(newHealthbar);
        UIElementTrackTarget.CreateTrackTarget(_healthbarsTrackTargetsContainer, newHealthbar);


        return newHealthbar;
    }

    public void RemoveHealthbar(Healthbar remove)
    {
        if (remove == null) return;

        if (_currentHealthbars.Remove(remove))
        {
            Destroy(remove.gameObject);
        }
    }
    public void RemoveHealthbar(CharacterUITrack healthbarOwner)
    {
        foreach (Healthbar healthbar in _currentHealthbars)
        {
            if (healthbar.UITrackSource == healthbarOwner && !healthbar.IsDestroyed())
            {
                _currentHealthbars.Remove(healthbar);
                Destroy(healthbar.gameObject);
                break;
            }
        }
    }

    private bool TryRemoveDeadHealthbar()
    {
        foreach (Healthbar healthbar in _currentHealthbars)
        {
            if (healthbar.UITrackSource.GetTrackedIsDead())
            {
                RemoveHealthbar(healthbar);
                return true;
            }
        }
        return false;
    }

    private void FixedUpdate()
    {
    }
}