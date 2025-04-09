using System.Collections;
using UnityEngine;

public abstract class AbstractAIInfo : AbstractCharacterComponent
{
    protected const float UPDATE_AI_DELAY_SECONDS = 0.1f;
    protected bool _requireUpdateInfo = true;

    protected override void OnAwake()
    {
        base.OnAwake();
        StartCoroutine(UpdateRequireInfoLoop());
    }

    private IEnumerator UpdateRequireInfoLoop()
    {
        while (true)
        {
            _requireUpdateInfo = true;
            yield return new WaitForSeconds(UPDATE_AI_DELAY_SECONDS);
        }
    }

    protected void TryUpdateInfo()
    {
        if (_requireUpdateInfo)
        {
            OnUpdateInfo();
            _requireUpdateInfo = false;
        }
    }

    protected abstract void OnUpdateInfo();
}
