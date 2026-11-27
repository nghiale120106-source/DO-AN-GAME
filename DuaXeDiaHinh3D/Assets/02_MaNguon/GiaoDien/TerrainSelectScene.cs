using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DuaXe3D
{
    /// <summary>Màn hình 04: chọn 1 trong 4 địa hình, xem trước bằng terrain 3D thu nhỏ.</summary>
    public class TerrainSelectScene : MonoBehaviour
    {
        private int _index;
        private GameObject _previewRoot;
        private RectTransform _detailContent;
        private RectTransform _canvasRoot;
        private readonly List<UICard> _cards = new List<UICard>();

        private static readonly string[] Steps = { "CHỌN XE", "TAY ĐUA", "ĐỊA HÌNH" };

        private static readonly string[] CardNotes =
        {
            "Nắng cháy · cát mềm",
            "Cây rậm · đường hẹp",
            "Dốc đá · đỉnh tuyết",
            "Bùn lầy · trơn trượt"
        };

        private void Start()
        {
            SceneSetup.Configure();
            GameSession.Load();
            _index = GameSession.TerrainIndex;
            StartCoroutine(BuildRoutine());
        }

        private IEnumerator BuildRoutine()
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlayMenuMusic();

            RebuildPreview();

            Camera cam = SceneSetup.CreateMainCamera(
                new Vector3(60f, 78f, 18f), Quaternion.Euler(24f, 32f, 0f), 42f);
            cam.farClipPlane = 1600f;
            cam.clearFlags = CameraClearFlags.Skybox;

            OrbitCamera orbit = cam.gameObject.AddComponent<OrbitCamera>();
            orbit.Target = new Vector3(100f, 8f, 100f);
            orbit.Distance = 152f;
            orbit.Pitch = 24f;
            orbit.Yaw = 214f;
            orbit.AutoOrbitSpeed = 1.1f;
            orbit.AllowDrag = true;
            orbit.DragSpeed = 5f;

            CameraPostFX fx = cam.GetComponent<CameraPostFX>();
            if (fx != null)
            {
                fx.vignette = 0.5f;
                fx.saturation = 1.09f;
                fx.contrast = 1.05f;
            }

            BuildUI();
            RefreshAll();

            yield return null;
            ScreenFade.FadeIn(0.6f);
            AutoTestRunner.MarkReady();
        }

        private void BuildUI()
        {
            Canvas canvas = MenuKit.CreateCanvas("TerrainSelectUI");
            RectTransform root = canvas.GetComponent<RectTransform>();
            _canvasRoot = root;
            MenuKit.AddPanelBackdrop(root, 930f);

            MenuKit.AddHeader(root, "BƯỚC 3 / 3 · ĐỊA HÌNH", "CHỌN ĐỊA HÌNH",
                "Bốn vùng đất, bốn thử thách. Mỗi địa hình có độ dốc, hố đất và khúc cua khác nhau " +
                "trên cùng một đường đua uốn lượn dài hơn 1 km.",
                Steps, 2, () => SceneFlow.Load(SceneFlow.CharacterSelect));

            MenuKit.AddDetailPanel(root, new Vector2(64f, 396f), new Vector2(548f, 404f));

            Button raceBtn = UIKit.MakeButton(root, "BẮT ĐẦU ĐUA  →", new Vector2(360f, 76f), true,
                StartRace);
            UIKit.PlaceBottomLeft((RectTransform)raceBtn.transform, new Vector2(64f, 62f), new Vector2(360f, 76f));

            Button editBtn = UIKit.MakeButton(root, "ĐỔI XE", new Vector2(190f, 76f), false,
                () => SceneFlow.Load(SceneFlow.CarSelect));
            UIKit.PlaceBottomLeft((RectTransform)editBtn.transform, new Vector2(440f, 62f), new Vector2(190f, 76f));

            for (int i = 0; i < ThemeLibrary.All.Length; i++)
            {
                TerrainTheme theme = ThemeLibrary.All[i];
                int idx = i;
                UICard card = MenuKit.AddCard(root, UISprites.Thumb(theme), theme.Name,
                    theme.EnglishName + " · " + theme.Difficulty,
                    CardNotes[Mathf.Clamp(i, 0, CardNotes.Length - 1)],
                    theme.AccentColor, new Vector2(420f, 148f), () => Select(idx));
                UIKit.PlaceTopRight((RectTransform)card.transform,
                    new Vector2(64f, 214f + i * 160f), new Vector2(420f, 148f));
                _cards.Add(card);
            }

            Button prev = UIKit.MakeIconButton(root, UISprites.Chevron, new Vector2(54f, 54f),
                () => Select(_index - 1), UITheme.Text, true);
            UIKit.Place((RectTransform)prev.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(-310f, 20f), new Vector2(54f, 54f));

            Button next = UIKit.MakeIconButton(root, UISprites.Chevron, new Vector2(54f, 54f),
                () => Select(_index + 1), UITheme.Text, false);
            UIKit.Place((RectTransform)next.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(310f, 20f), new Vector2(54f, 54f));

            MenuKit.AddFooter(root, "← →  ĐỔI ĐỊA HÌNH     ·     ENTER  BẮT ĐẦU ĐUA     ·     ESC  QUAY LẠI");
        }

        private void RebuildPreview()
        {
            if (_previewRoot != null) Destroy(_previewRoot);
            GameObject holder = new GameObject("PreviewHolder");
            holder.transform.SetParent(transform, false);
            EnvironmentBuilder.Build(_index, holder.transform, true, true);
            _previewRoot = holder;
        }

        public void Select(int index)
        {
            int count = ThemeLibrary.All.Length;
            int next = ((index % count) + count) % count;
            if (next == _index && _detailContent != null) return;
            _index = next;
            GameSession.SetTerrain(_index);
            RebuildPreview();
            RefreshAll();
        }

        private void RefreshAll()
        {
            for (int i = 0; i < _cards.Count; i++)
                _cards[i].SetSelected(i == _index);

            if (_detailContent != null) Destroy(_detailContent.gameObject);
            _detailContent = BuildDetail();
        }

        private RectTransform BuildDetail()
        {
            TerrainTheme theme = ThemeLibrary.Get(_index);
            RectTransform c = UIKit.Rect(_canvasRoot, "DetailContent");
            UIKit.PlaceTopLeft(c, new Vector2(84f, 414f), new Vector2(508f, 370f));

            Image badge = UIKit.Panel(c, "Badge", new Color(theme.AccentColor.r, theme.AccentColor.g,
                theme.AccentColor.b, 0.16f), 12f, true, 0.35f);
            UIKit.PlaceTopLeft(badge.rectTransform, new Vector2(0f, 0f), new Vector2(180f, 30f));
            Text badgeText = UIKit.Label(badge.rectTransform, "ĐỊA HÌNH ĐANG CHỌN", 13, theme.AccentColor,
                TextAnchor.MiddleCenter, UITheme.Semi);
            UIKit.Fill(badgeText.rectTransform);

            Text difficulty = UIKit.Label(c, "ĐỘ KHÓ  " + theme.Difficulty, 15,
                theme.Difficulty == "DỄ" ? UITheme.Good : (theme.Difficulty == "KHÓ" ? UITheme.Bad : UITheme.Gold),
                TextAnchor.UpperRight, UITheme.Semi);
            UIKit.PlaceTopLeft(difficulty.rectTransform, new Vector2(280f, 5f), new Vector2(228f, 24f));

            Text name = UIKit.Label(c, theme.Name, 42, UITheme.Text, TextAnchor.UpperLeft, UITheme.Heading);
            UIKit.PlaceTopLeft(name.rectTransform, new Vector2(0f, 42f), new Vector2(508f, 52f));

            Text english = UIKit.Label(c, theme.EnglishName + "  ·  ĐƯỜNG ĐUA 1.3 KM",
                17, theme.AccentColor, TextAnchor.UpperLeft, UITheme.Semi);
            UIKit.PlaceTopLeft(english.rectTransform, new Vector2(0f, 94f), new Vector2(508f, 24f));

            Text desc = UIKit.Label(c, theme.Description, 18, UITheme.Muted, TextAnchor.UpperLeft, UITheme.Body);
            UIKit.PlaceTopLeft(desc.rectTransform, new Vector2(0f, 128f), new Vector2(508f, 62f));
            desc.horizontalOverflow = HorizontalWrapMode.Wrap;

            Image divider = UIKit.Image(c, "Divider", new Color(1f, 1f, 1f, 0.09f), UISprites.Pill);
            UIKit.PlaceTopLeft(divider.rectTransform, new Vector2(0f, 196f), new Vector2(508f, 1.5f));

            CreateFeature(c, "ĐỒI DỐC", "Nhiều đoạn dốc lên xuống liên tiếp", theme.AccentColor, 216f);
            CreateFeature(c, "HỐ ĐẤT", "2 hố đất sâu hai bên đường", theme.AccentColor, 254f);
            CreateFeature(c, "CUA GẮT", "Ít nhất 1 khúc cua tay áo hiểm trở", theme.AccentColor, 292f);

            float best = GameSession.GetBestTime(_index);
            Text record = UIKit.Label(c, best > 0f
                    ? "KỶ LỤC CỦA BẠN  ·  " + UIKit.FormatTime(best)
                    : "KỶ LỤC CỦA BẠN  ·  CHƯA CÓ",
                16, UITheme.Gold, TextAnchor.UpperLeft, UITheme.Semi);
            UIKit.PlaceTopLeft(record.rectTransform, new Vector2(0f, 340f), new Vector2(508f, 24f));

            return c;
        }

        private void CreateFeature(RectTransform parent, string title, string detail, Color accent, float y)
        {
            Image dot = UIKit.Image(parent, "Dot", accent, UISprites.Circle);
            UIKit.PlaceTopLeft(dot.rectTransform, new Vector2(0f, y + 5f), new Vector2(9f, 9f));

            Text t = UIKit.Label(parent, title, 16, UITheme.Text, TextAnchor.UpperLeft, UITheme.Semi);
            UIKit.PlaceTopLeft(t.rectTransform, new Vector2(18f, y), new Vector2(120f, 24f));

            Text d = UIKit.Label(parent, detail, 15, UITheme.Muted, TextAnchor.UpperLeft, UITheme.Body);
            UIKit.PlaceTopLeft(d.rectTransform, new Vector2(138f, y + 1f), new Vector2(370f, 24f));
        }

        private void StartRace()
        {
            GameSession.SetTerrain(_index);
            SceneFlow.LoadNextAfterTerrain();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A)) Select(_index - 1);
            if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)) Select(_index + 1);
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) StartRace();
            if (Input.GetKeyDown(KeyCode.Escape)) SceneFlow.Load(SceneFlow.CharacterSelect);
        }
    }
}
