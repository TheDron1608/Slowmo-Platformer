using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization.Settings;

public abstract class AbstractModificator : MonoBehaviour, IModificatorInfo
{
    const string LOCALIZATION_TABLE_NAME = "GameplayUI";
    const string LOCALIZATION_PERMANENT_KEY = "ModificatorStatusPermanent";
    const string LOCALIZATION_TRADABLE_KEY = "ModificatorStatusTradable";
    const string LOCALIZATION_ARTIFACT_KEY = "ModificatorStatusArtifact";
    const string LOCALIZATION_PRICE_KEY = "ModificatorPrice";
    const string LOCALIZATION_SPOILED_KEY = "ModificatorSpoilProgress";

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
        NEUTRAL,
    }

    public enum ModificatorStatuses
    {
        UNSET,
        CHARACTER_DEFAULT,
        PERMANENT,
        CURSE,
        TRADED,
        NONE,
        ARTIFACT
    }

    public static string GetLocalizedStatus(ModificatorStatuses status, float price, float? spoilProgress)
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
            case ModificatorStatuses.ARTIFACT:
                return LocalizationSettings.StringDatabase.GetLocalizedString(LOCALIZATION_TABLE_NAME, LOCALIZATION_ARTIFACT_KEY) + "\n" +
                    LocalizationSettings.StringDatabase.GetLocalizedString(LOCALIZATION_TABLE_NAME, LOCALIZATION_PRICE_KEY) + ": " + price.ToString("0") +
                    (spoilProgress != null ? "\n" + LocalizationSettings.StringDatabase.GetLocalizedString(LOCALIZATION_TABLE_NAME, LOCALIZATION_SPOILED_KEY) + (spoilProgress.Value * 100f).ToString("F0") + "%" : "");
            default:
                return "";
        }
    }

    public bool Stackable = false;
    public bool AllowPermanent = true;
    public List<AbstractModificator> OverrideModificators = new();
    public List<AbstractModificator> RestrictModificators = new();
    public List<AbstractModificator> SynergingModificators = new();
    public List<AbstractModificator> UnsynergingModificators = new();
    public AbstractModificator HarderVersion = null;
    public ModificatorTypes ModificatorType;
    public ModificatorTiers ModificatorTier;
    public Sprite IconSprite;
    public Sprite CardSprite;
    public GameObject CustomIconContent = null;
    public GameObject CustomCardContent = null;

    [SerializeField] private bool _multiplierable = false;
    [SerializeField] private float _modificatorPrice = 0f;
    [SerializeField] private ModificatorLocalization _localization;

    private float _modificatorMultiplier = 1f;
    private ModificatorStatuses _status;
    private ModificatorIcon _currentIcon;
    private AbstractModificator _originalModificator = null;
    private bool _disabledModificator = false;
    private float _modificatorLivetime = 0f;

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
        get => _modificatorPrice;
        set => _modificatorPrice = value;
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
                OnModificatorRemoved();
            }
            else
            {
                OnModificatorAdded();
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

    public AbstractModificator OriginalOrSelf
    {
        get => OriginalModificator ?? this;
    }

    /// <summary>
    /// returns true 
    /// if any of modificators have restrict relation or
    /// if with modificator has override relation to this object or
    /// if classes equal and unstackable
    /// </summary>
    public bool GetIsRestrictedWith(AbstractModificator with)
    {
        return
            (!OriginalOrSelf.Stackable && with.OriginalOrSelf == OriginalOrSelf) ||
            with.OriginalOrSelf.OverrideModificators.Contains(OriginalOrSelf) ||
            OriginalOrSelf.RestrictModificators.Contains(OriginalOrSelf) ||
            with.OriginalOrSelf.RestrictModificators.Contains(OriginalOrSelf);
    }

    public bool GetIsSynergingWith(AbstractModificator with)
    {
        return
            OriginalOrSelf.SynergingModificators.Contains(with.OriginalOrSelf) ||
            with.OriginalOrSelf.SynergingModificators.Contains(OriginalOrSelf);
    }

    public bool GetIsUnsynergingWith(AbstractModificator with)
    {
        return
            OriginalOrSelf.UnsynergingModificators.Contains(with.OriginalOrSelf) ||
            with.OriginalOrSelf.UnsynergingModificators.Contains(OriginalOrSelf);
    }

    public bool GetIsOverriding(AbstractModificator overrideWho)
    {
        return OriginalOrSelf.OverrideModificators.Contains(overrideWho.OriginalOrSelf);
    }

    public float GetPriceDependedOnOverrides(List<AbstractModificator> possibleOverridedModificators)
    {
        float result = ModificatorPrice;
        foreach (AbstractModificator possibleOverridedModificator in possibleOverridedModificators)
        {
            if (GetIsOverriding(possibleOverridedModificator))
            {
                result -= possibleOverridedModificator.ModificatorPrice;
            }
        }

        return result;
    }

    public float? GetSpoilProgress()
    {
        if (Status == ModificatorStatuses.ARTIFACT)
        {
            return NumberMath.LimitFloatBetweenZeroAndOne(_modificatorLivetime / (ModificatorsManager.Instance?.ArtifactSpoilDurationSeconds ?? 1f));
        }
        else
        {
            return null;
        }
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

    public virtual void OnModificatorChoiseStarted(AbstractModificatorCardsManager choise)
    {

    }

    public virtual void OnModificatorChoiseFinished(AbstractModificatorCardsManager choise)
    {

    }

    protected virtual void OnObjectSpawned(object sender, GameObject e)
    {

    }

    private void FixedUpdate()
    {
        _modificatorLivetime += Time.fixedDeltaTime;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        _localization = transform.GetComponentInChildren<ModificatorLocalization>();
    }
#endif
}