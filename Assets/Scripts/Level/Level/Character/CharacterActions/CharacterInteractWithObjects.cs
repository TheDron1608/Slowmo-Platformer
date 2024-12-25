using System.Collections.Generic;
using UnityEngine;

public class CharacterInteractWithObjects : MonoBehaviour
{

    public float InteractRange = 1f;

    private bool _isAbleToInteractWithObjects = true;

    private CharacterChildNodes _characterChildNodesComponent;

    private void Awake()
    {
        if (!TryGetComponent(out _characterChildNodesComponent)) throw new UnityException("CharacterChildNodes component not found");
    }

    public bool IsAbleToInteractWithObjects
    {
        get => _isAbleToInteractWithObjects;
        set => _isAbleToInteractWithObjects = value;
    }

    public List<SelectableObject> GetAvaibleInteractableObjects()
    {
        if (!_isAbleToInteractWithObjects) return new List<SelectableObject>();

        var result = new List<SelectableObject>();

        Collider2D[] colliders = Physics2D.OverlapCircleAll(
            _characterChildNodesComponent.Center.transform.position, 
            InteractRange, 
            1 << LayerManager.Instance.GetZLayerOfGameObject(gameObject).ObjectsLayer
            );

        for (int i = 0; i < colliders.Length; i++)
        {
            if (!colliders[i].gameObject.TryGetComponent(out SelectableObject selectableObjectComponent)) continue;

            result.Add(selectableObjectComponent);
        }

        return result;
    }

    public SelectableObject GetInteractableObjectAtDirection(Vector2 direction)
    {
        if (!_isAbleToInteractWithObjects) return null;

        foreach (var raycastHit in Physics2D.RaycastAll(
                _characterChildNodesComponent.Center.transform.position, 
                direction, 
                InteractRange,
                1 << LayerManager.Instance.GetZLayerOfGameObject(gameObject).ObjectsLayer
                )
            )
        {
            if (
                raycastHit.collider.TryGetComponent(out SelectableObject selectableObjectComponent) &&
                raycastHit.distance <= InteractRange * selectableObjectComponent.SelectMaxRangeMultiplier
                )
            {
                return selectableObjectComponent;
            }
        }
        return null;
    }
}
