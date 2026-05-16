using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Victorina.Infrastructure;
using Victorina.UI;

public static class SceneWirer
{
    [MenuItem("Victorina/Навесить скрипты на сцену")]
    public static void Wire()
    {
        Create<GameBootstrapper>   ("Bootstrapper");
        Create<MainMenuView>       ("MainMenuController");
        Create<TeamSetupView>      ("TeamSetupController");
        Create<RouletteView>       ("GameController");
        Create<ScoreboardView>     ("ScoreboardController");
        Create<AdminPanelView>     ("AdminController");
        Create<QuestionEditorView> ("EditorController");
        Create<GameOverView>       ("GameOverController");

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Debug.Log("[SceneWirer] Готово. Сохрани сцену (Ctrl+S) и назначь ссылки вручную.");
    }

    static void Create<T>(string goName) where T : Component
    {
        var go = new GameObject(goName);
        go.AddComponent<T>();
        Debug.Log($"[SceneWirer] ✓ {goName} ({typeof(T).Name})");
    }
}
