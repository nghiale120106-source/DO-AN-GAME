using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace DuaXe3D
{
    public enum GroundStyle
    {
        Grass,
        Dirt,
        Rock,
        Sand,
        Snow
    }

    /// <summary>
    /// Sinh toàn bộ texture bằng code: đất, cỏ, đá, cát, tuyết, đường đua,
    /// bầu trời, thumbnail địa hình... Không phụ thuộc asset ngoài.
    /// </summary>
    public static class ProcTex
    {
        private static readonly Dictionary<string, Texture2D> Cache = new Dictionary<string, Texture2D>();

        private static float Fract(float v) { return v - Mathf.Floor(v); }

        private static float TileNoise(float u, float v, float scale, float seed)
        {
            float x = u * scale;
            float y = v * scale;
            float n = Mathf.PerlinNoise(x + seed, y + seed);
            float nx = Mathf.PerlinNoise(x - scale + seed, y + seed);
            float ny = Mathf.PerlinNoise(x + seed, y - scale + seed);
            float nxy = Mathf.PerlinNoise(x - scale + seed, y - scale + seed);
            return Mathf.Lerp(Mathf.Lerp(n, nx, u), Mathf.Lerp(ny, nxy, u), v);
        }

        private static float Fbm(float u, float v, float scale, float seed, int octaves = 4)
        {
            float total = 0f;
            float amp = 1f;
            float norm = 0f;
            for (int i = 0; i < octaves; i++)
            {
                total += TileNoise(u, v, scale * (1f + i * 1.7f), seed + i * 31.7f) * amp;
                norm += amp;
                amp *= 0.5f;
            }
            return total / Mathf.Max(0.0001f, norm);
        }

        public static Texture2D Ground(string name, Color baseColor, GroundStyle style, int seed)
        {
            Texture2D cached;
            if (Cache.TryGetValue(name, out cached) && cached != null) return cached;

            const int size = 512;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, true);
            tex.name = name;
            tex.wrapMode = TextureWrapMode.Repeat;
            tex.filterMode = FilterMode.Trilinear;
            tex.anisoLevel = 9;

            Color[] px = new Color[size * size];
            System.Random rnd = new System.Random(seed * 7919 + 13);
            float s = seed * 0.137f;

            for (int y = 0; y < size; y++)
            {
                float v = y / (float)size;
                for (int x = 0; x < size; x++)
                {
                    float u = x / (float)size;
                    float n1 = Fbm(u, v, 6f, s, 4);
                    float n2 = Fbm(u, v, 22f, s + 7.3f, 3);
                    float n3 = Mathf.PerlinNoise(u * 90f + s, v * 90f + s);

                    float shade = 0.78f + n1 * 0.34f + n2 * 0.16f;
                    Color c = baseColor * shade;

                    switch (style)
                    {
                        case GroundStyle.Grass:
                        {
                            float blade = Mathf.Pow(Mathf.Abs(Mathf.Sin((u * 260f + n2 * 22f)) * Mathf.Sin(v * 230f + n1 * 18f)), 16f);
                            c = Color.Lerp(c, baseColor * 1.32f + new Color(0.05f, 0.08f, 0.01f), blade * 0.5f);
                            if (n3 > 0.83f) c = Color.Lerp(c, new Color(0.62f, 0.55f, 0.24f), 0.35f);
                            break;
                        }
                        case GroundStyle.Dirt:
                        {
                            c *= 0.9f + n2 * 0.2f;
                            if (n3 > 0.86f) c = Color.Lerp(c, baseColor * 0.62f, 0.6f);
                            float crack = Mathf.Abs(Mathf.Sin((u + v) * 34f + n1 * 9f));
                            if (crack < 0.03f) c *= 0.66f;
                            break;
                        }
                        case GroundStyle.Rock:
                        {
                            float facet = Mathf.Floor(n1 * 7f + n2 * 3f) / 7f;
                            c = Color.Lerp(c * 0.8f, c * 1.22f, facet);
                            if (n3 > 0.88f) c = Color.Lerp(c, new Color(0.72f, 0.7f, 0.66f), 0.3f);
                            break;
                        }
                        case GroundStyle.Sand:
                        {
                            float ripple = Mathf.Sin((v * 110f + n1 * 16f)) * 0.5f + 0.5f;
                            c *= 0.94f + ripple * 0.09f;
                            if (n3 > 0.9f) c = Color.Lerp(c, new Color(0.95f, 0.9f, 0.78f), 0.5f);
                            break;
                        }
                        case GroundStyle.Snow:
                        {
                            c = Color.Lerp(baseColor * 0.9f, new Color(1f, 1f, 1f), n1 * 0.45f);
                            if (n3 > 0.93f) c = Color.Lerp(c, new Color(0.72f, 0.86f, 1f), 0.4f);
                            break;
                        }
                    }

                    c.a = 1f;
                    px[y * size + x] = c;
                }
            }

            tex.SetPixels(px);
            tex.Apply(true, false);
            Cache[name] = tex;
            return tex;
        }

        public static Texture2D GroundNormal(string name, Color baseColor, GroundStyle style, int seed)
        {
            string key = name + "_n";
            Texture2D cached;
            if (Cache.TryGetValue(key, out cached) && cached != null) return cached;

            Texture2D src = Ground(name, baseColor, style, seed);
            int size = src.width;
            Color[] px = src.GetPixels();
            float[] lum = new float[size * size];
            for (int i = 0; i < lum.Length; i++)
                lum[i] = px[i].r * 0.3f + px[i].g * 0.59f + px[i].b * 0.11f;

            Texture2D normal = new Texture2D(size, size, TextureFormat.RGBA32, true);
            normal.name = key;
            normal.wrapMode = TextureWrapMode.Repeat;
            normal.filterMode = FilterMode.Trilinear;
            normal.anisoLevel = 9;

            Color[] npx = new Color[size * size];
            float strength = style == GroundStyle.Rock ? 2.8f : (style == GroundStyle.Sand ? 1.1f : 1.8f);
            for (int y = 0; y < size; y++)
            {
                int ym = (y - 1 + size) % size;
                int yp = (y + 1) % size;
                for (int x = 0; x < size; x++)
                {
                    int xm = (x - 1 + size) % size;
                    int xp = (x + 1) % size;
                    float dx = (lum[y * size + xp] - lum[y * size + xm]) * strength;
                    float dy = (lum[yp * size + x] - lum[ym * size + x]) * strength;
                    Vector3 n = new Vector3(-dx, -dy, 1f).normalized;
                    npx[y * size + x] = new Color(n.x * 0.5f + 0.5f, n.y * 0.5f + 0.5f, n.z * 0.5f + 0.5f, 1f);
                }
            }
            normal.SetPixels(npx);
            normal.Apply(true, false);
            Cache[key] = normal;
            return normal;
        }

        public static Texture2D Road(string name, TerrainTheme theme, int seed)
        {
            Texture2D cached;
            if (Cache.TryGetValue(name, out cached) && cached != null) return cached;

            const int size = 512;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, true);
            tex.name = name;
            tex.wrapMode = TextureWrapMode.Repeat;
            tex.filterMode = FilterMode.Trilinear;
            tex.anisoLevel = 9;

            Color[] px = new Color[size * size];
            bool dirt = theme.Kind == ThemeKind.Desert || theme.Kind == ThemeKind.Mud;
            Color baseCol = theme.Kind == ThemeKind.Mud
                ? theme.DirtColor * 0.82f
                : (dirt ? theme.DirtColor * 0.92f : new Color(0.22f, 0.215f, 0.205f));
            Color gravel = theme.Kind == ThemeKind.Mud
                ? new Color(0.22f, 0.16f, 0.10f)
                : (dirt ? theme.SandColor * 0.85f : new Color(0.32f, 0.31f, 0.29f));
            float s = seed * 0.21f;

            for (int y = 0; y < size; y++)
            {
                float v = y / (float)size;
                for (int x = 0; x < size; x++)
                {
                    float u = x / (float)size;
                    float n = Fbm(u, v, 9f, s, 4);
                    float n2 = Fbm(u, v, 40f, s + 4.7f, 2);
                    float grain = Mathf.PerlinNoise(u * 160f + s, v * 160f + s);

                    Color c = baseCol * (0.78f + n * 0.34f + n2 * 0.2f);
                    if (grain > 0.74f) c = Color.Lerp(c, gravel, 0.55f);
                    if (grain < 0.16f) c *= 0.72f;

                    // Vệt bánh xe mờ hai bên
                    float track = Mathf.Exp(-Mathf.Pow((u - 0.29f) / 0.075f, 2f)) +
                                  Mathf.Exp(-Mathf.Pow((u - 0.71f) / 0.075f, 2f));
                    c *= 1f - track * 0.14f * (0.5f + n2);

                    // Mép đường sẫm hơn
                    float edge = Mathf.SmoothStep(0f, 0.09f, u) * Mathf.SmoothStep(0f, 0.09f, 1f - u);
                    c *= Mathf.Lerp(0.74f, 1f, edge);

                    c.a = 1f;
                    px[y * size + x] = c;
                }
            }

            tex.SetPixels(px);
            tex.Apply(true, false);
            Cache[name] = tex;
            return tex;
        }

        public static Texture2D Checker(int cells)
        {
            string key = "checker_" + cells;
            Texture2D cached;
            if (Cache.TryGetValue(key, out cached) && cached != null) return cached;

            int size = Mathf.Max(64, cells * 16);
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.name = key;
            tex.wrapMode = TextureWrapMode.Repeat;
            tex.filterMode = FilterMode.Point;
            Color32[] px = new Color32[size * size];
            int cell = Mathf.Max(1, size / cells);
            Color32 a = new Color32(238, 238, 238, 255);
            Color32 b = new Color32(22, 24, 28, 255);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    bool even = ((x / cell) + (y / cell)) % 2 == 0;
                    px[y * size + x] = even ? a : b;
                }
            }
            tex.SetPixels32(px);
            tex.Apply(false, false);
            Cache[key] = tex;
            return tex;
        }

        public static Texture2D SoftCircle(int size = 64)
        {
            string key = "soft_" + size;
            Texture2D cached;
            if (Cache.TryGetValue(key, out cached) && cached != null) return cached;

            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.name = key;
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;
            Color32[] px = new Color32[size * size];
            float half = size * 0.5f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(half, half)) / half;
                    float a = Mathf.Clamp01(1f - d);
                    a = a * a;
                    px[y * size + x] = new Color(1f, 1f, 1f, a);
                }
            }
            tex.SetPixels32(px);
            tex.Apply(false, false);
            Cache[key] = tex;
            return tex;
        }

        public static Texture2D StudioGrid()
        {
            const string key = "studio_grid";
            Texture2D cached;
            if (Cache.TryGetValue(key, out cached) && cached != null) return cached;

            const int size = 512;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, true);
            tex.name = key;
            tex.wrapMode = TextureWrapMode.Repeat;
            tex.filterMode = FilterMode.Trilinear;
            Color[] px = new Color[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float u = x / (float)size;
                    float v = y / (float)size;
                    float n = Fbm(u, v, 10f, 4.2f, 3);
                    Color c = new Color(0.055f, 0.065f, 0.085f) * (0.82f + n * 0.3f);
                    int gx = x % 64;
                    int gy = y % 64;
                    if (gx < 2 || gy < 2)
                        c = Color.Lerp(c, new Color(0.19f, 0.24f, 0.32f), 0.55f);
                    px[y * size + x] = c;
                }
            }
            tex.SetPixels(px);
            tex.Apply(true, false);
            Cache[key] = tex;
            return tex;
        }

        public static Texture2D Thumbnail(TerrainTheme theme)
        {
            string key = "thumb_" + theme.Index;
            Texture2D cached;
            if (Cache.TryGetValue(key, out cached) && cached != null) return cached;

            const int w = 640;
            const int h = 360;
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.name = key;
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;
            Color32[] px = new Color32[w * h];

            Color zenith = Color.Lerp(theme.SkyTint, new Color(0.16f, 0.36f, 0.66f), 0.42f);
            Color horizon = Color.Lerp(theme.SkyTint, theme.FogColor, 0.7f);
            Color ground = theme.GroundColor;
            Color groundDark = ground * 0.62f;

            for (int y = 0; y < h; y++)
            {
                float t = y / (float)(h - 1);
                for (int x = 0; x < w; x++)
                {
                    Color c;
                    if (t > 0.46f)
                    {
                        float k = (t - 0.46f) / 0.54f;
                        c = Color.Lerp(horizon, zenith, Mathf.Pow(k, 0.72f));
                    }
                    else
                    {
                        float k = 1f - t / 0.46f;
                        c = Color.Lerp(horizon, ground, Mathf.SmoothStep(0.05f, 0.9f, k));
                        c = Color.Lerp(c, groundDark, Mathf.SmoothStep(0.45f, 1f, k) * 0.45f);
                    }

                    // Mặt trời
                    float sx = w * 0.78f;
                    float sy = h * 0.72f;
                    float sd = Vector2.Distance(new Vector2(x, y), new Vector2(sx, sy));
                    if (sd < 62f) c = Color.Lerp(c, new Color(1f, 0.94f, 0.78f), Mathf.Clamp01(1f - sd / 62f) * 0.9f);
                    if (sd < 22f) c = Color.Lerp(c, Color.white, Mathf.Clamp01(1f - sd / 22f));

                    // Mây mờ
                    float cl = Mathf.PerlinNoise(x * 0.004f + theme.Index * 3.1f, y * 0.012f + 1.7f);
                    if (cl > 0.62f && t > 0.52f)
                        c = Color.Lerp(c, new Color(1f, 1f, 1f, 1f), (cl - 0.62f) * 1.6f);

                    px[y * w + x] = c;
                }
            }

            // Núi xa
            DrawMountains(px, w, h, 0.47f, 0.12f, 0.0013f, Color.Lerp(theme.FogColor, theme.RockColor, 0.45f), 2.3f);
            DrawMountains(px, w, h, 0.44f, 0.075f, 0.0024f, Color.Lerp(theme.RockColor, theme.GroundColor, 0.35f), 6.1f);

            if (theme.Kind == ThemeKind.Mountain)
            {
                DrawMountains(px, w, h, 0.52f, 0.2f, 0.0019f, Color.Lerp(theme.RockColor, Color.white, 0.35f), 9.4f);
            }

            // Đường uốn lượn
            float bottomY = 4f;
            float topY = h * 0.47f;
            Vector2 p0 = new Vector2(w * 0.18f, bottomY);
            Vector2 p1 = new Vector2(w * 0.92f, h * 0.2f);
            Vector2 p2 = new Vector2(w * 0.35f, h * 0.33f);
            Vector2 p3 = new Vector2(w * 0.56f, topY);
            int steps = 160;
            Vector2 prev = p0;
            for (int i = 1; i <= steps; i++)
            {
                float t = i / (float)steps;
                Vector2 p = Bezier(p0, p1, p2, p3, t);
                float width = Mathf.Lerp(30f, 2.2f, Mathf.Pow(t, 0.65f));
                Color roadCol = Color.Lerp(theme.DirtColor * 0.8f, theme.FogColor, t * 0.55f);
                DrawLine(px, w, h, prev.x, prev.y, p.x, p.y, roadCol, width);
                if (t < 0.9f)
                {
                    DrawLine(px, w, h, prev.x, prev.y, p.x, p.y, Color.Lerp(roadCol, Color.white, 0.45f), Mathf.Max(0.7f, width * 0.045f));
                }
                prev = p;
            }

            // Cây / xương rồng / cối xay
            System.Random rnd = new System.Random(theme.Index * 977 + 31);
            int props = theme.Kind == ThemeKind.Forest ? 34 : 16;
            for (int i = 0; i < props; i++)
            {
                float x = (float)rnd.NextDouble() * w;
                float y = 12f + (float)rnd.NextDouble() * (h * 0.36f);
                if (x > w * 0.12f && x < w * 0.92f && y < h * 0.2f) continue;
                float scale = 0.6f + (float)rnd.NextDouble() * 1.1f;
                if (theme.Kind == ThemeKind.Desert)
                {
                    Color cc = Color.Lerp(theme.PropColor, theme.GroundColor, 0.2f);
                    DrawLine(px, w, h, x, y, x, y + 16f * scale, cc, 2.4f * scale);
                    DrawLine(px, w, h, x, y + 9f * scale, x - 5f * scale, y + 13f * scale, cc, 1.8f * scale);
                    DrawLine(px, w, h, x, y + 11f * scale, x + 5f * scale, y + 15f * scale, cc, 1.8f * scale);
                }
                else if (theme.Kind == ThemeKind.Mud && i < 5)
                {
                    FillCircle(px, w, h, x, y, 16f * scale, new Color(0.20f, 0.14f, 0.09f));
                    FillCircle(px, w, h, x + 2f, y + 1f, 9f * scale, new Color(0.34f, 0.28f, 0.22f));
                }
                else
                {
                    Color trunk = new Color(0.24f, 0.16f, 0.1f);
                    DrawLine(px, w, h, x, y, x, y + 12f * scale, trunk, 2.2f * scale);
                    FillCircle(px, w, h, x, y + 16f * scale, 8f * scale, theme.PropColor);
                    FillCircle(px, w, h, x - 4f * scale, y + 13f * scale, 6f * scale, theme.PropColor2);
                    FillCircle(px, w, h, x + 4f * scale, y + 12f * scale, 5.5f * scale, theme.PropColor);
                }
            }

            // Viền tối nhẹ
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    float dx = (x - w * 0.5f) / (w * 0.5f);
                    float dy = (y - h * 0.5f) / (h * 0.5f);
                    float v = Mathf.Clamp01(1f - (dx * dx + dy * dy) * 0.34f);
                    int idx = y * w + x;
                    Color c = px[idx];
                    px[idx] = c * v;
                }
            }

            tex.SetPixels32(px);
            tex.Apply(false, false);
            Cache[key] = tex;
            return tex;
        }

        public static Texture2D CarThumb(CarSpec spec)
        {
            string key = "carthumb_" + spec.Index;
            Texture2D cached;
            if (Cache.TryGetValue(key, out cached) && cached != null) return cached;

            const int w = 480;
            const int h = 270;
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.name = key;
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;
            Color32[] px = new Color32[w * h];

            Color top = new Color(0.06f, 0.08f, 0.12f);
            Color bottom = new Color(0.02f, 0.03f, 0.045f);
            for (int y = 0; y < h; y++)
            {
                Color bg = Color.Lerp(bottom, top, y / (float)h);
                for (int x = 0; x < w; x++)
                {
                    float glow = Mathf.Exp(-Mathf.Pow((x - w * 0.55f) / (w * 0.32f), 2f) - Mathf.Pow((y - h * 0.55f) / (h * 0.34f), 2f));
                    Color c = bg + spec.BodyColor * glow * 0.16f;
                    px[y * w + x] = c;
                }
            }

            // Sàn
            for (int y = 0; y < 42; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    float a = 1f - y / 42f;
                    Color c = Color.Lerp(new Color(0.02f, 0.025f, 0.035f), new Color(0.12f, 0.14f, 0.18f), a * 0.5f);
                    px[y * w + x] = Color.Lerp(px[y * w + x], c, 0.8f);
                }
            }

            int baseY = 58;
            float scale = spec.Index == 2 ? 1.08f : 1f;
            Color body = spec.BodyColor;
            Color dark = new Color(0.07f, 0.08f, 0.09f);
            Color accent = spec.AccentColor;

            // Thân xe
            FillRect(px, w, h, 92, baseY + 26, 388, 62, body);
            FillRect(px, w, h, 132, baseY + 72, 292, 40, body * 1.05f);
            FillRect(px, w, h, 150, baseY + 78, 200, 28, new Color(0.16f, 0.26f, 0.34f));
            FillRect(px, w, h, 170, baseY + 84, 120, 18, new Color(0.3f, 0.55f, 0.7f, 1f) * 0.8f);
            FillRect(px, w, h, 86, baseY + 38, 36, 18, dark);
            FillRect(px, w, h, 356, baseY + 38, 34, 18, dark);
            FillRect(px, w, h, 96, baseY + 48, 376, 6, accent);
            FillCircle(px, w, h, 112, baseY + 60, 6, new Color(1f, 0.92f, 0.6f));
            FillRect(px, w, h, 382, baseY + 54, 10, 12, new Color(1f, 0.12f, 0.08f));

            if (spec.Index == 0)
            {
                FillRect(px, w, h, 160, baseY + 104, 120, 22, body * 0.9f);
                FillRect(px, w, h, 172, baseY + 110, 96, 12, new Color(0.35f, 0.55f, 0.65f) * 0.7f);
                FillRect(px, w, h, 120, baseY + 66, 8, 36, dark);
                FillRect(px, w, h, 348, baseY + 66, 8, 36, dark);
                FillCircle(px, w, h, 382, baseY + 88, 22, dark);
                FillCircle(px, w, h, 382, baseY + 88, 12, new Color(0.5f, 0.52f, 0.55f));
            }
            else if (spec.Index == 1)
            {
                FillRect(px, w, h, 120, baseY + 96, 12, 34, accent);
                FillRect(px, w, h, 330, baseY + 96, 12, 34, accent);
                FillRect(px, w, h, 118, baseY + 128, 230, 6, accent);
                FillRect(px, w, h, 320, baseY + 52, 58, 10, accent);
                FillRect(px, w, h, 210, baseY + 30, 68, 26, dark);
            }
            else
            {
                FillRect(px, w, h, 108, baseY + 86, 300, 18, body * 0.92f);
                FillRect(px, w, h, 150, baseY + 112, 220, 12, body * 0.86f);
                FillRect(px, w, h, 132, baseY + 104, 14, 42, dark);
                FillRect(px, w, h, 344, baseY + 104, 14, 42, dark);
            }

            // Bánh xe
            float wheelY = baseY + 6;
            float wheelR = spec.Index == 2 ? 44f : (spec.Index == 1 ? 36f : 34f);
            DrawWheel(px, w, h, 156, (int)wheelY, wheelR, dark, accent);
            DrawWheel(px, w, h, 346, (int)wheelY, wheelR, dark, accent);

            // Bóng đổ
            for (int x = 80; x < 410; x++)
            {
                float a = 1f - Mathf.Abs(x - 245f) / 180f;
                if (a <= 0f) continue;
                for (int y = 8; y < 22; y++)
                {
                    float fa = a * (1f - (y - 8) / 18f) * 0.5f;
                    px[y * w + x] = Color.Lerp(px[y * w + x], Color.black, fa);
                }
            }

            tex.SetPixels32(px);
            tex.Apply(false, false);
            Cache[key] = tex;
            return tex;
        }

        private static void DrawWheel(Color32[] px, int w, int h, int cx, int cy, float r, Color tire, Color rim)
        {
            FillCircle(px, w, h, cx, cy, r, tire);
            FillCircle(px, w, h, cx, cy, r * 0.56f, new Color(0.36f, 0.38f, 0.42f));
            FillCircle(px, w, h, cx, cy, r * 0.3f, rim);
            FillCircle(px, w, h, cx, cy, r * 0.12f, new Color(0.09f, 0.1f, 0.11f));
            for (int i = 0; i < 6; i++)
            {
                float a = i / 6f * Mathf.PI * 2f;
                DrawLine(px, w, h, cx + Mathf.Cos(a) * r * 0.18f, cy + Mathf.Sin(a) * r * 0.18f,
                    cx + Mathf.Cos(a) * r * 0.5f, cy + Mathf.Sin(a) * r * 0.5f, new Color(0.5f, 0.52f, 0.56f), 2f);
            }
        }

        public static Texture2D CharacterThumb(CharacterSpec spec)
        {
            string key = "charthumb_" + spec.Index;
            Texture2D cached;
            if (Cache.TryGetValue(key, out cached) && cached != null) return cached;

            const int w = 320;
            const int h = 320;
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.name = key;
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;
            Color32[] px = new Color32[w * h];

            Color top = Color.Lerp(spec.SuitColor, Color.black, 0.7f);
            Color bottom = new Color(0.02f, 0.03f, 0.045f);
            for (int y = 0; y < h; y++)
            {
                Color bg = Color.Lerp(bottom, top, Mathf.Pow(y / (float)h, 1.4f));
                for (int x = 0; x < w; x++)
                {
                    float glow = Mathf.Exp(-Mathf.Pow((x - 160f) / 120f, 2f) - Mathf.Pow((y - 150f) / 140f, 2f));
                    px[y * w + x] = bg + spec.SuitAccent * glow * 0.15f;
                }
            }

            // Vai + thân
            FillPoly(px, w, h, new[]
            {
                new Vector2(42, 0), new Vector2(74, 92), new Vector2(128, 126),
                new Vector2(192, 126), new Vector2(246, 92), new Vector2(278, 0)
            }, spec.SuitColor);
            FillRect(px, w, h, 128, 40, 64, 90, spec.SuitColor * 0.85f);
            FillRect(px, w, h, 142, 0, 36, 80, spec.SuitAccent * 0.75f);
            FillCircle(px, w, h, 206, 78, 17, new Color(0.92f, 0.9f, 0.84f));
            // Số
            DrawNumber(px, w, h, 160, 60, spec.Number.ToString(), new Color(0.95f, 0.95f, 0.92f));

            // Cổ + đầu
            FillRect(px, w, h, 138, 118, 44, 34, spec.SkinColor * 0.9f);
            FillCircle(px, w, h, 160, 178, 54, spec.HelmetColor);
            FillCircle(px, w, h, 160, 186, 44, spec.HelmetColor * 1.05f);
            FillPoly(px, w, h, new[]
            {
                new Vector2(118, 184), new Vector2(202, 184), new Vector2(196, 206),
                new Vector2(180, 216), new Vector2(140, 216), new Vector2(124, 206)
            }, spec.VisorColor);
            FillRect(px, w, h, 124, 170, 72, 5, spec.SuitAccent);
            FillRect(px, w, h, 150, 222, 20, 8, spec.SuitAccent * 0.8f);
            // Ánh sáng
            FillCircle(px, w, h, 142, 198, 8, new Color(1f, 1f, 1f, 1f) * 0.55f);

            tex.SetPixels32(px);
            tex.Apply(false, false);
            Cache[key] = tex;
            return tex;
        }

        private static void FillRect(Color32[] px, int w, int h, int x, int y, int rw, int rh, Color color)
        {
            int x0 = Mathf.Max(0, x);
            int y0 = Mathf.Max(0, y);
            int x1 = Mathf.Min(w, x + rw);
            int y1 = Mathf.Min(h, y + rh);
            for (int yy = y0; yy < y1; yy++)
                for (int xx = x0; xx < x1; xx++)
                    px[yy * w + xx] = color;
        }

        private static void FillPoly(Color32[] px, int w, int h, Vector2[] poly, Color color)
        {
            float minX = float.MaxValue, maxX = float.MinValue, minY = float.MaxValue, maxY = float.MinValue;
            for (int i = 0; i < poly.Length; i++)
            {
                minX = Mathf.Min(minX, poly[i].x); maxX = Mathf.Max(maxX, poly[i].x);
                minY = Mathf.Min(minY, poly[i].y); maxY = Mathf.Max(maxY, poly[i].y);
            }
            int x0 = Mathf.Max(0, Mathf.FloorToInt(minX));
            int x1 = Mathf.Min(w - 1, Mathf.CeilToInt(maxX));
            int y0 = Mathf.Max(0, Mathf.FloorToInt(minY));
            int y1 = Mathf.Min(h - 1, Mathf.CeilToInt(maxY));
            for (int y = y0; y <= y1; y++)
            {
                for (int x = x0; x <= x1; x++)
                {
                    if (PointInPoly(new Vector2(x + 0.5f, y + 0.5f), poly))
                        px[y * w + x] = color;
                }
            }
        }

        private static bool PointInPoly(Vector2 p, Vector2[] poly)
        {
            bool inside = false;
            for (int i = 0, j = poly.Length - 1; i < poly.Length; j = i++)
            {
                if (((poly[i].y > p.y) != (poly[j].y > p.y)) &&
                    (p.x < (poly[j].x - poly[i].x) * (p.y - poly[i].y) / (poly[j].y - poly[i].y) + poly[i].x))
                    inside = !inside;
            }
            return inside;
        }

        private static void DrawNumber(Color32[] px, int w, int h, int cx, int cy, string text, Color color)
        {
            // Vẽ số đơn giản bằng các đoạn thẳng 7 đoạn
            int[][] digits =
            {
                new[] { 0, 1, 2, 4, 5, 6 },
                new[] { 2, 5 },
                new[] { 0, 2, 3, 4, 6 },
                new[] { 0, 2, 3, 5, 6 },
                new[] { 1, 2, 3, 5 },
                new[] { 0, 1, 3, 5, 6 },
                new[] { 0, 1, 3, 4, 5, 6 },
                new[] { 0, 2, 5 },
                new[] { 0, 1, 2, 3, 4, 5, 6 },
                new[] { 0, 1, 2, 3, 5, 6 }
            };
            for (int c = 0; c < text.Length; c++)
            {
                int d = text[c] - '0';
                if (d < 0 || d > 9) continue;
                int ox = Mathf.RoundToInt(cx + (c - (text.Length - 1) * 0.5f) * 24f);
                for (int s = 0; s < digits[d].Length; s++)
                {
                    int seg = digits[d][s];
                    float x0, y0, x1, y1;
                    switch (seg)
                    {
                        case 0: x0 = ox - 7; y0 = cy + 12; x1 = ox + 7; y1 = cy + 12; break;
                        case 1: x0 = ox - 8; y0 = cy + 4; x1 = ox - 8; y1 = cy + 11; break;
                        case 2: x0 = ox + 8; y0 = cy + 4; x1 = ox + 8; y1 = cy + 11; break;
                        case 3: x0 = ox - 7; y0 = cy; x1 = ox + 7; y1 = cy; break;
                        case 4: x0 = ox - 8; y0 = cy - 11; x1 = ox - 8; y1 = cy - 3; break;
                        case 5: x0 = ox + 8; y0 = cy - 11; x1 = ox + 8; y1 = cy - 3; break;
                        default: x0 = ox - 7; y0 = cy - 12; x1 = ox + 7; y1 = cy - 12; break;
                    }
                    DrawLine(px, w, h, x0, y0, x1, y1, color, 2.2f);
                }
            }
        }

        private static Vector2 Bezier(Vector2 a, Vector2 b, Vector2 c, Vector2 d, float t)
        {
            float u = 1f - t;
            return u * u * u * a + 3f * u * u * t * b + 3f * u * t * t * c + t * t * t * d;
        }

        private static void DrawMountains(Color32[] px, int w, int h, float baseY, float amp, float freq, Color color, float seed)
        {
            for (int x = 0; x < w; x++)
            {
                float n = Mathf.PerlinNoise(x * freq + seed, seed * 0.7f);
                float n2 = Mathf.PerlinNoise(x * freq * 2.7f + seed, seed);
                float top = h * (baseY + (n * 0.7f + n2 * 0.3f) * amp);
                for (int y = 0; y < top && y < h; y++)
                {
                    float k = Mathf.Clamp01((y - top * 0.55f) / (top * 0.45f));
                    Color c = Color.Lerp(color * 1.16f, color * 0.66f, k);
                    px[y * w + x] = c;
                }
            }
        }

        private static void DrawWindmill(Color32[] px, int w, int h, float x, float y, float s, TerrainTheme theme)
        {
            Color wood = new Color(0.35f, 0.27f, 0.18f);
            DrawLine(px, w, h, x - s * 0.35f, y - s * 2.2f, x - s * 0.12f, y, wood, 2.4f);
            DrawLine(px, w, h, x + s * 0.35f, y - s * 2.2f, x + s * 0.12f, y, wood, 2.4f);
            FillCircle(px, w, h, x, y, s * 0.35f, theme.PropColor2);
            for (int i = 0; i < 4; i++)
            {
                float a = i * Mathf.PI * 0.5f + 0.4f;
                DrawLine(px, w, h, x, y, x + Mathf.Cos(a) * s, y + Mathf.Sin(a) * s, new Color(0.82f, 0.78f, 0.66f), 1.8f);
            }
        }

        private static void DrawBarn(Color32[] px, int w, int h, float x, float y, float s, TerrainTheme theme)
        {
            Color wall = new Color(0.55f, 0.16f, 0.12f);
            Color roof = new Color(0.28f, 0.24f, 0.22f);
            for (int yy = (int)(y - s * 0.9f); yy < y; yy++)
            {
                for (int xx = (int)(x - s); xx < x + s; xx++)
                {
                    if (xx < 0 || xx >= w || yy < 0 || yy >= h) continue;
                    px[yy * w + xx] = wall;
                }
            }
            for (int i = 0; i < s * 1.3f; i++)
            {
                int yy = (int)(y + i * 0.45f);
                int x0 = (int)(x - s + i);
                int x1 = (int)(x + s - i);
                if (yy < 0 || yy >= h) continue;
                for (int xx = x0; xx < x1; xx++)
                    if (xx >= 0 && xx < w) px[yy * w + xx] = roof;
            }
        }

        private static void DrawLine(Color32[] px, int w, int h, float x0, float y0, float x1, float y1, Color color, float radius)
        {
            float dx = x1 - x0;
            float dy = y1 - y0;
            int steps = Mathf.Max(1, Mathf.CeilToInt(Mathf.Sqrt(dx * dx + dy * dy) * 1.4f));
            for (int i = 0; i <= steps; i++)
            {
                float t = i / (float)steps;
                float x = x0 + dx * t;
                float y = y0 + dy * t;
                int r = Mathf.Max(1, Mathf.RoundToInt(radius));
                for (int oy = -r; oy <= r; oy++)
                {
                    for (int ox = -r; ox <= r; ox++)
                    {
                        if (ox * ox + oy * oy > r * r) continue;
                        int ix = (int)x + ox;
                        int iy = (int)y + oy;
                        if (ix < 0 || ix >= w || iy < 0 || iy >= h) continue;
                        float a = 1f - Mathf.Sqrt(ox * ox + oy * oy) / (r + 0.01f);
                        Color dst = px[iy * w + ix];
                        px[iy * w + ix] = Color.Lerp(dst, color, Mathf.Clamp01(a));
                    }
                }
            }
        }

        private static void FillCircle(Color32[] px, int w, int h, float cx, float cy, float r, Color color)
        {
            int x0 = Mathf.Max(0, Mathf.FloorToInt(cx - r));
            int x1 = Mathf.Min(w - 1, Mathf.CeilToInt(cx + r));
            int y0 = Mathf.Max(0, Mathf.FloorToInt(cy - r));
            int y1 = Mathf.Min(h - 1, Mathf.CeilToInt(cy + r));
            for (int y = y0; y <= y1; y++)
            {
                for (int x = x0; x <= x1; x++)
                {
                    float d = Vector2.Distance(new Vector2(x, y), new Vector2(cx, cy));
                    float a = Mathf.Clamp01(r - d);
                    if (a > 0f)
                    {
                        int idx = y * w + x;
                        px[idx] = Color.Lerp(px[idx], color, a);
                    }
                }
            }
        }
    }

    /// <summary>Kho vật liệu dùng chung, có cache để tránh tạo trùng.</summary>
    public static class MaterialLibrary
    {
        private static readonly Dictionary<string, Material> Cache = new Dictionary<string, Material>();

        private static Shader FindShader(string name, string fallback = "Standard")
        {
            Shader s = Shader.Find(name);
            if (s == null && !string.IsNullOrEmpty(fallback)) s = Shader.Find(fallback);
            return s;
        }

        private static Material Cached(string key, System.Func<Material> factory)
        {
            Material m;
            if (Cache.TryGetValue(key, out m) && m != null) return m;
            m = factory();
            if (m != null) Cache[key] = m;
            return m;
        }

        public static void Clear()
        {
            Cache.Clear();
        }

        public static Material Standard(string name, Color color, float metallic, float smoothness)
        {
            string key = "std_" + name + "_" + ColorUtility.ToHtmlStringRGBA(color) + "_" + metallic + "_" + smoothness;
            return Cached(key, () =>
            {
                Shader shader = FindShader("Standard");
                if (shader == null) shader = FindShader("Universal Render Pipeline/Lit", "Standard");
                Material m = new Material(shader);
                m.name = name;
                m.SetColor("_Color", color);
                m.SetFloat("_Metallic", Mathf.Clamp01(metallic));
                m.SetFloat("_Glossiness", Mathf.Clamp01(smoothness));
                if (color.a < 0.995f)
                {
                    m.SetFloat("_Mode", 3f);
                    m.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
                    m.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
                    m.SetInt("_ZWrite", 0);
                    m.DisableKeyword("_ALPHATEST_ON");
                    m.EnableKeyword("_ALPHABLEND_ON");
                    m.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    m.renderQueue = (int)RenderQueue.Transparent;
                }
                return m;
            });
        }

        public static Material Standard(string name, Color color, float metallic, float smoothness, Texture2D texture)
        {
            Material m = Standard(name, color, metallic, smoothness);
            m.mainTexture = texture;
            return m;
        }

        public static Material Standard(string name, Color color, float metallic, float smoothness,
            Texture2D mainTex, Texture2D normalMap, Color emission, float emissionIntensity)
        {
            string key = "stdx_" + name + "_" + ColorUtility.ToHtmlStringRGBA(color) + "_" + metallic + "_" + smoothness
                + "_" + (mainTex != null ? mainTex.name : "n")
                + "_" + (normalMap != null ? normalMap.name : "n")
                + "_" + ColorUtility.ToHtmlStringRGBA(emission) + "_" + emissionIntensity;
            return Cached(key, () =>
            {
                Material m = new Material(Standard(name, color, metallic, smoothness));
                m.name = name;
                if (mainTex != null) m.mainTexture = mainTex;
                if (normalMap != null)
                {
                    m.SetTexture("_BumpMap", normalMap);
                    m.EnableKeyword("_NORMALMAP");
                }
                if (emissionIntensity > 0f && emission.maxColorComponent > 0.001f)
                {
                    m.EnableKeyword("_EMISSION");
                    m.SetColor("_EmissionColor", emission * emissionIntensity);
                    m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
                }
                return m;
            });
        }

        public static Material Paint(string name, Color color)
        {
            return Standard(name, color, 0.42f, 0.72f);
        }

        public static Material Rubber()
        {
            return Cached("rubber", () => Standard("rubber", new Color(0.055f, 0.055f, 0.06f), 0.02f, 0.14f));
        }

        public static Material DarkMetal()
        {
            return Cached("darkmetal", () => Standard("darkmetal", new Color(0.085f, 0.09f, 0.1f), 0.72f, 0.36f));
        }

        public static Material Chrome()
        {
            return Cached("chrome", () => Standard("chrome", new Color(0.76f, 0.79f, 0.83f), 1f, 0.93f));
        }

        public static Material Glass()
        {
            return Cached("glass", () => Standard("glass", new Color(0.24f, 0.42f, 0.52f, 0.34f), 0.35f, 0.96f));
        }

        public static Material Emissive(string name, Color color, float intensity)
        {
            string key = "em_" + name + "_" + ColorUtility.ToHtmlStringRGBA(color) + "_" + intensity;
            return Cached(key, () =>
            {
                Material m = Standard(name, color, 0.05f, 0.5f);
                m.EnableKeyword("_EMISSION");
                m.SetColor("_EmissionColor", color * Mathf.Max(0f, intensity));
                m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
                return m;
            });
        }

        public static Material Unlit(string name, Color color)
        {
            string key = "unlit_" + name + "_" + ColorUtility.ToHtmlStringRGBA(color);
            return Cached(key, () =>
            {
                Shader shader = FindShader("Unlit/Color");
                Material m = shader != null ? new Material(shader) : Standard(name, color, 0f, 0.2f);
                m.name = name;
                m.SetColor("_Color", color);
                if (shader == null) m.EnableKeyword("_EMISSION");
                return m;
            });
        }

        public static Material Sprite(string name, Color color, Texture2D texture)
        {
            string key = "sprite_" + name + "_" + ColorUtility.ToHtmlStringRGBA(color);
            return Cached(key, () =>
            {
                Shader shader = FindShader("Sprites/Default", "Unlit/Color");
                Material m = new Material(shader);
                m.name = name;
                m.SetColor("_Color", color);
                if (texture != null) m.SetTexture("_MainTex", texture);
                return m;
            });
        }

        public static Material Particle(string name, bool additive)
        {
            string key = "part_" + name + "_" + additive;
            return Cached(key, () =>
            {
                Shader shader = additive
                    ? FindShader("Legacy Shaders/Particles/Additive", "Sprites/Default")
                    : FindShader("Legacy Shaders/Particles/Alpha Blended", "Sprites/Default");
                Material m = new Material(shader);
                m.name = name;
                m.SetTexture("_MainTex", ProcTex.SoftCircle(64));
                m.SetColor("_TintColor", additive
                    ? new Color(0.62f, 0.86f, 1f, 0.9f)
                    : new Color(0.85f, 0.8f, 0.72f, 0.75f));
                return m;
            });
        }

        public static Material Trail()
        {
            return Cached("trail", () => Sprite("trail", Color.white, ProcTex.SoftCircle(64)));
        }

        public static Material Confetti()
        {
            return Cached("confetti", () => Sprite("confetti", Color.white, ProcTex.SoftCircle(32)));
        }

        public static Material Cloud()
        {
            return Cached("cloud", () => Sprite("cloud", new Color(1f, 1f, 1f, 0.7f), ProcTex.SoftCircle(128)));
        }

        public static Material Road(TerrainTheme theme)
        {
            string key = "road_" + theme.Index;
            return Cached(key, () =>
            {
                float smooth = theme.Kind == ThemeKind.Mud ? 0.58f : 0.18f;
                Material m = Standard("road_" + theme.EnglishName, Color.white, 0.04f, smooth);
                m.mainTexture = ProcTex.Road("road_tex_" + theme.Index, theme, 700 + theme.Index);
                m.mainTextureScale = new Vector2(1f, 1f);
                return m;
            });
        }

        public static Material Dirt(TerrainTheme theme)
        {
            string key = "dirt_" + theme.Index;
            return Cached(key, () =>
            {
                Material m = Standard("dirt_" + theme.EnglishName, Color.white, 0.02f, 0.12f);
                m.mainTexture = ProcTex.Ground("dirttex_" + theme.Index, theme.DirtColor, GroundStyle.Dirt, 80 + theme.Index);
                m.mainTextureScale = new Vector2(2f, 2f);
                return m;
            });
        }

        public static Material GroundPlane(TerrainTheme theme)
        {
            string key = "groundplane_" + theme.Index;
            return Cached(key, () =>
            {
                Material m = Standard("groundplane_" + theme.EnglishName, Color.white, 0f, 0.08f);
                m.mainTexture = ProcTex.Ground("plane_" + theme.Index, theme.GroundColor, theme.Kind == ThemeKind.Desert ? GroundStyle.Sand : GroundStyle.Grass, 150 + theme.Index);
                m.mainTextureScale = new Vector2(160f, 160f);
                return m;
            });
        }

        public static Material RoadLine()
        {
            return Cached("roadline", () => Emissive("roadline", new Color(0.93f, 0.93f, 0.86f), 0.45f));
        }

        public static Material CenterLine()
        {
            return Cached("centerline", () => Emissive("centerline", new Color(1f, 0.76f, 0.24f), 0.55f));
        }

        public static Material Checker()
        {
            return Cached("checker", () =>
            {
                Material m = Standard("checker", Color.white, 0.05f, 0.42f, ProcTex.Checker(8));
                m.mainTextureScale = new Vector2(6f, 1f);
                return m;
            });
        }

        public static Material CheckerStrip()
        {
            return Cached("checkerstrip", () =>
            {
                Material m = Standard("checkerstrip", Color.white, 0.05f, 0.4f, ProcTex.Checker(8));
                m.mainTextureScale = new Vector2(1f, 0.25f);
                return m;
            });
        }

        public static Material Banner()
        {
            return Cached("banner", () =>
            {
                Material m = Unlit("banner", Color.white);
                m.mainTexture = ProcTex.Checker(8);
                m.mainTextureScale = new Vector2(8f, 1f);
                return m;
            });
        }

        public static Material Mountain(TerrainTheme theme)
        {
            string key = "mountain_" + theme.Index;
            return Cached(key, () =>
            {
                Shader shader = FindShader("DuaXe3D/Mountain", "Standard");
                Material m = new Material(shader);
                m.name = "mountain_" + theme.EnglishName;
                m.SetColor("_Color", theme.RockColor * 0.86f);
                m.SetColor("_SnowColor", new Color(0.92f, 0.95f, 1f));
                m.SetFloat("_SnowHeight", theme.SnowCaps ? 110f : 9999f);
                m.SetFloat("_SnowBlend", 45f);
                m.SetFloat("_Glossiness", 0.12f);
                if (shader == null || shader.name == "Standard")
                {
                    m.SetColor("_Color", theme.RockColor * 0.86f);
                    m.SetFloat("_Glossiness", 0.12f);
                }
                return m;
            });
        }

        public static Material TreeTrunk()
        {
            return Cached("trunk", () => Standard("trunk", new Color(0.26f, 0.18f, 0.11f), 0f, 0.16f));
        }

        public static Material Leaves(TerrainTheme theme, int variant = 0)
        {
            string key = "leaves_" + theme.Index + "_" + variant;
            return Cached(key, () =>
            {
                Color c = variant == 0 ? theme.PropColor : theme.PropColor2;
                return Standard("leaves_" + theme.Index + "_" + variant, c, 0f, 0.18f);
            });
        }

        public static Material Cactus()
        {
            return Cached("cactus", () => Standard("cactus", new Color(0.29f, 0.46f, 0.24f), 0f, 0.24f));
        }

        public static Material Rock(TerrainTheme theme)
        {
            string key = "rock_" + theme.Index;
            return Cached(key, () => Standard("rock_" + theme.Index, theme.RockColor * 0.88f, 0.02f, 0.16f));
        }

        public static Material Wood()
        {
            return Cached("wood", () => Standard("wood", new Color(0.45f, 0.31f, 0.17f), 0f, 0.2f));
        }

        public static Material DarkWood()
        {
            return Cached("darkwood", () => Standard("darkwood", new Color(0.25f, 0.17f, 0.1f), 0f, 0.18f));
        }

        public static Material Hay()
        {
            return Cached("hay", () => Standard("hay", new Color(0.82f, 0.68f, 0.28f), 0f, 0.15f));
        }

        public static Material Roof()
        {
            return Cached("roof", () => Standard("roof", new Color(0.36f, 0.12f, 0.1f), 0.05f, 0.3f));
        }

        public static Material Metal()
        {
            return Cached("metal", () => Standard("metal", new Color(0.42f, 0.45f, 0.48f), 0.8f, 0.5f));
        }

        public static Material StudioFloor()
        {
            return Cached("studiofloor", () =>
            {
                Material m = Standard("studiofloor", Color.white, 0.32f, 0.40f);
                m.mainTexture = ProcTex.StudioGrid();
                m.mainTextureScale = new Vector2(14f, 14f);
                return m;
            });
        }

        public static Material StudioWall()
        {
            return Cached("studiowall", () => Standard("studiowall", new Color(0.09f, 0.11f, 0.15f), 0.15f, 0.45f));
        }

        public static Material Podium(Color accent)
        {
            string key = "podium_" + ColorUtility.ToHtmlStringRGBA(accent);
            Color baseCol = Color.Lerp(new Color(0.07f, 0.085f, 0.11f), accent, 0.22f);
            return Cached(key, () => Standard(key, baseCol, 0.5f, 0.65f));
        }

        public static Material GlowRing(Color color)
        {
            string key = "glowring_" + ColorUtility.ToHtmlStringRGBA(color);
            return Cached(key, () => Emissive(key, color, 2.4f));
        }

        public static Material Skybox(TerrainTheme theme)
        {
            string key = "sky_" + theme.Index;
            return Cached(key, () =>
            {
                Cubemap cube = BuildSkyCubemap(theme, 128);
                Shader shader = FindShader("Skybox/Cubemap", "Skybox/Procedural");
                Material m = new Material(shader);
                m.name = "sky_" + theme.EnglishName;
                if (shader != null && shader.name == "Skybox/Cubemap")
                {
                    m.SetTexture("_Tex", cube);
                    m.SetFloat("_Exposure", 1.25f);
                }
                else
                {
                    m.SetColor("_SkyTint", theme.SkyTint);
                    m.SetColor("_GroundColor", theme.GroundColor);
                    m.SetFloat("_AtmosphereThickness", theme.Kind == ThemeKind.Desert ? 0.9f : 1.15f);
                    m.SetFloat("_Exposure", 1.25f);
                    m.SetFloat("_SunSize", 0.045f);
                }
                return m;
            });
        }

        private static Cubemap BuildSkyCubemap(TerrainTheme theme, int size)
        {
            Cubemap cube = new Cubemap(size, TextureFormat.RGBA32, true);
            cube.name = "skycube_" + theme.EnglishName;
            cube.wrapMode = TextureWrapMode.Clamp;
            cube.filterMode = FilterMode.Trilinear;
            cube.anisoLevel = 4;

            Color zenith = Color.Lerp(theme.SkyTint, new Color(0.11f, 0.31f, 0.64f), 0.5f);
            Color horizon = Color.Lerp(theme.SkyTint, theme.FogColor, 0.55f);
            Color below = Color.Lerp(theme.GroundColor, theme.FogColor, 0.45f);

            Quaternion sunRot = Quaternion.Euler(theme.SunPitch, theme.SunYaw, 0f);
            Vector3 sunDir = -(sunRot * Vector3.forward);

            Vector3[] dirs = new Vector3[size * size];
            for (int face = 0; face < 6; face++)
            {
                for (int y = 0; y < size; y++)
                {
                    float v = (y + 0.5f) / size * 2f - 1f;
                    for (int x = 0; x < size; x++)
                    {
                        float u = (x + 0.5f) / size * 2f - 1f;
                        Vector3 dir;
                        switch (face)
                        {
                            case 0: dir = new Vector3(1f, -v, -u); break;  // +X
                            case 1: dir = new Vector3(-1f, -v, u); break;  // -X
                            case 2: dir = new Vector3(u, 1f, v); break;    // +Y
                            case 3: dir = new Vector3(u, -1f, -v); break;  // -Y
                            case 4: dir = new Vector3(u, -v, 1f); break;   // +Z
                            default: dir = new Vector3(-u, -v, -1f); break; // -Z
                        }
                        dirs[y * size + x] = dir.normalized;
                    }
                }

                Color[] px = new Color[size * size];
                for (int i = 0; i < px.Length; i++)
                {
                    Vector3 dir = dirs[i];
                    float h = dir.y;
                    Color c;
                    if (h >= 0f)
                    {
                        float t = Mathf.Pow(h, 0.58f);
                        c = Color.Lerp(horizon, zenith, t);
                        // Mây cao
                        float cl = Mathf.PerlinNoise(dir.x * 4.2f + 11f, dir.z * 4.2f + 7f) *
                                   Mathf.PerlinNoise(dir.x * 9f + 2f, dir.z * 9f + 5f);
                        if (cl > 0.34f && h > 0.08f)
                        {
                            float cloud = Mathf.SmoothStep(0.34f, 0.62f, cl) * Mathf.Clamp01(h * 2.4f);
                            c = Color.Lerp(c, new Color(1f, 1f, 1f, 1f) * 1.05f, cloud * 0.5f);
                        }
                    }
                    else
                    {
                        c = Color.Lerp(horizon, below, Mathf.Clamp01(-h * 2.2f));
                    }

                    float sunDot = Vector3.Dot(dir, sunDir);
                    if (sunDot > 0.985f)
                    {
                        float k = Mathf.Clamp01((sunDot - 0.985f) / 0.015f);
                        c = Color.Lerp(c, new Color(1f, 0.97f, 0.88f), k);
                    }
                    if (sunDot > 0.86f)
                    {
                        float glow = Mathf.Pow(Mathf.Clamp01((sunDot - 0.86f) / 0.14f), 2.2f);
                        c = Color.Lerp(c, theme.SunColor, glow * 0.5f);
                    }
                    c.a = 1f;
                    px[i] = c;
                }
                cube.SetPixels(px, (CubemapFace)face);
            }
            cube.Apply(true, false);
            return cube;
        }
    }
}
