
using TMPro;
using UnityEngine;

public class ModificatorCardClusterWithPrice : ModificatorCardsCluster
{
    const string PRICE_TEXT_GO_NAME = "Price";

    private Color _ableToBuyTextColor = Color.greenYellow;
    private Color _unableToBuyTextColor = Color.red;

    [SerializeField] TextMeshProUGUI _priceText;

    public override void AddModificator(AbstractModificator modificator, Sprite overrideCardsBg = null)
    {
        base.AddModificator(modificator, overrideCardsBg);
    }

    public override void RemoveModificator(AbstractModificator modificator)
    {
        base.RemoveModificator(modificator);
    }

    protected override void Awake()
    {
        _ableToBuyTextColor = _priceText.color;
    }

    private void FixedUpdate()
    {
        float totalPrice = GetTotalPrice();
        _priceText.text = totalPrice.ToString("0") + "$";

        bool ableToBuy = ComboMoneyModificator.Instance?.CurrentMoney > totalPrice - 0.05f;

        _priceText.color = ableToBuy ? _ableToBuyTextColor : _unableToBuyTextColor;
        interactable = ableToBuy;
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        _priceText = transform.Find(PRICE_TEXT_GO_NAME)?.GetComponentInChildren<TextMeshProUGUI>() ?? throw new UnityException(PRICE_TEXT_GO_NAME + " gameObject not found");
    }
#endif
}