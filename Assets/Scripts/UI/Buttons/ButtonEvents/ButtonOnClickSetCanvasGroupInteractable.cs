using System;
using System.Collections.Generic;
using UnityEngine;

public class ButtonOnClickSetCanvasGroupInteractable : MonoBehaviour
{
    [Serializable]
    public class CanvasGroupInteractableSet
    {
        public CanvasGroup CanvasGroup;
        public bool SetInteractableNewValue;
    }

    [SerializeField]
    public List<CanvasGroupInteractableSet> CanvasGroupInteractables = new List<CanvasGroupInteractableSet>();
    
    //called when button is clicked
    public void SetCancasGroupInstactable ()
    {
        for (int i = 0; i < CanvasGroupInteractables.Count; i++)
        {
            CanvasGroupInteractables[i].CanvasGroup.interactable = CanvasGroupInteractables[i].SetInteractableNewValue;
        }
    }
}
