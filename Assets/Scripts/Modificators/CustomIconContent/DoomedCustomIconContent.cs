using System;
using TMPro;
using UnityEngine;

public class DoomedCustomIconContent : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _countDownText;

    private DoomedModificator _modificator;

    private void Start()
    {
        GameObjectUtility.TryGetComponentInParentRecursive(transform, out ModificatorIcon icon);
        _modificator = icon.CurrentModificator as DoomedModificator;
    }

    private void Update()
    {
        _countDownText.text = new TimeSpan(0, 0, (int)_modificator.TimeLeft).ToString("m':'ss");
    }
}