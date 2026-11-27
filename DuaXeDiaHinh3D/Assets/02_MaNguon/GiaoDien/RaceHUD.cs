using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DuaXe3D
{
    /// <summary>HUD trong đua: đồng hồ, tốc độ, hạng, bản đồ nhỏ, nitro, kết quả.</summary>
    public class RaceHUD : MonoBehaviour
    {
        public RaceManager Manager;

        private Canvas _canvas;
        private Text _positionText;
        private Text _carNameText;
        private Text _timeText;
        private Text _bestText;
        private Text _speedText;
        private Text _gearText;
        private Text _driftText;
        private Text _offroadText;
        private Text _countdownText;
        private Text _introTitle;
        private Text _introSub;
        private Image _needle;
        private Image _speedRing;
        private Image _nitroFill;
        private Image _nitroGlow;
        private Image[] _lights;
        private RectTransform _lightsRoot;
        private RectTransform _countdownRoot;
        private RectTransform _introRoot;

        private GameObject _finishRoot;
        private CanvasGroup _finishGroup;
        private Text _finishTitle;
        private Text _finishSub;
        private Text _finishPlace;
        private Text _finishTime;
        private Text _finishBest;
        private Text _finishRecord;

        private GameObject _pauseRoot;
        private CanvasGroup _pauseGroup;
        private Text _musicLabel;
        private Text _sfxLabel;

        private RawImage _minimap;
        private Texture2D _mapTex;
        private Color32[] _mapPixels;
        private float _mapTimer;
        private float _countdownPulse;
        private float _finishTarget;
        private float _pauseTarget;
        private int _lastPlace = -1;
        private Color _lastPlaceColor;

        private const int MapSize = 256;

        public static RaceHUD Create(RaceManager manager)
        {
            Canvas canvas = UIKit.CreateCanvas("RaceHUD", 100);
            RaceHUD hud = canvas.gameObject.AddComponent<RaceHUD>();
            hud.Manager = manager;
            hud.Build(canvas);
            return hud;
        }

        private void Build(Canvas canvas)
        {
            _canvas = canvas;
            RectTransform root = canvas.GetComponent<RectTransform>();
            UIKit.AddScreenEdges(root, 220f, 300f);
            UIKit.AddNoise(root, 0.025f);

            BuildPosition(root);
            BuildCarBadge(root);
            BuildTimer(root);
            BuildMinimap(root);
            BuildSpeedometer(root);
            BuildNitro(root);
            BuildControls(root);
            BuildLights(root);
            BuildCountdown(root);
            BuildFinish(root);
            BuildPause(root);

            _mapTex = new Texture2D(MapSize, MapSize, TextureFormat.RGBA32, false);
            _mapTex.name = "minimap";
            _mapTex.wrapMode = TextureWrapMode.Clamp;
            _mapTex.filterMode = FilterMode.Bilinear;
            _mapPixels = new Color32[MapSize * MapSize];
            if (_minimap != null) _minimap.texture = _mapTex;

            // Hiển thị đúng tổng số tay đua và vẽ bản đồ ngay từ màn giới thiệu,
            // tránh khung xám trống trong vài giây đầu.
            int total = Manager != null && Manager.Racers != null ? Manager.Racers.Count : 4;
            if (_positionText != null)
            {
                _positionText.text = "1/" + total;
                _positionText.color = UITheme.Gold;
                _lastPlace = 1;
            }
            RedrawMinimap();
        }

        private void BuildPosition(RectTransform root)
        {
            Image panel = UIKit.Panel(root, "PositionPanel", new Color(0.035f, 0.05f, 0.075f, 0.86f), 22f, true, 0.14f);
            UIKit.PlaceTopLeft(panel.rectTransform, new Vector2(34f, 32f), new Vector2(276f, 122f));

            Image bar = UIKit.Image(panel.transform, "Accent", UITheme.Accent, UISprites.Pill);
            bar.rectTransform.anchorMin = new Vector2(0f, 0f);
            bar.rectTransform.anchorMax = new Vector2(0f, 1f);
            bar.rectTransform.pivot = new Vector2(0f, 0.5f);
            bar.rectTransform.anchoredPosition = new Vector2(14f, 0f);
            bar.rectTransform.sizeDelta = new Vector2(4f, -28f);

            Text label = UIKit.Label(panel.transform, "VỊ TRÍ", 17, UITheme.Muted, TextAnchor.UpperLeft, UITheme.Semi);
            UIKit.PlaceTopLeft(label.rectTransform, new Vector2(34f, 16f), new Vector2(180f, 24f));

            _positionText = UIKit.Label(panel.transform, "1/3", 52, UITheme.Gold, TextAnchor.MiddleLeft, UITheme.Display);
            UIKit.PlaceTopLeft(_positionText.rectTransform, new Vector2(32f, 38f), new Vector2(220f, 74f));

            Text sub = UIKit.Label(panel.transform, "TOP 3", 16, UITheme.Muted, TextAnchor.LowerRight, UITheme.Semi);
            UIKit.PlaceBottomRight(sub.rectTransform, new Vector2(20f, 14f), new Vector2(120f, 22f));
        }

        /// <summary>Nhãn cố định hiển thị tên xe và tay đua đang điều khiển (yêu cầu đề bài).</summary>
        private void BuildCarBadge(RectTransform root)
        {
            Image panel = UIKit.Panel(root, "CarBadge", new Color(0.035f, 0.05f, 0.075f, 0.86f), 16f, true, 0.1f);
            UIKit.PlaceTopLeft(panel.rectTransform, new Vector2(34f, 166f), new Vector2(316f, 48f));

            Image dot = UIKit.Image(panel.transform, "Dot", UITheme.Accent, UISprites.Circle);
            UIKit.Place(dot.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(16f, 0f), new Vector2(10f, 10f));

            _carNameText = UIKit.Label(panel.transform, "JEEP", 16, UITheme.Text, TextAnchor.MiddleLeft, UITheme.Semi);
            UIKit.Place(_carNameText.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(36f, 0f), new Vector2(266f, 30f));
            _carNameText.resizeTextForBestFit = true;
            _carNameText.resizeTextMinSize = 11;
            _carNameText.resizeTextMaxSize = 16;
        }

        public void SetCarName(string carName, string racerName)
        {
            if (_carNameText == null) return;
            _carNameText.text = string.IsNullOrEmpty(racerName)
                ? carName
                : carName + "  ·  " + racerName;
        }

        private void BuildTimer(RectTransform root)
        {
            Image panel = UIKit.Panel(root, "TimerPanel", new Color(0.035f, 0.05f, 0.075f, 0.86f), 22f, true, 0.14f);
            UIKit.Place(panel.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -32f), new Vector2(370f, 116f));

            Text label = UIKit.Label(panel.transform, "THỜI GIAN", 16, UITheme.Muted, TextAnchor.UpperCenter, UITheme.Semi);
            UIKit.Place(label.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -12f), new Vector2(300f, 22f));

            _timeText = UIKit.Label(panel.transform, "00:00.00", 48, UITheme.Text, TextAnchor.MiddleCenter, UITheme.Display);
            UIKit.Place(_timeText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 2f), new Vector2(340f, 58f));

            _bestText = UIKit.Label(panel.transform, "KỶ LỤC  --:--.--", 17, UITheme.Muted, TextAnchor.LowerCenter, UITheme.Semi);
            UIKit.Place(_bestText.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 10f), new Vector2(340f, 22f));
        }

        private void BuildMinimap(RectTransform root)
        {
            Image panel = UIKit.Panel(root, "MinimapPanel", new Color(0.035f, 0.05f, 0.075f, 0.86f), 22f, true, 0.14f);
            UIKit.PlaceTopRight(panel.rectTransform, new Vector2(34f, 32f), new Vector2(276f, 316f));

            Text title = UIKit.Label(panel.transform, "BẢN ĐỒ", 16, UITheme.Muted, TextAnchor.UpperLeft, UITheme.Semi);
            UIKit.PlaceTopLeft(title.rectTransform, new Vector2(20f, 14f), new Vector2(160f, 22f));

            Text sub = UIKit.Label(panel.transform, "TOP 1 · 2 · 3", 14, UITheme.Muted, TextAnchor.UpperRight, UITheme.Semi);
            UIKit.PlaceTopRight(sub.rectTransform, new Vector2(20f, 14f), new Vector2(160f, 22f));

            RectTransform mapRt = UIKit.Rect(panel.transform, "Map");
            UIKit.Place(mapRt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -10f), new Vector2(232f, 232f));

            Image bg = UIKit.Image(mapRt, "Bg", new Color(0.02f, 0.03f, 0.045f, 0.9f), UISprites.Rounded);
            bg.type = Image.Type.Sliced;
            UIKit.Fill(bg.rectTransform);

            RectTransform rawRt = UIKit.Rect(mapRt, "Raw");
            UIKit.Fill(rawRt, 8f, 8f, 8f, 8f);
            _minimap = rawRt.gameObject.AddComponent<RawImage>();
            _minimap.raycastTarget = false;
            _minimap.color = Color.white;

            Image border = UIKit.Image(mapRt, "Border", new Color(1f, 1f, 1f, 0.1f), UISprites.Border);
            border.type = Image.Type.Sliced;
            UIKit.Fill(border.rectTransform);
        }

        private void BuildSpeedometer(RectTransform root)
        {
            Image panel = UIKit.Panel(root, "SpeedPanel", new Color(0.03f, 0.04f, 0.06f, 0.8f), 24f, true, 0.12f);
            UIKit.PlaceBottomLeft(panel.rectTransform, new Vector2(34f, 34f), new Vector2(340f, 340f));

            RectTransform gauge = UIKit.Rect(panel.transform, "Gauge");
            UIKit.Place(gauge, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 8f), new Vector2(306f, 306f));

            Image glow = UIKit.Image(gauge, "Glow", new Color(UITheme.Accent.r, UITheme.Accent.g, UITheme.Accent.b, 0.12f), UISprites.SoftGlow);
            UIKit.Fill(glow.rectTransform, -24f, -24f, -24f, -24f);

            Image bg = UIKit.Image(gauge, "Bg", new Color(0.02f, 0.028f, 0.04f, 0.92f), UISprites.Circle);
            UIKit.Fill(bg.rectTransform);

            _speedRing = UIKit.Image(gauge, "Ring", new Color(1f, 1f, 1f, 0.22f), UISprites.Ring);
            UIKit.Fill(_speedRing.rectTransform);

            Image ticks = UIKit.Image(gauge, "Ticks", new Color(1f, 1f, 1f, 0.72f), UISprites.GaugeTicks);
            UIKit.Fill(ticks.rectTransform);

            Image accentArc = UIKit.Image(gauge, "AccentArc", new Color(UITheme.Accent.r, UITheme.Accent.g, UITheme.Accent.b, 0.55f), UISprites.Ring);
            accentArc.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            accentArc.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            accentArc.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            accentArc.rectTransform.anchoredPosition = Vector2.zero;
            accentArc.rectTransform.sizeDelta = new Vector2(286f, 286f);
            accentArc.rectTransform.localEulerAngles = new Vector3(0f, 0f, 135f);

            RectTransform pivot = UIKit.Rect(gauge, "NeedlePivot");
            UIKit.Place(pivot, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 0f), new Vector2(0f, 0f));
            pivot.localEulerAngles = new Vector3(0f, 0f, 135f);

            _needle = UIKit.Image(pivot, "Needle", UITheme.Accent, UISprites.Pill);
            _needle.rectTransform.anchorMin = new Vector2(0.5f, 0f);
            _needle.rectTransform.anchorMax = new Vector2(0.5f, 0f);
            _needle.rectTransform.pivot = new Vector2(0.5f, 0.02f);
            _needle.rectTransform.anchoredPosition = new Vector2(0f, 8f);
            _needle.rectTransform.sizeDelta = new Vector2(5f, 116f);

            Image cap = UIKit.Image(gauge, "Cap", UITheme.Panel3, UISprites.Circle);
            cap.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            cap.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            cap.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            cap.rectTransform.anchoredPosition = Vector2.zero;
            cap.rectTransform.sizeDelta = new Vector2(34f, 34f);

            Image capDot = UIKit.Image(cap.rectTransform, "Dot", UITheme.Accent, UISprites.Circle);
            UIKit.Fill(capDot.rectTransform, 8f, 8f, 8f, 8f);

            _speedText = UIKit.Label(gauge, "0", 68, UITheme.Text, TextAnchor.MiddleCenter, UITheme.Display);
            UIKit.Place(_speedText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -58f), new Vector2(220f, 80f));

            Text unit = UIKit.Label(gauge, "KM/H", 16, UITheme.Muted, TextAnchor.MiddleCenter, UITheme.Semi);
            UIKit.Place(unit.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -102f), new Vector2(140f, 22f));

            Image gearBg = UIKit.Panel(panel.transform, "GearBg", new Color(0.08f, 0.12f, 0.18f, 0.92f), 12f, true, 0.16f);
            UIKit.PlaceTopRight(gearBg.rectTransform, new Vector2(16f, 16f), new Vector2(44f, 40f));
            _gearText = UIKit.Label(gearBg.rectTransform, "N", 24, UITheme.Accent2, TextAnchor.MiddleCenter, UITheme.Display);
            UIKit.Fill(_gearText.rectTransform);

            _driftText = UIKit.Label(root, "DRIFT!", 30, UITheme.Accent, TextAnchor.MiddleCenter, UITheme.Display);
            UIKit.Place(_driftText.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 146f), new Vector2(240f, 38f));
            _driftText.gameObject.SetActive(false);

            _offroadText = UIKit.Label(root, "OFF-ROAD", 19, UITheme.Gold, TextAnchor.MiddleCenter, UITheme.Semi);
            UIKit.Place(_offroadText.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 184f), new Vector2(240f, 26f));
            _offroadText.gameObject.SetActive(false);
        }

        private void BuildNitro(RectTransform root)
        {
            Image panel = UIKit.Panel(root, "NitroPanel", new Color(0.035f, 0.05f, 0.075f, 0.86f), 20f, true, 0.14f);
            UIKit.Place(panel.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 34f), new Vector2(500f, 96f));

            _nitroGlow = UIKit.Image(panel.transform, "Glow", new Color(UITheme.Accent2.r, UITheme.Accent2.g, UITheme.Accent2.b, 0.1f), UISprites.SoftGlow);
            UIKit.Fill(_nitroGlow.rectTransform, -18f, -18f, -18f, -18f);

            Text label = UIKit.Label(panel.transform, "NITRO", 20, UITheme.Accent2, TextAnchor.MiddleLeft, UITheme.Heading);
            UIKit.PlaceTopLeft(label.rectTransform, new Vector2(22f, 14f), new Vector2(160f, 26f));

            Text hint = UIKit.Label(panel.transform, "SHIFT", 14, UITheme.Muted, TextAnchor.MiddleRight, UITheme.Semi);
            UIKit.PlaceTopRight(hint.rectTransform, new Vector2(22f, 15f), new Vector2(120f, 24f));

            Image barBg = UIKit.Panel(panel.transform, "BarBg", new Color(0f, 0f, 0f, 0.55f), 12f, false);
            UIKit.Place(barBg.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 16f), new Vector2(452f, 26f));
            barBg.sprite = UISprites.Pill;
            barBg.type = Image.Type.Sliced;
            Mask barMask = barBg.gameObject.AddComponent<Mask>();
            barMask.showMaskGraphic = true;

            _nitroFill = UIKit.Image(barBg.transform, "Fill", UITheme.Accent2, null);
            _nitroFill.type = Image.Type.Filled;
            _nitroFill.fillMethod = Image.FillMethod.Horizontal;
            _nitroFill.fillOrigin = 0;
            _nitroFill.fillAmount = 1f;
            UIKit.Fill(_nitroFill.rectTransform, 0f, 0f, 0f, 0f);

            Image shine = UIKit.Image(barBg.transform, "Shine", new Color(1f, 1f, 1f, 0.18f), null);
            shine.rectTransform.anchorMin = new Vector2(0f, 0.52f);
            shine.rectTransform.anchorMax = new Vector2(1f, 1f);
            shine.rectTransform.offsetMin = new Vector2(0f, 0f);
            shine.rectTransform.offsetMax = new Vector2(0f, 0f);
        }

        private void BuildControls(RectTransform root)
        {
            Image panel = UIKit.Panel(root, "ControlsPanel", new Color(0.035f, 0.05f, 0.075f, 0.8f), 20f, true, 0.1f);
            UIKit.PlaceBottomRight(panel.rectTransform, new Vector2(34f, 34f), new Vector2(430f, 156f));

            Text title = UIKit.Label(panel.transform, "ĐIỀU KHIỂN", 15, UITheme.Muted, TextAnchor.UpperLeft, UITheme.Semi);
            UIKit.PlaceTopLeft(title.rectTransform, new Vector2(20f, 14f), new Vector2(200f, 22f));

            string[] lines =
            {
                "<color=#FFB020>W / S</color>  Ga · Phanh",
                "<color=#FFB020>A / D</color>  Lái trái · phải",
                "<color=#22D3EE>SPACE</color>  Drift   <color=#22D3EE>SHIFT</color>  Nitro",
                "<color=#8EA2BA>R</color>  Hồi xe   <color=#8EA2BA>ESC</color>  Tạm dừng"
            };
            for (int i = 0; i < lines.Length; i++)
            {
                Text line = UIKit.Label(panel.transform, lines[i], 17, UITheme.Text, TextAnchor.MiddleLeft, UITheme.Semi);
                UIKit.PlaceTopLeft(line.rectTransform, new Vector2(20f, 42f + i * 27f), new Vector2(400f, 24f));
            }
        }

        private void BuildLights(RectTransform root)
        {
            RectTransform holder = UIKit.Rect(root, "StartLights");
            UIKit.Place(holder, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -176f), new Vector2(250f, 62f));
            _lightsRoot = holder;
            _lights = new Image[3];
            for (int i = 0; i < 3; i++)
            {
                Image light = UIKit.Image(holder, "Light" + i, new Color(0.16f, 0.04f, 0.04f, 0.9f), UISprites.Circle);
                light.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                light.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                light.rectTransform.pivot = new Vector2(0.5f, 0.5f);
                light.rectTransform.anchoredPosition = new Vector2((i - 1) * 66f, 0f);
                light.rectTransform.sizeDelta = new Vector2(48f, 48f);
                Image border = UIKit.Image(light.rectTransform, "Border", new Color(1f, 1f, 1f, 0.16f), UISprites.Ring);
                UIKit.Fill(border.rectTransform);
                _lights[i] = light;
            }
            _lightsRoot.gameObject.SetActive(false);
        }

        private void BuildCountdown(RectTransform root)
        {
            RectTransform holder = UIKit.Rect(root, "Countdown");
            UIKit.Place(holder, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 40f), new Vector2(900f, 320f));
            _countdownRoot = holder;

            Image glow = UIKit.Image(holder, "Glow", new Color(UITheme.Accent.r, UITheme.Accent.g, UITheme.Accent.b, 0.16f), UISprites.SoftGlow);
            UIKit.Place(glow.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(760f, 460f));

            _countdownText = UIKit.Label(holder, "", 190, UITheme.Text, TextAnchor.MiddleCenter, UITheme.Display);
            UIKit.Fill(_countdownText.rectTransform);
            Outline outline = _countdownText.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0f, 0f, 0.75f);
            outline.effectDistance = new Vector2(5f, -5f);

            _introRoot = UIKit.Rect(root, "Intro");
            UIKit.Place(_introRoot, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -262f), new Vector2(900f, 90f));
            _introTitle = UIKit.Label(_introRoot, "", 34, UITheme.Text, TextAnchor.MiddleCenter, UITheme.Heading);
            UIKit.Place(_introTitle.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), Vector2.zero, new Vector2(900f, 44f));
            _introSub = UIKit.Label(_introRoot, "", 19, UITheme.Muted, TextAnchor.MiddleCenter, UITheme.Semi);
            UIKit.Place(_introSub.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), Vector2.zero, new Vector2(900f, 28f));
            _introRoot.gameObject.SetActive(false);
            _countdownRoot.gameObject.SetActive(false);
        }

        private void BuildFinish(RectTransform root)
        {
            _finishRoot = new GameObject("FinishPanel");
            RectTransform rt = _finishRoot.AddComponent<RectTransform>();
            rt.SetParent(root, false);
            UIKit.Fill(rt);
            _finishGroup = _finishRoot.AddComponent<CanvasGroup>();
            _finishGroup.alpha = 0f;
            _finishGroup.blocksRaycasts = false;
            _finishGroup.interactable = false;

            Image dim = UIKit.Image(rt, "Dim", new Color(0.01f, 0.015f, 0.025f, 0.82f), null);
            UIKit.Fill(dim.rectTransform);

            Image panel = UIKit.Panel(rt, "Card", new Color(0.045f, 0.06f, 0.09f, 0.98f), 28f, true, 0.18f);
            UIKit.Place(panel.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(860f, 600f));

            Image topGlow = UIKit.Image(panel.transform, "TopGlow", new Color(UITheme.Accent.r, UITheme.Accent.g, UITheme.Accent.b, 0.18f), UISprites.SoftGlow);
            UIKit.Place(topGlow.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, 20f), new Vector2(700f, 280f));

            Image icon = UIKit.Image(panel.transform, "Icon", UITheme.Gold, UISprites.Star);
            UIKit.Place(icon.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -46f), new Vector2(96f, 96f));

            _finishTitle = UIKit.Label(panel.transform, "Bạn đã chiến thắng", 58, UITheme.Text, TextAnchor.MiddleCenter, UITheme.Heading);
            UIKit.Place(_finishTitle.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -148f), new Vector2(820f, 76f));

            _finishSub = UIKit.Label(panel.transform, "HOÀN THÀNH ĐƯỜNG ĐUA", 18, UITheme.Muted, TextAnchor.MiddleCenter, UITheme.Semi);
            UIKit.Place(_finishSub.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -216f), new Vector2(820f, 26f));

            _finishPlace = UIKit.Label(panel.transform, "HẠNG 1", 40, UITheme.Gold, TextAnchor.MiddleCenter, UITheme.Display);
            UIKit.Place(_finishPlace.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -256f), new Vector2(400f, 54f));

            Image statPanel = UIKit.Panel(panel.transform, "Stats", new Color(0f, 0f, 0f, 0.28f), 18f, false);
            UIKit.Place(statPanel.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -334f), new Vector2(660f, 116f));

            Text timeLabel = UIKit.Label(statPanel.transform, "THỜI GIAN", 15, UITheme.Muted, TextAnchor.UpperLeft, UITheme.Semi);
            UIKit.PlaceTopLeft(timeLabel.rectTransform, new Vector2(24f, 14f), new Vector2(280f, 22f));
            _finishTime = UIKit.Label(statPanel.transform, "00:00.00", 42, UITheme.Text, TextAnchor.MiddleLeft, UITheme.Display);
            UIKit.PlaceTopLeft(_finishTime.rectTransform, new Vector2(22f, 36f), new Vector2(300f, 56f));

            Text bestLabel = UIKit.Label(statPanel.transform, "KỶ LỤC", 15, UITheme.Muted, TextAnchor.UpperRight, UITheme.Semi);
            UIKit.PlaceTopRight(bestLabel.rectTransform, new Vector2(24f, 14f), new Vector2(280f, 22f));
            _finishBest = UIKit.Label(statPanel.transform, "--:--.--", 42, UITheme.Gold, TextAnchor.MiddleRight, UITheme.Display);
            UIKit.PlaceTopRight(_finishBest.rectTransform, new Vector2(22f, 36f), new Vector2(300f, 56f));

            _finishRecord = UIKit.Label(panel.transform, "KỶ LỤC MỚI!", 20, UITheme.Good, TextAnchor.MiddleCenter, UITheme.Heading);
            UIKit.Place(_finishRecord.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -432f), new Vector2(400f, 30f));

            Button continueBtn = UIKit.MakeButton(panel.transform, "XEM KẾT QUẢ", new Vector2(360f, 66f), true, () =>
            {
                if (Manager != null) Manager.GoToResults();
            });
            UIKit.Place(continueBtn.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 54f), new Vector2(360f, 66f));

            Text auto = UIKit.Label(panel.transform, "Tự động chuyển sau vài giây...", 15, UITheme.Muted, TextAnchor.MiddleCenter, UITheme.Body);
            UIKit.Place(auto.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 24f), new Vector2(420f, 22f));

            _finishRoot.SetActive(false);
        }

        private void BuildPause(RectTransform root)
        {
            _pauseRoot = new GameObject("PausePanel");
            RectTransform rt = _pauseRoot.AddComponent<RectTransform>();
            rt.SetParent(root, false);
            UIKit.Fill(rt);
            _pauseGroup = _pauseRoot.AddComponent<CanvasGroup>();
            _pauseGroup.alpha = 0f;
            _pauseGroup.blocksRaycasts = false;

            Image dim = UIKit.Image(rt, "Dim", new Color(0.01f, 0.015f, 0.025f, 0.8f), null);
            UIKit.Fill(dim.rectTransform);

            Image panel = UIKit.Panel(rt, "Card", new Color(0.045f, 0.06f, 0.09f, 0.98f), 28f, true, 0.18f);
            UIKit.Place(panel.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(620f, 640f));

            Text title = UIKit.Label(panel.transform, "TẠM DỪNG", 52, UITheme.Text, TextAnchor.MiddleCenter, UITheme.Display);
            UIKit.Place(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -36f), new Vector2(500f, 70f));

            Text sub = UIKit.Label(panel.transform, "ĐƯỜNG ĐUA ĐANG CHỜ BẠN", 17, UITheme.Muted, TextAnchor.MiddleCenter, UITheme.Semi);
            UIKit.Place(sub.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -104f), new Vector2(500f, 26f));

            Button resume = UIKit.MakeButton(panel.transform, "TIẾP TỤC ĐUA", new Vector2(440f, 64f), true, () =>
            {
                if (Manager != null) Manager.TogglePause(false);
            });
            UIKit.Place(resume.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -160f), new Vector2(440f, 64f));

            Button restart = UIKit.MakeButton(panel.transform, "ĐUA LẠI", new Vector2(440f, 58f), false, () =>
            {
                if (Manager != null) Manager.RestartRace();
            });
            UIKit.Place(restart.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -238f), new Vector2(440f, 58f));

            Button terrain = UIKit.MakeButton(panel.transform, "ĐỔI ĐỊA HÌNH", new Vector2(440f, 58f), false, () =>
            {
                SceneFlow.Load(SceneFlow.TerrainSelect);
            });
            UIKit.Place(terrain.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -306f), new Vector2(440f, 58f));

            Button menu = UIKit.MakeButton(panel.transform, "MENU CHÍNH", new Vector2(440f, 58f), false, () =>
            {
                SceneFlow.Load(SceneFlow.MainMenu);
            });
            UIKit.Place(menu.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -374f), new Vector2(440f, 58f));

            Button music = UIKit.MakeButton(panel.transform, "", new Vector2(210f, 52f), false, () =>
            {
                AudioManager mgr = AudioManager.Instance;
                if (mgr != null) mgr.SetMusicEnabled(!GameSession.MusicOn);
                RefreshAudioLabels();
            });
            UIKit.Place(music.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(-114f, -454f), new Vector2(210f, 52f));
            _musicLabel = music.GetComponentInChildren<Text>();
            if (_musicLabel != null) _musicLabel.fontSize = 19;

            Button sfx = UIKit.MakeButton(panel.transform, "", new Vector2(210f, 52f), false, () =>
            {
                GameSession.SfxOn = !GameSession.SfxOn;
                RefreshAudioLabels();
            });
            UIKit.Place(sfx.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(114f, -454f), new Vector2(210f, 52f));
            _sfxLabel = sfx.GetComponentInChildren<Text>();
            if (_sfxLabel != null) _sfxLabel.fontSize = 19;

            Text hint = UIKit.Label(panel.transform, "Nhấn ESC để tiếp tục", 15, UITheme.Muted, TextAnchor.MiddleCenter, UITheme.Body);
            UIKit.Place(hint.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 28f), new Vector2(400f, 22f));

            _pauseRoot.SetActive(false);
            RefreshAudioLabels();
        }

        private void RefreshAudioLabels()
        {
            if (_musicLabel != null) _musicLabel.text = GameSession.MusicOn ? "NHẠC: BẬT" : "NHẠC: TẮT";
            if (_sfxLabel != null) _sfxLabel.text = GameSession.SfxOn ? "ÂM THANH: BẬT" : "ÂM THANH: TẮT";
        }

        public void ShowIntro(string themeName, string carName)
        {
            if (_introRoot == null) return;
            _introRoot.gameObject.SetActive(true);
            _introTitle.text = "SẴN SÀNG";
            _introSub.text = themeName + "  ·  " + carName;
        }

        public void HideIntro()
        {
            if (_introRoot != null) _introRoot.gameObject.SetActive(false);
        }

        public void ShowCountdown(string text, Color color, int stage)
        {
            if (_countdownRoot == null) return;
            if (_lightsRoot != null) _lightsRoot.gameObject.SetActive(true);
            _countdownRoot.gameObject.SetActive(true);
            _countdownText.text = text;
            _countdownText.color = color;
            _countdownPulse = 1f;
            SetLights(stage);
        }

        public void HideCountdown()
        {
            if (_countdownRoot != null) _countdownRoot.gameObject.SetActive(false);
            if (_lightsRoot != null) _lightsRoot.gameObject.SetActive(false);
            SetLights(-1);
        }

        private void SetLights(int stage)
        {
            if (_lights == null) return;
            for (int i = 0; i < _lights.Length; i++)
            {
                Color c = new Color(0.16f, 0.04f, 0.04f, 0.9f);
                if (stage == 0) c = new Color(0.18f, 0.85f, 0.42f, 1f);
                else if (stage > 0 && i < 4 - stage) c = new Color(1f, 0.2f, 0.14f, 1f);
                _lights[i].color = c;
            }
        }

        public void ShowFinish(bool win, int place, float time, float best, bool newRecord)
        {
            if (_finishRoot == null) return;
            _finishRoot.SetActive(true);
            _finishGroup.blocksRaycasts = true;
            _finishGroup.interactable = true;
            _finishTarget = 1f;
            _finishTitle.text = win ? "Bạn đã chiến thắng" : "Bạn đã về đích!";
            _finishTitle.color = win ? UITheme.Gold : UITheme.Text;
            _finishSub.text = win ? "TUYỆT VỜI! BẠN LÀ NHÀ VÔ ĐỊCH" : "CỐ GẮNG HƠN Ở LẦN SAU NHÉ";
            _finishPlace.text = "HẠNG " + place;
            _finishPlace.color = place == 1 ? UITheme.Gold : (place == 2 ? UITheme.Silver : UITheme.Bronze);

            // Đồng bộ luôn chỉ số vị trí trên HUD để không bị lệch với bảng kết quả.
            if (_positionText != null)
            {
                int total = Manager != null && Manager.Racers != null ? Manager.Racers.Count : 4;
                _lastPlace = place;
                _lastPlaceColor = place == 1 ? UITheme.Gold : (place == 2 ? UITheme.Silver : (place == 3 ? UITheme.Bronze : UITheme.Text));
                _positionText.text = place + "/" + total;
                _positionText.color = _lastPlaceColor;
            }

            _finishTime.text = UIKit.FormatTime(time);
            _finishBest.text = best > 0f ? UIKit.FormatTime(best) : "--:--.--";
            _finishRecord.gameObject.SetActive(newRecord);
        }

        public void ShowPause(bool show)
        {
            if (_pauseRoot == null) return;
            if (show) _pauseRoot.SetActive(true);
            _pauseTarget = show ? 1f : 0f;
            _pauseGroup.blocksRaycasts = show;
            _pauseGroup.interactable = show;
            RefreshAudioLabels();
        }

        public void SetBest(float best)
        {
            if (_bestText != null)
                _bestText.text = best > 0f ? "KỶ LỤC  " + UIKit.FormatTime(best) : "KỶ LỤC  --:--.--";
        }

        public void UpdateHud(float time, float best, float speedKmh, float nitro01,
            int place, int total, bool boosting, bool drifting, bool offroad, float throttle)
        {
            if (_timeText != null) _timeText.text = UIKit.FormatTime(time);
            if (_speedText != null) _speedText.text = Mathf.RoundToInt(Mathf.Max(0f, speedKmh)).ToString("0");
            if (_needle != null)
            {
                float t = Mathf.Clamp01(speedKmh / 200f);
                _needle.rectTransform.parent.localEulerAngles = new Vector3(0f, 0f, Mathf.Lerp(135f, -135f, t));
            }
            if (_gearText != null)
            {
                if (speedKmh < 1f && Mathf.Abs(throttle) < 0.05f) _gearText.text = "N";
                else if (throttle < -0.1f && speedKmh < 3f) _gearText.text = "R";
                else _gearText.text = "D";
                _gearText.color = boosting ? UITheme.Accent : UITheme.Accent2;
            }
            if (_nitroFill != null)
            {
                _nitroFill.fillAmount = Mathf.Clamp01(nitro01);
                _nitroFill.color = boosting ? UITheme.Accent : UITheme.Accent2;
            }
            if (_nitroGlow != null)
            {
                Color c = UITheme.Accent2;
                c.a = boosting ? 0.34f : 0.1f;
                _nitroGlow.color = Color.Lerp(_nitroGlow.color, c, 1f - Mathf.Exp(-10f * Time.unscaledDeltaTime));
            }
            if (_speedRing != null)
                _speedRing.color = boosting ? new Color(0.25f, 0.9f, 1f, 0.65f) : new Color(1f, 1f, 1f, 0.22f);
            if (_driftText != null) _driftText.gameObject.SetActive(drifting);
            if (_offroadText != null) _offroadText.gameObject.SetActive(offroad);

            if (_positionText != null && place != _lastPlace)
            {
                _lastPlace = place;
                _positionText.text = place + "/" + total;
                _lastPlaceColor = place == 1 ? UITheme.Gold : (place == 2 ? UITheme.Silver : (place == 3 ? UITheme.Bronze : UITheme.Text));
                _positionText.color = _lastPlaceColor;
            }
            if (_bestText != null)
                _bestText.text = best > 0f ? "KỶ LỤC  " + UIKit.FormatTime(best) : "KỶ LỤC  --:--.--";

            _mapTimer -= Time.unscaledDeltaTime;
            if (_mapTimer <= 0f)
            {
                _mapTimer = 0.1f;
                RedrawMinimap();
            }
        }

        private void Update()
        {
            float dt = Time.unscaledDeltaTime;
            if (_countdownRoot != null && _countdownRoot.gameObject.activeSelf)
            {
                _countdownPulse = Mathf.Max(0f, _countdownPulse - dt * 1.5f);
                float s = 0.78f + Mathf.Sin(Mathf.Clamp01(_countdownPulse) * Mathf.PI * 0.5f) * 0.35f;
                _countdownText.rectTransform.localScale = Vector3.one * s;
            }

            if (_finishGroup != null)
                _finishGroup.alpha = Mathf.Lerp(_finishGroup.alpha, _finishTarget, 1f - Mathf.Exp(-10f * dt));
            if (_finishRoot != null && _finishRoot.activeSelf && _finishGroup.alpha < 0.01f && _finishTarget <= 0f)
                _finishRoot.SetActive(false);

            if (_pauseGroup != null)
            {
                _pauseGroup.alpha = Mathf.Lerp(_pauseGroup.alpha, _pauseTarget, 1f - Mathf.Exp(-12f * dt));
                if (_pauseRoot != null && _pauseRoot.activeSelf && _pauseGroup.alpha < 0.01f && _pauseTarget <= 0f)
                    _pauseRoot.SetActive(false);
            }
        }

        private void RedrawMinimap()
        {
            if (_mapTex == null || Manager == null || Manager.Path == null) return;
            RoadPath path = Manager.Path;

            Color32 bg = new Color32(7, 11, 17, 255);
            for (int i = 0; i < _mapPixels.Length; i++) _mapPixels[i] = bg;

            float minX = float.MaxValue, maxX = float.MinValue, minZ = float.MaxValue, maxZ = float.MinValue;
            for (int i = 0; i < path.Count; i++)
            {
                Vector3 p = path.Points[i];
                if (p.x < minX) minX = p.x;
                if (p.x > maxX) maxX = p.x;
                if (p.z < minZ) minZ = p.z;
                if (p.z > maxZ) maxZ = p.z;
            }
            float pad = 18f;
            float spanX = Mathf.Max(1f, maxX - minX);
            float spanZ = Mathf.Max(1f, maxZ - minZ);
            float span = Mathf.Max(spanX, spanZ);
            float offX = (span - spanX) * 0.5f;
            float offZ = (span - spanZ) * 0.5f;
            float scale = (MapSize - pad * 2f) / span;

            for (int i = 0; i < path.Count - 1; i++)
            {
                Vector2 a = MapPoint(path.Points[i], minX, minZ, offX, offZ, scale, pad);
                Vector2 b = MapPoint(path.Points[i + 1], minX, minZ, offX, offZ, scale, pad);
                DrawLine(a, b, new Color32(54, 66, 84, 255), 7f);
            }
            for (int i = 0; i < path.Count - 1; i++)
            {
                Vector2 a = MapPoint(path.Points[i], minX, minZ, offX, offZ, scale, pad);
                Vector2 b = MapPoint(path.Points[i + 1], minX, minZ, offX, offZ, scale, pad);
                DrawLine(a, b, new Color32(150, 170, 196, 255), 3.2f);
            }

            // Xuất phát / đích
            Vector2 sp = MapPoint(path.GetPointAtDistance(5.5f), minX, minZ, offX, offZ, scale, pad);
            DrawDot(sp, new Color32(53, 227, 155, 255), 5f);
            Vector2 fp = MapPoint(path.GetPointAtDistance(Mathf.Max(10f, path.Length - 7f)), minX, minZ, offX, offZ, scale, pad);
            DrawDot(fp, new Color32(255, 106, 26, 255), 6f);

            if (Manager.Racers != null)
            {
                for (int i = 0; i < Manager.Racers.Count; i++)
                {
                    RacerInfo r = Manager.Racers[i];
                    if (r == null || r.IsPlayer) continue;
                    Vector2 p = MapPoint(r.transform.position, minX, minZ, offX, offZ, scale, pad);
                    DrawDot(p, new Color32(235, 240, 250, 255), 4.5f);
                }
                if (Manager.Player != null)
                {
                    Vector2 p = MapPoint(Manager.Player.transform.position, minX, minZ, offX, offZ, scale, pad);
                    DrawDot(p, new Color32(255, 176, 32, 255), 7.5f);
                    DrawDot(p, new Color32(255, 255, 255, 255), 3f);
                }
            }

            _mapTex.SetPixels32(_mapPixels);
            _mapTex.Apply(false, false);
        }

        private static Vector2 MapPoint(Vector3 p, float minX, float minZ, float offX, float offZ, float scale, float pad)
        {
            return new Vector2(
                pad + (p.x - minX + offX) * scale,
                pad + (p.z - minZ + offZ) * scale);
        }

        private void DrawDot(Vector2 p, Color32 color, float radius)
        {
            int r = Mathf.CeilToInt(radius);
            int cx = Mathf.RoundToInt(p.x);
            int cy = Mathf.RoundToInt(p.y);
            for (int y = -r; y <= r; y++)
            {
                for (int x = -r; x <= r; x++)
                {
                    if (x * x + y * y > r * r) continue;
                    int px = cx + x;
                    int py = cy + y;
                    if (px < 0 || px >= MapSize || py < 0 || py >= MapSize) continue;
                    _mapPixels[py * MapSize + px] = color;
                }
            }
        }

        private void DrawLine(Vector2 a, Vector2 b, Color32 color, float width)
        {
            float dx = b.x - a.x;
            float dy = b.y - a.y;
            float len = Mathf.Sqrt(dx * dx + dy * dy);
            int steps = Mathf.Max(1, Mathf.CeilToInt(len));
            float radius = width * 0.5f;
            for (int i = 0; i <= steps; i++)
            {
                float t = i / (float)steps;
                DrawDot(new Vector2(a.x + dx * t, a.y + dy * t), color, radius);
            }
        }
    }
}
