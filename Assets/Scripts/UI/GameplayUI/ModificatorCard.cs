using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ModificatorCard : MonoBehaviour
{
    const string MODIFICATOR_TITLE_GO_NAME = "ModificatorTitle";

    public AbstractModificator ModificatorInstance;
    [SerializeField] private Image _cardImage;

    private float _multiplier = 1f;
    private string _localizedTitle;
    private string _localizedDescription;
    private Sprite _defaultSprite;
    private Sprite _overrideSprite;
    private ModificatorCardsCluster _currentCluster = null;

    public float Multiplier
    {
        get => _multiplier;
        set
        {
            _multiplier = value;
            if (TryGetComponent(out ModificatorLocalizationMultiplierableVariables localizedVars))
            {
                localizedVars.UpdateLocalizedValues();
            }
        }
    }

    public string LocalizedTitle
    {
        get => _localizedTitle;
        set => _localizedTitle = value;
    }

    public string LocalizedDescription
    {
        get => _localizedDescription;
        set => _localizedDescription = value;
    }

    public Sprite OverrideSprite
    {
        get => _overrideSprite;
        set
        {
            _overrideSprite = value;
            _cardImage.sprite = _overrideSprite ?? _defaultSprite;
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

    private void Awake()
    {
        _cardImage = GameObjectUtility.FindGameObjectInChildrenByName(transform, MODIFICATOR_TITLE_GO_NAME).GetComponent<Image>();
        _defaultSprite = _cardImage.sprite;
    }
}