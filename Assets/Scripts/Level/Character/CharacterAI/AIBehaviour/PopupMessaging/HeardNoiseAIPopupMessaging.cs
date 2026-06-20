
public class HeardNoiseAIPopupMessaging : AbstractAIPopupMessaging
{
    private bool _detectedEnemyOnce = false;

    void FixedUpdate()
    {
        if (_detectedEnemyOnce) return;

        if (_selfStateBehaviourAI.NearestEnemyInfo.NearestEnemy != null)
        {
            _detectedEnemyOnce = true;
            CharComponents.CharacterVisual.PopupDetectedEnemy();
        }
        else if (_selfStateBehaviourAI.NearestEnemyInfo.NearestEnemy == null && _selfStateBehaviourAI.NearestEnemyInfo.LastHeardEnemy != null)
        {
            CharComponents.CharacterVisual.PopupHeardNoise();
        }
        else
        {
            CharComponents.CharacterVisual.RemovePopupMessage();
        }
    }

    private void OnDisable()
    {
        _detectedEnemyOnce = false;
        CharComponents.CharacterVisual.RemovePopupMessage();
    }
}