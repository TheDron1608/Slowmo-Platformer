using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LightManager : MonoBehaviour
{
    public enum LightManagedType
    {
        FURNITURE,
        CHARACTER,
        GLOBAL,
        WEAPON
    }

    public static LightManager Instance;

    public float FurnitureDynmicIntensityMaxDistance = 10f;
    public float FurnitureDynmicIntensityMinDistance = 5f;

    private List<LightManagerManagedLightSource> _furnitureLightSources = new();
    private List<LightManagerManagedLightSource> _characterLightSources = new();
    private List<LightManagerManagedLightSource> _globalLightSources = new();
    private List<LightManagerManagedLightSource> _weaponLightSources = new();

    [SerializeField] private float _furnitureLightIntensityMultiplier = 1f;
    [SerializeField] private float _characterLightIntensityMultiplier = 0f;
    [SerializeField] private float _globalLightIntensityMultiplier = 1f;
    [SerializeField] private float _weaponLightIntensityMultiplier = 0f;

    public void AddLightSource(LightManagerManagedLightSource lightSource, LightManagedType type)
    {
        switch (type)
        {
            case LightManagedType.FURNITURE:
                _furnitureLightSources.Add(lightSource);
                lightSource.SetLightIntensityMultiplier(_furnitureLightIntensityMultiplier);
                break;
            case LightManagedType.CHARACTER:
                _characterLightSources.Add(lightSource);
                lightSource.SetLightIntensityMultiplier(_characterLightIntensityMultiplier);
                break;
            case LightManagedType.GLOBAL:
                _globalLightSources.Add(lightSource);
                lightSource.SetLightIntensityMultiplier(_globalLightIntensityMultiplier);
                break;
            case LightManagedType.WEAPON:
                _weaponLightSources.Add(lightSource);
                lightSource.SetLightIntensityMultiplier(_weaponLightIntensityMultiplier);
                break;
        }
    }
    public void RemoveLightSource(LightManagerManagedLightSource lightSource, LightManagedType type)
    {
        switch (type)
        {
            case LightManagedType.FURNITURE:
                _furnitureLightSources.Remove(lightSource);
                break;
            case LightManagedType.CHARACTER:
                _characterLightSources.Remove(lightSource);
                break;
            case LightManagedType.GLOBAL:
                _globalLightSources.Remove(lightSource);
                break;
            case LightManagedType.WEAPON:
                _weaponLightSources.Remove(lightSource);
                break;
        }
    }

    public float FurnitureLightIntensityMultiplier
    {
        get => _furnitureLightIntensityMultiplier;
        set
        {
            if (_furnitureLightIntensityMultiplier == value) return;
            foreach (LightManagerManagedLightSource light in _furnitureLightSources)
            {
                light.SetLightIntensityMultiplier(value);
            }
            _furnitureLightIntensityMultiplier = value;
        }
    }

    public float CharacterLightIntensityMultiplier
    {
        get => _characterLightIntensityMultiplier;
        set
        {
            if (_characterLightIntensityMultiplier == value) return;
            foreach (LightManagerManagedLightSource light in _characterLightSources)
            {
                light.SetLightIntensityMultiplier(value);
            }
            _characterLightIntensityMultiplier = value;
        }
    }

    public float GlobalLightIntensityMultiplier
    {
        get => _globalLightIntensityMultiplier;
        set
        {
            if (_globalLightIntensityMultiplier == value) return;
            foreach (LightManagerManagedLightSource light in _globalLightSources)
            {
                light.SetLightIntensityMultiplier(value);
            }
            _globalLightIntensityMultiplier = value;
        }
    }

    public float WeaponLightIntensityMultiplier
    {
        get => _weaponLightIntensityMultiplier;
        set
        {
            if (_weaponLightIntensityMultiplier == value) return;
            foreach (LightManagerManagedLightSource light in _weaponLightSources)
            {
                light.SetLightIntensityMultiplier(value);
            }
            _weaponLightIntensityMultiplier = value;
        }
    }

    private void Awake()
    {
        if (Instance != null && !Instance.IsDestroyed()) throw new UnityException("Limit of 1 LightManager instance per scene");
        Instance = this;
    }

    private void OnDestroy()
    {
        Instance = null;
    }
}