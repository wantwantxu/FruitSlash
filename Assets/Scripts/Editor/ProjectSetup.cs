using System;
using System.IO;
using FruitSlash;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace FruitSlash.EditorTools
{
    public static class ProjectSetup
    {
        const string BootPath = "Assets/Scenes/Boot.scene";
        const string MenuPath = "Assets/Scenes/MainMenu.scene";
        const string GamePath = "Assets/Scenes/Game.scene";

        [MenuItem("Fruit Slash/配置工程")]
        public static void Apply()
        {
            Directory.CreateDirectory("Assets/Scenes");
            ConfigurePlayer();
            if (!File.Exists(BootPath)) CreateBoot();
            if (!File.Exists(MenuPath)) CreateMenu();
            if (!File.Exists(GamePath)) CreateGame();
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(BootPath, true),
                new EditorBuildSettingsScene(MenuPath, true),
                new EditorBuildSettingsScene(GamePath, true)
            };
            AssetDatabase.SaveAssets();
            Debug.Log("Fruit Slash project settings applied.");
        }

        [MenuItem("Fruit Slash/导出 Android APK")]
        public static void BuildApkMenu()
        {
            BuildInternal(false, false);
        }

        [MenuItem("Fruit Slash/导出 Android AAB")]
        public static void BuildAabMenu()
        {
            BuildInternal(true, false);
        }

        public static void BuildApkBatch()
        {
            int code = BuildInternal(false, true);
            EditorApplication.Exit(code);
        }

        public static void BuildAabBatch()
        {
            int code = BuildInternal(true, true);
            EditorApplication.Exit(code);
        }

        static int BuildInternal(bool appBundle, bool fromBatch)
        {
            Apply();
            if (!Directory.Exists("Builds")) Directory.CreateDirectory("Builds");
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
            EditorUserBuildSettings.selectedBuildTargetGroup = BuildTargetGroup.Android;
            string player = Path.Combine(Path.GetDirectoryName(EditorApplication.applicationPath), "Data", "PlaybackEngines", "AndroidPlayer");
            string sdk = Path.Combine(player, "SDK");
            string ndk = Path.Combine(player, "NDK");
            if (Directory.Exists(sdk))
                UnityEditor.Android.AndroidExternalToolsSettings.sdkRootPath = sdk;
            if (Directory.Exists(ndk))
                UnityEditor.Android.AndroidExternalToolsSettings.ndkRootPath = ndk;
            string javaHome = Environment.GetEnvironmentVariable("JAVA_HOME");
            if (!string.IsNullOrEmpty(javaHome) && Directory.Exists(javaHome))
                UnityEditor.Android.AndroidExternalToolsSettings.jdkRootPath = javaHome;
            EditorUserBuildSettings.buildAppBundle = appBundle;
            PlayerSettings.SetScriptingBackend(
                UnityEditor.Build.NamedBuildTarget.Android,
                ScriptingImplementation.IL2CPP);
            int architectures = (int)(AndroidArchitecture.ARM64 | AndroidArchitecture.X86_64);
            PlayerSettings.Android.targetArchitectures = (AndroidArchitecture)architectures;
            PlayerSettings.SetArchitecture(UnityEditor.Build.NamedBuildTarget.Android, architectures);
            Debug.Log("Android architectures = " + (int)PlayerSettings.Android.targetArchitectures
                + " named=" + PlayerSettings.GetArchitecture(UnityEditor.Build.NamedBuildTarget.Android)
                + " ndk=" + UnityEditor.Android.AndroidExternalToolsSettings.ndkRootPath);
            string path = appBundle ? "Builds/FruitSlash.aab" : "Builds/FruitSlash.apk";
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { BootPath, MenuPath, GamePath },
                locationPathName = path,
                target = BuildTarget.Android,
                options = BuildOptions.None
            });
            Debug.Log("Android build " + report.summary.result + " -> " + path);
            if (report.summary.result != BuildResult.Succeeded)
            {
                Debug.LogError("Android build failed: " + report.summary.totalErrors + " errors.");
                return 1;
            }

            if (!fromBatch)
                EditorUtility.RevealInFinder(path);
            return 0;
        }

        static void ConfigurePlayer()
        {
            PlayerSettings.companyName = "FruitSlash";
            PlayerSettings.productName = "切西瓜";
            PlayerSettings.bundleVersion = "1.0.0";
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.allowedAutorotateToPortrait = true;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = false;
            PlayerSettings.allowedAutorotateToLandscapeRight = false;
            PlayerSettings.SetApplicationIdentifier(UnityEditor.Build.NamedBuildTarget.Android, "com.fruitslash.game");
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel25;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64 | AndroidArchitecture.X86_64;
            PlayerSettings.SetScriptingBackend(UnityEditor.Build.NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, false);
            PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new[] { UnityEngine.Rendering.GraphicsDeviceType.OpenGLES3 });
            PlayerSettings.Android.bundleVersionCode = 1;
            PlayerSettings.SplashScreen.showUnityLogo = false;
        }

        static void CreateBoot()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            CreateCamera();
            var boot = new GameObject("Boot");
            boot.AddComponent<BootLoader>();
            EditorSceneManager.SaveScene(scene, BootPath);
        }

        static void CreateMenu()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            CreateCamera();
            new GameObject("Menu").AddComponent<MenuController>();
            EditorSceneManager.SaveScene(scene, MenuPath);
        }

        static void CreateGame()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            CreateCamera();
            new GameObject("Game").AddComponent<GameDirector>();
            EditorSceneManager.SaveScene(scene, GamePath);
        }

        static void CreateCamera()
        {
            var go = new GameObject("Main Camera");
            go.tag = "MainCamera";
            var cam = go.AddComponent<Camera>();
            go.AddComponent<AudioListener>();
            cam.orthographic = true;
            cam.orthographicSize = 9f;
            cam.transform.position = new Vector3(0f, 0f, -10f);
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.05f, 0.14f, 0.09f);
        }
    }

    [InitializeOnLoad]
    static class ProjectAutoSetup
    {
        static ProjectAutoSetup()
        {
            EditorApplication.delayCall += () =>
            {
                if (EditorApplication.isPlayingOrWillChangePlaymode) return;
                if (EditorApplication.isCompiling) return;
                if (!File.Exists("Assets/Scenes/Boot.scene"))
                    ProjectSetup.Apply();
            };
        }
    }
}
