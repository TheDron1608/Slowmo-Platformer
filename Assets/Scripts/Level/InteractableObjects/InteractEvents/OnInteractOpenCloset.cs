using System;
using System.Collections.Generic;
using UnityEngine;

public class OnInteractOpenCloset : Interactable
{
    const string ANIMATOR_CLOSED_PROP_NAME = "Closed";

    private bool _closed = true;

    private Animator _animator;

    protected override void OnAwake()
    {
        base.OnAwake();
        if (!TryGetComponent(out _animator)) throw new UnityException("Animator component not found at " + gameObject.name);
    }

    public bool Closed
    {
        get => _closed;
        set
        {
            _closed = value;
            _animator.SetBool(ANIMATOR_CLOSED_PROP_NAME, _closed);

            List<GameObject> ObjectsInside = GetComponent<BreakableObject>()?.SpawnObjectsOnBreak;
            if (!_closed && ObjectsInside != null && ObjectsInside.Count > 0)
            {
                ZIndexLayer layer = LayerManager.Instance.GetZLayerOfGameObject(gameObject);
                Vector3 position = GameObjectUtility.GetCenterOfCollider(GetComponent<Collider2D>());
                foreach (GameObject objectInside in ObjectsInside)
                {
                    GameObject newObject = Instantiate(objectInside, transform);
                    LayerManager.Instance.ChangeZIndexForGameObject(layer, newObject);
                    newObject.transform.position = position;
                }
                ObjectsInside.Clear();
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
