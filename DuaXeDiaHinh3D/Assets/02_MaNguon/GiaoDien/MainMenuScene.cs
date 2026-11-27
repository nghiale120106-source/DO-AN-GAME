using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace DuaXe3D
{
    /// <summary>Màn hình 01: menu chính với xe trưng bày trên sân khấu 3D.</summary>
    public class MainMenuScene : MonoBehaviour
    {
        private Text _musicText;
        private Text _sfxText;
        private Text _carChip;
        private Text _characterChip;
        private Text _terrainChip;
        private Text _recordChip;
        private GameObject _showcaseCar;

        private void Start()
        {
            SceneSetup.Configure();
            GameSession.Load();
            StartCoroutine(BuildRoutine());
        }

        private IEnumerator BuildRoutine()
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlayMenuMusic();

            Camera cam = MenuKit.CreateMenuCamera(
                new Vector3(3.8f, 2.55f, 9.2f),
                new Vector3(-2.3f, 1.05f, 0f),
                37f);
            cam.farClipPlane = 300f;

            MenuKit.BuildStudio(transform, UITheme.Accent);
            RebuildShowcaseCar();

            BuildUI();

            yield return null;
            ScreenFade.FadeIn(0.65f);
            AutoTestRunner.MarkReady();
        }

        private void RebuildShowcaseCar()
        {
            if (_showcaseCar != null) Destroy(_showcaseCar);
            _showcaseCar = MenuKit.AddCar(transform, CarLibrary.Get(GameSession.CarIndex),
                CharacterLibrary.Get(GameSession.CharacterIndex), new Vector3(0f, 0.19f, 0f), 14f);
        }

        private void BuildUI()
        {
            Canvas canvas = MenuKit.CreateCanvas("MainMenuUI");
            RectTransform root = canvas.GetComponent<RectTransform>();
            MenuKit.AddPanelBackdrop(root, 1010f);
            MenuKit.AddBrand(root);

            // Nút âm thanh góc phải
            Button music = UIKit.MakeButton(root, "", new Vector2(176f, 44f), false, ToggleMusic);
            UIKit.PlaceTopRight((RectTransform)music.transform, new Vector2(64f, 34f), new Vector2(176f, 44f));
            _musicText = music.GetComponentInChildren<Text>();

            Button sfx = UIKit.MakeButton(root, "", new Vector2(154f, 44f), false, ToggleSfx);
            UIKit.PlaceTopRight((RectTransform)sfx.transform, new Vector2(252f, 34f), new Vector2(154f, 44f));
            _sfxText = sfx.GetComponentInChildren<Text>();

            // Tiêu đề lớn
            Text eyebrow = UIKit.Label(root, "GIẢI ĐUA ĐỊA HÌNH · MÙA 2026", 19, UITheme.Accent,
                TextAnchor.UpperLeft, UITheme.Semi);
            UIKit.PlaceTopLeft(eyebrow.rectTransform, new Vector2(64f, 178f), new Vector2(620f, 28f));

            Text line1 = UIKit.Label(root, "ĐUA XE", 118, UITheme.Text, TextAnchor.UpperLeft, UITheme.Display);
            UIKit.PlaceTopLeft(line1.rectTransform, new Vector2(56f, 196f), new Vector2(800f, 128f));

            Text line2 = UIKit.Label(root, "ĐỊA HÌNH <color=#FF6A1A>3D</color>", 112, UITheme.Text,
                TextAnchor.UpperLeft, UITheme.Display);
            UIKit.PlaceTopLeft(line2.rectTransform, new Vector2(56f, 304f), new Vector2(920f, 126f));

            Image rule = UIKit.Image(root, "Rule", UITheme.Accent, UISprites.Pill);
            UIKit.PlaceTopLeft(rule.rectTransform, new Vector2(64f, 438f), new Vector2(96f, 5f));

            Text desc = UIKit.Label(root,
                "Chọn xe, tay đua và địa hình rồi lao vào cung đường đất hiểm trở.\n" +
                "W/S/A/D để lái · Space drift · Shift nitro · Đồ họa 3D thuần Unity.",
                20, UITheme.Muted, TextAnchor.UpperLeft, UITheme.Body);
            UIKit.PlaceTopLeft(desc.rectTransform, new Vector2(64f, 458f), new Vector2(730f, 66f));
            desc.horizontalOverflow = HorizontalWrapMode.Wrap;
            desc.verticalOverflow = VerticalWrapMode.Truncate;

            Button start = UIKit.MakeButton(root, "BẮT ĐẦU HÀNH TRÌNH", new Vector2(520f, 84f), true,
                () => SceneFlow.Load(SceneFlow.CarSelect));
            UIKit.PlaceTopLeft((RectTransform)start.transform, new Vector2(64f, 552f), new Vector2(520f, 84f));

            Button quick = UIKit.MakeButton(root, "ĐUA NHANH", new Vector2(250f, 60f), false,
                () => SceneFlow.Load(SceneFlow.Race));
            UIKit.PlaceTopLeft((RectTransform)quick.transform, new Vector2(64f, 658f), new Vector2(250f, 60f));

            Button quit = UIKit.MakeButton(root, "THOÁT", new Vector2(180f, 60f), false, QuitGame);
            UIKit.PlaceTopLeft((RectTransform)quit.transform, new Vector2(330f, 658f), new Vector2(180f, 60f));

            Button carBtn = UIKit.MakeButton(root, "CHỌN XE", new Vector2(190f, 56f), false,
                () => SceneFlow.Load(SceneFlow.CarSelect));
            UIKit.PlaceTopLeft((RectTransform)carBtn.transform, new Vector2(64f, 738f), new Vector2(190f, 56f));

            Button charBtn = UIKit.MakeButton(root, "TAY ĐUA", new Vector2(190f, 56f), false,
                () => SceneFlow.Load(SceneFlow.CharacterSelect));
            UIKit.PlaceTopLeft((RectTransform)charBtn.transform, new Vector2(266f, 738f), new Vector2(190f, 56f));

            Button terrainBtn = UIKit.MakeButton(root, "ĐỊA HÌNH", new Vector2(190f, 56f), false,
                () => SceneFlow.Load(SceneFlow.TerrainSelect));
            UIKit.PlaceTopLeft((RectTransform)terrainBtn.transform, new Vector2(468f, 738f), new Vector2(190f, 56f));

            // Thông tin lựa chọn hiện tại
            CarSpec car = CarLibrary.Get(GameSession.CarIndex);
            CharacterSpec ch = CharacterLibrary.Get(GameSession.CharacterIndex);
            TerrainTheme theme = ThemeLibrary.Get(GameSession.TerrainIndex);
            float best = GameSession.GetBestTime(GameSession.TerrainIndex);

            _carChip = MenuKit.AddChip(root, "XE · " + car.Name, car.AccentColor,
                new Vector2(64f, 902f), new Vector2(330f, 44f));
            _characterChip = MenuKit.AddChip(root, "TAY ĐUA · " + ch.Nickname, ch.SuitAccent,
                new Vector2(410f, 902f), new Vector2(330f, 44f));
            _terrainChip = MenuKit.AddChip(root, "ĐỊA HÌNH · " + theme.Name, theme.AccentColor,
                new Vector2(756f, 902f), new Vector2(330f, 44f));

            _recordChip = MenuKit.AddChip(root, best > 0f
                    ? "KỶ LỤC · " + UIKit.FormatTime(best)
                    : "KỶ LỤC · CHƯA CÓ",
                UITheme.Gold, new Vector2(64f, 956f), new Vector2(330f, 44f));

            Text hint = UIKit.Label(root, "ENTER  BẮT ĐẦU     ·     ← →  CHỌN NHANH     ·     ESC  THOÁT",
                16, UITheme.Muted, TextAnchor.LowerLeft, UITheme.Body);
            UIKit.PlaceBottomLeft(hint.rectTransform, new Vector2(64f, 22f), new Vector2(760f, 24f));

            RefreshSoundLabels();
        }

        private void ToggleMusic()
        {
            bool on = !GameSession.MusicOn;
            if (AudioManager.Instance != null) AudioManager.Instance.SetMusicEnabled(on);
            else GameSession.MusicOn = on;
            RefreshSoundLabels();
        }

        private void ToggleSfx()
        {
            GameSession.SfxOn = !GameSession.SfxOn;
            RefreshSoundLabels();
            if (GameSession.SfxOn) AudioManager.PlayUiClickStatic();
        }

        private void RefreshSoundLabels()
        {
            if (_musicText != null) _musicText.text = GameSession.MusicOn ? "NHẠC  BẬT" : "NHẠC  TẮT";
            if (_sfxText != null) _sfxText.text = GameSession.SfxOn ? "SFX  BẬT" : "SFX  TẮT";
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
                CycleQuickCar(-1);
            if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
                CycleQuickCar(1);
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                SceneFlow.Load(SceneFlow.CarSelect);
            if (Input.GetKeyDown(KeyCode.Escape)) QuitGame();
        }

        private void CycleQuickCar(int delta)
        {
            int count = CarLibrary.All.Length;
            int next = ((GameSession.CarIndex + delta) % count + count) % count;
            GameSession.SetCar(next);
            RebuildShowcaseCar();
            if (_carChip != null) _carChip.text = "XE · " + CarLibrary.Get(next).Name;
            AudioManager.PlayUiClickStatic();
        }

        private void QuitGame()
        {
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }
}
