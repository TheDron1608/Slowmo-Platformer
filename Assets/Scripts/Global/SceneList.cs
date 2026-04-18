using System.Linq;
using UnityEngine.SceneManagement;

public static class SceneList
{
    public const string MAIN_MENU = "MainMenu";
    public const string GAMEPLAY = "Gameplay";
    public const string SHOP = "Shop";
    public const string CURSE = "Curse";
    public const string MODIFICATOR_DEBUG = "ModificatorDebug";
    public const string GAME_FINISHED = "GameFinished";

    private static readonly string[] GAMEPLAY_SCENES = {
        GAMEPLAY,
        GAME_FINISHED
    };

    private static readonly string[] MODIFICATOR_SCENES = { 
        SHOP,
        CURSE,
        MODIFICATOR_DEBUG
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