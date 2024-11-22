using System;
using UnityEngine;

public class UnhideUIEventHandler : MonoBehaviour
{
    [SerializeField]
    private CanvasAplha _unhideCanvas;

    private void Start()
    {
        UnhideUIEventEmitter.UnhideUIEventCalled += UnhideUIEventEmitter_OnUnhideUIEventCalled;
    }

    private void UnhideUIEventEmitter_OnUnhideUIEventCalled(object sender, EventArgs e)
    {
        _unhideCanvas.ShowCanvas();
    }

    private void OnDestroy()
    {
        UnhideUIEventEmitter.UnhideUIEventCalled -= UnhideUIEventEmitter_OnUnhideUIEventCalled;
    }
}
