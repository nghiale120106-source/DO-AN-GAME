using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace DuaXe3D
{
    public static class SceneFlow
    {
        public const string MainMenu = "01_MenuChinh";
        public const string CarSelect = "02_ChonXe";
        public const string CharacterSelect = "03_ChonNhanVat";
        public const string TerrainSelect = "04_ChonDiaHinh";
        public const string Race = "05_DuaXe";
        public const string Results = "06_KetQua";

        public static readonly string[] All =
        {
            MainMenu, CarSelect, CharacterSelect, TerrainSelect, Race, Results
        };

        public static void Load(string sceneName)
        {
            Time.timeScale = 1f;
            ScreenFade.Load(sceneName);
        }

        public static void LoadNextAfterCar() { Load(CharacterSelect); }
        public static void LoadNextAfterCharacter() { Load(TerrainSelect); }
        public static void LoadNextAfterTerrain() { Load(Race); }
    }

    /// <summary>
    /// Lớp phủ đen chuyển cảnh, tồn tại xuyên scene.
    /// </summary>
    public class ScreenFade : MonoBehaviour
    {
        public static ScreenFade Instance { get; private set; }

        private CanvasGroup _group;
        private Image _image;
        private bool _loading;

        public static void Ensure()
        {
            if (Instance != null) return;

            GameObject go = new GameObject("ScreenFade");
            DontDestroyOnLoad(go);
            Instance = go.AddComponent<ScreenFade>();
            Instance.Build();
        }

        private void Build()
        {
            Canvas canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 32000;

            CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            _group = gameObject.AddComponent<CanvasGroup>();
            _group.alpha = 1f;
            _group.blocksRaycasts = true;

            GameObject imgGo = new GameObject("Black");
            imgGo.transform.SetParent(transform, false);
            _image = imgGo.AddComponent<Image>();
            _image.color = Color.black;
            RectTransform rt = _image.rectTransform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        public static void FadeIn(float duration = 0.55f)
        {
            Ensure();
            Instance.StartCoroutine(Instance.FadeRoutine(0f, duration));
        }

        public static void Load(string sceneName)
        {
            Ensure();
            if (Instance._loading) return;
            Instance.StartCoroutine(Instance.LoadRoutine(sceneName));
        }

        private IEnumerator FadeRoutine(float target, float duration)
        {
            _group.blocksRaycasts = target > 0.99f;
            float start = _group.alpha;
            float t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                _group.alpha = Mathf.Lerp(start, target, Mathf.Clamp01(t / duration));
                yield return null;
            }
            _group.alpha = target;
            _group.blocksRaycasts = target > 0.99f;
        }

        private IEnumerator LoadRoutine(string sceneName)
        {
            _loading = true;
            yield return FadeRoutine(1f, 0.35f);
            yield return new WaitForSecondsRealtime(0.08f);
            Time.timeScale = 1f;
            SceneManager.LoadScene(sceneName);
            _loading = false;
        }
    }

    public static class SceneSetup
    {
        private static bool _configured;

        public static void Configure()
        {
            Time.timeScale = 1f;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            Application.targetFrameRate = 120;

            if (!_configured)
            {
                _configured = true;
                QualitySettings.vSyncCount = 1;
                QualitySettings.antiAliasing = 4;
                QualitySettings.shadowDistance = 170f;
                QualitySettings.shadowCascades = 4;
                QualitySettings.shadowResolution = ShadowResolution.High;
                QualitySettings.pixelLightCount = 6;
                QualitySettings.skinWeights = SkinWeights.FourBones;
            }

            UITheme.Init();
            AudioManager.Ensure();
            ScreenFade.Ensure();
            EnsureEventSystem();
            AutoTestRunner.Ensure();
        }

        public static void EnsureEventSystem()
        {
            EventSystem existing = Object.FindFirstObjectByType<EventSystem>();
            if (existing != null) return;

            GameObject go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();
            go.AddComponent<StandaloneInputModule>();
        }

        public static Camera CreateMainCamera(Vector3 position, Quaternion rotation, float fov = 60f)
        {
            GameObject go = new GameObject("Main Camera");
            go.tag = "MainCamera";
            go.transform.SetPositionAndRotation(position, rotation);

            Camera cam = go.AddComponent<Camera>();
            cam.fieldOfView = fov;
            cam.nearClipPlane = 0.15f;
            cam.farClipPlane = 1600f;
            cam.allowHDR = true;
            cam.allowMSAA = true;
            cam.clearFlags = CameraClearFlags.Skybox;
            cam.backgroundColor = new Color(0.03f, 0.05f, 0.08f);

            go.AddComponent<AudioListener>();
            go.AddComponent<CameraPostFX>();
            return cam;
        }

        public static int CarLayer
        {
            get
            {
                int layer = LayerMask.NameToLayer("Car");
                return layer >= 0 ? layer : 0;
            }
        }

        public static int GroundMask
        {
            get
            {
                int car = LayerMask.NameToLayer("Car");
                return car >= 0 ? ~(1 << car) : ~0;
            }
        }

        public static void SetLayerRecursive(GameObject go, int layer)
        {
            go.layer = layer;
            foreach (Transform child in go.transform)
                SetLayerRecursive(child.gameObject, layer);
        }

        public static void RemoveAllColliders(GameObject go)
        {
            foreach (Collider c in go.GetComponentsInChildren<Collider>(true))
                Object.Destroy(c);
        }
    }
}
