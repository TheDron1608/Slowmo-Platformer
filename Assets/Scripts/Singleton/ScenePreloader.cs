using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Internal;
using UnityEngine.SceneManagement;

//link: https://gamedev.stackexchange.com/questions/185528/preload-scene-in-unity

public class ScenePreloader
{
    private static Dictionary<string, AsyncOperation> _loadingScences = new Dictionary<string, AsyncOperation>();

    /// <summary>
    /// Preloads scence and awaits to activate it via TryLoadPreloadedScence(string scenceName) function
    /// </summary>
    public static void PreloadScence (string sceneName, [DefaultValue("LoadSceneMode.Single")] LoadSceneMode mode)
    {
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneName, mode);
        asyncOperation.allowSceneActivation = false;
        _loadingScences.Add(sceneName, asyncOperation);
    }

    /// <summary>
    /// Loads preloaded scene, not work with unpreloaded scenes
    /// Don't forget to call UnloadPreloadedScene(string sceneName) when you don't need to move to this scene again
    /// Load scene mode depends on calling "PreloadScence (string sceneName, LoadSceneMode mode)" before
    /// </summary>
    /// <returns>true if sucessfully loaded preloaded scene
    /// false if scene not preloaded or progress of preloading is not completed</returns>
    public static bool TryLoadPreloadedScence(string scenceName)
    {
        if (!_loadingScences.TryGetValue(scenceName, out AsyncOperation scene))
        {
            return false;
        }
        if (scene.progress > 0.89f)
        {
            scene.allowSceneActivation = true;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Does same as TryLoadPreloadedScence(string scenceName), but loads scene via SceneManager.LoadScene(sceneName) if scene is not preloaded
    /// Don't forget to call UnloadPreloadedScene(string sceneName) when you don't need to move to this scene again
    /// Load scene mode depends on calling "PreloadScence (string sceneName, LoadSceneMode mode)" before
    /// </summary>
    public static void TryLoadPreloadedScenceElseLoadRegulary(string sceneName)
    {
        if (!TryLoadPreloadedScence(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
    }

    /// <summary>
    /// return progress of preloading scene
    /// </summary>
    /// <returns>float between 0.0 and 1.0, null if scene not found</returns>
    public static float? GetScencePreloadingProgress(string sceneName)
    {
        if (!_loadingScences.TryGetValue(sceneName, out AsyncOperation scene))
        {
            return null;
        }
        return scene.progress * 10f / 9f; //fixes loading progress from maximum progress = 0.9 to 1.0
    }

    /// <summary>
    /// rreturn  progress of preloading scene
    /// </summary>
    /// <returns>true if preloaded, else false, returns null if scene not found</returns>
    public static bool? GetScebcePreloadingIsDone(string sceneName)
    {
        if (!_loadingScences.TryGetValue(sceneName, out AsyncOperation scene))
        {
            return null;
        }
        return (scene.progress > 0.89f);
    }

    /// <summary>
    /// removes scenes from _loadingScences dict, saves memory
    /// does not calls automatically, call it after loading scene if you don't need to move to this scene again
    /// </summary>
    /// <param name="sceneName"></param>
    public static void UnloadPreloadedScene(string sceneName)
    {   
        _loadingScences.Remove(sceneName);
    }

    /// <summary>
    /// removes all scenes from _loadingScenes, saves memory
    /// does not calls automatically, call it after loading scene if you don't need to move to any scenes again
    /// </summary>
    public static void ClearPreloadedScenes()
    {
        _loadingScences.Clear();
    }
}
