using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-1)]
public class LayerManager : MonoBehaviour
{
    public const string ZLAYER_TAG_NAME = "ZLayer";
    public const string ENVIROMENT_TAG_NAME = "Enviroment";
    public const string PROJECTILE_TAG_NAME = "Projectile";
    public const string CHARACTER_TAG_NAME = "Character";
    public const string HOLDABLE_TAG_NAME = "Holdable";
    public const string FURNITURE_TAG_NAME = "Furniture";
    public const string PHYSICS_PARTICLE_TAG_NAME = "PhysicsParticle";
    public const string FLUID_PARTICLE_TAG_NAME = "FluidParticle";
    public const string WORLD_GENERATION_DATA_TAG_NAME = "WorldGenerationData";

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
        if (moveGameObject == null) return;

        switch (moveGameObject.tag)
        {
            case PROJECTILE_TAG_NAME:
                moveGameObject.transform.SetParent(targetLayer.ProjectilesContainer);
                break;
            case FURNITURE_TAG_NAME:
                moveGameObject.transform.SetParent(targetLayer.FurnitureContainer);
                break;
            case HOLDABLE_TAG_NAME:
                moveGameObject.transform.SetParent(targetLayer.HoldablesContainer);
                break;
            case PHYSICS_PARTICLE_TAG_NAME:
                moveGameObject.transform.SetParent(targetLayer.PhysicsParticlesContainer);
                break;
            case FLUID_PARTICLE_TAG_NAME:
                moveGameObject.transform.SetParent(targetLayer.FluidParticlesContainer);
                break;
            case CHARACTER_TAG_NAME:
                moveGameObject.transform.SetParent(targetLayer.CharactersContainer);
                break;
            default:
                moveGameObject.transform.SetParent(targetLayer.transform);
                break;
        }

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
                targetLayer.transform.position.z
                );
        }

        targetLayer.UpdateLayerForAllChildren(moveGameObject.transform);
    }

    private void OnDestroy()
    {
        Instance = null;
    }
}
