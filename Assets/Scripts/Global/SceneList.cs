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
    public const string TUTORIAL_1 = "Tutorial1";
    public const string TUTORIAL_2 = "Tutorial2";
    public const string TUTORIAL_3 = "Tutorial3";
    public const string TUTORIAL_CURSE = "TutorialCurse";
    public const string TUTORIAL_BLESS = "TutorialBless";

    private static readonly string[] GAMEPLAY_SCENES = {
        GAMEPLAY,
        GAME_FINISHED,
        TUTORIAL_1,
        TUTORIAL_2,
        TUTORIAL_3
    };

    private static readonly string[] MODIFICATOR_SCENES = { 
        SHOP,
        CURSE,
        MODIFICATOR_DEBUG,
        TUTORIAL_BLESS,
        TUTORIAL_CURSE
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