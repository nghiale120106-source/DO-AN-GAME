using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace DuaXe3D
{
    /// <summary>
    /// Bộ dựng sân khấu 3D + các khối giao diện dùng chung cho 4 màn hình menu.
    /// Toàn bộ hình khối, vật liệu và hiệu ứng đều sinh bằng code.
    /// </summary>
    public static class MenuKit
    {
        public static Canvas CreateCanvas(string name)
        {
            Canvas canvas = UIKit.CreateCanvas(name, 10);
            UIKit.AddScreenEdges(canvas.transform, 230f, 290f);
            UIKit.AddNoise(canvas.transform, 0.022f);
            return canvas;
        }

        // ------------------------------------------------------------------
        // 3D studio
        // ------------------------------------------------------------------
        public static Transform BuildStudio(Transform parent, Color accent, bool withDust = true)
        {
            GameObject root = new GameObject("Studio");
            root.transform.SetParent(parent, false);

            RenderSettings.skybox = null;
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = 0.006f;
            RenderSettings.fogColor = new Color(0.018f, 0.024f, 0.04f);
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.15f, 0.19f, 0.27f);
            RenderSettings.ambientIntensity = 0.9f;
            RenderSettings.reflectionIntensity = 0.7f;
            RenderSettings.defaultReflectionMode = DefaultReflectionMode.Custom;
            RenderSettings.customReflectionTexture = null;

            // Sàn studio
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = "StudioFloor";
            floor.transform.SetParent(root.transform, false);
            floor.transform.localScale = Vector3.one * 5f;
            Renderer fr = floor.GetComponent<Renderer>();
            fr.sharedMaterial = MaterialLibrary.StudioFloor();
            fr.receiveShadows = true;
            Collider fc = floor.GetComponent<Collider>();
            if (fc != null) Object.Destroy(fc);

            // Tường cong phía sau
            Mesh wall = BuildInwardCylinder(27f, 23f, 96);
            GameObject wallGo = MeshKit.MeshObject("StudioWall", wall, MaterialLibrary.StudioWall(),
                root.transform, Vector3.zero, Quaternion.identity, Vector3.one);
            MeshKit.SetShadows(wallGo, false, true);

            // Dải đèn dọc trên tường
            Material stripMat = MaterialLibrary.Emissive("studio_strip", Color.Lerp(accent, Color.white, 0.35f), 2.2f);
            for (int i = 0; i < 5; i++)
            {
                float a = Mathf.Lerp(118f, 242f, i / 4f) * Mathf.Deg2Rad;
                Vector3 pos = new Vector3(Mathf.Cos(a) * 24.4f, 9.5f, Mathf.Sin(a) * 24.4f);
                Vector3 dir = -new Vector3(pos.x, 0f, pos.z).normalized;
                MeshKit.Box("LightStrip", root.transform, pos, new Vector3(0.14f, 15.5f, 0.14f),
                    Quaternion.LookRotation(dir).eulerAngles, stripMat);
            }

            // Vòng sáng quanh bệ
            MeshKit.MeshObject("AccentRing", MeshKit.Ring(4.05f, 4.32f, 96),
                MaterialLibrary.GlowRing(accent), root.transform,
                new Vector3(0f, 0.028f, 0f), Quaternion.identity, Vector3.one);
            MeshKit.MeshObject("InnerRing", MeshKit.Ring(2.25f, 2.34f, 96),
                MaterialLibrary.GlowRing(Color.Lerp(accent, Color.white, 0.55f)), root.transform,
                new Vector3(0f, 0.031f, 0f), Quaternion.identity, Vector3.one);

            // Bệ trưng bày thấp
            GameObject podium = MeshKit.Cylinder("Podium", root.transform, new Vector3(0f, 0.09f, 0f),
                new Vector3(4.4f, 0.09f, 4.4f), Vector3.zero, MaterialLibrary.Podium(accent));
            MeshKit.SetShadows(podium, true, true);

            BuildStudioLights(root.transform, accent);
            if (withDust) AddDust(root.transform, new Color(1f, 0.86f, 0.66f, 0.45f), 90);
            return root.transform;
        }

        public static void BuildStudioLights(Transform parent, Color accent)
        {
            GameObject keyGo = new GameObject("KeyLight");
            keyGo.transform.SetParent(parent, false);
            keyGo.transform.rotation = Quaternion.Euler(38f, 205f, 0f);
            Light key = keyGo.AddComponent<Light>();
            key.type = LightType.Directional;
            key.color = new Color(1f, 0.95f, 0.88f);
            key.intensity = 1.3f;
            key.shadows = LightShadows.Soft;
            key.shadowStrength = 0.82f;
            key.shadowBias = 0.04f;
            key.shadowNormalBias = 0.3f;
            RenderSettings.sun = key;

            GameObject rimGo = new GameObject("RimLight");
            rimGo.transform.SetParent(parent, false);
            rimGo.transform.position = new Vector3(-3.6f, 8.2f, -5.6f);
            rimGo.transform.LookAt(new Vector3(0f, 1.1f, 0f));
            Light rim = rimGo.AddComponent<Light>();
            rim.type = LightType.Spot;
            rim.color = Color.Lerp(accent, Color.white, 0.25f);
            rim.intensity = 6.2f;
            rim.range = 24f;
            rim.spotAngle = 46f;
            rim.innerSpotAngle = 20f;
            rim.shadows = LightShadows.None;

            GameObject fillGo = new GameObject("FillLight");
            fillGo.transform.SetParent(parent, false);
            fillGo.transform.position = new Vector3(5.5f, 4.6f, 4.2f);
            fillGo.transform.LookAt(new Vector3(0f, 1f, 0f));
            Light fill = fillGo.AddComponent<Light>();
            fill.type = LightType.Point;
            fill.color = new Color(0.55f, 0.72f, 1f);
            fill.intensity = 1.7f;
            fill.range = 17f;
            fill.shadows = LightShadows.None;

            GameObject warmGo = new GameObject("WarmLight");
            warmGo.transform.SetParent(parent, false);
            warmGo.transform.position = new Vector3(-5f, 2.2f, 3.5f);
            Light warm = warmGo.AddComponent<Light>();
            warm.type = LightType.Point;
            warm.color = accent;
            warm.intensity = 1.1f;
            warm.range = 12f;
            warm.shadows = LightShadows.None;

            PulseLight pulse = warmGo.AddComponent<PulseLight>();
            pulse.Light = warm;
            pulse.Min = 0.7f;
            pulse.Max = 1.3f;
            pulse.Speed = 1.35f;
        }

        public static Camera CreateMenuCamera(Vector3 position, Vector3 lookAt, float fov = 38f)
        {
            Camera cam = SceneSetup.CreateMainCamera(position, Quaternion.LookRotation(lookAt - position, Vector3.up), fov);
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.015f, 0.021f, 0.034f);
            cam.farClipPlane = 260f;
            CameraPostFX fx = cam.GetComponent<CameraPostFX>();
            if (fx != null)
            {
                fx.vignette = 0.72f;
                fx.saturation = 1.08f;
                fx.contrast = 1.06f;
                fx.grain = 0.016f;
            }
            return cam;
        }

        public static GameObject AddCar(Transform parent, CarSpec spec, CharacterSpec character, Vector3 position, float spin = 16f)
        {
            CarVisual visual = CarFactory.BuildVisual(spec, character, parent, true);
            GameObject go = visual.Root;
            go.transform.localPosition = position;
            go.transform.localRotation = Quaternion.identity;
            Spinner spinner = go.AddComponent<Spinner>();
            spinner.Speed = spin;
            spinner.BobHeight = 0.015f;
            spinner.BobSpeed = 1.1f;
            return go;
        }

        public static GameObject AddCharacter(Transform parent, CharacterSpec spec, Vector3 position, float scale = 1.35f)
        {
            GameObject go = CharacterFactory.Build(spec, parent, position, scale, true);
            Spinner spinner = go.AddComponent<Spinner>();
            spinner.Speed = 18f;
            spinner.BobHeight = 0.012f;
            spinner.BobSpeed = 1.6f;
            return go;
        }

        public static void AddDust(Transform parent, Color tint, int count)
        {
            GameObject go = new GameObject("DustMotes");
            go.transform.SetParent(parent, false);
            go.transform.localPosition = new Vector3(0f, 3.2f, 0f);

            ParticleSystem ps = go.AddComponent<ParticleSystem>();
            ParticleSystem.MainModule main = ps.main;
            main.loop = true;
            main.prewarm = true;
            main.startLifetime = new ParticleSystem.MinMaxCurve(6f, 12f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.04f, 0.22f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.035f, 0.11f);
            main.startColor = tint;
            main.maxParticles = Mathf.Max(24, count);
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.gravityModifier = -0.005f;

            ParticleSystem.EmissionModule emission = ps.emission;
            emission.rateOverTime = count / 8f;

            ParticleSystem.ShapeModule shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(25f, 11f, 25f);

            ParticleSystem.VelocityOverLifetimeModule vel = ps.velocityOverLifetime;
            vel.enabled = true;
            vel.space = ParticleSystemSimulationSpace.World;
            vel.x = new ParticleSystem.MinMaxCurve(-0.06f, 0.06f);
            vel.y = new ParticleSystem.MinMaxCurve(0.02f, 0.12f);
            vel.z = new ParticleSystem.MinMaxCurve(-0.06f, 0.06f);

            Gradient g = new Gradient();
            g.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.22f), new GradientAlphaKey(0.7f, 0.7f), new GradientAlphaKey(0f, 1f) });
            ParticleSystem.ColorOverLifetimeModule col = ps.colorOverLifetime;
            col.enabled = true;
            col.color = new ParticleSystem.MinMaxGradient(g);

            ParticleSystemRenderer r = go.GetComponent<ParticleSystemRenderer>();
            r.material = MaterialLibrary.Particle("menu_dust", true);
            r.renderMode = ParticleSystemRenderMode.Billboard;
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            r.receiveShadows = false;
        }

        public static void AddConfetti(Transform parent, Color accent)
        {
            GameObject go = new GameObject("Confetti");
            go.transform.SetParent(parent, false);
            go.transform.localPosition = new Vector3(0f, 9.5f, 0f);

            ParticleSystem ps = go.AddComponent<ParticleSystem>();
            ParticleSystem.MainModule main = ps.main;
            main.loop = true;
            main.startLifetime = new ParticleSystem.MinMaxCurve(6f, 10f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.6f, 2.2f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.07f, 0.17f);
            main.startColor = new ParticleSystem.MinMaxGradient(accent, UITheme.Gold);
            main.gravityModifier = 0.55f;
            main.maxParticles = 500;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            ParticleSystem.EmissionModule emission = ps.emission;
            emission.rateOverTime = 55f;

            ParticleSystem.ShapeModule shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(15f, 0.4f, 9f);

            ParticleSystem.RotationOverLifetimeModule rot = ps.rotationOverLifetime;
            rot.enabled = true;
            rot.z = new ParticleSystem.MinMaxCurve(-220f, 220f);

            Gradient g = new Gradient();
            g.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 0.8f), new GradientAlphaKey(0f, 1f) });
            ParticleSystem.ColorOverLifetimeModule col = ps.colorOverLifetime;
            col.enabled = true;
            col.color = new ParticleSystem.MinMaxGradient(g);

            ParticleSystemRenderer r = go.GetComponent<ParticleSystemRenderer>();
            r.material = MaterialLibrary.Confetti();
            r.renderMode = ParticleSystemRenderMode.Billboard;
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }

        // ------------------------------------------------------------------
        // UI building blocks
        // ------------------------------------------------------------------
        public static void AddBrand(Transform root, string product = "ĐUA XE ĐỊA HÌNH 3D")
        {
            RectTransform row = UIKit.Rect(root, "Brand");
            UIKit.PlaceTopLeft(row, new Vector2(64f, 34f), new Vector2(520f, 46f));

            Image mark = UIKit.Image(row, "Mark", UITheme.Accent, UISprites.RoundedSmall);
            mark.rectTransform.sizeDelta = new Vector2(38f, 38f);
            mark.rectTransform.anchoredPosition = new Vector2(19f, -23f);
            Image chev = UIKit.Image(mark.rectTransform, "Chevron", UITheme.Bg, UISprites.Chevron);
            chev.rectTransform.sizeDelta = new Vector2(20f, 20f);
            chev.rectTransform.anchoredPosition = Vector2.zero;

            Text name = UIKit.Label(row, product, 22, UITheme.Text, TextAnchor.MiddleLeft, UITheme.Heading);
            name.rectTransform.anchoredPosition = new Vector2(52f, -23f);
            name.rectTransform.sizeDelta = new Vector2(470f, 34f);

            Text sub = UIKit.Label(row, "OFF-ROAD RACING · 2026", 13, UITheme.Muted, TextAnchor.MiddleLeft, UITheme.Semi);
            sub.rectTransform.anchoredPosition = new Vector2(54f, -45f);
            sub.rectTransform.sizeDelta = new Vector2(420f, 20f);
        }

        public static Button AddBackButton(Transform root, UnityAction onBack)
        {
            Button btn = UIKit.MakeIconButton(root, UISprites.Chevron, new Vector2(50f, 50f), onBack,
                UITheme.Muted, true);
            UIKit.PlaceTopLeft((RectTransform)btn.transform, new Vector2(64f, 96f), new Vector2(50f, 50f));
            return btn;
        }

        public static StepsWidget AddSteps(Transform root, string[] labels, int active)
        {
            RectTransform holder = UIKit.Rect(root, "StepsHolder");
            UIKit.PlaceTopRight(holder, new Vector2(64f, 38f), new Vector2(760f, 46f));
            StepsWidget steps = UIKit.BuildSteps(holder, labels, active, Vector2.zero);
            return steps;
        }

        public static void AddHeader(Transform root, string eyebrow, string title, string subtitle,
            string[] steps, int active, UnityAction onBack)
        {
            AddBrand(root);
            if (onBack != null) AddBackButton(root, onBack);
            if (steps != null) AddSteps(root, steps, active);

            Text eye = UIKit.Label(root, eyebrow, 18, UITheme.Accent, TextAnchor.UpperLeft, UITheme.Semi);
            UIKit.PlaceTopLeft(eye.rectTransform, new Vector2(64f, 178f), new Vector2(620f, 26f));

            Text head = UIKit.Label(root, title, 76, UITheme.Text, TextAnchor.UpperLeft, UITheme.Display);
            UIKit.PlaceTopLeft(head.rectTransform, new Vector2(60f, 206f), new Vector2(760f, 92f));

            Text sub = UIKit.Label(root, subtitle, 19, UITheme.Muted, TextAnchor.UpperLeft, UITheme.Body);
            UIKit.PlaceTopLeft(sub.rectTransform, new Vector2(64f, 298f), new Vector2(610f, 88f));
            sub.horizontalOverflow = HorizontalWrapMode.Wrap;
            sub.verticalOverflow = VerticalWrapMode.Overflow;
        }

        public static void AddFooter(Transform root, string hint)
        {
            Image line = UIKit.Image(root, "FooterLine", new Color(1f, 1f, 1f, 0.08f), UISprites.Pill);
            line.rectTransform.anchorMin = new Vector2(0f, 0f);
            line.rectTransform.anchorMax = new Vector2(1f, 0f);
            line.rectTransform.pivot = new Vector2(0.5f, 0f);
            line.rectTransform.anchoredPosition = new Vector2(0f, 34f);
            line.rectTransform.sizeDelta = new Vector2(-128f, 1.5f);

            Text t = UIKit.Label(root, hint, 17, UITheme.Muted, TextAnchor.LowerLeft, UITheme.Body);
            UIKit.PlaceBottomLeft(t.rectTransform, new Vector2(64f, 14f), new Vector2(1000f, 24f));
        }

        public static Text AddChip(Transform parent, string text, Color accent, Vector2 pos, Vector2 size, Font font = null)
        {
            Image bg = UIKit.Panel(parent, "Chip", new Color(0.05f, 0.075f, 0.11f, 0.9f), 14f, true, 0.12f);
            UIKit.PlaceTopLeft(bg.rectTransform, pos, size);
            Image dot = UIKit.Image(bg.rectTransform, "Dot", accent, UISprites.Circle);
            dot.rectTransform.anchorMin = new Vector2(0f, 0.5f);
            dot.rectTransform.anchorMax = new Vector2(0f, 0.5f);
            dot.rectTransform.pivot = new Vector2(0f, 0.5f);
            dot.rectTransform.anchoredPosition = new Vector2(16f, 0f);
            dot.rectTransform.sizeDelta = new Vector2(8f, 8f);

            Text label = UIKit.Label(bg.rectTransform, text, 17, UITheme.Text, TextAnchor.MiddleLeft, font ?? UITheme.Semi);
            label.rectTransform.anchorMin = new Vector2(0f, 0f);
            label.rectTransform.anchorMax = new Vector2(1f, 1f);
            label.rectTransform.offsetMin = new Vector2(32f, 2f);
            label.rectTransform.offsetMax = new Vector2(-12f, -2f);
            return label;
        }

        public static Image AddStat(Transform parent, string label, float value01, Color color,
            Vector2 pos, float width)
        {
            Text lab = UIKit.Label(parent, label, 16, UITheme.Muted, TextAnchor.UpperLeft, UITheme.Semi);
            UIKit.PlaceTopLeft(lab.rectTransform, pos, new Vector2(width * 0.65f, 22f));

            Text val = UIKit.Label(parent, Mathf.RoundToInt(value01 * 100f).ToString(), 16, color,
                TextAnchor.UpperRight, UITheme.Semi);
            UIKit.PlaceTopLeft(val.rectTransform, new Vector2(pos.x + width - 70f, pos.y), new Vector2(70f, 22f));

            Image fill = UIKit.ProgressBar(parent, new Vector2(width, 8f), new Color(1f, 1f, 1f, 0.08f), color);
            UIKit.PlaceTopLeft((RectTransform)fill.transform.parent, new Vector2(pos.x, pos.y + 26f), new Vector2(width, 8f));
            fill.fillAmount = Mathf.Clamp01(value01);
            return fill;
        }

        public static UICard AddCard(Transform parent, Sprite thumb, string title, string subtitle, string tag,
            Color accent, Vector2 size, UnityAction onClick, bool compact = true)
        {
            RectTransform rt = UIKit.Rect(parent, "Card_" + title);
            rt.sizeDelta = size;

            Image bg = rt.gameObject.AddComponent<Image>();
            bg.sprite = UISprites.Rounded;
            bg.type = Image.Type.Sliced;
            bg.color = UITheme.Panel;
            bg.raycastTarget = true;

            Image glow = UIKit.Image(rt, "Glow", new Color(accent.r, accent.g, accent.b, 0f), UISprites.SoftGlow);
            UIKit.Fill(glow.rectTransform, -22f, -22f, -22f, -22f);

            Image border = UIKit.Image(rt, "Border", new Color(1f, 1f, 1f, 0.09f), UISprites.Border);
            border.type = Image.Type.Sliced;
            UIKit.Fill(border.rectTransform);

            Image bar = UIKit.Image(rt, "AccentBar", new Color(1f, 1f, 1f, 0.12f), UISprites.Pill);
            bar.rectTransform.anchorMin = new Vector2(0f, 0.5f);
            bar.rectTransform.anchorMax = new Vector2(0f, 0.5f);
            bar.rectTransform.pivot = new Vector2(0f, 0.5f);
            bar.rectTransform.anchoredPosition = new Vector2(3f, 0f);
            bar.rectTransform.sizeDelta = new Vector2(5f, size.y * 0.62f);

            float thumbSize = compact ? Mathf.Min(112f, size.y - 26f) : Mathf.Min(size.x - 28f, size.y * 0.52f);
            Image thumbBg = UIKit.Panel(rt, "ThumbBg", UITheme.Panel2, 14f, false);
            if (compact)
            {
                UIKit.PlaceTopLeft(thumbBg.rectTransform, new Vector2(15f, 13f), new Vector2(thumbSize, size.y - 26f));
            }
            else
            {
                UIKit.Place(thumbBg.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                    new Vector2(0f, -14f), new Vector2(size.x - 28f, thumbSize));
            }

            Image thumbImg = UIKit.Image(thumbBg.rectTransform, "Thumb", Color.white, thumb);
            UIKit.Fill(thumbImg.rectTransform, 3f, 3f, 3f, 3f);
            thumbImg.preserveAspect = true;

            float textX = compact ? thumbSize + 28f : 18f;
            float textTop = compact ? 18f : thumbSize + 28f;

            Text titleText = UIKit.Label(rt, title, compact ? 25 : 28, UITheme.Text, TextAnchor.UpperLeft, UITheme.Heading);
            UIKit.PlaceTopLeft(titleText.rectTransform, new Vector2(textX, textTop), new Vector2(size.x - textX - 18f, 34f));
            titleText.horizontalOverflow = HorizontalWrapMode.Wrap;

            Text subText = UIKit.Label(rt, subtitle, 17, UITheme.Muted, TextAnchor.UpperLeft, UITheme.Body);
            UIKit.PlaceTopLeft(subText.rectTransform, new Vector2(textX, textTop + 34f), new Vector2(size.x - textX - 18f, 46f));
            subText.horizontalOverflow = HorizontalWrapMode.Wrap;
            subText.verticalOverflow = VerticalWrapMode.Truncate;

            Text tagText = UIKit.Label(rt, tag, 15, accent, TextAnchor.LowerLeft, UITheme.Semi);
            UIKit.PlaceBottomLeft(tagText.rectTransform, new Vector2(textX, 15f), new Vector2(size.x - textX - 18f, 24f));

            Image badge = UIKit.Image(rt, "CheckBadge", accent, UISprites.Circle);
            badge.rectTransform.anchorMin = new Vector2(1f, 1f);
            badge.rectTransform.anchorMax = new Vector2(1f, 1f);
            badge.rectTransform.pivot = new Vector2(1f, 1f);
            badge.rectTransform.anchoredPosition = new Vector2(-14f, -14f);
            badge.rectTransform.sizeDelta = new Vector2(30f, 30f);
            Image check = UIKit.Image(badge.rectTransform, "Check", UITheme.Bg, UISprites.Check);
            check.rectTransform.sizeDelta = new Vector2(17f, 17f);
            CanvasGroup badgeGroup = badge.gameObject.AddComponent<CanvasGroup>();
            badgeGroup.alpha = 0f;

            Button btn = rt.gameObject.AddComponent<Button>();
            btn.targetGraphic = bg;
            btn.transition = Selectable.Transition.None;
            if (onClick != null) btn.onClick.AddListener(onClick);

            UICard card = rt.gameObject.AddComponent<UICard>();
            card.background = bg;
            card.border = border;
            card.glow = glow;
            card.accentBar = bar;
            card.thumbnail = thumbImg;
            card.title = titleText;
            card.subtitle = subText;
            card.tagLabel = tagText;
            card.checkBadge = badge.rectTransform;
            card.accent = accent;
            return card;
        }

        public static Text AddBigValue(Transform parent, string caption, string value, Color accent,
            Vector2 pos, Vector2 size)
        {
            Text cap = UIKit.Label(parent, caption, 14, UITheme.Muted, TextAnchor.UpperLeft, UITheme.Semi);
            UIKit.PlaceTopLeft(cap.rectTransform, pos, new Vector2(size.x, 20f));
            Text val = UIKit.Label(parent, value, 26, accent, TextAnchor.UpperLeft, UITheme.Heading);
            UIKit.PlaceTopLeft(val.rectTransform, new Vector2(pos.x, pos.y + 19f), new Vector2(size.x, 34f));
            return val;
        }

        public static void AddPanelBackdrop(Transform root, float width = 900f)
        {
            Image shade = UIKit.Image(root, "LeftShade",
                new Color(0.018f, 0.028f, 0.045f, 0.96f),
                UISprites.HorizontalGradient(new Color(0.018f, 0.028f, 0.045f, 0.98f), new Color(0.018f, 0.028f, 0.045f, 0f)));
            shade.rectTransform.anchorMin = new Vector2(0f, 0f);
            shade.rectTransform.anchorMax = new Vector2(0f, 1f);
            shade.rectTransform.pivot = new Vector2(0f, 0.5f);
            shade.rectTransform.anchoredPosition = Vector2.zero;
            shade.rectTransform.sizeDelta = new Vector2(width, 0f);
        }

        public static void AddDetailPanel(Transform root, Vector2 pos, Vector2 size)
        {
            Image panel = UIKit.Panel(root, "DetailPanel", new Color(0.035f, 0.055f, 0.082f, 0.86f), 24f, true, 0.09f);
            UIKit.PlaceTopLeft(panel.rectTransform, pos, size);
            UIKit.AddCornerBrackets(panel.rectTransform, new Color(UITheme.Accent.r, UITheme.Accent.g, UITheme.Accent.b, 0.55f), 18f, 2f);
        }

        public static void AddSoundToggle(Transform root, string label, bool on, UnityAction onClick)
        {
            Button btn = UIKit.MakeButton(root, label, new Vector2(150f, 44f), false, onClick);
            btn.name = "Sound_" + label;
        }

        private static Mesh BuildInwardCylinder(float radius, float height, int segments)
        {
            Mesh mesh = new Mesh();
            mesh.name = "InwardCylinder";

            Vector3[] verts = new Vector3[(segments + 1) * 2];
            Vector2[] uvs = new Vector2[verts.Length];
            Vector3[] normals = new Vector3[verts.Length];

            for (int i = 0; i <= segments; i++)
            {
                float a = (i / (float)segments) * Mathf.PI * 2f;
                float x = Mathf.Cos(a) * radius;
                float z = Mathf.Sin(a) * radius;
                verts[i * 2] = new Vector3(x, 0f, z);
                verts[i * 2 + 1] = new Vector3(x, height, z);
                uvs[i * 2] = new Vector2(i / (float)segments * 6f, 0f);
                uvs[i * 2 + 1] = new Vector2(i / (float)segments * 6f, 1f);
                Vector3 n = new Vector3(-Mathf.Cos(a), 0f, -Mathf.Sin(a));
                normals[i * 2] = n;
                normals[i * 2 + 1] = n;
            }

            int[] tris = new int[segments * 12];
            int t = 0;
            for (int i = 0; i < segments; i++)
            {
                int b0 = i * 2;
                int t0 = b0 + 1;
                int b1 = b0 + 2;
                int t1 = b0 + 3;

                tris[t++] = b0; tris[t++] = t0; tris[t++] = b1;
                tris[t++] = t0; tris[t++] = t1; tris[t++] = b1;

                // Mặt sau để tường nhìn thấy từ cả hai phía
                tris[t++] = b1; tris[t++] = t0; tris[t++] = b0;
                tris[t++] = b1; tris[t++] = t1; tris[t++] = t0;
            }

            mesh.vertices = verts;
            mesh.uv = uvs;
            mesh.normals = normals;
            mesh.triangles = tris;
            mesh.RecalculateBounds();
            return mesh;
        }
    }

    public class Spinner : MonoBehaviour
    {
        public Vector3 Axis = Vector3.up;
        public float Speed = 18f;
        public float BobHeight = 0f;
        public float BobSpeed = 1.4f;

        private Vector3 _origin;
        private bool _ready;

        private void Start()
        {
            _origin = transform.localPosition;
            _ready = true;
        }

        private void Update()
        {
            transform.Rotate(Axis, Speed * Time.deltaTime, Space.Self);
            if (BobHeight > 0f && _ready)
            {
                Vector3 p = _origin;
                p.y += Mathf.Sin(Time.time * BobSpeed) * BobHeight;
                transform.localPosition = p;
            }
        }
    }

    public class PulseLight : MonoBehaviour
    {
        public Light Light;
        public float Min = 0.6f;
        public float Max = 1.4f;
        public float Speed = 1.2f;

        private void Update()
        {
            if (Light == null) return;
            Light.intensity = Mathf.Lerp(Min, Max, 0.5f + 0.5f * Mathf.Sin(Time.time * Speed));
        }
    }

    public class MenuFloat : MonoBehaviour
    {
        public float Height = 0.08f;
        public float Speed = 1.1f;
        public float Roll = 3f;

        private Vector3 _origin;

        private void Start()
        {
            _origin = transform.localPosition;
        }

        private void Update()
        {
            Vector3 p = _origin;
            p.y += Mathf.Sin(Time.time * Speed) * Height;
            transform.localPosition = p;
            transform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(Time.time * Speed * 0.7f) * Roll);
        }
    }
}
