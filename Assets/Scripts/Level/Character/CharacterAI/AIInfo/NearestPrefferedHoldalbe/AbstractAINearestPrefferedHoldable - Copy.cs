using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractAINearestPrefferedHoldable : AbstractAIInfo
{
    public float MaxWeaponDetectRange = 5f;
    public int MinWeaponPriority = 1;
    public bool CanPickUpRangedWeapon = true;
    public bool CanPickMeleeWeapon = true;
    public bool CanPickUpOnlyWhitelistItems = false;
    public List<Holdable> WhitelistItems = new();

    protected Holdable _nearestPrefferedHoldable = null;

    public Holdable NearestPrefferedHoldable
    {
        get
        {
            TryUpdateInfo();
            return _nearestPrefferedHoldable;
        }
    }
}
