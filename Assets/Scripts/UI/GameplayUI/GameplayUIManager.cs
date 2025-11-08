using System;
using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(-1)]
public class GameplayUIManager : MonoBehaviour
{
    public static GameplayUIManager Instance = null;

    private void Awake()
    {
        if (Instance != null) throw new UnityException("limit of 1 GameplayUIManager instance per scene");
        Instance = this;
    }

    private void OnDestroy()
    {
        Instance = null;
    }

    public MultiHealthbarsManager MultiHealthbarsManager;
}