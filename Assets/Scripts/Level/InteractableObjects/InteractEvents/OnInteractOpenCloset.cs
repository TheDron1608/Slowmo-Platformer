using System;
using System.Collections.Generic;
using UnityEngine;

public class OnInteractOpenCloset : Interactable
{
    const string ANIMATOR_CLOSED_PROP_NAME = "Closed";

    public List<GameObject> ObjectsInside = new();

    private bool _closed = true;

    private Animator _animator;
    [SerializeField] private GameObject _spawnSource;
    private ParticleSpawner _spawnSourceParticleSpawner;

    protected override void OnAwake()
    {
        base.OnAwake();
        if (!TryGetComponent(out _animator)) throw new UnityException("Animator component not found at " + gameObject.name);
        if (!_spawnSource.TryGetComponent(out _spawnSourceParticleSpawner)) throw new UnityException("ParticleSpawner component not found at " + _spawnSource.name);
    }

    public bool Closed
    {
        get => _closed;
        set
        {
            _closed = value;
            _animator.SetBool(ANIMATOR_CLOSED_PROP_NAME, _closed);

            if (!_closed)
            {
                if (ObjectsInside.Count > 0)
                {
                    foreach (GameObject objectInside in ObjectsInside)
                    {
                        GameObject newObject = Instantiate(objectInside, _spawnSource.transform);
                        LayerManager.Instance.ChangeZIndexForGameObject(LayerManager.Instance.GetZLayerOfGameObject(_spawnSource), newObject);
                    }
                    ObjectsInside.Clear();
                }
                else
                {
                    _spawnSourceParticleSpawner.SpawnParticle();
                }
            }
        }
    }

    protected override void OnStartInteact(GameObject interactor)
    {
        base.OnStartInteact(interactor);
        Closed = false;
    }

    protected override bool StartInteractCondition(GameObject interactor)
    {
        return base.StartInteractCondition(interactor) && Closed;
    }
}
