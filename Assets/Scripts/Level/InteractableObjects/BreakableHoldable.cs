using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class BreakableHoldable : BreakbleObject
{
    public Holdable ObjectOnBreak = null;
    [SerializeField] private int _maxUses = 10;
    public bool UnlimitedUses = true;

    private int _usesLeft;

    public int MaxUses
    {
        get => _maxUses;
        set
        {
            _maxUses = value;
            if (_maxUses < _usesLeft)
            {
                _usesLeft = _maxUses;
            }
        }
    }

    public int UsesLeft
    {
        get => _usesLeft;
        set
        {
            _usesLeft = value;
            if (_usesLeft <= 0 && !UnlimitedUses)
            {
                BreakObject(GetComponent<Holdable>()?.CurrentHolder);
            }
        }
    }

    private void Awake()
    {
        UsesLeft = MaxUses;
    }

    public void ResetUsesLeft()
    {
        UsesLeft = MaxUses;
    }

    public void SpendOneUse()
    {
        UsesLeft--;
    }

    public override void BreakObject(MonoBehaviour breaker)
    {
        for (int i = 0; i < _brokenPartsParticleSpawners.Count; i++)
        {
            _brokenPartsParticleSpawners[i].SpawnParticle();
        }

        if (ObjectOnBreak != null)
        {
            GetComponent<Holdable>().TransformToAnotherObject(ObjectOnBreak);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
