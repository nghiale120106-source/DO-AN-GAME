using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DuaXe3D
{
    /// <summary>Màn hình 03: chọn 1 trong 3 tay đua.</summary>
    public class CharacterSelectScene : MonoBehaviour
    {
        private int _index;
        private GameObject _characterRoot;
        private RectTransform _detailContent;
        private RectTransform _canvasRoot;
        private readonly List<UICard> _cards = new List<UICard>();

        private static readonly string[] Steps = { "CHỌN XE", "TAY ĐUA", "ĐỊA HÌNH" };

        private static readonly string[][] Stats =
        {
            new[] { "TỐC ĐỘ", "82", "SỨC BỀN", "90", "BẢN LĨNH", "86" },
            new[] { "TỐC ĐỘ", "92", "SỨC BỀN", "76", "BẢN LĨNH", "84" },
            new[] { "TỐC ĐỘ", "80", "SỨC BỀN", "88", "BẢN LĨNH", "96" }
        };

        private void Start()
        {
            SceneSetup.Configure();
            GameSession.Load();
            _index = GameSession.CharacterIndex;
            StartCoroutine(BuildRoutine());
        }

        private IEnumerator BuildRoutine()
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlayMenuMusic();

            Camera cam = MenuKit.CreateMenuCamera(
                new Vector3(3.7f, 2.9f, 7.6f),
                new Vector3(-0.1f, 1.5f, 0f),
                34f);
            cam.farClipPlane = 300f;

            MenuKit.BuildStudio(transform, CharacterLibrary.Get(_index).SuitAccent);
            RebuildCharacter();

            BuildUI();
            RefreshAll();

            yield return null;
            ScreenFade.FadeIn(0.6f);
            AutoTestRunner.MarkReady();
        }

        private void BuildUI()
        {
            Canvas canvas = MenuKit.CreateCanvas("CharacterSelectUI");
            RectTransform root = canvas.GetComponent<RectTransform>();
            _canvasRoot = root;
            MenuKit.AddPanelBackdrop(root, 930f);

            MenuKit.AddHeader(root, "BƯỚC 2 / 3 · TAY ĐUA", "CHỌN TAY ĐUA",
                "Mỗi tay đua có thế mạnh riêng. Chọn người đồng hành phù hợp với chiếc xe và " +
                "địa hình mà bạn sắp chinh phục.",
                Steps, 1, () => SceneFlow.Load(SceneFlow.CarSelect));

            MenuKit.AddDetailPanel(root, new Vector2(64f, 396f), new Vector2(548f, 404f));

            Button continueBtn = UIKit.MakeButton(root, "TIẾP TỤC  →", new Vector2(330f, 74f), true,
                GoNext);
            UIKit.PlaceBottomLeft((RectTransform)continueBtn.transform, new Vector2(64f, 64f), new Vector2(330f, 74f));

            Button raceBtn = UIKit.MakeButton(root, "ĐUA NGAY", new Vector2(190f, 74f), false,
                () => SceneFlow.Load(SceneFlow.Race));
            UIKit.PlaceBottomLeft((RectTransform)raceBtn.transform, new Vector2(410f, 64f), new Vector2(190f, 74f));

            for (int i = 0; i < CharacterLibrary.All.Length; i++)
            {
                CharacterSpec spec = CharacterLibrary.All[i];
                int idx = i;
                UICard card = MenuKit.AddCard(root, UISprites.CharacterThumb(spec), spec.Nickname,
                    spec.Name + " · " + spec.Specialty, "SỐ " + spec.Number, spec.SuitAccent,
                    new Vector2(420f, 206f), () => Select(idx));
                UIKit.PlaceTopRight((RectTransform)card.transform,
                    new Vector2(64f, 214f + i * 222f), new Vector2(420f, 206f));
                _cards.Add(card);
            }

            Button prev = UIKit.MakeIconButton(root, UISprites.Chevron, new Vector2(54f, 54f),
                () => Select(_index - 1), UITheme.Text, true);
            UIKit.Place((RectTransform)prev.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(-330f, 40f), new Vector2(54f, 54f));

            Button next = UIKit.MakeIconButton(root, UISprites.Chevron, new Vector2(54f, 54f),
                () => Select(_index + 1), UITheme.Text, false);
            UIKit.Place((RectTransform)next.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(330f, 40f), new Vector2(54f, 54f));

            MenuKit.AddFooter(root, "← →  ĐỔI TAY ĐUA     ·     ENTER  TIẾP TỤC     ·     ESC  QUAY LẠI");
        }

        private void RebuildCharacter()
        {
            if (_characterRoot != null) Destroy(_characterRoot);
            _characterRoot = MenuKit.AddCharacter(transform, CharacterLibrary.Get(_index),
                new Vector3(0f, 0.18f, 0f), 1.38f);
        }

        public void Select(int index)
        {
            int count = CharacterLibrary.All.Length;
            int next = ((index % count) + count) % count;
            if (next == _index && _detailContent != null) return;
            _index = next;
            GameSession.SetCharacter(_index);
            RebuildCharacter();
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
            CharacterSpec spec = CharacterLibrary.Get(_index);
            RectTransform c = UIKit.Rect(_canvasRoot, "DetailContent");
            UIKit.PlaceTopLeft(c, new Vector2(84f, 414f), new Vector2(508f, 370f));

            Image badge = UIKit.Panel(c, "Badge", new Color(spec.SuitAccent.r, spec.SuitAccent.g,
                spec.SuitAccent.b, 0.16f), 12f, true, 0.35f);
            UIKit.PlaceTopLeft(badge.rectTransform, new Vector2(0f, 0f), new Vector2(190f, 30f));
            Text badgeText = UIKit.Label(badge.rectTransform, "TAY ĐUA ĐANG CHỌN", 13, spec.SuitAccent,
                TextAnchor.MiddleCenter, UITheme.Semi);
            UIKit.Fill(badgeText.rectTransform);

            Text number = UIKit.Label(c, "SỐ " + spec.Number, 34, spec.SuitAccent, TextAnchor.UpperRight, UITheme.Display);
            UIKit.PlaceTopLeft(number.rectTransform, new Vector2(350f, 28f), new Vector2(158f, 46f));

            Text nick = UIKit.Label(c, spec.Nickname, 42, UITheme.Text, TextAnchor.UpperLeft, UITheme.Heading);
            UIKit.PlaceTopLeft(nick.rectTransform, new Vector2(0f, 42f), new Vector2(330f, 52f));

            Text name = UIKit.Label(c, spec.Name + "   ·   " + spec.Specialty, 18, UITheme.Muted,
                TextAnchor.UpperLeft, UITheme.Semi);
            UIKit.PlaceTopLeft(name.rectTransform, new Vector2(0f, 94f), new Vector2(508f, 26f));

            Text tagline = UIKit.Label(c, spec.Tagline, 18, UITheme.Muted, TextAnchor.UpperLeft, UITheme.Body);
            UIKit.PlaceTopLeft(tagline.rectTransform, new Vector2(0f, 128f), new Vector2(508f, 48f));
            tagline.horizontalOverflow = HorizontalWrapMode.Wrap;

            Image divider = UIKit.Image(c, "Divider", new Color(1f, 1f, 1f, 0.09f), UISprites.Pill);
            UIKit.PlaceTopLeft(divider.rectTransform, new Vector2(0f, 182f), new Vector2(508f, 1.5f));

            string[] stats = Stats[Mathf.Clamp(_index, 0, Stats.Length - 1)];
            for (int i = 0; i < 3; i++)
            {
                float y = 204f + i * 48f;
                CreateStat(c, stats[i * 2], int.Parse(stats[i * 2 + 1]), spec.SuitAccent, y);
            }

            Text note = UIKit.Label(c, "Bộ đồ đua · Mũ bảo hiểm · Kính chắn sáng", 15, UITheme.Muted,
                TextAnchor.UpperLeft, UITheme.Body);
            UIKit.PlaceTopLeft(note.rectTransform, new Vector2(0f, 346f), new Vector2(508f, 24f));

            return c;
        }

        private void CreateStat(RectTransform parent, string label, int value, Color color, float y)
        {
            Text lab = UIKit.Label(parent, label, 16, UITheme.Muted, TextAnchor.UpperLeft, UITheme.Semi);
            UIKit.PlaceTopLeft(lab.rectTransform, new Vector2(0f, y), new Vector2(300f, 22f));

            Text val = UIKit.Label(parent, value.ToString(), 17, color, TextAnchor.UpperRight, UITheme.Semi);
            UIKit.PlaceTopLeft(val.rectTransform, new Vector2(438f, y - 1f), new Vector2(70f, 22f));

            Image fill = UIKit.ProgressBar(parent, new Vector2(508f, 8f),
                new Color(1f, 1f, 1f, 0.08f), color);
            UIKit.PlaceTopLeft((RectTransform)fill.transform.parent, new Vector2(0f, y + 26f),
                new Vector2(508f, 8f));
            fill.fillAmount = value / 100f;
        }

        private void GoNext()
        {
            GameSession.SetCharacter(_index);
            SceneFlow.LoadNextAfterCharacter();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A)) Select(_index - 1);
            if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)) Select(_index + 1);
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) GoNext();
            if (Input.GetKeyDown(KeyCode.Escape)) SceneFlow.Load(SceneFlow.CarSelect);
        }
    }
}
