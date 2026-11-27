using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DuaXe3D.EditorTools
{
    /// <summary>
    /// Công cụ một lần: tạo 6 scene, cấu hình Build Settings, layer, shader
    /// và build bản Windows. Chạy từ menu DuaXe3D hoặc bằng -executeMethod.
    /// </summary>
    public static class ProjectBuilder
    {
        private const string SceneFolder = "Assets/01_Canh";
        private const string ProductName = "Dua Xe Dia Hinh 3D";

        [MenuItem("DuaXe3D/1. Thiết lập dự án (6 Scene)")]
        public static void SetupProject()
        {
            EnsureProject();
            EditorUtility.DisplayDialog("DuaXe3D",
                "Đã tạo 6 scene trong Assets/01_Canh và cấu hình Build Settings.", "OK");
        }

        [MenuItem("DuaXe3D/2. Build Windows")]
        public static void BuildWindowsMenu()
        {
            EnsureProject();
            DoBuild();
        }

        [MenuItem("DuaXe3D/3. Build + Auto Test")]
        public static void BuildAndTestMenu()
        {
            EnsureProject();
            string exe = DoBuild();
            if (!string.IsNullOrEmpty(exe)) RunAutoTest(exe);
        }

        public static void BuildFromCommandLine()
        {
            try
            {
                EnsureProject();
                string exe = DoBuild();
                EditorApplication.Exit(string.IsNullOrEmpty(exe) ? 1 : 0);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError("[DuaXe3D] Build lỗi: " + e);
                EditorApplication.Exit(1);
            }
        }

        public static void SetupFromCommandLine()
        {
            EnsureProject();
            EditorApplication.Exit(0);
        }

        // ------------------------------------------------------------------
        public static void EnsureProject()
        {
            EnsureFolder("Assets/01_Canh");
            EnsureFolder("Assets/02_MaNguon");
            EnsureFolder("Assets/03_Xe");
            EnsureFolder("Assets/04_NhanVat");
            EnsureFolder("Assets/05_DiaHinh");
            EnsureFolder("Assets/06_AmThanh");
            EnsureFolder("Assets/07_VatLieu");
            EnsureFolder("Assets/08_Prefab");
            EnsureFolder("Assets/09_HinhAnh");

            ConfigurePlayer();
            ConfigureLayers();
            ConfigureShaders();

            CreateScene<MainMenuScene>(SceneFlow.MainMenu);
            CreateScene<CarSelectScene>(SceneFlow.CarSelect);
            CreateScene<CharacterSelectScene>(SceneFlow.CharacterSelect);
            CreateScene<TerrainSelectScene>(SceneFlow.TerrainSelect);
            CreateScene<RaceScene>(SceneFlow.Race);
            CreateScene<ResultsScene>(SceneFlow.Results);

            string[] paths = new string[SceneFlow.All.Length];
            for (int i = 0; i < SceneFlow.All.Length; i++)
                paths[i] = ScenePath(SceneFlow.All[i]);

            List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>();
            foreach (string p in paths)
                scenes.Add(new EditorBuildSettingsScene(p, true));
            EditorBuildSettings.scenes = scenes.ToArray();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            UnityEngine.Debug.Log("[DuaXe3D] Đã tạo 6 scene + Build Settings.");
        }

        private static string ScenePath(string sceneName)
        {
            return SceneFolder + "/" + sceneName + ".unity";
        }

        private static void CreateScene<T>(string sceneName) where T : Component
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            GameObject go = new GameObject("Bootstrap");
            go.AddComponent<T>();

            string path = ScenePath(sceneName);
            bool saved = EditorSceneManager.SaveScene(scene, path);
            if (!saved)
                throw new IOException("Không lưu được scene " + path);
        }

        private static void ConfigurePlayer()
        {
            PlayerSettings.companyName = "DO AN GAME";
            PlayerSettings.productName = ProductName;
            PlayerSettings.bundleVersion = "1.0.0";
            PlayerSettings.defaultScreenWidth = 1600;
            PlayerSettings.defaultScreenHeight = 900;
            PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
            PlayerSettings.resizableWindow = true;
            PlayerSettings.runInBackground = true;
            PlayerSettings.colorSpace = ColorSpace.Linear;
            PlayerSettings.usePlayerLog = true;
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Standalone, "com.doangame.duaxe3d");

            SerializedObject so = new SerializedObject(
                AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset")[0]);
            SerializedProperty handler = so.FindProperty("activeInputHandler");
            if (handler != null)
            {
                handler.intValue = 0; // Input Manager (legacy) - các script dùng Input.GetKey
                so.ApplyModifiedProperties();
            }

            EditorSettings.enterPlayModeOptionsEnabled = false;
        }

        private static void ConfigureLayers()
        {
            UnityEngine.Object tagManager = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0];
            SerializedObject so = new SerializedObject(tagManager);
            SerializedProperty layers = so.FindProperty("layers");
            bool hasCar = false;

            for (int i = 8; i < layers.arraySize; i++)
            {
                SerializedProperty p = layers.GetArrayElementAtIndex(i);
                if (p.stringValue == "Car") { hasCar = true; break; }
            }

            if (!hasCar)
            {
                for (int i = 8; i < layers.arraySize; i++)
                {
                    SerializedProperty p = layers.GetArrayElementAtIndex(i);
                    if (string.IsNullOrEmpty(p.stringValue))
                    {
                        p.stringValue = "Car";
                        break;
                    }
                }
                so.ApplyModifiedProperties();
            }
        }

        private static void ConfigureShaders()
        {
            string[] names =
            {
                "Standard", "Skybox/Cubemap", "Skybox/Procedural", "Sprites/Default", "UI/Default",
                "Legacy Shaders/Particles/Additive", "Legacy Shaders/Particles/Alpha Blended",
                "Unlit/Color", "Unlit/Texture"
            };

            UnityEngine.Object graphics = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/GraphicsSettings.asset")[0];
            SerializedObject so = new SerializedObject(graphics);
            SerializedProperty list = so.FindProperty("m_AlwaysIncludedShaders");

            List<Shader> wanted = new List<Shader>();
            foreach (string n in names)
            {
                Shader s = Shader.Find(n);
                if (s != null) wanted.Add(s);
            }

            Shader mountain = AssetDatabase.LoadAssetAtPath<Shader>("Assets/07_VatLieu/Mountain.shader");
            Shader post = AssetDatabase.LoadAssetAtPath<Shader>("Assets/07_VatLieu/PostFX.shader");
            if (mountain != null) wanted.Add(mountain);
            if (post != null) wanted.Add(post);

            for (int i = 0; i < wanted.Count; i++)
            {
                Shader s = wanted[i];
                bool exists = false;
                for (int j = 0; j < list.arraySize; j++)
                {
                    if (list.GetArrayElementAtIndex(j).objectReferenceValue == s)
                    {
                        exists = true;
                        break;
                    }
                }

                if (!exists)
                {
                    list.InsertArrayElementAtIndex(list.arraySize);
                    list.GetArrayElementAtIndex(list.arraySize - 1).objectReferenceValue = s;
                }
            }

            so.ApplyModifiedProperties();
        }

        // ------------------------------------------------------------------
        public static string DoBuild()
        {
            string root = Directory.GetParent(Application.dataPath).FullName;
            string outDir = Path.Combine(root, "Build");
            Directory.CreateDirectory(outDir);
            string exe = Path.Combine(outDir, "DuaXeDiaHinh3D.exe");

            List<string> scenes = new List<string>();
            foreach (string s in SceneFlow.All) scenes.Add(ScenePath(s));

            BuildPlayerOptions options = new BuildPlayerOptions
            {
                scenes = scenes.ToArray(),
                locationPathName = exe,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            UnityEditor.Build.Reporting.BuildReport report = BuildPipeline.BuildPlayer(options);
            UnityEditor.Build.Reporting.BuildSummary summary = report.summary;

            if (summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
            {
                UnityEngine.Debug.LogError("[DuaXe3D] Build thất bại: " + summary.result +
                    " · lỗi " + summary.totalErrors);
                return null;
            }

            UnityEngine.Debug.Log("[DuaXe3D] Build thành công: " + exe +
                " · " + (summary.totalSize / (1024f * 1024f)).ToString("0.0") + " MB");
            return exe;
        }

        public static void RunAutoTest(string exe)
        {
            string root = Directory.GetParent(Application.dataPath).FullName;
            string shots = Path.Combine(root, "Screenshots");
            Directory.CreateDirectory(shots);

            ProcessStartInfo info = new ProcessStartInfo
            {
                FileName = exe,
                Arguments = "-autotest -screenshotdir \"" + shots + "\" -screen-width 1600 -screen-height 900 -screen-fullscreen 0",
                UseShellExecute = false,
                WorkingDirectory = Path.GetDirectoryName(exe)
            };

            UnityEngine.Debug.Log("[DuaXe3D] Chạy auto test...");
            using (Process p = Process.Start(info))
            {
                p.WaitForExit(180000);
            }
            UnityEngine.Debug.Log("[DuaXe3D] Ảnh kiểm thử: " + shots);
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = Path.GetDirectoryName(path).Replace('\\', '/');
            string name = Path.GetFileName(path);
            AssetDatabase.CreateFolder(parent, name);
        }
    }
}
