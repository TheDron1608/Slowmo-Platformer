using UnityEngine;

[CreateAssetMenu(fileName = "LevelInfo", menuName = "Levels/LevelInfo")]
public class LevelInfo : ScriptableObject
{
    public int Level;

    public bool BossLevel = false;


    [SerializeField]
    private string _levelName;

    public string LevelName
    {
        get => (_levelName != null && _levelName != "") ? _levelName : Level.ToString();
        set  => _levelName = value;
    }

    public string SceneName = ""; 
}
