using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DuaXe3D
{
    /// <summary>Màn hình 02: chọn 1 trong 3 mẫu xe.</summary>
    public class CarSelectScene : MonoBehaviour
    {
        private int _index;
        private GameObject _carRoot;
        private RectTransform _detailContent;
        private RectTransform _canvasRoot;
        private readonly List<UICard> _cards = new List<UICard>();

        private static readonly string[] Steps = { "CHỌN XE", "TAY ĐUA", "ĐỊA HÌNH" };

        private void Start()
        {
            SceneSetup.Configure();
            GameSession.Load();
            _index = GameSession.CarIndex;
            StartCoroutine(BuildRoutine());
        }

        private IEnumerator BuildRoutine()
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlayMenuMusic();

            Camera cam = MenuKit.CreateMenuCamera(
                new Vector3(4.7f, 2.5f, 8.5f),
                new Vector3(-0.15f, 1.0f, 0f),
                36f);
            cam.farClipPlane = 300f;

            MenuKit.BuildStudio(transform, UITheme.Accent);
            RebuildCar();

            BuildUI();
            RefreshAll(false);

            yield return null;
            ScreenFade.FadeIn(0.6f);
            AutoTestRunner.MarkReady();
        }

        private void BuildUI()
        {
            Canvas canvas = MenuKit.CreateCanvas("CarSelectUI");
            RectTransform root = canvas.GetComponent<RectTransform>();
            _canvasRoot = root;
            MenuKit.AddPanelBackdrop(root, 930f);

            MenuKit.AddHeader(root, "BƯỚC 1 / 3 · XE ĐUA", "CHỌN XE",
                "Mỗi mẫu xe có trọng lượng, độ bám đường và tốc độ tối đa khác nhau. " +
                "Hãy chọn chiếc xe hợp với phong cách lái của bạn.",
                Steps, 0, () => SceneFlow.Load(SceneFlow.MainMenu));

            MenuKit.AddDetailPanel(root, new Vector2(64f, 396f), new Vector2(548f, 404f));
            RectTransform detail = UIKit.Rect(root, "DetailContent");
            UIKit.PlaceTopLeft(detail, new Vector2(84f, 414f), new Vector2(508f, 370f));
            _detailContent = detail;

            // Nút điều hướng
            Button continueBtn = UIKit.MakeButton(root, "TIẾP TỤC  →", new Vector2(330f, 74f), true,
                GoNext);
            UIKit.PlaceBottomLeft((RectTransform)continueBtn.transform, new Vector2(64f, 64f), new Vector2(330f, 74f));

            Button raceBtn = UIKit.MakeButton(root, "ĐUA NGAY", new Vector2(190f, 74f), false,
                () => SceneFlow.Load(SceneFlow.Race));
            UIKit.PlaceBottomLeft((RectTransform)raceBtn.transform, new Vector2(410f, 64f), new Vector2(190f, 74f));

            // 3 thẻ xe bên phải
            for (int i = 0; i < CarLibrary.All.Length; i++)
            {
                CarSpec spec = CarLibrary.All[i];
                int idx = i;
                UICard card = MenuKit.AddCard(root, UISprites.CarThumb(spec), spec.Name,
                    spec.Tagline, spec.EngineNote, spec.AccentColor, new Vector2(420f, 206f),
                    () => Select(idx));
                UIKit.PlaceTopRight((RectTransform)card.transform,
                    new Vector2(64f, 214f + i * 222f), new Vector2(420f, 206f));
                _cards.Add(card);
            }

            // Nút mũi tên hai bên khu vực 3D
            Button prev = UIKit.MakeIconButton(root, UISprites.Chevron, new Vector2(54f, 54f),
                () => Select(_index - 1), UITheme.Text, true);
            UIKit.Place((RectTransform)prev.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(-330f, 40f), new Vector2(54f, 54f));

            Button next = UIKit.MakeIconButton(root, UISprites.Chevron, new Vector2(54f, 54f),
                () => Select(_index + 1), UITheme.Text, false);
            UIKit.Place((RectTransform)next.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(330f, 40f), new Vector2(54f, 54f));

            MenuKit.AddFooter(root, "← →  ĐỔI XE     ·     ENTER  TIẾP TỤC     ·     ESC  QUAY LẠI");
        }

        private void RebuildCar()
        {
            if (_carRoot != null) Destroy(_carRoot);
            _carRoot = MenuKit.AddCar(transform, CarLibrary.Get(_index),
                CharacterLibrary.Get(GameSession.CharacterIndex), new Vector3(0f, 0.19f, 0f), 15f);
        }

        public void Select(int index)
        {
            int count = CarLibrary.All.Length;
            int next = ((index % count) + count) % count;
            if (next == _index && _detailContent != null) return;
            _index = next;
            GameSession.SetCar(_index);
            RebuildCar();
            RefreshAll(true);
        }

        private void RefreshAll(bool animated)
        {
            for (int i = 0; i < _cards.Count; i++)
                _cards[i].SetSelected(i == _index);

            if (_detailContent != null) Destroy(_detailContent.gameObject);
            _detailContent = BuildDetail();
        }

        private RectTransform BuildDetail()
        {
            CarSpec spec = CarLibrary.Get(_index);
            RectTransform c = UIKit.Rect(_canvasRoot, "DetailContent");
            UIKit.PlaceTopLeft(c, new Vector2(84f, 414f), new Vector2(508f, 370f));

            Image badge = UIKit.Panel(c, "Badge", new Color(spec.AccentColor.r, spec.AccentColor.g,
                spec.AccentColor.b, 0.16f), 12f, true, 0.35f);
            UIKit.PlaceTopLeft(badge.rectTransform, new Vector2(0f, 0f), new Vector2(176f, 30f));
            Text badgeText = UIKit.Label(badge.rectTransform, "XE ĐANG CHỌN", 14, spec.AccentColor,
                TextAnchor.MiddleCenter, UITheme.Semi);
            UIKit.Fill(badgeText.rectTransform);

            Text name = UIKit.Label(c, spec.Name, 42, UITheme.Text, TextAnchor.UpperLeft, UITheme.Heading);
            UIKit.PlaceTopLeft(name.rectTransform, new Vector2(0f, 42f), new Vector2(508f, 52f));

            Text tagline = UIKit.Label(c, spec.Tagline, 19, UITheme.Muted, TextAnchor.UpperLeft, UITheme.Body);
            UIKit.PlaceTopLeft(tagline.rectTransform, new Vector2(0f, 96f), new Vector2(508f, 30f));

            Text engine = UIKit.Label(c, "⚙  " + spec.EngineNote + "   ·   " +
                Mathf.RoundToInt(spec.MaxSpeedKmh) + " km/h tối đa", 17, spec.AccentColor,
                TextAnchor.UpperLeft, UITheme.Semi);
            UIKit.PlaceTopLeft(engine.rectTransform, new Vector2(0f, 132f), new Vector2(508f, 26f));

            Image divider = UIKit.Image(c, "Divider", new Color(1f, 1f, 1f, 0.09f), UISprites.Pill);
            UIKit.PlaceTopLeft(divider.rectTransform, new Vector2(0f, 172f), new Vector2(508f, 1.5f));

            CreateStat(c, "TỐC ĐỘ", spec.AccelerationStat * 0.55f + spec.MaxSpeedKmh * 0.30f,
                UITheme.Accent, 198f);
            CreateStat(c, "TĂNG TỐC", spec.AccelerationStat, UITheme.Accent2, 246f);
            CreateStat(c, "XỬ LÝ / BÁM ĐƯỜNG", spec.HandlingStat, UITheme.Good, 294f);

            Text weight = UIKit.Label(c, "Trọng lượng " + Mathf.RoundToInt(spec.Mass) + " kg   ·   " +
                "Gầm " + spec.WheelRadius.ToString("0.00") + " m   ·   " +
                "Cơ sở bánh " + spec.WheelBase.ToString("0.00") + " m",
                15, UITheme.Muted, TextAnchor.UpperLeft, UITheme.Body);
            UIKit.PlaceTopLeft(weight.rectTransform, new Vector2(0f, 340f), new Vector2(508f, 24f));

            return c;
        }

        private void CreateStat(RectTransform parent, string label, float value, Color color, float y)
        {
            Text lab = UIKit.Label(parent, label, 16, UITheme.Muted, TextAnchor.UpperLeft, UITheme.Semi);
            UIKit.PlaceTopLeft(lab.rectTransform, new Vector2(0f, y), new Vector2(320f, 22f));

            Text val = UIKit.Label(parent, Mathf.RoundToInt(Mathf.Clamp(value, 0f, 100f)).ToString(),
                17, color, TextAnchor.UpperRight, UITheme.Semi);
            UIKit.PlaceTopLeft(val.rectTransform, new Vector2(438f, y - 1f), new Vector2(70f, 22f));

            Image fill = UIKit.ProgressBar(parent, new Vector2(508f, 8f),
                new Color(1f, 1f, 1f, 0.08f), color);
            UIKit.PlaceTopLeft((RectTransform)fill.transform.parent, new Vector2(0f, y + 26f),
                new Vector2(508f, 8f));
            fill.fillAmount = Mathf.Clamp01(value / 100f);
        }

        private void GoNext()
        {
            GameSession.SetCar(_index);
            SceneFlow.LoadNextAfterCar();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A)) Select(_index - 1);
            if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)) Select(_index + 1);
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) GoNext();
            if (Input.GetKeyDown(KeyCode.Escape)) SceneFlow.Load(SceneFlow.MainMenu);
        }
    }
}
