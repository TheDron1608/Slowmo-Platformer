using UnityEngine;
using UnityEngine.UI;

public class ModificatorCard : MonoBehaviour, IModificatorInfo
{
    public AbstractModificator ModificatorInstance;
    public Image TitleImage;
    public Image BgImage;
    public RectTransform CustomContentContainer;

    private float _modificatorMultiplier = 1f;
    private Sprite _defaultSprite;
    private Sprite _overrideSprite;
    private ModificatorCardsCluster _currentCluster = null;
    private ModificatorLocalization _localization;
    private bool _disabledModificator = false;

    public float ModificatorMultiplier
    {
        get => _modificatorMultiplier;
        set
        {
            _modificatorMultiplier = value;
            if (TryGetComponent(out ModificatorLocalizationMultiplierableVariables localizedVars))
            {
                localizedVars.UpdateLocalizedValues();
            }
        }
    }

    public Sprite OverrideSprite
    {
        get => _overrideSprite;
        set
        {
            _overrideSprite = value;
            TitleImage.sprite = _overrideSprite ?? _defaultSprite;
        }
    }

    public ModificatorCardsCluster CurrentCluster
    {
        get => _currentCluster;
        set => _currentCluster = value;
    }

    public AbstractModificator.ModificatorStatuses Status
    {
        get => CurrentCluster.AddStatusOnPick;
    }

    public ModificatorLocalization Localization
    {
        get => _localization;
        set => _localization = value;
    }

    public bool DisabledModificator
    {
        get => _disabledModificator;
        set => _disabledModificator = value;
    }

    public float ModificatorPrice
    {
        get => ModificatorInstance.ModificatorPrice;
    }

    public bool Multiplierable
    {
        get => ModificatorInstance.Multiplierable;
    }

    public float? GetSpoilProgress()
    {
        return null;
    }

    private void Start()
    {
        _defaultSprite = TitleImage.sprite;
    }
}