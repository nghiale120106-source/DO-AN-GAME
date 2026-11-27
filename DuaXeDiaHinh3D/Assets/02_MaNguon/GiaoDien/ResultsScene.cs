using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DuaXe3D
{
    /// <summary>Màn hình 06: bảng xếp hạng Top 1/2/3 và lựa chọn chơi lại.</summary>
    public class ResultsScene : MonoBehaviour
    {
        private static readonly Color[] PlaceColors =
        {
            UITheme.Gold, UITheme.Silver, UITheme.Bronze, UITheme.Muted
        };

        private void Start()
        {
            SceneSetup.Configure();
            GameSession.Load();
            EnsureResults();
            StartCoroutine(BuildRoutine());
        }

        private IEnumerator BuildRoutine()
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlayMenuMusic();

            Camera cam = MenuKit.CreateMenuCamera(
                new Vector3(-1.4f, 4.5f, 20.8f),
                new Vector3(-1.8f, 1.55f, 0f),
                35f);
            cam.farClipPlane = 300f;

            MenuKit.BuildStudio(transform, PlayerColor());
            HideStudioPodium();
            BuildPodium();

            BuildUI();

            yield return null;
            ScreenFade.FadeIn(0.6f);
            AutoTestRunner.MarkReady();
        }

        private void EnsureResults()
        {
            if (RaceResults.HasData && RaceResults.Entries.Count > 0) return;

            RaceResults.Clear();
            RaceResults.HasData = true;
            RaceResults.TerrainIndex = GameSession.TerrainIndex;
            RaceResults.PlayerPlace = 1;
            RaceResults.PlayerTime = 92.35f;
            RaceResults.NewRecord = true;

            AddDemo("BẠN", GameSession.CarIndex, GameSession.CharacterIndex, 92.35f, true, 1);
            AddDemo(CharacterLibrary.Get((GameSession.CharacterIndex + 1) % 3).Nickname,
                (GameSession.CarIndex + 1) % 3, (GameSession.CharacterIndex + 1) % 3, 96.42f, false, 2);
            AddDemo(CharacterLibrary.Get((GameSession.CharacterIndex + 2) % 3).Nickname,
                (GameSession.CarIndex + 2) % 3, (GameSession.CharacterIndex + 2) % 3, 101.18f, false, 3);
            AddDemo("TAY ĐUA 1", (GameSession.CarIndex + 1) % 3, 0, 108.74f, false, 4);
        }

        private void AddDemo(string name, int car, int character, float time, bool isPlayer, int place)
        {
            RaceResults.Entries.Add(new RaceResultEntry
            {
                Name = name,
                CarName = CarLibrary.Get(car).Name,
                CharacterName = CharacterLibrary.Get(character).Name,
                CarIndex = car,
                CharacterIndex = character,
                Time = time,
                IsPlayer = isPlayer,
                Finished = true,
                Place = place
            });
        }

        private Color PlayerColor()
        {
            List<RaceResultEntry> entries = SortedEntries();
            for (int i = 0; i < entries.Count; i++)
                if (entries[i].IsPlayer) return PlaceColor(entries[i].Place);
            return UITheme.Accent;
        }

        private static void HideStudioPodium()
        {
            // Bỏ bệ trưng bày trung tâm của studio để nhường chỗ cho podium kết quả.
            GameObject studio = GameObject.Find("Studio");
            if (studio == null) return;
            Transform root = studio.transform;
            for (int i = root.childCount - 1; i >= 0; i--)
            {
                Transform c = root.GetChild(i);
                if (c.name == "Podium" || c.name == "AccentRing" || c.name == "InnerRing")
                    c.gameObject.SetActive(false);
            }
        }

        private static List<RaceResultEntry> SortedEntries()
        {
            List<RaceResultEntry> list = new List<RaceResultEntry>(RaceResults.Entries);
            list.Sort((a, b) => a.Place.CompareTo(b.Place));
            return list;
        }

        private static Color PlaceColor(int place)
        {
            return PlaceColors[Mathf.Clamp(place - 1, 0, PlaceColors.Length - 1)];
        }

        // ------------------------------------------------------------------
        // Podium 3D
        // ------------------------------------------------------------------
        private void BuildPodium()
        {
            List<RaceResultEntry> entries = SortedEntries();

            // Camera nhìn theo hướng -Z nên phía bên phải màn hình là trục -X => đặt podium tại -X để không bị bảng xếp hạng che.
            GameObject podiumRoot = new GameObject("PodiumRoot");
            podiumRoot.transform.SetParent(transform, false);
            podiumRoot.transform.localPosition = new Vector3(-4.8f, 0f, 0f);

            float[] xs = { 0f, -3.3f, 3.3f };
            float[] heights = { 1.05f, 0.72f, 0.48f };
            Color[] colors = { UITheme.Gold, UITheme.Silver, UITheme.Bronze };

            for (int i = 0; i < 3; i++)
            {
                float h = heights[i];
                GameObject block = MeshKit.Box("Podium" + (i + 1), podiumRoot.transform,
                    new Vector3(xs[i], h * 0.5f, 0f), new Vector3(3.0f, h, 5.0f),
                    Vector3.zero, MaterialLibrary.Podium(colors[i]));
                MeshKit.SetShadows(block, true, true);

                // Viền phát sáng mảnh ở cạnh trước thay cho tấm phủ kín mặt bệ.
                MeshKit.Box("PodiumEdge", podiumRoot.transform,
                    new Vector3(xs[i], h + 0.02f, 2.46f), new Vector3(3.0f, 0.06f, 0.12f),
                    Vector3.zero, MaterialLibrary.GlowRing(colors[i]));

                MeshKit.MeshObject("PodiumRing", MeshKit.Ring(1.5f, 1.66f, 64),
                    MaterialLibrary.GlowRing(colors[i]), podiumRoot.transform,
                    new Vector3(xs[i], h + 0.04f, 0f), Quaternion.identity, Vector3.one);

                if (i < entries.Count)
                {
                    RaceResultEntry entry = entries[i];
                    CarSpec spec = CarLibrary.Get(entry.CarIndex);
                    CharacterSpec ch = CharacterLibrary.Get(entry.CharacterIndex);
                    CarVisual visual = CarFactory.BuildVisual(spec, ch, podiumRoot.transform, true);
                    visual.Root.transform.localPosition = new Vector3(xs[i], h + 0.03f, 0f);
                    visual.Root.transform.localRotation = Quaternion.identity;
                    Spinner spinner = visual.Root.AddComponent<Spinner>();
                    // Xe đứng yên hướng về camera để dễ nhìn trên bục, chỉ nhấp nhô nhẹ.
                    spinner.Speed = 0f;
                    spinner.BobHeight = 0.012f;
                }
            }

            // Đèn spot tô sáng cụm podium.
            GameObject spotGo = new GameObject("PodiumSpot");
            spotGo.transform.SetParent(podiumRoot.transform, false);
            spotGo.transform.localPosition = new Vector3(0f, 9.5f, 6.5f);
            spotGo.transform.LookAt(podiumRoot.transform.TransformPoint(new Vector3(0f, 1.4f, 0f)));
            Light spot = spotGo.AddComponent<Light>();
            spot.type = LightType.Spot;
            spot.color = new Color(1f, 0.95f, 0.86f);
            spot.intensity = 6.5f;
            spot.range = 30f;
            spot.spotAngle = 58f;
            spot.innerSpotAngle = 30f;
            spot.shadows = LightShadows.None;

            if (RaceResults.PlayerPlace == 1)
                MenuKit.AddConfetti(podiumRoot.transform, UITheme.Gold);
            else
                MenuKit.AddDust(podiumRoot.transform, new Color(0.8f, 0.85f, 1f, 0.35f), 70);
        }

        // ------------------------------------------------------------------
        // UI
        // ------------------------------------------------------------------
        private void BuildUI()
        {
            Canvas canvas = MenuKit.CreateCanvas("ResultsUI");
            RectTransform root = canvas.GetComponent<RectTransform>();
            MenuKit.AddPanelBackdrop(root, 1030f);
            MenuKit.AddBrand(root);

            TerrainTheme theme = ThemeLibrary.Get(RaceResults.TerrainIndex);
            List<RaceResultEntry> entries = SortedEntries();
            int place = RaceResults.PlayerPlace;
            Color pc = PlaceColor(place);

            Text eye = UIKit.Label(root, "KẾT QUẢ · " + theme.Name, 18, theme.AccentColor,
                TextAnchor.UpperLeft, UITheme.Semi);
            UIKit.PlaceTopLeft(eye.rectTransform, new Vector2(64f, 178f), new Vector2(620f, 26f));

            string headline = place == 1 ? "CHIẾN THẮNG!" :
                place == 2 ? "VỀ NHÌ!" :
                place == 3 ? "VỀ BA!" : "HOÀN THÀNH!";
            Text title = UIKit.Label(root, headline, 88, pc, TextAnchor.UpperLeft, UITheme.Display);
            UIKit.PlaceTopLeft(title.rectTransform, new Vector2(58f, 198f), new Vector2(760f, 104f));

            string sub = place == 1
                ? "Bạn đã chiến thắng"
                : "Bạn về đích ở vị trí " + place + " / " + entries.Count;
            Text subtitle = UIKit.Label(root, sub, 26, UITheme.Text, TextAnchor.UpperLeft, UITheme.Semi);
            UIKit.PlaceTopLeft(subtitle.rectTransform, new Vector2(64f, 304f), new Vector2(700f, 34f));

            // Ba ô thông số
            float best = GameSession.GetBestTime(RaceResults.TerrainIndex);
            BuildResultStat(root, "THỜI GIAN", UIKit.FormatTime(RaceResults.PlayerTime), pc,
                new Vector2(64f, 356f));
            BuildResultStat(root, "KỶ LỤC", best > 0f ? UIKit.FormatTime(best) : "—",
                UITheme.Gold, new Vector2(270f, 356f));
            BuildResultStat(root, "HẠNG", place + " / " + entries.Count,
                pc, new Vector2(476f, 356f));

            if (RaceResults.NewRecord)
            {
                Image rec = UIKit.Panel(root, "NewRecord",
                    new Color(UITheme.Gold.r, UITheme.Gold.g, UITheme.Gold.b, 0.16f), 12f, true, 0.45f);
                UIKit.PlaceTopLeft(rec.rectTransform, new Vector2(64f, 432f), new Vector2(190f, 32f));
                Text recText = UIKit.Label(rec.rectTransform, "★  KỶ LỤC MỚI", 15, UITheme.Gold,
                    TextAnchor.MiddleCenter, UITheme.Semi);
                UIKit.Fill(recText.rectTransform);
            }

            BuildLeaderboard(root, entries);

            Button again = UIKit.MakeButton(root, "ĐUA LẠI", new Vector2(250f, 72f), true,
                () => SceneFlow.Load(SceneFlow.Race));
            UIKit.PlaceBottomLeft((RectTransform)again.transform, new Vector2(64f, 64f), new Vector2(250f, 72f));

            Button terrain = UIKit.MakeButton(root, "ĐỔI ĐỊA HÌNH", new Vector2(240f, 72f), false,
                () => SceneFlow.Load(SceneFlow.TerrainSelect));
            UIKit.PlaceBottomLeft((RectTransform)terrain.transform, new Vector2(330f, 64f), new Vector2(240f, 72f));

            Button menu = UIKit.MakeButton(root, "VỀ MENU", new Vector2(200f, 72f), false,
                () => SceneFlow.Load(SceneFlow.MainMenu));
            UIKit.PlaceBottomLeft((RectTransform)menu.transform, new Vector2(586f, 64f), new Vector2(200f, 72f));

            MenuKit.AddFooter(root, "ENTER  ĐUA LẠI     ·     ESC  VỀ MENU CHÍNH");
        }

        private void BuildResultStat(RectTransform root, string caption, string value, Color color, Vector2 pos)
        {
            Image panel = UIKit.Panel(root, "Stat_" + caption, new Color(0.035f, 0.055f, 0.082f, 0.86f),
                16f, true, 0.08f);
            UIKit.PlaceTopLeft(panel.rectTransform, pos, new Vector2(190f, 64f));

            Text cap = UIKit.Label(panel.rectTransform, caption, 12, UITheme.Muted,
                TextAnchor.UpperLeft, UITheme.Semi);
            UIKit.PlaceTopLeft(cap.rectTransform, new Vector2(16f, 10f), new Vector2(160f, 18f));

            Text val = UIKit.Label(panel.rectTransform, value, 27, color, TextAnchor.UpperLeft, UITheme.Heading);
            UIKit.PlaceTopLeft(val.rectTransform, new Vector2(16f, 28f), new Vector2(160f, 32f));
        }

        private void BuildLeaderboard(RectTransform root, List<RaceResultEntry> entries)
        {
            Image panel = UIKit.Panel(root, "Leaderboard", new Color(0.035f, 0.055f, 0.082f, 0.9f),
                22f, true, 0.09f);
            UIKit.PlaceTopLeft(panel.rectTransform, new Vector2(64f, 500f), new Vector2(700f, 330f));
            UIKit.AddCornerBrackets(panel.rectTransform, new Color(1f, 1f, 1f, 0.18f), 16f, 2f);

            Text head = UIKit.Label(panel.rectTransform, "BẢNG XẾP HẠNG", 15, UITheme.Muted,
                TextAnchor.UpperLeft, UITheme.Semi);
            UIKit.PlaceTopLeft(head.rectTransform, new Vector2(22f, 14f), new Vector2(300f, 22f));

            Text cols = UIKit.Label(panel.rectTransform, "TAY ĐUA / XE", 12, UITheme.Muted,
                TextAnchor.UpperLeft, UITheme.Semi);
            UIKit.PlaceTopLeft(cols.rectTransform, new Vector2(78f, 42f), new Vector2(300f, 18f));

            Text timeCol = UIKit.Label(panel.rectTransform, "THỜI GIAN", 12, UITheme.Muted,
                TextAnchor.UpperRight, UITheme.Semi);
            UIKit.PlaceTopLeft(timeCol.rectTransform, new Vector2(520f, 42f), new Vector2(156f, 18f));

            int count = Mathf.Min(4, entries.Count);
            for (int i = 0; i < count; i++)
            {
                RaceResultEntry e = entries[i];
                float y = 70f + i * 60f;
                Color color = PlaceColor(e.Place);

                if (e.IsPlayer)
                {
                    Image hi = UIKit.Panel(panel.rectTransform, "PlayerRow",
                        new Color(color.r, color.g, color.b, 0.13f), 12f, true, 0.35f);
                    UIKit.PlaceTopLeft(hi.rectTransform, new Vector2(12f, y - 6f), new Vector2(676f, 54f));
                }

                Image badge = UIKit.Image(panel.rectTransform, "Place", color, UISprites.Circle);
                UIKit.PlaceTopLeft(badge.rectTransform, new Vector2(22f, y + 2f), new Vector2(38f, 38f));
                Text placeText = UIKit.Label(badge.rectTransform, e.Place.ToString(), 19, UITheme.Bg,
                    TextAnchor.MiddleCenter, UITheme.Heading);
                UIKit.Fill(placeText.rectTransform);

                Text name = UIKit.Label(panel.rectTransform, e.Name,
                    21, UITheme.Text, TextAnchor.UpperLeft, UITheme.Semi);
                UIKit.PlaceTopLeft(name.rectTransform, new Vector2(76f, y - 1f), new Vector2(360f, 28f));

                Text info = UIKit.Label(panel.rectTransform, e.CarName + " · " + e.CharacterName,
                    14, UITheme.Muted, TextAnchor.UpperLeft, UITheme.Body);
                UIKit.PlaceTopLeft(info.rectTransform, new Vector2(76f, y + 25f), new Vector2(400f, 20f));

                Text time = UIKit.Label(panel.rectTransform, e.Finished && e.Time > 0f
                        ? UIKit.FormatTime(e.Time) : "—",
                    22, color, TextAnchor.UpperRight, UITheme.Heading);
                UIKit.PlaceTopLeft(time.rectTransform, new Vector2(520f, y + 3f), new Vector2(156f, 30f));
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                SceneFlow.Load(SceneFlow.Race);
            if (Input.GetKeyDown(KeyCode.Escape))
                SceneFlow.Load(SceneFlow.MainMenu);
        }
    }
}
