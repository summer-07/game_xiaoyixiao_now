using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace SheepStrike.EditorTools
{
    /// <summary>
    /// One-click Android test build, for trying the game on a phone before the mini-game converters are wired.
    /// IL2CPP + ARM64 so it installs on current 64-bit-only phones. Also callable from the command line:
    /// Unity -batchmode -quit -projectPath . -executeMethod SheepStrike.EditorTools.AndroidBuild.BuildApk
    /// </summary>
    public static class AndroidBuild
    {
        private const string ApplicationId = "com.summer07.sheepstrike";
        private const string OutputPath = "../build/SheepStrike.apk";

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
            Debug.Log($"[SheepStrike] Android build {summary.result}: {Path.GetFullPath(OutputPath)} ({summary.totalSize / (1024 * 1024)} MB, {summary.totalErrors} errors)");

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
