using System.Collections;
using UnityEngine;

[DefaultExecutionOrder(3)]
public abstract class AbstractAIInfo : AbstractCharacterComponent
{
    protected const float UPDATE_AI_DELAY_SECONDS = 0.25f;
    protected bool _requireUpdateInfo = true;

    private void OnEnable()
    {
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

    private void FixedUpdate()
    {
        OnFixedUpdate();
    }

    protected virtual void OnFixedUpdate()
    {

    }

    private void OnDisable()
    {
        _requireUpdateInfo = true;
    }
}
