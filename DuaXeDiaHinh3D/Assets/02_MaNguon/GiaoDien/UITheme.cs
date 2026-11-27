using UnityEngine;

namespace DuaXe3D
{
    /// <summary>Bảng màu + font dùng chung cho toàn bộ giao diện.</summary>
    public static class UITheme
    {
        public static readonly Color Bg = Hex("070B12");
        public static readonly Color Bg2 = Hex("0C121D");
        public static readonly Color Panel = Hex("111A28");
        public static readonly Color Panel2 = Hex("172335");
        public static readonly Color Panel3 = Hex("1E2D42");
        public static readonly Color Accent = Hex("FF6A1A");
        public static readonly Color AccentDeep = Hex("D94E00");
        public static readonly Color Accent2 = Hex("22D3EE");
        public static readonly Color Text = Hex("F3F7FC");
        public static readonly Color Muted = Hex("8EA2BA");
        public static readonly Color Good = Hex("35E39B");
        public static readonly Color Bad = Hex("FF4967");
        public static readonly Color Gold = Hex("FFC53D");
        public static readonly Color Silver = Hex("CBD6E4");
        public static readonly Color Bronze = Hex("D98A54");
        public static readonly Color White = Color.white;

        public static Font Display;
        public static Font Heading;
        public static Font Semi;
        public static Font Body;

        private static bool _initialized;

        public static void Init()
        {
            if (_initialized) return;
            _initialized = true;

            Display = LoadFont("Fonts/BarlowCondensed-Bold");
            Heading = LoadFont("Fonts/BeVietnamPro-ExtraBold");
            Semi = LoadFont("Fonts/BeVietnamPro-SemiBold");
            Body = LoadFont("Fonts/BeVietnamPro-Regular");
        }

        private static Font LoadFont(string path)
        {
            Font f = Resources.Load<Font>(path);
            if (f != null) return f;

            try { f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); } catch { }
            if (f == null)
            {
                try { f = Resources.GetBuiltinResource<Font>("Arial.ttf"); } catch { }
            }
            if (f == null)
            {
                f = Font.CreateDynamicFontFromOSFont("Segoe UI", 24);
            }
            return f;
        }

        public static Color Hex(string hex)
        {
            Color c;
            if (ColorUtility.TryParseHtmlString("#" + hex, out c)) return c;
            return Color.magenta;
        }

        public static Color Alpha(Color c, float a)
        {
            c.a = a;
            return c;
        }
    }
}
