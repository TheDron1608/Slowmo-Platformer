using System;
using Unity.VisualScripting;
using UnityEngine;

public class ReselectFloorEventHandler : MonoBehaviour
{
    [SerializeField]
    private int _floor;

    private Renderer _renderer;
    private Material _baseMaterial;

    private void Start()
    {
        ReselectFloorEventEmitter.ReselectFloorEventCalled += ReselectFloorEventEmitter_OnReselectFloorEventCalled;

        if (!TryGetComponent<Renderer>(out _renderer))
        {
            throw new UnityException("renderer component not found in " + gameObject.name);
        }
        _baseMaterial = _renderer.material;
    }


    private void ReselectFloorEventEmitter_OnReselectFloorEventCalled(object sender, int e)
    {
        if (e== _floor)
        {
            _renderer.material = MaterialManager.Instance.SelectMaterial;
        }
        else
        {
            _renderer.material = _baseMaterial;
        }
    }

    private void OnDestroy()
    {
        ReselectFloorEventEmitter.ReselectFloorEventCalled -= ReselectFloorEventEmitter_OnReselectFloorEventCalled;
    }
}
