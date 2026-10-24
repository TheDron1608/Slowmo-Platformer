using Unity.Mathematics;
using UnityEngine;

public class ComboMoneyModificator : AbstractModificator
{
    const float DISPLAY_MONEY_SPEED_MULT = 5f;

    public static ComboMoneyModificator Instance = null;

    private int _currentMoney;
    private float _displayedMoney;

    public int CurrentMoney
    {
        get => _currentMoney;
        set
        {
            _currentMoney = value;
        }
    }

    public float DisplayedMoney
    {
        get => _displayedMoney;
    }

    public override void OnModificatorAdded()
    {
        base.OnModificatorAdded();

        if (Instance != null) throw new UnityException("Limit of 1 ComboMoneyModificator instance, but Stackable = false to prevent it");
        Instance = this;
    }

    public override void OnLevelGenerated()
    {
        base.OnLevelGenerated();

        GameplayUIManager.GetInstance()?.Combo.gameObject.SetActive(false);
        UIManager.Instance.MoneyBallanceScreenOverlay.Show();
        _displayedMoney = _currentMoney;
    }

    private void Update()
    {
        _displayedMoney = math.lerp(_displayedMoney, _currentMoney, Time.deltaTime * DISPLAY_MONEY_SPEED_MULT);

        if (UIManager.Instance.MoneyBallanceScreenOverlay.IsShown())
        {
            UIManager.Instance.MoneyBallanceScreenOverlay.TextContent = _displayedMoney.ToString("0") + "$";
        }
    }

    public override void OnModificatorRemoved()
    {
        base.OnModificatorRemoved();

        Instance = null;

        GameplayUIManager.GetInstance()?.Combo.gameObject.SetActive(true);
        UIManager.Instance.MoneyBallanceScreenOverlay.Show();
    }
}