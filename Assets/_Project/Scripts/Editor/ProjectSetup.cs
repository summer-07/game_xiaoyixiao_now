using System.IO;
using SheepStrike.Bootstrap;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SheepStrike.EditorTools
{
    /// <summary>
    /// One-click (and first-open) project setup: creates the Main scene with the composition root,
    /// registers it in Build Settings and applies portrait player settings for mini-game targets.
    /// </summary>
    [InitializeOnLoad]
    public static class ProjectSetup
    {
        public const string ScenePath = "Assets/_Project/Scenes/Main.unity";
        public const string ProductName = "小羊消消";

        static ProjectSetup()
        {
            EditorApplication.delayCall += () =>
            {
                if (!File.Exists(ScenePath)) Run();
            };
        }

        [MenuItem("SheepStrike/Setup Project (Scene + Player Settings)")]
        public static void Run()
        {
            CreateMainScene();
            ApplyPlayerSettings();
            Debug.Log("[SheepStrike] Project setup done. Open " + ScenePath + " and press Play.");
        }

        [MenuItem("SheepStrike/Reset Progress")]
        public static void ResetProgress()
        {
            PlayerPrefs.DeleteAll();
            Debug.Log("[SheepStrike] PlayerPrefs cleared.");
        }

        private static void CreateMainScene()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(ScenePath));
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            new GameObject(nameof(GameBootstrap)).AddComponent<GameBootstrap>();
            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        }

        private static void ApplyPlayerSettings()
        {
            PlayerSettings.productName = ProductName;
            PlayerSettings.companyName = "summer-07";
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.colorSpace = ColorSpace.Gamma;
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
            PlayerSettings.WebGL.dataCaching = false;
            PlayerSettings.WebGL.decompressionFallback = false;
        }
    }
}
