using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Localization;

public class HurryUpManager : MonoBehaviour
{
    const float MIN_SNIPER_DELAY = 0.1f;

    public float SniperSpawnDelay = 120f; //2 minutes
    public float SniperMoveSpeedUpPerSecond = 5f / 30;
    public float SniperMaxMoveSpeedUpPerSecond = 5f / 30;
    public float SniperAttackDelayDownPerSecond = 0.5f / 30f;
    public Sniper SniperInstance;
    public LocalizedString HurryUpText;

    private float _currentSpawnDelay = 0f;
    private Sniper _currentSniper = null;
    private string _hurryUpText;

    private void FixedUpdate()
    {
        _currentSpawnDelay += Time.fixedDeltaTime;

        if (_currentSniper == null && _currentSpawnDelay > SniperSpawnDelay)
        {
            _currentSniper = SniperInstance.SpawnSelf();
            _hurryUpText = HurryUpText.GetLocalizedString();
            UIManager.Instance.FastMessageScreenOverlay.Show(_hurryUpText);
        }

        if (_currentSniper != null)
        {
            _currentSniper.MoveSpeed += SniperMoveSpeedUpPerSecond * Time.fixedDeltaTime;
            _currentSniper.MaxMoveSpeed += SniperMaxMoveSpeedUpPerSecond * Time.fixedDeltaTime;
            _currentSniper.AttackDelay = math.max(MIN_SNIPER_DELAY, _currentSniper.AttackDelay - SniperAttackDelayDownPerSecond * Time.fixedDeltaTime);
        }
    }
}