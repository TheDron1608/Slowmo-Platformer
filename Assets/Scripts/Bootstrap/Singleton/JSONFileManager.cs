using NUnit.Framework;
using System.Collections.Generic;
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

    public const string JSON_ROOT_FILES_PATH = "Json\\";

    [Header("controls")]
    public string ControlsFileName;
    [Header("window")]
    public string WindowFileName;
    [Header("sounds")]
    public string SoundFileName;
    [Header("saves")]
    public string SavesFolder;
    public string SaveFileRootName;
    

    private static string GetJSONRootPath()
    {
        return Application.streamingAssetsPath + "\\" + JSON_ROOT_FILES_PATH;
    }

    public static string ReadJSON(string fileName)
    {
        if (!File.Exists(GetJSONRootPath() + fileName)) return null;

        return File.ReadAllText(GetJSONRootPath() + fileName);
    }

    public static string ReadJSON(string folderName, string fileRootName, int fileIndex)
    {
        if (!File.Exists(GetJSONRootPath() + folderName + "\\" + fileRootName + fileIndex + ".json")) return null;

        return File.ReadAllText(GetJSONRootPath() + folderName + "\\" + fileRootName + fileIndex + ".json");
    }

    public static void SaveJSON(string fileName, string jsonData)
    {
        File.WriteAllText(GetJSONRootPath() + fileName, jsonData);
    }

    public static void SaveJSON(string folderName, string fileRootName, int fileIndex, string jsonData)
    {
        File.WriteAllText(GetJSONRootPath() + folderName + "\\" + fileRootName + fileIndex + ".json", jsonData);
    }

    public static void DeleteJSON(string fileName)
    {
        File.Delete(GetJSONRootPath() + fileName);
    }

    public static void DeleteJSON(string folderName, string fileRootName, int fileIndex)
    {
        File.Delete(GetJSONRootPath() + folderName + "\\" + fileRootName + fileIndex + ".json");
    }



    public static int CountFilesInFolder(string folderName)
    {
        return Directory.GetFiles(GetJSONRootPath() + folderName).Length;
    }

    public static List<string> ReadAllFiles(string folderName)
    {
        List<string> result = new List<string>();

        DirectoryInfo newDirInfo = new DirectoryInfo(GetJSONRootPath() + folderName);
        FileInfo[] fileInfos = newDirInfo.GetFiles("*.json");
        for (int i = 0; i < fileInfos.Length; i++)
        {
            if (fileInfos[i].Extension != "meta")
            {
                result.Add(File.ReadAllText(fileInfos[i].FullName));
            }
        }
        return result;
    }

    public bool GetFileExist(string fileName)
    {
        return File.Exists(GetJSONRootPath() + fileName);
    }
    public static bool GetFileExist(string folderName, string fileRootName, int fileIndex)
    {
        return File.Exists(GetJSONRootPath() + folderName + "\\" + fileRootName + fileIndex + ".json");
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
