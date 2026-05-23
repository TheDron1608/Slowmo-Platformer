using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-5)]
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
    public const string CLOUD_PARTICLE_TAG_NAME = "CloudParticle";
    public const string LIGHT_PARTICLE_TAG_NAME = "LightParticle";
    public const string OTHER_TAG_NAME = "Other";
    public const string FOG_TAG_NAME = "Fog";
    public const string WORLD_GENERATION_DATA_TAG_NAME = "WorldGenerationData";

    public static LayerManager Instance;

    public List<ZIndexLayer> ZLayers;
    public event EventHandler<GameObject> OnObjectSpawned;

    private float _levelBottom = float.MaxValue;

    public void TrySetLevelBottom(float value)
    {
        _levelBottom = Mathf.Min(value, _levelBottom);
    }
    public float GetLevelBottom()
    {
        return _levelBottom;
    }

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
        ZIndexLayer result = null;
        while (parentGameObj != null && !parentGameObj.gameObject.TryGetComponent(out result))
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
            case CLOUD_PARTICLE_TAG_NAME:
                moveGameObject.transform.SetParent(targetLayer.CloudParticlesContainer);
                break;
            case CHARACTER_TAG_NAME:
                moveGameObject.transform.SetParent(targetLayer.CharactersContainer);
                break;
            case ENVIROMENT_TAG_NAME:
                moveGameObject.transform.SetParent(targetLayer.InteractableEnviromentContainer);
                break;
            default:
                moveGameObject.transform.SetParent(targetLayer.OtherContainer);
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

    public void InvokeOnObjectSpawned(GameObject spawnedObject)
    {
        OnObjectSpawned?.Invoke(this, spawnedObject);
    }

    private void OnDestroy()
    {
        Instance = null;
    }
}
