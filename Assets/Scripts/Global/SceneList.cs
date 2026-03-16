using System.Linq;
using UnityEngine.SceneManagement;

public static class SceneList
{
    public const string MAIN_MENU = "MainMenu";
    public const string GAMEPLAY = "Gameplay";
    public const string MODIFICATOR_CHOISE = "ModificatorChoise";

    private static readonly string[] GAMEPLAY_SCENES = {
        GAMEPLAY
    };

    private static readonly string[] MODIFICATOR_SCENES = { 
        MODIFICATOR_CHOISE
    };

    public static bool GetCurrentSceneIsGameplay()
    {
        return GAMEPLAY_SCENES.Contains(SceneManager.GetActiveScene().name);
    }

    public static bool GetCurrentSceneIsModificatorChoise()
    {
        return GAMEPLAY_SCENES.Contains(SceneManager.GetActiveScene().name);
    }
}