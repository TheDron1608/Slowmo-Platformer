
using UnityEngine;
using UnityEngine.TextCore.Text;

internal interface IInteractable
{
    const string INTERACTABLE_TAG_NAME = "Interactable";
    public void Interact(GameObject interactor);
}
