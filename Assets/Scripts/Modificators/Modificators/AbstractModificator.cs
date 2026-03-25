using UnityEngine;
using UnityEngine.Localization.Settings;

public abstract class AbstractModificator : MonoBehaviour
{
    const string LOCALIZATION_TABLE_NAME = "GameplayUI";
    const string LOCALIZATION_PERMANENT_KEY = "ModificatorStatusPermanent";
    const string LOCALIZATION_TRADABLE_KEY = "ModificatorStatusTradable";
    const string LOCALIZATION_PRICE_KEY = "ModificatorPrice";


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

    public float ModificatorPrice = 0f;

    public ModificatorTypes ModificatorType;
    public ModificatorIcon IconInstance;
    public ModificatorCard CardInstance;
    public bool Multiplierable = false;
    public float ModificatorMultiplier = 1f;
    private ModificatorStatuses _status;

    private ModificatorIcon _currentIcon;
    private bool _disabledModificator = false;

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

            if (_currentIcon != null) _currentIcon.DisabledIcon = value;
        }
    }

    public ModificatorIcon CurrentIcon
    {
        get => _currentIcon;
        set => _currentIcon = value;
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
}