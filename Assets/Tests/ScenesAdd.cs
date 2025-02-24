using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

public class ScenesAdd: IPrebuildSetup, IPostBuildCleanup
{
    private const string TestSceneFolder = "Assets/Tests/Scenes";
    public void Setup()
    {
        // Load test scene if not already loaded
        AddTestScenesToBuildSettings();
    }

    public void Cleanup()
    {
        // Clean up test scene
        RemoveTestScenesFromBuildSettings();
    }


    public static void AddTestScenesToBuildSettings()
    {
        
#if UNITY_EDITOR
        var scenes = new List<EditorBuildSettingsScene>();
        var guids = AssetDatabase.FindAssets("t:Scene", new[] { TestSceneFolder });
        if (guids != null)
        {
            foreach (string guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (!string.IsNullOrEmpty(path) && File.Exists(path))
                {
                    var scene = new EditorBuildSettingsScene(path, true);
                    scenes.Add(scene);
                }
            }
        }

        Debug.Log("Adding test scenes to build settings:\n" + string.Join("\n", scenes.Select(scene => scene.path)));
        EditorBuildSettings.scenes = EditorBuildSettings.scenes.Union(scenes).ToArray();
#endif
    }

    /// Remove all scenes from the build settings that are in the test scene folder.
    public static void RemoveTestScenesFromBuildSettings()
    {
#if UNITY_EDITOR
        EditorBuildSettings.scenes = EditorBuildSettings.scenes
            .Where(scene => !scene.path.StartsWith(TestSceneFolder)).ToArray();
#endif
    }
}
