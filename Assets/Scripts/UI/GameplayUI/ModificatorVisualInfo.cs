using TMPro;
using UnityEngine;

public class ModificatorVisualInfo : MonoBehaviour
{
    public TextMeshProUGUI Title;
    public TextMeshProUGUI Description;
    public TextMeshProUGUI Status;

    private IModificatorInfo _targetInfo;

    public IModificatorInfo TargetInfo
    {
        get => _targetInfo;
        set
        {
            if (_targetInfo == value) return;

            _targetInfo = value;
            UpdateInfo();
        }
    }

    private void UpdateInfo()
    {
        if (TargetInfo == null) return;

        Title.text = TargetInfo.Localization.LocalizedTitle;
        Description.text = TargetInfo.Localization.LocalizedDescription;
        Status.text = AbstractModificator.GetLocalizedStatus(TargetInfo.Status, TargetInfo.ModificatorPrice * TargetInfo.ModificatorMultiplier, TargetInfo.GetSpoilProgress());
        Description.fontStyle = TargetInfo.DisabledModificator ? FontStyles.Strikethrough : FontStyles.Normal;
    }
}