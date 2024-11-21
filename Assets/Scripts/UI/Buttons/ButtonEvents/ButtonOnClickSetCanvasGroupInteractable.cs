using System;
using System.Collections.Generic;
using UnityEngine;

public class ButtonOnClickSetCanvasGroupInteractable : MonoBehaviour
{
    [Serializable]
    private class CanvasGroupInteractableSet
    {
        public CanvasGroup CanvasGroup;
        public bool SetIncactableNewValue;
    }

    [SerializeField]
    private List<CanvasGroupInteractableSet> _canvasGroupInteractables = new List<CanvasGroupInteractableSet>();
    
    //called when burron is clicked
    public void SetCancasGroupInstactable ()
    {
        for (int i = 0; i < _canvasGroupInteractables.Count; i++)
        {
            _canvasGroupInteractables[i].CanvasGroup.interactable = _canvasGroupInteractables[i].SetIncactableNewValue;
        }
    }
}
