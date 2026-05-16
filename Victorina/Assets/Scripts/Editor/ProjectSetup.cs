using UnityEditor;
using UnityEngine;

public static class ProjectSetup
{
    [MenuItem("Victorina/Настроить проект (API + плагин)")]
    public static void Setup()
    {
        // .NET Framework — нужен для Mono.Data.Sqlite
        var target = EditorUserBuildSettings.activeBuildTarget;
        var group  = BuildPipeline.GetBuildTargetGroup(target);
        PlayerSettings.SetApiCompatibilityLevel(group, ApiCompatibilityLevel.NET_4_6);

        // Input Handling → Both (старый + новый Input System)
        var ps = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset");
        if (ps.Length > 0)
        {
            var so   = new SerializedObject(ps[0]);
            var prop = so.FindProperty("activeInputHandler");
            if (prop != null) { prop.intValue = 2; so.ApplyModifiedProperties(); }
        }

        // Пометить sqlite3.dll как нативный плагин для Windows x64
        var importer = AssetImporter.GetAtPath("Assets/Plugins/x86_64/sqlite3.dll")
                       as PluginImporter;
        if (importer != null)
        {
            importer.SetCompatibleWithAnyPlatform(false);
            importer.SetCompatibleWithEditor(true);
            importer.SetCompatibleWithPlatform(BuildTarget.StandaloneWindows64, true);
            importer.SaveAndReimport();
            Debug.Log("[Victorina] sqlite3.dll настроен.");
        }
        else
        {
            Debug.LogWarning("[Victorina] sqlite3.dll не найден в Assets/Plugins/x86_64/");
        }

        AssetDatabase.SaveAssets();
        Debug.Log("[Victorina] API Compatibility Level → .NET Framework. Готово!");
    }
}
