using System.IO;
using System.Linq;
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

    public const string JSON_ROOT_FILES_PATH = "Json/";

    [Header("language")]
    public string LanguageFileName;
    [Header("controls")]
    public string ControlsFileName;
    [Header("window")]
    public string WindowFileName;
    [Header("sounds")]
    public string SoundFileName;
    [Header("saves")]
    public string SavesFolder;
    public string SaveFileRootName;
    


    public static string ReadJSON(string fileName)
    {
        return File.ReadAllText(JSON_ROOT_FILES_PATH + fileName);
    }

    public static string ReadJSON(string folderName, string fileRootName, int fileIndex)
    {
        return File.ReadAllText(JSON_ROOT_FILES_PATH + folderName + "/" + fileRootName + fileIndex + ".json");
    }

    public static void SaveJSON(string fileName, string jsonData)
    {
        File.WriteAllText(JSON_ROOT_FILES_PATH + fileName, jsonData);
    }

    public static void SaveJSON(string folderName, string fileRootName, int fileIndex, string jsonData)
    {
        File.WriteAllText(JSON_ROOT_FILES_PATH + folderName + "/" + fileRootName + fileIndex + ".json", jsonData);
    }

    public static int CountFilesInFolder(string folderName)
    {
        return Directory.GetFiles(JSON_ROOT_FILES_PATH + folderName).Length;
    }

    public static string[] ReadAllFiles(string folderName)
    {
        string[] result = new string[CountFilesInFolder(folderName)];

        DirectoryInfo newDirInfo = new DirectoryInfo(JSON_ROOT_FILES_PATH + folderName);
        FileInfo[] fileInfos = newDirInfo.GetFiles("*.json");
        for (int i = 0; i < fileInfos.Length; i++)
        {
            result[i] = File.ReadAllText(fileInfos[i].FullName);
        }
        return result;
    }

    public bool GetFileExist(string fileName)
    {
        return File.Exists(JSON_ROOT_FILES_PATH + fileName);
    }
    public static bool GetFileExist(string folderName, string fileRootName, int fileIndex)
    {
        return File.Exists(JSON_ROOT_FILES_PATH + folderName + "/" + fileRootName + fileIndex + ".json");
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
