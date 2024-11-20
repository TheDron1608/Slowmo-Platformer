using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.ResourceManagement.ResourceProviders;

public class JSONFileManager : MonoBehaviour
{
    public class WindowOptionsSaveData
    {
        public string WindowMode;
        public int resolutionX, resolutionY;

        public void ApplyOptions()
        {
            Debug.Log("applied");
            switch (WindowMode)
            {
                case "Windowed":
                    Screen.SetResolution(resolutionX, resolutionY, false);
                    break;
                case "Borderless":
#if !UNITY_STANDALONE_LINUX
                    Screen.SetResolution(resolutionX, resolutionY, FullScreenMode.MaximizedWindow);
#endif
                    break;
                case "Fullscreen":
                    Screen.SetResolution(resolutionX, resolutionY, FullScreenMode.FullScreenWindow);
                    break;
            }
        }
    }



    public static JSONFileManager Instance {  get; private set; }

    const string JSON_ROOT_FILES_PATH = "Json/";

    public string LanguageFileName;
    public string ControlsFileName;
    public string WindowFileName;
    


    public static string ReadJSON(string fileName)
    {
        return File.ReadAllText(JSON_ROOT_FILES_PATH + fileName);
    }

    public static void SaveJSON(string fileName, string jsonData)
    {
        File.WriteAllText(JSON_ROOT_FILES_PATH + fileName, jsonData);
    }



    private void Awake()
    {
        DontDestroyOnLoad(this);

        if (Instance != null) throw new UnityException("Limit of 1 Instance of JSONFileManager objects");
        Instance = this;
    }

    private void OnDestroy()
    {
        Instance = null;
    }
}
