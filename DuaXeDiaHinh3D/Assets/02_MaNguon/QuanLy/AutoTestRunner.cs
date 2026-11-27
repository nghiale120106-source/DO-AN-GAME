using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DuaXe3D
{
    /// <summary>
    /// Chế độ kiểm thử tự động khi chạy player với tham số -autotest.
    /// Chụp ảnh từng màn hình vào thư mục -screenshotdir rồi thoát.
    /// </summary>
    public class AutoTestRunner : MonoBehaviour
    {
        public static bool Enabled { get; private set; }
        public static bool CaptureAll { get; private set; }
        public static bool SceneReady { get; private set; }

        private static bool _parsed;
        private static AutoTestRunner _instance;
        private static string _screenshotDir;
        private static int _overrideTerrain = -1;
        private static int _overrideCar = -1;
        private static bool _raceOnly;
        private static string _raceTag = "";

        /// <summary>Các scene gọi hàm này sau khi dựng xong nội dung runtime.</summary>
        public static void MarkReady()
        {
            SceneReady = true;
        }

        public static void Ensure()
        {
            ParseArgs();
            if (!Enabled || _instance != null) return;

            GameObject go = new GameObject("AutoTestRunner");
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<AutoTestRunner>();
        }

        private static void ParseArgs()
        {
            if (_parsed) return;
            _parsed = true;

            string[] args = System.Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
            {
                string a = args[i].ToLowerInvariant();
                if (a == "-autotest") Enabled = true;
                if (a == "-captureall") { Enabled = true; CaptureAll = true; }
                if (a == "-raceonly") { Enabled = true; _raceOnly = true; }
                if ((a == "-screenshotdir" || a == "-screenshots") && i + 1 < args.Length)
                    _screenshotDir = args[i + 1];
                if (a == "-terrain" && i + 1 < args.Length && int.TryParse(args[i + 1], out int ti))
                    _overrideTerrain = ti;
                if (a == "-car" && i + 1 < args.Length && int.TryParse(args[i + 1], out int ci))
                    _overrideCar = ci;
                if (a == "-tag" && i + 1 < args.Length)
                    _raceTag = args[i + 1];
            }

            if (string.IsNullOrEmpty(_screenshotDir))
                _screenshotDir = Path.Combine(Application.persistentDataPath, "Screenshots");
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);
            Application.runInBackground = true;
            if (!_raceOnly)
            {
                GameSession.SetCar(0);
                GameSession.SetCharacter(0);
                GameSession.SetTerrain(0);
            }
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            if (_instance == this) _instance = null;
        }

        private void Start()
        {
            StartCoroutine(RunScene(SceneManager.GetActiveScene().name));
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            SceneReady = false;
            StartCoroutine(RunScene(scene.name));
        }

        private IEnumerator RunScene(string sceneName)
        {
            // Chờ scene dựng xong nội dung runtime (có timeout an toàn).
            float waited = 0f;
            while (!SceneReady && waited < 25f)
            {
                waited += Time.unscaledDeltaTime;
                yield return null;
            }

            // Chờ hiệu ứng fade-in kết thúc.
            yield return new WaitForSecondsRealtime(1.0f);

            if (_raceOnly)
            {
                if (sceneName == SceneFlow.MainMenu)
                {
                    if (_overrideCar >= 0) GameSession.SetCar(_overrideCar);
                    if (_overrideTerrain >= 0) GameSession.SetTerrain(_overrideTerrain);
                    SceneFlow.Load(SceneFlow.Race);
                    yield break;
                }
                if (sceneName == SceneFlow.Race)
                {
                    yield return new WaitForSecondsRealtime(11.0f);
                    string suffix = string.IsNullOrEmpty(_raceTag) ? "Race" : _raceTag;
                    yield return Capture("05_DuaXe_" + suffix);
                    yield return new WaitForSecondsRealtime(0.4f);
                    Quit();
                    yield break;
                }
            }

            if (sceneName == SceneFlow.MainMenu)
            {
                GameSession.SetCar(0);
                GameSession.SetCharacter(0);
                GameSession.SetTerrain(0);
                yield return new WaitForSecondsRealtime(0.4f);
                yield return Capture("01_MenuChinh");
                yield return new WaitForSecondsRealtime(0.6f);
                SceneFlow.Load(SceneFlow.CarSelect);
                yield break;
            }

            if (sceneName == SceneFlow.CarSelect)
            {
                CarSelectScene cs = FindFirstObjectByType<CarSelectScene>();
                if (cs != null) cs.Select(0);
                yield return new WaitForSecondsRealtime(0.6f);
                yield return Capture("02_ChonXe");
                if (CaptureAll && cs != null)
                {
                    cs.Select(1);
                    yield return new WaitForSecondsRealtime(0.6f);
                    yield return Capture("02_ChonXe_Buggy");
                    cs.Select(2);
                    yield return new WaitForSecondsRealtime(0.6f);
                    yield return Capture("02_ChonXe_Monster");
                    cs.Select(0);
                    yield return new WaitForSecondsRealtime(0.3f);
                }
                SceneFlow.Load(SceneFlow.CharacterSelect);
                yield break;
            }

            if (sceneName == SceneFlow.CharacterSelect)
            {
                CharacterSelectScene ch = FindFirstObjectByType<CharacterSelectScene>();
                if (ch != null) ch.Select(0);
                yield return new WaitForSecondsRealtime(0.6f);
                yield return Capture("03_ChonNhanVat");
                if (CaptureAll && ch != null)
                {
                    ch.Select(1);
                    yield return new WaitForSecondsRealtime(0.6f);
                    yield return Capture("03_ChonNhanVat_2");
                    ch.Select(2);
                    yield return new WaitForSecondsRealtime(0.6f);
                    yield return Capture("03_ChonNhanVat_3");
                    ch.Select(0);
                    yield return new WaitForSecondsRealtime(0.3f);
                }
                SceneFlow.Load(SceneFlow.TerrainSelect);
                yield break;
            }

            if (sceneName == SceneFlow.TerrainSelect)
            {
                TerrainSelectScene ts = FindFirstObjectByType<TerrainSelectScene>();
                if (ts != null) ts.Select(0);
                yield return new WaitForSecondsRealtime(1.0f);
                yield return Capture("04_ChonDiaHinh");
                if (CaptureAll && ts != null)
                {
                    ts.Select(1);
                    yield return new WaitForSecondsRealtime(1.2f);
                    yield return Capture("04_ChonDiaHinh_Rung");
                    ts.Select(2);
                    yield return new WaitForSecondsRealtime(1.2f);
                    yield return Capture("04_ChonDiaHinh_DoiNui");
                    ts.Select(3);
                    yield return new WaitForSecondsRealtime(1.2f);
                    yield return Capture("04_ChonDiaHinh_DuongBun");
                    ts.Select(0);
                    yield return new WaitForSecondsRealtime(0.8f);
                }
                SceneFlow.Load(SceneFlow.Race);
                yield break;
            }

            if (sceneName == SceneFlow.Race)
            {
                yield return new WaitForSecondsRealtime(0.4f);
                yield return Capture("05_DuaXe_Intro");

                yield return new WaitForSecondsRealtime(3.0f);
                yield return Capture("05_DuaXe_Countdown");

                // Chụp sớm (~2.5 giây sau GO) khi các xe còn bám đường, tránh AI chạy lệch ra bãi cỏ.
                yield return new WaitForSecondsRealtime(4.2f);
                yield return Capture("05_DuaXe_DangDua");

                if (CaptureAll && RaceManager.Instance != null)
                {
                    RaceManager.Instance.TogglePause(true);
                    yield return new WaitForSecondsRealtime(0.55f);
                    yield return Capture("05_DuaXe_TamDung");
                    RaceManager.Instance.TogglePause(false);
                }

                yield return new WaitForSecondsRealtime(8.5f);
                yield return Capture("05_DuaXe_VeDich");
                yield break;
            }

            if (sceneName == SceneFlow.Results)
            {
                yield return Capture("06_KetQua");
                yield return new WaitForSecondsRealtime(1.2f);
                Quit();
            }
        }

        private IEnumerator Capture(string name)
        {
            try
            {
                Directory.CreateDirectory(_screenshotDir);
                string path = Path.Combine(_screenshotDir, name + ".png");
                ScreenCapture.CaptureScreenshot(path);
                Debug.Log("[AutoTest] Chụp ảnh: " + path);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("[AutoTest] Không chụp được ảnh: " + e.Message);
            }

            yield return new WaitForEndOfFrame();
            yield return new WaitForSecondsRealtime(0.25f);
        }

        private void Quit()
        {
            Debug.Log("[AutoTest] Hoàn tất.");
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }
}
