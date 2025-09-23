using UnityEngine;

public class AIPrefferedHoldableOrderByDistance : AbstractAIPrefferedHoldable
{
    protected override bool OrderByPattern(Holdable oldHoldable, Holdable newHoldable)
    {
        return
            Vector2.Distance(CharComponents.Center.transform.position, newHoldable.transform.position) <
            Vector2.Distance(CharComponents.Center.transform.position, oldHoldable.transform.position);
    }
}
