using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AIPrefferedHoldableOrederByPickUpPriority : AbstractAIPrefferedHoldable
{
    protected override bool OrderByPattern(Holdable oldHoldable, Holdable newHoldable)
    {
        return
            newHoldable.AIPickUpPriority > oldHoldable.AIPickUpPriority ||
            (
                newHoldable.AIPickUpPriority == oldHoldable.AIPickUpPriority &&
                Vector2.Distance(CharComponents.Center.transform.position, newHoldable.transform.position) < Vector2.Distance(CharComponents.Center.transform.position, oldHoldable.transform.position)
            );
    }
}
