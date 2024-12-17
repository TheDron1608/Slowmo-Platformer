using NUnit.Framework;
using System.Collections.Generic;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;
using static UnityEditor.Experimental.GraphView.GraphView;

public class LayerManager : MonoBehaviour
{
    public const string ZLAYER_TAG_NAME = "ZLayer";
    public const string ENVIROMENT_TAG_NAME = "Enviroment";
    public const string CHARACTER_TAG_NAME = "Character";
    public const string HOLDABLE_TAG_NAME = "Holdable";
    public const string FURNITURE_TAG_NAME = "Furniture";

    public static LayerManager Instance;

    public List<ZIndexLayer> ZLayers;

    private void Awake()
    {
        if (Instance != null) throw new UnityException("maximum of 1 LayerManager instance");
        Instance = this;

        UpdateZLayers();
    }

    private void UpdateZLayers()
    {
        ZLayers = new();

        foreach (var rootGameObject in SceneManager.GetActiveScene().GetRootGameObjects())
        {
            if (rootGameObject.TryGetComponent(out ZIndexLayer zIndexLayerComponent))
            {
                ZLayers.Add(zIndexLayerComponent);
            }
        }
    }

    public ZIndexLayer GetZLayerOfGameObject(GameObject gameObj)
    {
        Transform parentGameObj = gameObj.transform.parent;
        ZIndexLayer result;
        while (!parentGameObj.gameObject.TryGetComponent(out result))
        {
            parentGameObj = parentGameObj.parent;
        }
        return result;
    }

    public void ChangeZIndexForGameObject(ZIndexLayer targetLayer, GameObject moveGameObject, GameObject newPosition = null)
    {
        Transform parentTransform;

        switch (moveGameObject.tag)
        {
            case LayerManager.ENVIROMENT_TAG_NAME:
                parentTransform = targetLayer.EnviromentContainer.transform;
                break;
            case LayerManager.CHARACTER_TAG_NAME:
                parentTransform = targetLayer.CharacterContainer.transform;
                break;
            case LayerManager.HOLDABLE_TAG_NAME:
                parentTransform = targetLayer.HoldablesContainer.transform;
                break;
            case LayerManager.FURNITURE_TAG_NAME:
                parentTransform = targetLayer.FurnitureContainer.transform;
                break;
            default:
                throw new UnityException($"{moveGameObject.tag} tag is not valid");
        }

        moveGameObject.transform.parent = parentTransform;

        if (newPosition == null)
        {
            moveGameObject.transform.localPosition = new Vector3(
                moveGameObject.transform.localPosition.x,
                moveGameObject.transform.localPosition.y,
                0f
                );
        }
        else
        {
            moveGameObject.transform.position = new Vector3(
                newPosition.transform.position.x,
                newPosition.transform.position.y,
                parentTransform.position.z
                );
        }

        moveGameObject.layer = targetLayer.gameObject.layer;
    }

    private void OnDestroy()
    {
        Instance = null;
    }
}
