using UnityEngine;

[DefaultExecutionOrder(3)]
public abstract class AbstractAI : AbstractCharacterComponent
{
    protected AbstractCharacterStateBehaviourAI _selfStateBehaviourAI;

    protected override void OnAwake()
    {
        base.OnAwake();
        if (!TryGetComponent(out _selfStateBehaviourAI)) throw new UnityException("not found AbstractCharacterStateBehaviourAI component at " + gameObject.name);
    }
}
