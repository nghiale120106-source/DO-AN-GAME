using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace DuaXe3D
{
    public class EnvironmentBuildResult
    {
        public GameObject Root;
        public TerrainBuildResult Terrain;
        public RoadBuildResult Road;
        public RoadPath Path;
        public TerrainTheme Theme;
        public float TerrainSize;
        public Vector3 StartPoint;
        public Vector3 StartForward;
        public Vector3 FinishPoint;
        public Vector3 FinishForward;
        public Transform FinishTrigger;
    }

    /// <summary>
    /// Dựng môi trường 3D hoàn chỉnh: bầu trời, nắng, sương, núi xa,
    /// mặt đất, mặt đường, cột mốc, cổng xuất phát/đích, bình nitro.
    /// </summary>
    public static class EnvironmentBuilder
    {
        public const float FullTerrainSize = 600f;
        public const float FullTerrainHeight = 120f;

        public static EnvironmentBuildResult Build(int themeIndex, Transform parent, bool mini = false, bool withProps = true)
        {
            TerrainTheme theme = ThemeLibrary.Get(themeIndex);
            EnvironmentBuildResult result = new EnvironmentBuildResult();
            result.Theme = theme;

            GameObject root = new GameObject(mini ? "Preview_" + theme.EnglishName : "Environment_" + theme.EnglishName);
            root.transform.SetParent(parent, false);
            result.Root = root;

            float size = mini ? 260f : FullTerrainSize;
            int resolution = mini ? 129 : 513;
            float maxHeight = mini ? 72f : FullTerrainHeight;
            result.TerrainSize = size;

            TerrainBuildResult terrain = TerrainBuilder.Build(theme, size, resolution, maxHeight, mini, root.transform);
            result.Terrain = terrain;
            result.Path = terrain.Path;

            RoadBuildResult road = RoadMeshBuilder.Build(terrain.Path, theme, root.transform);
            result.Road = road;

            SetupSkyAndLight(theme, root.transform);
            BuildGroundPlane(theme, size, root.transform);
            BuildBackdrop(theme, size, root.transform, mini);
            BuildRoadPosts(theme, terrain.Path, root.transform, mini);
            // Cổng xuất phát đặt ngay phía trước vạch xuất phát.
            BuildGantry(theme, terrain.Path, road.StartLineDistance + (mini ? 6f : 12f),
                "START", false, root.transform);
            BuildGantry(theme, terrain.Path, road.FinishLineDistance, "FINISH", true, root.transform);

            if (!mini)
            {
                result.FinishTrigger = BuildFinishTrigger(terrain.Path, road.FinishLineDistance, root.transform);
                BuildNitroPickups(terrain.Path, root.transform);
            }

            if (withProps)
                PropBuilder.Build(theme, terrain, terrain.Path, root.transform, mini);

            result.StartPoint = terrain.Path.GetPointAtDistance(2f);
            result.StartForward = terrain.Path.GetDirectionAtDistance(2f);
            result.FinishPoint = road.FinishPoint;
            result.FinishForward = road.FinishForward;

            if (mini)
            {
                RenderSettings.fogDensity = theme.FogDensity * 1.4f;
            }

            return result;
        }

        public static void SetupSkyAndLight(TerrainTheme theme, Transform parent)
        {
            RenderSettings.skybox = MaterialLibrary.Skybox(theme);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = theme.FogDensity;
            RenderSettings.fogColor = theme.FogColor;

            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = Color.Lerp(theme.SkyTint, Color.white, 0.35f) * 0.75f;
            RenderSettings.ambientEquatorColor = theme.FogColor * 0.58f;
            RenderSettings.ambientGroundColor = theme.GroundColor * 0.32f;
            RenderSettings.ambientIntensity = 1.02f;
            RenderSettings.reflectionIntensity = 1f;
            RenderSettings.defaultReflectionMode = DefaultReflectionMode.Skybox;

            GameObject sunGo = new GameObject("Sun");
            sunGo.transform.SetParent(parent, false);
            sunGo.transform.rotation = Quaternion.Euler(theme.SunPitch, theme.SunYaw, 0f);
            Light sun = sunGo.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.color = theme.SunColor;
            sun.intensity = theme.SunIntensity;
            sun.shadows = LightShadows.Soft;
            sun.shadowStrength = 0.86f;
            sun.shadowBias = 0.045f;
            sun.shadowNormalBias = 0.35f;
            sun.shadowNearPlane = 0.2f;
            sun.shadowCustomResolution = 2048;
            RenderSettings.sun = sun;

            GameObject fillGo = new GameObject("FillLight");
            fillGo.transform.SetParent(parent, false);
            fillGo.transform.rotation = Quaternion.Euler(28f, theme.SunYaw + 155f, 0f);
            Light fill = fillGo.AddComponent<Light>();
            fill.type = LightType.Directional;
            fill.color = Color.Lerp(theme.FogColor, theme.SkyTint, 0.45f);
            fill.intensity = 0.28f;
            fill.shadows = LightShadows.None;
            RenderSettings.sun = sun;

            DynamicGI.UpdateEnvironment();
        }

        private static void BuildGroundPlane(TerrainTheme theme, float size, Transform parent)
        {
            GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
            plane.name = "FarGround";
            plane.transform.SetParent(parent, false);
            plane.transform.position = new Vector3(size * 0.5f, 0.4f, size * 0.5f);
            plane.transform.localScale = Vector3.one * 620f;
            Renderer r = plane.GetComponent<Renderer>();
            r.sharedMaterial = MaterialLibrary.GroundPlane(theme);
            r.shadowCastingMode = ShadowCastingMode.Off;
            r.receiveShadows = false;
            Collider c = plane.GetComponent<Collider>();
            if (c != null) Object.Destroy(c);
        }

        private static void BuildBackdrop(TerrainTheme theme, float size, Transform parent, bool mini)
        {
            GameObject backdrop = new GameObject("Backdrop");
            backdrop.transform.SetParent(parent, false);
            Vector3 center = new Vector3(size * 0.5f, 0f, size * 0.5f);
            System.Random rnd = new System.Random(500 + theme.Index * 37);
            Material mat = MaterialLibrary.Mountain(theme);

            int farCount = mini ? 10 : 22;
            float scale = mini ? 0.38f : 1f;
            for (int i = 0; i < farCount; i++)
            {
                float a = (i / (float)farCount) * Mathf.PI * 2f + (float)rnd.NextDouble() * 0.12f;
                float radius = (mini ? 225f : 610f) + (float)rnd.NextDouble() * (mini ? 50f : 200f);
                float h = (mini ? 45f : 130f) + (float)rnd.NextDouble() * (mini ? 55f : 170f);
                float rad = (mini ? 28f : 90f) + (float)rnd.NextDouble() * (mini ? 22f : 90f);
                Vector3 pos = center + new Vector3(Mathf.Cos(a) * radius, -18f, Mathf.Sin(a) * radius);
                Mesh mesh = MeshKit.Mountain(1000 + theme.Index * 100 + i, rad, h, 9, 4);
                GameObject m = MeshKit.MeshObject("Mountain", mesh, mat, backdrop.transform, pos,
                    Quaternion.Euler(0f, (float)rnd.NextDouble() * 360f, 0f), Vector3.one * scale);
                MeshKit.SetShadows(m, false, false);
            }

            // Núi gần hơn, tối hơn để tạo chiều sâu (đặt ngoài biên terrain để không xuyên đường đua)
            int nearCount = mini ? 5 : 12;
            for (int i = 0; i < nearCount; i++)
            {
                float a = (i / (float)nearCount) * Mathf.PI * 2f + 0.35f;
                float radius = (mini ? 195f : 485f) + (float)rnd.NextDouble() * (mini ? 25f : 95f);
                float h = (mini ? 28f : 80f) + (float)rnd.NextDouble() * (mini ? 35f : 90f);
                float rad = (mini ? 20f : 55f) + (float)rnd.NextDouble() * (mini ? 16f : 55f);
                Vector3 pos = center + new Vector3(Mathf.Cos(a) * radius, -12f, Mathf.Sin(a) * radius);
                Mesh mesh = MeshKit.Mountain(2200 + theme.Index * 100 + i, rad, h, 8, 3);
                GameObject m = MeshKit.MeshObject("Hill", mesh, mat, backdrop.transform, pos,
                    Quaternion.Euler(0f, (float)rnd.NextDouble() * 360f, 0f), Vector3.one * scale);
                MeshKit.SetShadows(m, false, false);
            }

            if (!mini)
                BuildClouds(theme, center, backdrop.transform, rnd);
        }

        private static void BuildClouds(TerrainTheme theme, Vector3 center, Transform parent, System.Random rnd)
        {
            Material cloudMat = MaterialLibrary.Standard("cloudpuff", new Color(1f, 1f, 1f, 0.5f), 0f, 0.05f);
            int count = 9;
            for (int i = 0; i < count; i++)
            {
                float a = (float)rnd.NextDouble() * Mathf.PI * 2f;
                float radius = 160f + (float)rnd.NextDouble() * 460f;
                Vector3 pos = center + new Vector3(Mathf.Cos(a) * radius, 92f + (float)rnd.NextDouble() * 65f, Mathf.Sin(a) * radius);
                GameObject group = new GameObject("Cloud");
                group.transform.SetParent(parent, false);
                group.transform.position = pos;
                int puffs = 4 + rnd.Next(4);
                for (int p = 0; p < puffs; p++)
                {
                    GameObject puff = MeshKit.Sphere("Puff", group.transform,
                        new Vector3((float)rnd.NextDouble() * 44f - 22f, (float)rnd.NextDouble() * 9f, (float)rnd.NextDouble() * 24f - 12f),
                        new Vector3(30f + (float)rnd.NextDouble() * 26f, 8f + (float)rnd.NextDouble() * 7f, 18f + (float)rnd.NextDouble() * 16f),
                        cloudMat);
                    MeshKit.SetShadows(puff, false, false);
                }
            }
        }

        private static void BuildRoadPosts(TerrainTheme theme, RoadPath path, Transform parent, bool mini)
        {
            GameObject posts = new GameObject("RoadPosts");
            posts.transform.SetParent(parent, false);
            float half = path.RoadWidth * 0.5f + 1.6f;
            float step = mini ? 30f : 17f;
            Material poleMat = MaterialLibrary.Standard("post", new Color(0.88f, 0.88f, 0.84f), 0.1f, 0.4f);
            Material redMat = MaterialLibrary.Emissive("post_red", new Color(1f, 0.16f, 0.1f), 0.7f);
            Material whiteMat = MaterialLibrary.Emissive("post_white", new Color(1f, 0.96f, 0.8f), 0.8f);

            int i = 0;
            for (float d = 8f; d < path.Length - 4f; d += step, i++)
            {
                for (int s = -1; s <= 1; s += 2)
                {
                    Vector3 p = path.GetPointAtDistance(d);
                    Vector3 dir = path.GetDirectionAtDistance(d);
                    Vector3 right = Vector3.Cross(Vector3.up, dir).normalized;
                    Vector3 pos = p + right * (half * s);
                    MeshKit.Cylinder("Post", posts.transform, pos + Vector3.up * 0.62f,
                        new Vector3(0.14f, 0.62f, 0.14f), Vector3.zero, poleMat);
                    MeshKit.Box("Band", posts.transform, pos + Vector3.up * 0.9f,
                        new Vector3(0.17f, 0.16f, 0.17f), Vector3.zero, (i % 2 == 0) ? redMat : whiteMat);
                    MeshKit.Sphere("Reflector", posts.transform, pos + Vector3.up * 1.24f,
                        Vector3.one * 0.14f, (i % 2 == 0) ? redMat : whiteMat);
                }
            }
            StaticBatchingUtility.Combine(posts);
        }

        private static void BuildGantry(TerrainTheme theme, RoadPath path, float distance, string label,
            bool finish, Transform parent)
        {
            GameObject gantry = new GameObject(finish ? "FinishGantry" : "StartGantry");
            gantry.transform.SetParent(parent, false);

            Vector3 p = path.GetPointAtDistance(distance);
            Vector3 forward = path.GetDirectionAtDistance(distance);
            gantry.transform.position = p;
            gantry.transform.rotation = Quaternion.LookRotation(forward, Vector3.up);

            float half = path.RoadWidth * 0.5f;
            float span = half + 2.6f;
            float height = finish ? 8.4f : 7.4f;

            Material metal = MaterialLibrary.DarkMetal();
            Material accent = MaterialLibrary.Emissive(finish ? "gantry_finish" : "gantry_start",
                finish ? theme.AccentColor : new Color(1f, 0.72f, 0.2f), 1.1f);
            Material dark = MaterialLibrary.Standard("gantry_dark", new Color(0.055f, 0.06f, 0.075f), 0.4f, 0.4f);

            for (int s = -1; s <= 1; s += 2)
            {
                Vector3 basePos = new Vector3(span * s, 0f, 0f);
                MeshKit.Box("Pillar", gantry.transform, basePos + Vector3.up * (height * 0.5f),
                    new Vector3(0.55f, height, 0.55f), Vector3.zero, metal);
                MeshKit.Box("PillarFoot", gantry.transform, basePos + Vector3.up * 0.35f,
                    new Vector3(1.1f, 0.7f, 1.1f), Vector3.zero, dark);
                MeshKit.Box("PillarLight", gantry.transform, basePos + Vector3.up * (height * 0.78f),
                    new Vector3(0.62f, 0.18f, 0.62f), Vector3.zero, accent);
            }

            Vector3 beamPos = Vector3.up * height;
            MeshKit.Box("Beam", gantry.transform, beamPos, new Vector3(span * 2f + 0.6f, 0.55f, 0.55f), Vector3.zero, metal);
            MeshKit.Box("BeamAccent", gantry.transform, beamPos + Vector3.up * 0.32f,
                new Vector3(span * 2f + 0.6f, 0.1f, 0.6f), Vector3.zero, accent);

            // Bảng lớn căn vuông góc hoàn toàn với hướng đường đua
            Vector3 bannerPos = Vector3.up * (height - 1.35f);
            MeshKit.Box("Banner", gantry.transform, bannerPos,
                new Vector3(span * 2f - 0.4f, 2.1f, 0.22f), Vector3.zero, dark);
            MeshKit.Box("BannerStripeT", gantry.transform, bannerPos + Vector3.up * 1.0f,
                new Vector3(span * 2f - 0.4f, 0.16f, 0.26f), Vector3.zero, accent);
            MeshKit.Box("BannerStripeB", gantry.transform, bannerPos - Vector3.up * 1.0f,
                new Vector3(span * 2f - 0.4f, 0.16f, 0.26f), Vector3.zero, accent);

            // TextMesh trong Unity hướng chữ về -Z cục bộ => đặt ở mặt -Z (hướng xe chạy tới) với Quaternion.identity
            CreateText(gantry.transform, label, bannerPos + new Vector3(0f, 0f, -0.15f),
                Quaternion.identity, finish ? 0.13f : 0.14f, Color.white);
            // Mặt sau xoay 180 độ để chữ đọc đúng khi nhìn từ phía bên kia cổng.
            CreateText(gantry.transform, label, bannerPos + new Vector3(0f, 0f, 0.15f),
                Quaternion.Euler(0f, 180f, 0f), finish ? 0.13f : 0.14f, Color.white);

            // Cờ hai bên
            Material flagMat = MaterialLibrary.Emissive("flag_" + theme.Index, theme.AccentColor, 0.9f);
            for (int s = -1; s <= 1; s += 2)
            {
                Vector3 top = new Vector3(span * s, height + 0.55f, 0f);
                MeshKit.Cylinder("FlagPole", gantry.transform, top + Vector3.up * 0.7f,
                    new Vector3(0.06f, 0.7f, 0.06f), Vector3.zero, metal);
                MeshKit.Quad("Flag", gantry.transform, top + Vector3.up * 1.15f + new Vector3(0.45f * s, 0f, 0f),
                    new Vector3(0.9f, 0.55f, 1f), Vector3.zero, flagMat);
            }

            StaticBatchingUtility.Combine(gantry);
        }

        private static void CreateText(Transform parent, string text, Vector3 localPos, Quaternion localRot, float charSize, Color color)
        {
            GameObject go = new GameObject("Text_" + text);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localRotation = localRot;
            TextMesh tm = go.AddComponent<TextMesh>();
            tm.text = text;
            tm.font = UITheme.Display != null ? UITheme.Display : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            tm.fontSize = 110;
            tm.characterSize = charSize;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.alignment = TextAlignment.Center;
            tm.color = color;
            tm.fontStyle = FontStyle.Bold;
            MeshRenderer r = go.GetComponent<MeshRenderer>();
            if (r != null && tm.font != null)
            {
                r.sharedMaterial = tm.font.material;
                r.shadowCastingMode = ShadowCastingMode.Off;
            }
        }

        private static Transform BuildFinishTrigger(RoadPath path, float distance, Transform parent)
        {
            GameObject trigger = new GameObject("FinishTrigger");
            trigger.transform.SetParent(parent, false);
            Vector3 p = path.GetPointAtDistance(distance);
            Vector3 forward = path.GetDirectionAtDistance(distance);
            trigger.transform.position = p + Vector3.up * 3.5f;
            trigger.transform.rotation = Quaternion.LookRotation(forward, Vector3.up);

            BoxCollider box = trigger.AddComponent<BoxCollider>();
            box.isTrigger = true;
            box.size = new Vector3(path.RoadWidth + 7f, 8f, 5f);
            trigger.AddComponent<FinishLine>();
            return trigger.transform;
        }

        private static void BuildNitroPickups(RoadPath path, Transform parent)
        {
            Material core = MaterialLibrary.Emissive("nitro_core", new Color(0.16f, 0.85f, 1f), 3.2f);
            float[] fractions = { 0.18f, 0.36f, 0.54f, 0.72f, 0.88f };
            for (int i = 0; i < fractions.Length; i++)
            {
                float d = path.Length * fractions[i];
                float lateral = (i % 2 == 0) ? 2.2f : -2.2f;
                Vector3 p = path.GetLanePoint(d, lateral);
                NitroPickup.Create(parent, p + Vector3.up * 1.35f, core);
            }
        }

        /// <summary>Vị trí lưới xuất phát cho từng tay đua (2 hàng x 2 cột phía sau vạch xuất phát).</summary>
        public static void GetGridSlot(RoadPath path, int index, out Vector3 position, out Quaternion rotation)
        {
            float[] lateral = { -2.8f, 2.8f, -2.8f, 2.8f, 0f };
            float[] dists = { 9.5f, 16.5f, 16.5f, 9.5f, 4.5f };
            int slot = Mathf.Clamp(index, 0, dists.Length - 1);
            float d = dists[slot];
            Vector3 p = path.GetLanePoint(d, lateral[slot]);
            Vector3 forward = path.GetDirectionAtDistance(d);
            position = p + Vector3.up * 0.22f;
            rotation = Quaternion.LookRotation(forward, Vector3.up);
        }
    }
}
