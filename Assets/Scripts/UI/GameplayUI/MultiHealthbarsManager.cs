using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class MultiHealthbarsManager : MonoBehaviour
{
    const int MAX_HEALTHBARS = 6;
    const float DAMAGED_OVERLAY_FILL_SPEED_MULTIPLIER = 5f;

    [SerializeField] private Healthbar _spawnHealthbar;
    [SerializeField] private RectTransform _healthbarsSpawnPosition;
    [SerializeField] private RectTransform _healthbarsTrackTargetsContainer;

    private List<Healthbar> _currentHealthbars = new();

    public Healthbar AddHealthbar(CharacterHealth healthbarOwner)
    {
        Healthbar newHealthbar = Instantiate(_spawnHealthbar, _healthbarsSpawnPosition);
        newHealthbar.HealthTrackedCharacter = healthbarOwner;
        _currentHealthbars.Add(newHealthbar);
        UIElementTrackTarget.CreateTrackTarget(_healthbarsTrackTargetsContainer, newHealthbar.transform);

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
    public void RemoveHealthbar(CharacterHealth healthbarOwner)
    {
        foreach (Healthbar healthbar in _currentHealthbars)
        {
            if (healthbar.HealthTrackedCharacter == healthbarOwner)
            {
                _currentHealthbars.Remove(healthbar);
                Destroy(healthbar.gameObject);
                break;
            }
        }
    }

    public float PickAvgHealthbarsFillAmount()
    {
        float result = 0;
        foreach (Healthbar healthbar in _currentHealthbars)
        {
            result += healthbar.GetFillAmount();
        }
        result /= _currentHealthbars.Count;
        return result;
    }

    private bool TryRemoveDeadHealthbar()
    {
        foreach (Healthbar healthbar in _currentHealthbars)
        {
            if (healthbar.GetTrackedIsDead())
            {
                RemoveHealthbar(healthbar);
                return true;
            }
        }
        return false;
    }

    private void FixedUpdate()
    {
        if (_currentHealthbars.Count >= MAX_HEALTHBARS) TryRemoveDeadHealthbar();

        if (_currentHealthbars.Count > 0)
        {
            UIManager.Instance.DamagedScreenOverlay.Show();
            UIManager.Instance.DamagedScreenOverlay.FillAmount = math.lerp(
                UIManager.Instance.DamagedScreenOverlay.FillAmount, 
                1f - math.sin(PickAvgHealthbarsFillAmount() * math.PI / 2),
                Time.deltaTime * DAMAGED_OVERLAY_FILL_SPEED_MULTIPLIER
                );
        }
        else
        {
            UIManager.Instance.DamagedScreenOverlay.Hide();
        }
    }
}