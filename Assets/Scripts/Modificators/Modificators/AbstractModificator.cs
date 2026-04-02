using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.Localization.Settings;

public abstract class AbstractModificator : MonoBehaviour, IModificatorInfo
{
    const string LOCALIZATION_TABLE_NAME = "GameplayUI";
    const string LOCALIZATION_PERMANENT_KEY = "ModificatorStatusPermanent";
    const string LOCALIZATION_TRADABLE_KEY = "ModificatorStatusTradable";
    const string LOCALIZATION_PRICE_KEY = "ModificatorPrice";


    public enum ModificatorTiers 
    { 
        TIER_0 = 0,
        TIER_1 = 1,
        TIER_2 = 2,
        TIER_3 = 3
    }

    public enum ModificatorTypes
    {
        POSITIVE,
        NEGATIVE,
        NEUTRAL
    }

    public enum ModificatorStatuses
    {
        UNSET,
        CHARACTER_DEFAULT,
        PERMANENT,
        CURSE,
        TRADED
    }

    public static string GetLocalizedStatus(ModificatorStatuses status, float price)
    {
        switch (status)
        {
            case ModificatorStatuses.CHARACTER_DEFAULT:
                return LocalizationSettings.StringDatabase.GetLocalizedString(LOCALIZATION_TABLE_NAME, LOCALIZATION_PERMANENT_KEY);
            case ModificatorStatuses.PERMANENT:
                return LocalizationSettings.StringDatabase.GetLocalizedString(LOCALIZATION_TABLE_NAME, LOCALIZATION_PERMANENT_KEY);
            case ModificatorStatuses.CURSE:
                return
                    LocalizationSettings.StringDatabase.GetLocalizedString(LOCALIZATION_TABLE_NAME, LOCALIZATION_TRADABLE_KEY) + "\n" +
                    LocalizationSettings.StringDatabase.GetLocalizedString(LOCALIZATION_TABLE_NAME, LOCALIZATION_PRICE_KEY) + ": " + price.ToString("0");
            case ModificatorStatuses.TRADED:
                return LocalizationSettings.StringDatabase.GetLocalizedString(LOCALIZATION_TABLE_NAME, LOCALIZATION_PERMANENT_KEY) + "\n" +
                    LocalizationSettings.StringDatabase.GetLocalizedString(LOCALIZATION_TABLE_NAME, LOCALIZATION_PRICE_KEY) + ": " + price.ToString("0");
            default:
                return "";
        }
    }

    public ModificatorTypes ModificatorType;
    public ModificatorTiers ModificatorTier;
    public Sprite IconSprite;
    public Sprite CardSprite;

    [SerializeField] private bool _multiplierable = false;
    [SerializeField] private float _modificatorMultiplier = 1f;
    [SerializeField] private float _modificatorPrice = 0f;
    [SerializeField] private ModificatorLocalization _localization;

    private ModificatorStatuses _status;
    private ModificatorIcon _currentIcon;
    private AbstractModificator _originalModificator;
    private bool _disabledModificator = false;

    public bool Multiplierable
    {
        get => _multiplierable;
        set => _multiplierable = value;
    }

    public float ModificatorMultiplier
    {
        get => _modificatorMultiplier;
        set => _modificatorMultiplier = value;
    }

    public float ModificatorPrice
    {
        get => _modificatorMultiplier;
        set => _modificatorMultiplier = value;
    }

    public ModificatorLocalization Localization
    {
        get => _localization;
    }

    public bool DisabledModificator
    {
        get => _disabledModificator;
        set
        {
            if (value == _disabledModificator) return;
            _disabledModificator = value;

            if (_disabledModificator)
            {
                OnModificatorAdded();
            }
            else
            {
                OnModificatorRemoved();
            }

            if (_currentIcon != null) _currentIcon.DisabledModificator = value;
        }
    }

    public ModificatorIcon CurrentIcon
    {
        get => _currentIcon;
        set => _currentIcon = value;
    }

    public AbstractModificator OriginalModificator
    {
        get => _originalModificator;
        set => _originalModificator = value;
    }

    public ModificatorStatuses Status
    {
        get => _status;
        set
        {
            _status = value;
        }
    }

    public virtual bool GetEqualType(AbstractModificator other)
    {
        return
            GetType() == other.GetType() &&
            ((!Multiplierable && !other.Multiplierable) || ModificatorMultiplier == other.ModificatorMultiplier);
    }

    public void TryTriggerIconAnimation()
    {
        CurrentIcon?.TriggerAnimation();
    }

    public virtual void OnModificatorAdded()
    {
        if (LayerManager.Instance != null)
        {
            LayerManager.Instance.OnObjectSpawned += OnObjectSpawned;
        }
    }

    public virtual void OnModificatorRemoved()
    {
        if (LayerManager.Instance != null)
        {
            LayerManager.Instance.OnObjectSpawned -= OnObjectSpawned;
        }
    }

    public virtual void OnLevelPreGenerated()
    {
        LayerManager.Instance.OnObjectSpawned += OnObjectSpawned;
    }

    public virtual void OnLevelGenerated()
    {

    }

    public virtual void OnLevelFinished()
    {
        LayerManager.Instance.OnObjectSpawned -= OnObjectSpawned;
    }

    public virtual void OnModificatorChoiseStarted()
    {

    }

    public virtual void OnModificatorChoiseFinished()
    {

    }

    protected virtual void OnObjectSpawned(object sender, GameObject e)
    {

    }

    private void OnDestroy()
    {
        if (!DisabledModificator)
        {
            OnModificatorRemoved();
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        _localization = transform.GetComponentInChildren<ModificatorLocalization>();
    }
#endif
}