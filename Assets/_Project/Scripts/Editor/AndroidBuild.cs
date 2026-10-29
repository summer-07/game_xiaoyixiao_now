using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace SheepStrike.EditorTools
{
    /// <summary>
    /// One-click Android test build, for trying the game on a phone before the mini-game converters are wired.
    /// IL2CPP + ARM64 so it installs on current 64-bit-only phones.
    /// Batch mode can't use the Hub sign-in licence, so builds run in the open editor: from the menu, or by
    /// creating ../build/build-request.txt, which the editor picks up, refreshes, compiles and builds, writing
    /// the outcome to ../build/build-result.txt. That lets a remote session ask for an APK without touching the UI.
    /// </summary>
    [InitializeOnLoad]
    public static class AndroidBuild
    {
        private const string ApplicationId = "com.summer07.sheepstrike";
        private const string OutputPath = "../build/SheepStrike.apk";
        private const string RequestPath = "../build/build-request.txt";
        private const string ResultPath = "../build/build-result.txt";
        private const double PollSeconds = 2.0;

        private static double _nextPoll;

        static AndroidBuild()
        {
            EditorApplication.update += PollForRequest;
        }

        private static void PollForRequest()
        {
            if (EditorApplication.timeSinceStartup < _nextPoll) return;
            _nextPoll = EditorApplication.timeSinceStartup + PollSeconds;
            if (!File.Exists(RequestPath)) return;

            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorApplication.isPlaying = false;
                return;
            }
            if (EditorApplication.isCompiling || EditorApplication.isUpdating) return;

            AssetDatabase.Refresh();
            if (EditorApplication.isCompiling || EditorApplication.isUpdating) return;

            // Build right here: a delayCall can be dropped by a domain reload, which silently lost a request before.
            File.Delete(RequestPath);
            try
            {
                BuildApk();
            }
            catch (System.Exception e)
            {
                File.WriteAllText(ResultPath, $"{System.DateTime.Now:yyyy-MM-dd HH:mm:ss} [SheepStrike] Android build crashed: {e}\n");
            }
        }

        [MenuItem("SheepStrike/Build Android APK")]
        public static void BuildApk()
        {
            ApplyAndroidSettings();
            Directory.CreateDirectory(Path.GetDirectoryName(OutputPath));

            var options = new BuildPlayerOptions
            {
                scenes = new[] { ProjectSetup.ScenePath },
                locationPathName = OutputPath,
                target = BuildTarget.Android,
                options = BuildOptions.None
            };
            var report = BuildPipeline.BuildPlayer(options);
            var summary = report.summary;
            string message = $"[SheepStrike] Android build {summary.result}: {Path.GetFullPath(OutputPath)} ({summary.totalErrors} errors, {summary.totalTime.TotalSeconds:0}s)";
            Debug.Log(message);
            File.WriteAllText(ResultPath, $"{System.DateTime.Now:yyyy-MM-dd HH:mm:ss} {message}\n");

            if (Application.isBatchMode) EditorApplication.Exit(summary.result == BuildResult.Succeeded ? 0 : 1);
        }

        private static void ApplyAndroidSettings()
        {
            EditorUserBuildSettings.buildAppBundle = false;
            PlayerSettings.SetApplicationIdentifier(UnityEditor.Build.NamedBuildTarget.Android, ApplicationId);
            PlayerSettings.SetScriptingBackend(UnityEditor.Build.NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
        }
    }
}
