using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.ResourceManagement.ResourceProviders;

public class JSONFileManager : MonoBehaviour
{
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
