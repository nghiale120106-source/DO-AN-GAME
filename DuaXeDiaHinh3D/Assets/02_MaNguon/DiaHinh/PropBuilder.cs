using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace DuaXe3D
{
    /// <summary>
    /// Rải cây, xương rồng, đá, bụi cỏ, hàng rào, cột cỏ, cối xay gió, nhà kho...
    /// Tất cả được static batch để giữ FPS ổn định.
    /// </summary>
    public static class PropBuilder
    {
        public static void Build(TerrainTheme theme, TerrainBuildResult terrain, RoadPath path, Transform parent, bool mini)
        {
            GameObject root = new GameObject("Props");
            root.transform.SetParent(parent, false);

            float density = mini ? 0.22f : 1f;
            System.Random rnd = new System.Random(900 + theme.Index * 53);

            if (theme.Kind == ThemeKind.Desert)
            {
                BuildCacti(theme, terrain, path, root.transform, Mathf.RoundToInt(theme.TreeCount * density), rnd);
                BuildRocks(theme, terrain, path, root.transform, Mathf.RoundToInt(theme.RockCount * density), rnd);
                BuildBushes(theme, terrain, path, root.transform, Mathf.RoundToInt(theme.BushCount * density), rnd, true);
            }
            else if (theme.Kind == ThemeKind.Forest)
            {
                BuildTrees(theme, terrain, path, root.transform, Mathf.RoundToInt(theme.TreeCount * density), rnd, true);
                BuildRocks(theme, terrain, path, root.transform, Mathf.RoundToInt(theme.RockCount * density), rnd);
                BuildBushes(theme, terrain, path, root.transform, Mathf.RoundToInt(theme.BushCount * density), rnd, false);
            }
            else if (theme.Kind == ThemeKind.Mountain)
            {
                BuildTrees(theme, terrain, path, root.transform, Mathf.RoundToInt(theme.TreeCount * density), rnd, false);
                BuildRocks(theme, terrain, path, root.transform, Mathf.RoundToInt(theme.RockCount * density), rnd);
                BuildBushes(theme, terrain, path, root.transform, Mathf.RoundToInt(theme.BushCount * density), rnd, false);
            }
            else
            {
                BuildTrees(theme, terrain, path, root.transform, Mathf.RoundToInt(theme.TreeCount * density), rnd, true);
                BuildRocks(theme, terrain, path, root.transform, Mathf.RoundToInt(theme.RockCount * density), rnd);
                BuildBushes(theme, terrain, path, root.transform, Mathf.RoundToInt(theme.BushCount * density), rnd, false);
                BuildFences(theme, terrain, path, root.transform, mini);
                BuildMudPuddles(theme, terrain, path, root.transform, mini, rnd);
                BuildLogs(theme, terrain, path, root.transform, mini, rnd);
            }

            if (!mini)
                StaticBatchingUtility.Combine(root);
        }

        private static bool FindSpot(TerrainBuildResult terrain, RoadPath path, System.Random rnd,
            float minRoadDistance, float maxSlope, out Vector3 pos)
        {
            pos = Vector3.zero;
            float size = terrain.Size;
            float margin = 10f;
            for (int attempt = 0; attempt < 26; attempt++)
            {
                float x = margin + (float)rnd.NextDouble() * (size - margin * 2f);
                float z = margin + (float)rnd.NextDouble() * (size - margin * 2f);
                int idx;
                float d = path.DistanceToRoad(new Vector3(x, 0f, z), -1, out idx);
                if (d < minRoadDistance) continue;

                float slope = terrain.SlopeAt(x, z);
                if (slope > maxSlope) continue;

                float y = terrain.HeightAt(x, z);
                if (y < 1.8f || y > terrain.MaxHeight - 4f) continue;

                pos = new Vector3(x, y - 0.35f, z);
                return true;
            }
            return false;
        }

        private static void BuildTrees(TerrainTheme theme, TerrainBuildResult terrain, RoadPath path,
            Transform parent, int count, System.Random rnd, bool broadleaf)
        {
            Material trunk = MaterialLibrary.TreeTrunk();
            Material leafA = MaterialLibrary.Leaves(theme, 0);
            Material leafB = MaterialLibrary.Leaves(theme, 1);

            for (int i = 0; i < count; i++)
            {
                Vector3 pos;
                if (!FindSpot(terrain, path, rnd, path.RoadWidth * 0.5f + 7f, 34f, out pos)) continue;

                GameObject tree = new GameObject("Tree");
                tree.transform.SetParent(parent, false);
                tree.transform.position = pos;
                tree.transform.rotation = Quaternion.Euler(0f, (float)rnd.NextDouble() * 360f, 0f);
                float scale = 0.75f + (float)rnd.NextDouble() * 0.9f;

                if (broadleaf)
                {
                    float h = 3.4f * scale;
                    MeshKit.Cylinder("Trunk", tree.transform, Vector3.up * (h * 0.5f),
                        new Vector3(0.34f * scale, h * 0.5f, 0.34f * scale), Vector3.zero, trunk);
                    MeshKit.Sphere("CanopyA", tree.transform, Vector3.up * (h + 1.1f * scale),
                        new Vector3(3.3f, 2.7f, 3.3f) * scale, leafA);
                    MeshKit.Sphere("CanopyB", tree.transform, Vector3.up * (h + 2.0f * scale) + Vector3.right * 0.5f * scale,
                        new Vector3(2.2f, 1.9f, 2.2f) * scale, leafB);
                    MeshKit.Sphere("CanopyC", tree.transform, Vector3.up * (h + 1.6f * scale) - Vector3.right * 1.1f * scale,
                        new Vector3(2.0f, 1.6f, 2.0f) * scale, leafA);
                }
                else
                {
                    float h = 2.2f * scale;
                    MeshKit.Cylinder("Trunk", tree.transform, Vector3.up * (h * 0.5f),
                        new Vector3(0.28f * scale, h * 0.5f, 0.28f * scale), Vector3.zero, trunk);
                    float y = h;
                    for (int c = 0; c < 3; c++)
                    {
                        float rad = (2.0f - c * 0.45f) * scale;
                        float height = (2.7f - c * 0.35f) * scale;
                        Mesh mesh = MeshKit.Cone(rad, height, 9);
                        MeshKit.MeshObject("Pine", mesh, c == 1 ? leafB : leafA, tree.transform,
                            Vector3.up * (y + height * 0.45f), Quaternion.identity, Vector3.one);
                        y += height * 0.55f;
                    }
                }

                MeshKit.SetShadows(tree, true, true);
            }
        }

        private static void BuildCacti(TerrainTheme theme, TerrainBuildResult terrain, RoadPath path,
            Transform parent, int count, System.Random rnd)
        {
            Material cactus = MaterialLibrary.Cactus();
            Material cactusDark = MaterialLibrary.Standard("cactus_dark", theme.PropColor * 0.82f, 0f, 0.2f);

            for (int i = 0; i < count; i++)
            {
                Vector3 pos;
                if (!FindSpot(terrain, path, rnd, path.RoadWidth * 0.5f + 6f, 28f, out pos)) continue;

                GameObject plant = new GameObject("Cactus");
                plant.transform.SetParent(parent, false);
                plant.transform.position = pos;
                plant.transform.rotation = Quaternion.Euler(0f, (float)rnd.NextDouble() * 360f, 0f);
                float scale = 0.7f + (float)rnd.NextDouble() * 0.8f;
                float h = 3.2f * scale;

                MeshKit.Cylinder("Trunk", plant.transform, Vector3.up * (h * 0.5f),
                    new Vector3(0.62f * scale, h * 0.5f, 0.62f * scale), Vector3.zero, cactus);
                MeshKit.Sphere("Top", plant.transform, Vector3.up * h,
                    new Vector3(0.62f, 0.4f, 0.62f) * scale, cactus);

                int arms = 1 + rnd.Next(2);
                for (int a = 0; a < arms; a++)
                {
                    int side = (a % 2 == 0) ? -1 : 1;
                    float ay = h * (0.48f + (float)rnd.NextDouble() * 0.22f);
                    MeshKit.Cylinder("Arm", plant.transform,
                        new Vector3(side * 0.72f * scale, ay, 0f),
                        new Vector3(0.3f * scale, 0.55f * scale, 0.3f * scale),
                        new Vector3(0f, 0f, 90f), cactusDark);
                    MeshKit.Cylinder("ArmUp", plant.transform,
                        new Vector3(side * 1.22f * scale, ay + 0.5f * scale, 0f),
                        new Vector3(0.28f * scale, 0.62f * scale, 0.28f * scale),
                        Vector3.zero, cactus);
                }

                MeshKit.SetShadows(plant, true, true);
            }
        }

        private static void BuildRocks(TerrainTheme theme, TerrainBuildResult terrain, RoadPath path,
            Transform parent, int count, System.Random rnd)
        {
            Material rock = MaterialLibrary.Rock(theme);
            Material rockLight = MaterialLibrary.Standard("rocklight_" + theme.Index,
                Color.Lerp(theme.RockColor, theme.SandColor, 0.3f), 0.02f, 0.22f);

            for (int i = 0; i < count; i++)
            {
                Vector3 pos;
                if (!FindSpot(terrain, path, rnd, path.RoadWidth * 0.5f + 4.5f, 55f, out pos)) continue;

                float scale = 0.5f + (float)rnd.NextDouble() * 1.8f;
                float radius = (1.0f + (float)rnd.NextDouble() * 1.3f) * scale;
                float height = (0.7f + (float)rnd.NextDouble() * 1.0f) * scale;
                Mesh mesh = MeshKit.Mountain(3000 + theme.Index * 71 + i, radius, height, 7, 3);
                GameObject go = MeshKit.MeshObject("Rock", mesh, (i % 4 == 0) ? rockLight : rock, parent,
                    pos + Vector3.up * (height * 0.12f),
                    Quaternion.Euler(0f, (float)rnd.NextDouble() * 360f, 0f), Vector3.one);
                MeshKit.SetShadows(go, true, true);
            }
        }

        private static void BuildBushes(TerrainTheme theme, TerrainBuildResult terrain, RoadPath path,
            Transform parent, int count, System.Random rnd, bool dry)
        {
            Material bushA = MaterialLibrary.Leaves(theme, 0);
            Material bushB = MaterialLibrary.Leaves(theme, 1);
            if (dry)
            {
                bushA = MaterialLibrary.Standard("drybush", theme.PropColor * 0.75f, 0f, 0.15f);
                bushB = MaterialLibrary.Standard("drybush2", theme.SandColor * 0.7f, 0f, 0.15f);
            }

            for (int i = 0; i < count; i++)
            {
                Vector3 pos;
                if (!FindSpot(terrain, path, rnd, path.RoadWidth * 0.5f + 5f, 40f, out pos)) continue;

                GameObject bush = new GameObject("Bush");
                bush.transform.SetParent(parent, false);
                bush.transform.position = pos;
                bush.transform.rotation = Quaternion.Euler(0f, (float)rnd.NextDouble() * 360f, 0f);
                float scale = 0.4f + (float)rnd.NextDouble() * 0.8f;
                int puffs = 2 + rnd.Next(2);
                for (int p = 0; p < puffs; p++)
                {
                    Vector3 off = new Vector3((float)rnd.NextDouble() * 1.2f - 0.6f, 0.25f + (float)rnd.NextDouble() * 0.35f, (float)rnd.NextDouble() * 1.2f - 0.6f) * scale;
                    MeshKit.Sphere("Puff", bush.transform, off,
                        new Vector3(1.2f, 0.8f, 1.2f) * scale * (0.7f + (float)rnd.NextDouble() * 0.5f),
                        p % 2 == 0 ? bushA : bushB);
                }
                MeshKit.SetShadows(bush, true, true);
            }
        }

        private static void BuildFences(TerrainTheme theme, TerrainBuildResult terrain, RoadPath path, Transform parent, bool mini)
        {
            GameObject fences = new GameObject("Fences");
            fences.transform.SetParent(parent, false);
            Material wood = MaterialLibrary.Wood();
            Material darkWood = MaterialLibrary.DarkWood();
            float half = path.RoadWidth * 0.5f + 3.6f;
            float step = mini ? 22f : 8f;

            for (int side = -1; side <= 1; side += 2)
            {
                List<Vector3> posts = new List<Vector3>();
                for (float d = 18f; d < path.Length - 18f; d += step)
                {
                    Vector3 p = path.GetPointAtDistance(d);
                    Vector3 dir = path.GetDirectionAtDistance(d);
                    Vector3 right = Vector3.Cross(Vector3.up, dir).normalized;
                    Vector3 basePos = p + right * (half * side);
                    float y = terrain.HeightAt(Mathf.Clamp(basePos.x, 1f, terrain.Size - 1f), Mathf.Clamp(basePos.z, 1f, terrain.Size - 1f));
                    posts.Add(new Vector3(basePos.x, y, basePos.z));
                }

                for (int i = 0; i < posts.Count; i++)
                {
                    Vector3 p = posts[i];
                    MeshKit.Box("FencePost", fences.transform, p + Vector3.up * 0.6f,
                        new Vector3(0.18f, 1.2f, 0.18f), Vector3.zero, darkWood);

                    if (i >= posts.Count - 1) continue;
                    Vector3 q = posts[i + 1];
                    Vector3 mid = (p + q) * 0.5f;
                    Vector3 dir = q - p;
                    dir.y = 0f;
                    float len = dir.magnitude;
                    if (len < 0.1f) continue;
                    float yaw = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
                    for (int r = 0; r < 2; r++)
                    {
                        MeshKit.Box("FenceRail", fences.transform, mid + Vector3.up * (0.45f + r * 0.42f),
                            new Vector3(0.09f, 0.11f, len), new Vector3(0f, yaw, 0f), wood);
                    }
                }
            }

            if (!mini) StaticBatchingUtility.Combine(fences);
        }

        private static void BuildMudPuddles(TerrainTheme theme, TerrainBuildResult terrain, RoadPath path,
            Transform parent, bool mini, System.Random rnd)
        {
            Material puddleMat = MaterialLibrary.Standard("mud_puddle", new Color(0.24f, 0.17f, 0.11f, 0.88f), 0.15f, 0.92f);
            GameObject group = new GameObject("MudPuddles");
            group.transform.SetParent(parent, false);

            // Vũng bùn trên mặt đường và hai bên lề tạo cảm giác đường bùn đặc trưng
            int roadPuddles = mini ? 8 : 22;
            for (int i = 0; i < roadPuddles; i++)
            {
                float d = Mathf.Lerp(32f, path.Length - 28f, (i + 0.3f) / roadPuddles);
                float lat = ((i % 2 == 0) ? 1f : -1f) * (1.2f + (float)rnd.NextDouble() * 3.0f);
                Vector3 p = path.GetLanePoint(d, lat);
                float sx = 3.2f + (float)rnd.NextDouble() * 3.5f;
                float sz = 4.2f + (float)rnd.NextDouble() * 4.5f;
                GameObject go = MeshKit.Cylinder("RoadPuddle", group.transform,
                    p + Vector3.up * 0.145f, new Vector3(sx, 0.015f, sz),
                    new Vector3(0f, (float)rnd.NextDouble() * 180f, 0f), puddleMat);
                MeshKit.SetShadows(go, false, true);
            }

            int offPuddles = mini ? 6 : 24;
            for (int i = 0; i < offPuddles; i++)
            {
                Vector3 pos;
                if (!FindSpot(terrain, path, rnd, path.RoadWidth * 0.5f + 5f, 12f, out pos)) continue;
                float s = 4f + (float)rnd.NextDouble() * 6f;
                GameObject go = MeshKit.Cylinder("MudPool", group.transform,
                    pos + Vector3.up * 0.38f, new Vector3(s, 0.02f, s * 0.78f),
                    new Vector3(0f, (float)rnd.NextDouble() * 180f, 0f), puddleMat);
                MeshKit.SetShadows(go, false, true);
            }
            if (!mini) StaticBatchingUtility.Combine(group);
        }

        private static void BuildLogs(TerrainTheme theme, TerrainBuildResult terrain, RoadPath path,
            Transform parent, bool mini, System.Random rnd)
        {
            Material darkWood = MaterialLibrary.DarkWood();
            int count = mini ? 5 : 26;
            GameObject group = new GameObject("FallenLogs");
            group.transform.SetParent(parent, false);
            for (int i = 0; i < count; i++)
            {
                Vector3 pos;
                if (!FindSpot(terrain, path, rnd, path.RoadWidth * 0.5f + 7f, 22f, out pos)) continue;
                float len = 1.6f + (float)rnd.NextDouble() * 1.8f;
                MeshKit.Cylinder("Log", group.transform, pos + Vector3.up * 0.52f,
                    new Vector3(0.55f, len, 0.55f),
                    new Vector3(0f, (float)rnd.NextDouble() * 360f, 88f), darkWood);
            }
            if (!mini) StaticBatchingUtility.Combine(group);
        }
    }
}
