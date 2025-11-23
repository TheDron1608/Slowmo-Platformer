using System;
using TMPro;
using UnityEngine;

public class ButtonOnClickToggleDeleteSaves : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _textContainer;
    [SerializeField]
    private TextMeshProUGUI _cancelTextContainer;


    public static event EventHandler<bool> OnDeleteSavesChanged;

    public static bool DeleteSaves = false;

    private void Start()
    {
        SaveButton.OnSaveDeleted += SaveButton_OnSaveDeleted;
    }

    private void SaveButton_OnSaveDeleted(object sender, EventArgs e)
    {
        if (DeleteSaves)
        {
            ToggleDeleteSaves();
        }
    }

    public void ToggleDeleteSaves()
    {
        DeleteSaves = !DeleteSaves;
        _textContainer.gameObject.SetActive(!DeleteSaves);
        _cancelTextContainer.gameObject.SetActive(DeleteSaves);

        OnDeleteSavesChanged?.Invoke(this, DeleteSaves);
    }


    private void OnDestroy()
    {
        DeleteSaves = false;
        SaveButton.OnSaveDeleted -= SaveButton_OnSaveDeleted;
    }
}
