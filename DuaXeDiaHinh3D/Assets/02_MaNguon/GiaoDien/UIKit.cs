using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DuaXe3D
{
    /// <summary>Toàn bộ sprite giao diện được vẽ bằng code, không cần asset ngoài.</summary>
    public static class UISprites
    {
        private static Sprite _rounded;
        private static Sprite _roundedSmall;
        private static Sprite _pill;
        private static Sprite _border;
        private static Sprite _borderSmall;
        private static Sprite _circle;
        private static Sprite _ring;
        private static Sprite _softGlow;
        private static Sprite _triangle;
        private static Sprite _chevron;
        private static Sprite _bolt;
        private static Sprite _check;
        private static Sprite _star;
        private static Sprite _flag;
        private static Sprite _ticks;
        private static Sprite _noise;
        private static Sprite _checker;
        private static readonly Dictionary<string, Sprite> Gradients = new Dictionary<string, Sprite>();

        public static Sprite Rounded
        {
            get
            {
                if (_rounded == null) _rounded = MakeRounded(96, 28, false, new Vector4(28, 28, 28, 28));
                return _rounded;
            }
        }

        public static Sprite RoundedSmall
        {
            get
            {
                if (_roundedSmall == null) _roundedSmall = MakeRounded(48, 14, false, new Vector4(14, 14, 14, 14));
                return _roundedSmall;
            }
        }

        public static Sprite Pill
        {
            get
            {
                if (_pill == null) _pill = MakeRounded(64, 32, false, new Vector4(31, 31, 31, 31));
                return _pill;
            }
        }

        public static Sprite Border
        {
            get
            {
                if (_border == null) _border = MakeRounded(96, 28, true, new Vector4(28, 28, 28, 28));
                return _border;
            }
        }

        public static Sprite BorderSmall
        {
            get
            {
                if (_borderSmall == null) _borderSmall = MakeRounded(48, 14, true, new Vector4(14, 14, 14, 14));
                return _borderSmall;
            }
        }

        public static Sprite Circle
        {
            get
            {
                if (_circle == null)
                {
                    _circle = Make("circle", 128, 128, (x, y) =>
                    {
                        float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(64f, 64f)) / 64f;
                        float a = Mathf.Clamp01((1f - d) * 64f);
                        return new Color(1f, 1f, 1f, a);
                    });
                }
                return _circle;
            }
        }

        public static Sprite Ring
        {
            get
            {
                if (_ring == null)
                {
                    _ring = Make("ring", 256, 256, (x, y) =>
                    {
                        float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(128f, 128f)) / 128f;
                        float a = Mathf.Clamp01((0.965f - d) * 128f) * Mathf.Clamp01((d - 0.88f) * 128f);
                        return new Color(1f, 1f, 1f, a);
                    });
                }
                return _ring;
            }
        }

        public static Sprite SoftGlow
        {
            get
            {
                if (_softGlow == null)
                {
                    _softGlow = Make("soft_glow", 128, 128, (x, y) =>
                    {
                        float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(64f, 64f)) / 64f;
                        float a = Mathf.Clamp01(1f - d);
                        a = a * a * a;
                        return new Color(1f, 1f, 1f, a);
                    });
                }
                return _softGlow;
            }
        }

        public static Sprite Triangle
        {
            get
            {
                if (_triangle == null)
                {
                    _triangle = Make("triangle", 64, 64, (x, y) =>
                    {
                        Vector2 p = new Vector2((x + 0.5f) / 64f, (y + 0.5f) / 64f);
                        bool inside = PointInPolygon(p, new[]
                        {
                            new Vector2(0.22f, 0.1f), new Vector2(0.9f, 0.5f), new Vector2(0.22f, 0.9f)
                        });
                        return new Color(1f, 1f, 1f, inside ? 1f : 0f);
                    });
                }
                return _triangle;
            }
        }

        public static Sprite Chevron
        {
            get
            {
                if (_chevron == null)
                {
                    _chevron = Make("chevron", 64, 64, (x, y) =>
                    {
                        float a = Mathf.Max(
                            Segment(x, y, 18, 14, 44, 32, 7f),
                            Segment(x, y, 44, 32, 18, 50, 7f));
                        return new Color(1f, 1f, 1f, Mathf.Clamp01(a));
                    });
                }
                return _chevron;
            }
        }

        public static Sprite Bolt
        {
            get
            {
                if (_bolt == null)
                {
                    _bolt = Make("bolt", 64, 64, (x, y) =>
                    {
                        Vector2 p = new Vector2((x + 0.5f) / 64f, (y + 0.5f) / 64f);
                        bool inside = PointInPolygon(p, new[]
                        {
                            new Vector2(0.58f, 0.04f), new Vector2(0.16f, 0.56f), new Vector2(0.44f, 0.56f),
                            new Vector2(0.3f, 0.96f), new Vector2(0.84f, 0.42f), new Vector2(0.54f, 0.42f),
                            new Vector2(0.68f, 0.04f)
                        });
                        return new Color(1f, 1f, 1f, inside ? 1f : 0f);
                    });
                }
                return _bolt;
            }
        }

        public static Sprite Check
        {
            get
            {
                if (_check == null)
                {
                    _check = Make("check", 64, 64, (x, y) =>
                    {
                        float a = Mathf.Max(
                            Segment(x, y, 12, 34, 27, 19, 6f),
                            Segment(x, y, 27, 19, 52, 46, 6f));
                        return new Color(1f, 1f, 1f, Mathf.Clamp01(a));
                    });
                }
                return _check;
            }
        }

        public static Sprite Star
        {
            get
            {
                if (_star == null)
                {
                    _star = Make("star", 128, 128, (x, y) =>
                    {
                        Vector2 p = new Vector2((x + 0.5f) / 128f, (y + 0.5f) / 128f);
                        Vector2[] pts = new Vector2[10];
                        for (int i = 0; i < 10; i++)
                        {
                            float ang = Mathf.PI * 0.5f + i * Mathf.PI / 5f;
                            float rad = (i % 2 == 0) ? 0.46f : 0.2f;
                            pts[i] = new Vector2(0.5f + Mathf.Cos(ang) * rad, 0.5f + Mathf.Sin(ang) * rad);
                        }
                        return new Color(1f, 1f, 1f, PointInPolygon(p, pts) ? 1f : 0f);
                    });
                }
                return _star;
            }
        }

        public static Sprite Flag
        {
            get
            {
                if (_flag == null)
                {
                    _flag = Make("flag", 64, 64, (x, y) =>
                    {
                        float pole = Segment(x, y, 16, 6, 16, 58, 4f);
                        bool cloth = x >= 18 && x <= 50 && y >= 30 && y <= 56;
                        if (cloth)
                        {
                            if (Mathf.Abs(y - 43f) < (x - 18f) * 0.18f) cloth = false;
                        }
                        return new Color(1f, 1f, 1f, Mathf.Clamp01(Mathf.Max(pole, cloth ? 1f : 0f)));
                    });
                }
                return _flag;
            }
        }

        public static Sprite GaugeTicks
        {
            get
            {
                if (_ticks == null)
                {
                    _ticks = Make("gauge_ticks", 512, 512, (x, y) =>
                    {
                        float cx = 256f;
                        float cy = 256f;
                        Vector2 p = new Vector2(x + 0.5f, y + 0.5f);
                        float ang = Mathf.Atan2(p.y - cy, p.x - cx) * Mathf.Rad2Deg;
                        if (ang < 0) ang += 360f;
                        float dist = Vector2.Distance(p, new Vector2(cx, cy));
                        float a = 0f;
                        float start = 135f;
                        float sweep = 270f;
                        float delta = Mathf.Repeat(start - ang, 360f);
                        if (delta <= sweep)
                        {
                            float step = 5f;
                            float nearest = Mathf.Round(delta / step) * step;
                            if (Mathf.Abs(delta - nearest) < 1.1f)
                            {
                                bool major = Mathf.Abs(nearest % 30f) < 0.1f;
                                float inner = major ? 214f : 224f;
                                float outer = 238f;
                                if (dist >= inner && dist <= outer) a = major ? 1f : 0.55f;
                            }
                        }
                        return new Color(1f, 1f, 1f, a);
                    });
                }
                return _ticks;
            }
        }

        public static Sprite Noise
        {
            get
            {
                if (_noise == null)
                {
                    _noise = Make("ui_noise", 128, 128, (x, y) =>
                    {
                        float n = Mathf.PerlinNoise(x * 0.21f + 3f, y * 0.21f + 7f);
                        float a = Mathf.Clamp01((n - 0.35f) * 0.32f);
                        return new Color(1f, 1f, 1f, a);
                    });
                }
                return _noise;
            }
        }

        public static Sprite Checker
        {
            get
            {
                if (_checker == null)
                {
                    Texture2D tex = ProcTex.Checker(8);
                    _checker = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
                }
                return _checker;
            }
        }

        public static Sprite Gradient(Color top, Color bottom)
        {
            string key = ColorUtility.ToHtmlStringRGBA(top) + "_" + ColorUtility.ToHtmlStringRGBA(bottom);
            Sprite s;
            if (Gradients.TryGetValue(key, out s)) return s;

            const int w = 8;
            const int h = 256;
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false, false);
            tex.name = "gradient";
            Color32[] px = new Color32[w * h];
            for (int y = 0; y < h; y++)
            {
                Color c = Color.Lerp(bottom, top, y / (float)(h - 1));
                for (int x = 0; x < w; x++) px[y * w + x] = c;
            }
            tex.SetPixels32(px);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;
            tex.Apply(false, false);
            s = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 100f);
            Gradients[key] = s;
            return s;
        }

        public static Sprite HorizontalGradient(Color left, Color right)
        {
            string key = "H_" + ColorUtility.ToHtmlStringRGBA(left) + "_" + ColorUtility.ToHtmlStringRGBA(right);
            Sprite s;
            if (Gradients.TryGetValue(key, out s)) return s;

            const int w = 256;
            const int h = 8;
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false, false);
            tex.name = "h_gradient";
            Color32[] px = new Color32[w * h];
            for (int x = 0; x < w; x++)
            {
                Color c = Color.Lerp(left, right, x / (float)(w - 1));
                for (int y = 0; y < h; y++) px[y * w + x] = c;
            }
            tex.SetPixels32(px);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;
            tex.Apply(false, false);
            s = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 100f);
            Gradients[key] = s;
            return s;
        }

        public static Sprite Thumb(TerrainTheme theme)
        {
            Texture2D tex = ProcTex.Thumbnail(theme);
            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
        }

        private static readonly Dictionary<int, Sprite> CarThumbs = new Dictionary<int, Sprite>();
        private static readonly Dictionary<int, Sprite> CharacterThumbs = new Dictionary<int, Sprite>();

        public static Sprite CarThumb(CarSpec spec)
        {
            Sprite s;
            if (CarThumbs.TryGetValue(spec.Index, out s)) return s;
            Texture2D tex = ProcTex.CarThumb(spec);
            s = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
            CarThumbs[spec.Index] = s;
            return s;
        }

        public static Sprite CharacterThumb(CharacterSpec spec)
        {
            Sprite s;
            if (CharacterThumbs.TryGetValue(spec.Index, out s)) return s;
            Texture2D tex = ProcTex.CharacterThumb(spec);
            s = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
            CharacterThumbs[spec.Index] = s;
            return s;
        }

        private static Sprite Make(string name, int w, int h, Func<int, int, Color> pixel)
        {
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false, false);
            tex.name = name;
            Color32[] px = new Color32[w * h];
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                    px[y * w + x] = pixel(x, y);
            tex.SetPixels32(px);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;
            tex.Apply(false, false);
            return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 100f);
        }

        private static Sprite MakeRounded(int size, int radius, bool border, Vector4 slice)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false, false);
            tex.name = border ? "rounded_border" : "rounded";
            Color32[] px = new Color32[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float a = RoundAlpha(x + 0.5f, y + 0.5f, size, radius);
                    if (border)
                    {
                        float b = RoundAlpha(x + 0.5f, y + 0.5f, size - 4f, radius - 2f);
                        a = Mathf.Clamp01(a - b);
                    }
                    px[y * size + x] = new Color(1f, 1f, 1f, a);
                }
            }
            tex.SetPixels32(px);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;
            tex.Apply(false, false);
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, slice);
        }

        private static float RoundAlpha(float x, float y, float size, float radius)
        {
            float cx = Mathf.Clamp(x, radius, size - radius);
            float cy = Mathf.Clamp(y, radius, size - radius);
            float d = Vector2.Distance(new Vector2(x, y), new Vector2(cx, cy));
            return Mathf.Clamp01(radius - d + 0.5f);
        }

        private static bool PointInPolygon(Vector2 p, Vector2[] poly)
        {
            bool inside = false;
            for (int i = 0, j = poly.Length - 1; i < poly.Length; j = i++)
            {
                if (((poly[i].y > p.y) != (poly[j].y > p.y)) &&
                    (p.x < (poly[j].x - poly[i].x) * (p.y - poly[i].y) / (poly[j].y - poly[i].y) + poly[i].x))
                {
                    inside = !inside;
                }
            }
            return inside;
        }

        private static float Segment(float px, float py, float x1, float y1, float x2, float y2, float thickness)
        {
            Vector2 p = new Vector2(px, py);
            Vector2 a = new Vector2(x1, y1);
            Vector2 b = new Vector2(x2, y2);
            Vector2 ab = b - a;
            float t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / Mathf.Max(0.001f, ab.sqrMagnitude));
            Vector2 closest = a + ab * t;
            float d = Vector2.Distance(p, closest);
            return Mathf.Clamp01(thickness * 0.5f - d + 0.6f);
        }
    }

    public static class UIKit
    {
        public static Canvas CreateCanvas(string name, int sortOrder = 0)
        {
            GameObject go = new GameObject(name);
            Canvas canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortOrder;

            CanvasScaler scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            go.AddComponent<GraphicRaycaster>();
            SceneSetup.EnsureEventSystem();
            return canvas;
        }

        public static RectTransform Rect(Transform parent, string name)
        {
            GameObject go = new GameObject(name);
            RectTransform rt = go.AddComponent<RectTransform>();
            rt.SetParent(parent, false);
            rt.localScale = Vector3.one;
            return rt;
        }

        public static void Fill(RectTransform rt, float left = 0f, float bottom = 0f, float right = 0f, float top = 0f)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.offsetMin = new Vector2(left, bottom);
            rt.offsetMax = new Vector2(-right, -top);
        }

        public static RectTransform Place(RectTransform rt, Vector2 anchor, Vector2 pivot, Vector2 pos, Vector2 size)
        {
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.pivot = pivot;
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            return rt;
        }

        public static RectTransform PlaceTopLeft(RectTransform rt, Vector2 pos, Vector2 size)
        {
            return Place(rt, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(pos.x, -pos.y), size);
        }

        public static RectTransform PlaceTopRight(RectTransform rt, Vector2 pos, Vector2 size)
        {
            return Place(rt, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-pos.x, -pos.y), size);
        }

        public static RectTransform PlaceBottomLeft(RectTransform rt, Vector2 pos, Vector2 size)
        {
            return Place(rt, new Vector2(0f, 0f), new Vector2(0f, 0f), pos, size);
        }

        public static RectTransform PlaceBottomRight(RectTransform rt, Vector2 pos, Vector2 size)
        {
            return Place(rt, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-pos.x, pos.y), size);
        }

        public static Image Image(Transform parent, string name, Color color, Sprite sprite = null)
        {
            RectTransform rt = Rect(parent, name);
            Image img = rt.gameObject.AddComponent<Image>();
            img.color = color;
            img.sprite = sprite;
            img.raycastTarget = false;
            return img;
        }

        public static Image Panel(Transform parent, string name, Color color, float radius = 22f, bool border = true,
            float borderAlpha = 0.1f, bool raycast = false)
        {
            RectTransform rt = Rect(parent, name);
            Image img = rt.gameObject.AddComponent<Image>();
            img.sprite = radius >= 18f ? UISprites.Rounded : UISprites.RoundedSmall;
            img.type = UnityEngine.UI.Image.Type.Sliced;
            img.color = color;
            img.raycastTarget = raycast;

            if (border)
            {
                Image b = Image(rt, "Border", new Color(1f, 1f, 1f, borderAlpha),
                    radius >= 18f ? UISprites.Border : UISprites.BorderSmall);
                b.type = UnityEngine.UI.Image.Type.Sliced;
                Fill(b.rectTransform);
            }
            return img;
        }

        public static Text Label(Transform parent, string content, int size, Color color, TextAnchor anchor,
            Font font = null, bool shadow = true)
        {
            RectTransform rt = Rect(parent, "Label");
            Text t = rt.gameObject.AddComponent<Text>();
            t.text = content;
            t.font = font != null ? font : UITheme.Body;
            t.fontSize = size;
            t.color = color;
            t.alignment = anchor;
            t.horizontalOverflow = HorizontalWrapMode.Overflow;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.raycastTarget = false;
            t.supportRichText = true;

            if (shadow)
            {
                Shadow s = rt.gameObject.AddComponent<Shadow>();
                s.effectColor = new Color(0f, 0f, 0f, 0.62f);
                s.effectDistance = new Vector2(0f, -2f);
            }
            return t;
        }

        public static Text LabelFill(Transform parent, string content, int size, Color color, TextAnchor anchor,
            Font font = null, float padding = 0f)
        {
            Text t = Label(parent, content, size, color, anchor, font);
            Fill(t.rectTransform, padding, padding, padding, padding);
            return t;
        }

        public static Button MakeButton(Transform parent, string label, Vector2 size, bool primary, UnityAction onClick)
        {
            RectTransform rt = Rect(parent, "Button_" + label);
            rt.sizeDelta = size;

            Image bg = rt.gameObject.AddComponent<Image>();
            bg.sprite = UISprites.Rounded;
            bg.type = UnityEngine.UI.Image.Type.Sliced;
            bg.color = primary ? UITheme.Accent : UITheme.Panel2;

            Button btn = rt.gameObject.AddComponent<Button>();
            btn.targetGraphic = bg;
            btn.transition = Selectable.Transition.None;
            if (onClick != null) btn.onClick.AddListener(onClick);

            if (primary)
            {
                Image glow = Image(rt, "Glow", new Color(UITheme.Accent.r, UITheme.Accent.g, UITheme.Accent.b, 0.16f), UISprites.SoftGlow);
                Fill(glow.rectTransform, -26, -26, -26, -26);
            }
            else
            {
                Image border = Image(rt, "Border", new Color(1f, 1f, 1f, 0.14f), UISprites.Border);
                border.type = UnityEngine.UI.Image.Type.Sliced;
                Fill(border.rectTransform);

                Image bar = Image(rt, "AccentBar", UITheme.Accent, UISprites.Pill);
                bar.rectTransform.anchorMin = new Vector2(0f, 0.5f);
                bar.rectTransform.anchorMax = new Vector2(0f, 0.5f);
                bar.rectTransform.pivot = new Vector2(0f, 0.5f);
                bar.rectTransform.anchoredPosition = new Vector2(0f, 0f);
                bar.rectTransform.sizeDelta = new Vector2(4f, size.y * 0.52f);
            }

            Text text = Label(rt, label, primary ? 30 : 26, primary ? UITheme.Bg : UITheme.Text,
                TextAnchor.MiddleCenter, UITheme.Heading);
            Fill(text.rectTransform, 18, 4, 18, 4);

            UIButtonFX fx = rt.gameObject.AddComponent<UIButtonFX>();
            fx.background = bg;
            fx.label = text;
            fx.baseBg = bg.color;
            fx.hoverBg = primary ? UITheme.Hex("FF8A3D") : UITheme.Panel3;
            fx.baseText = text.color;
            fx.hoverText = primary ? UITheme.Bg : UITheme.White;
            fx.hoverScale = 1.035f;
            return btn;
        }

        public static Button MakeIconButton(Transform parent, Sprite icon, Vector2 size, UnityAction onClick,
            Color? tint = null, bool flip = false)
        {
            RectTransform rt = Rect(parent, "IconButton");
            rt.sizeDelta = size;

            Image bg = rt.gameObject.AddComponent<Image>();
            bg.sprite = UISprites.Rounded;
            bg.type = UnityEngine.UI.Image.Type.Sliced;
            bg.color = UITheme.Panel2;

            Image border = Image(rt, "Border", new Color(1f, 1f, 1f, 0.12f), UISprites.Border);
            border.type = UnityEngine.UI.Image.Type.Sliced;
            Fill(border.rectTransform);

            Image ic = Image(rt, "Icon", tint ?? UITheme.Text, icon);
            float s = Mathf.Min(size.x, size.y) * 0.46f;
            ic.rectTransform.sizeDelta = new Vector2(s, s);
            if (flip) ic.rectTransform.localScale = new Vector3(-1f, 1f, 1f);

            Button btn = rt.gameObject.AddComponent<Button>();
            btn.targetGraphic = bg;
            btn.transition = Selectable.Transition.None;
            if (onClick != null) btn.onClick.AddListener(onClick);

            UIButtonFX fx = rt.gameObject.AddComponent<UIButtonFX>();
            fx.background = bg;
            fx.baseBg = bg.color;
            fx.hoverBg = UITheme.Panel3;
            fx.hoverScale = 1.06f;
            return btn;
        }

        public static Image ProgressBar(Transform parent, Vector2 size, Color bgColor, Color fillColor)
        {
            Image bg = Panel(parent, "BarBg", bgColor, 12f, false);
            bg.rectTransform.sizeDelta = size;
            bg.sprite = UISprites.Pill;
            bg.type = UnityEngine.UI.Image.Type.Sliced;
            Mask mask = bg.gameObject.AddComponent<Mask>();
            mask.showMaskGraphic = true;

            Image fill = Image(bg.rectTransform, "Fill", fillColor, null);
            fill.type = UnityEngine.UI.Image.Type.Filled;
            fill.fillMethod = UnityEngine.UI.Image.FillMethod.Horizontal;
            fill.fillOrigin = 0;
            fill.fillAmount = 0f;
            Fill(fill.rectTransform, 0, 0, 0, 0);
            return fill;
        }

        public static Image Icon(Transform parent, Sprite sprite, Color color, float size)
        {
            Image img = Image(parent, "Icon", color, sprite);
            img.rectTransform.sizeDelta = new Vector2(size, size);
            return img;
        }

        public static void AddScreenEdges(Transform canvas, float topHeight = 190f, float bottomHeight = 250f)
        {
            Image top = Image(canvas, "TopShade", new Color(0f, 0f, 0f, 0.55f), UISprites.Gradient(new Color(0f, 0f, 0f, 1f), new Color(0f, 0f, 0f, 0f)));
            top.rectTransform.anchorMin = new Vector2(0f, 1f);
            top.rectTransform.anchorMax = new Vector2(1f, 1f);
            top.rectTransform.pivot = new Vector2(0.5f, 1f);
            top.rectTransform.anchoredPosition = Vector2.zero;
            top.rectTransform.sizeDelta = new Vector2(0f, topHeight);

            Image bottom = Image(canvas, "BottomShade", new Color(0f, 0f, 0f, 0.8f), UISprites.Gradient(new Color(0f, 0f, 0f, 0f), new Color(0f, 0f, 0f, 1f)));
            bottom.rectTransform.anchorMin = new Vector2(0f, 0f);
            bottom.rectTransform.anchorMax = new Vector2(1f, 0f);
            bottom.rectTransform.pivot = new Vector2(0.5f, 0f);
            bottom.rectTransform.anchoredPosition = Vector2.zero;
            bottom.rectTransform.sizeDelta = new Vector2(0f, bottomHeight);
        }

        public static RawImage AddNoise(Transform canvas, float alpha = 0.035f)
        {
            RectTransform rt = Rect(canvas, "Noise");
            Fill(rt);
            RawImage raw = rt.gameObject.AddComponent<RawImage>();
            raw.texture = UISprites.Noise.texture;
            raw.color = new Color(1f, 1f, 1f, alpha);
            raw.uvRect = new Rect(0f, 0f, 10f, 6f);
            raw.raycastTarget = false;
            return raw;
        }

        public static Image Glow(Transform parent, Color color, Vector2 size, Vector2 pos)
        {
            Image img = Image(parent, "Glow", color, UISprites.SoftGlow);
            img.rectTransform.anchoredPosition = pos;
            img.rectTransform.sizeDelta = size;
            return img;
        }

        public static void AddCornerBrackets(Transform parent, Color color, float length = 22f, float thickness = 2f)
        {
            Vector2[] anchors = { new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, 0f), new Vector2(1f, 0f) };
            foreach (Vector2 a in anchors)
            {
                Image h = Image(parent, "BracketH", color, UISprites.Pill);
                h.rectTransform.anchorMin = a;
                h.rectTransform.anchorMax = a;
                h.rectTransform.pivot = a;
                h.rectTransform.sizeDelta = new Vector2(length, thickness);
                h.rectTransform.anchoredPosition = new Vector2(a.x == 0f ? 0f : 0f, a.y == 0f ? 0f : 0f);

                Image v = Image(parent, "BracketV", color, UISprites.Pill);
                v.rectTransform.anchorMin = a;
                v.rectTransform.anchorMax = a;
                v.rectTransform.pivot = a;
                v.rectTransform.sizeDelta = new Vector2(thickness, length);
                v.rectTransform.anchoredPosition = Vector2.zero;
            }
        }

        public static string FormatTime(float seconds)
        {
            if (seconds < 0f) seconds = 0f;
            int minutes = Mathf.FloorToInt(seconds / 60f);
            float rest = seconds - minutes * 60f;
            return string.Format("{0:00}:{1:00.00}", minutes, rest);
        }

        public static StepsWidget BuildSteps(Transform parent, string[] labels, int active, Vector2 pos)
        {
            GameObject root = new GameObject("Steps");
            RectTransform rootRt = root.AddComponent<RectTransform>();
            rootRt.SetParent(parent, false);
            PlaceTopLeft(rootRt, pos, new Vector2(760f, 46f));

            StepsWidget widget = new StepsWidget();
            widget.dots = new Image[labels.Length];
            widget.labels = new Text[labels.Length];
            widget.connectors = new Image[labels.Length - 1];

            float x = 0f;
            for (int i = 0; i < labels.Length; i++)
            {
                Image dot = Image(rootRt, "Dot" + i, UITheme.Panel3, UISprites.Circle);
                dot.rectTransform.anchorMin = new Vector2(0f, 0.5f);
                dot.rectTransform.anchorMax = new Vector2(0f, 0.5f);
                dot.rectTransform.pivot = new Vector2(0f, 0.5f);
                dot.rectTransform.anchoredPosition = new Vector2(x, 0f);
                dot.rectTransform.sizeDelta = new Vector2(30f, 30f);

                Text num = Label(dot.rectTransform, (i + 1).ToString(), 16, UITheme.Bg, TextAnchor.MiddleCenter, UITheme.Heading);
                Fill(num.rectTransform);

                Text lab = Label(rootRt, labels[i], 19, UITheme.Muted, TextAnchor.MiddleLeft, UITheme.Semi);
                lab.rectTransform.anchorMin = new Vector2(0f, 0.5f);
                lab.rectTransform.anchorMax = new Vector2(0f, 0.5f);
                lab.rectTransform.pivot = new Vector2(0f, 0.5f);
                lab.rectTransform.anchoredPosition = new Vector2(x + 38f, 0f);
                lab.rectTransform.sizeDelta = new Vector2(126f, 30f);

                widget.dots[i] = dot;
                widget.labels[i] = lab;

                if (i < labels.Length - 1)
                {
                    Image line = Image(rootRt, "Line" + i, new Color(1f, 1f, 1f, 0.16f), UISprites.Pill);
                    line.rectTransform.anchorMin = new Vector2(0f, 0.5f);
                    line.rectTransform.anchorMax = new Vector2(0f, 0.5f);
                    line.rectTransform.pivot = new Vector2(0f, 0.5f);
                    line.rectTransform.anchoredPosition = new Vector2(x + 168f, 0f);
                    line.rectTransform.sizeDelta = new Vector2(68f, 2f);
                    widget.connectors[i] = line;
                }

                x += 252f;
            }

            widget.SetStep(active);
            return widget;
        }
    }

    public class StepsWidget
    {
        public Image[] dots;
        public Text[] labels;
        public Image[] connectors;

        public void SetStep(int active)
        {
            for (int i = 0; i < dots.Length; i++)
            {
                bool done = i < active;
                bool current = i == active;
                dots[i].color = current ? UITheme.Accent : (done ? UITheme.Good : UITheme.Panel3);
                if (dots[i].transform.childCount > 0)
                {
                    Text t = dots[i].transform.GetChild(0).GetComponent<Text>();
                    if (t != null) t.color = current || done ? UITheme.Bg : UITheme.Muted;
                }
                labels[i].color = current ? UITheme.Text : (done ? UITheme.Good : UITheme.Muted);
                if (i < connectors.Length)
                    connectors[i].color = i < active ? new Color(UITheme.Good.r, UITheme.Good.g, UITheme.Good.b, 0.5f) : new Color(1f, 1f, 1f, 0.12f);
            }
        }
    }

    public class UIButtonFX : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        public Image background;
        public Text label;
        public Image glow;
        public Color baseBg;
        public Color hoverBg;
        public Color baseText;
        public Color hoverText;
        public float hoverScale = 1.04f;
        public bool interactable = true;

        private bool _hover;
        private bool _down;
        private Vector3 _targetScale = Vector3.one;
        private Button _button;

        private void Awake()
        {
            _button = GetComponent<Button>();
            if (background != null) baseBg = background.color;
            if (label != null) baseText = label.color;
        }

        private void OnEnable()
        {
            transform.localScale = Vector3.one;
            _targetScale = Vector3.one;
            _hover = false;
            _down = false;
            if (background != null) background.color = baseBg;
            if (label != null) label.color = baseText;
        }

        private void Update()
        {
            float k = 1f - Mathf.Exp(-14f * Time.unscaledDeltaTime);
            transform.localScale = Vector3.Lerp(transform.localScale, _targetScale, k);
            if (background != null)
                background.color = Color.Lerp(background.color, _hover && interactable ? hoverBg : baseBg, k);
            if (label != null)
                label.color = Color.Lerp(label.color, _hover && interactable ? hoverText : baseText, k);
            if (glow != null)
            {
                Color c = glow.color;
                c.a = Mathf.Lerp(c.a, _hover && interactable ? 0.35f : 0.16f, k);
                glow.color = c;
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!interactable) return;
            _hover = true;
            _targetScale = Vector3.one * hoverScale;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _hover = false;
            _down = false;
            _targetScale = Vector3.one;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!interactable) return;
            _down = true;
            _targetScale = Vector3.one * 0.97f;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!interactable) return;
            _down = false;
            _targetScale = Vector3.one * (_hover ? hoverScale : 1f);
            if (_button == null || _button.IsInteractable())
                AudioManager.PlayUiClickStatic();
        }

        public void SetInteractable(bool value)
        {
            interactable = value;
            if (background != null) background.color = value ? baseBg : new Color(baseBg.r, baseBg.g, baseBg.b, 0.4f);
        }
    }

    public class UICard : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public Image background;
        public Image border;
        public Image glow;
        public Image accentBar;
        public Image thumbnail;
        public Text title;
        public Text subtitle;
        public Text tagLabel;
        public RectTransform checkBadge;
        public Color accent = UITheme.Accent;
        public bool selected;

        private bool _hover;
        private Vector3 _baseScale = Vector3.one;

        private void OnEnable()
        {
            transform.localScale = Vector3.one;
        }

        private void Update()
        {
            float k = 1f - Mathf.Exp(-13f * Time.unscaledDeltaTime);
            float targetScale = selected ? 1.035f : (_hover ? 1.015f : 1f);
            transform.localScale = Vector3.Lerp(transform.localScale, _baseScale * targetScale, k);

            if (background != null)
            {
                Color target = selected ? UITheme.Panel3 : (_hover ? UITheme.Panel2 : UITheme.Panel);
                target.a = 0.96f;
                background.color = Color.Lerp(background.color, target, k);
            }
            if (border != null)
            {
                Color target = selected ? new Color(accent.r, accent.g, accent.b, 0.95f)
                    : new Color(1f, 1f, 1f, _hover ? 0.26f : 0.09f);
                border.color = Color.Lerp(border.color, target, k);
            }
            if (glow != null)
            {
                Color c = accent;
                c.a = selected ? 0.26f : (_hover ? 0.12f : 0f);
                glow.color = Color.Lerp(glow.color, c, k);
            }
            if (checkBadge != null)
            {
                CanvasGroup cg = checkBadge.GetComponent<CanvasGroup>();
                if (cg != null) cg.alpha = Mathf.Lerp(cg.alpha, selected ? 1f : 0f, k);
            }
            if (accentBar != null)
                accentBar.color = Color.Lerp(accentBar.color, selected ? accent : new Color(1f, 1f, 1f, 0.12f), k);
        }

        public void SetSelected(bool value)
        {
            selected = value;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _hover = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _hover = false;
        }
    }
}
