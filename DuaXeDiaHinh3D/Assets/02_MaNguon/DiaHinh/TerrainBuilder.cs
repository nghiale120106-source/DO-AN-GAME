using System.Collections.Generic;
using UnityEngine;

namespace DuaXe3D
{
    public class TerrainBuildResult
    {
        public GameObject Root;
        public Terrain Terrain;
        public TerrainData Data;
        public RoadPath Path;
        public float Size;
        public float MaxHeight;
        public List<Vector2> Holes = new List<Vector2>();
        public List<float> HoleRadii = new List<float>();
        public List<float> HoleDepths = new List<float>();

        public float HeightAt(float x, float z)
        {
            if (Data == null) return 0f;
            float u = Mathf.Clamp01(x / Size);
            float v = Mathf.Clamp01(z / Size);
            return Data.GetInterpolatedHeight(u, v);
        }

        public float SlopeAt(float x, float z)
        {
            if (Data == null) return 0f;
            return Data.GetSteepness(Mathf.Clamp01(x / Size), Mathf.Clamp01(z / Size));
        }
    }

    public static class TerrainBuilder
    {
        private class RoadGrid
        {
            public int Cell;
            public int N;
            public List<int>[] Buckets;

            public RoadGrid(RoadPath path, float size)
            {
                Cell = 18;
                N = Mathf.CeilToInt(size / Cell) + 2;
                Buckets = new List<int>[N * N];
                for (int i = 0; i < path.Count; i++)
                {
                    Vector3 p = path.Points[i];
                    int gx = Mathf.Clamp((int)(p.x / Cell), 0, N - 1);
                    int gz = Mathf.Clamp((int)(p.z / Cell), 0, N - 1);
                    int key = gz * N + gx;
                    if (Buckets[key] == null) Buckets[key] = new List<int>();
                    Buckets[key].Add(i);
                }
            }

            public float Nearest(RoadPath path, float x, float z, out int bestIndex)
            {
                int gx = Mathf.Clamp((int)(x / Cell), 0, N - 1);
                int gz = Mathf.Clamp((int)(z / Cell), 0, N - 1);
                float best = float.MaxValue;
                bestIndex = -1;

                for (int dz = -3; dz <= 3; dz++)
                {
                    int cz = gz + dz;
                    if (cz < 0 || cz >= N) continue;
                    for (int dx = -3; dx <= 3; dx++)
                    {
                        int cx = gx + dx;
                        if (cx < 0 || cx >= N) continue;
                        List<int> bucket = Buckets[cz * N + cx];
                        if (bucket == null) continue;
                        for (int k = 0; k < bucket.Count; k++)
                        {
                            int i = bucket[k];
                            Vector3 p = path.Points[i];
                            float ddx = p.x - x;
                            float ddz = p.z - z;
                            float d = ddx * ddx + ddz * ddz;
                            if (d < best) { best = d; bestIndex = i; }
                        }
                    }
                }

                return bestIndex >= 0 ? Mathf.Sqrt(best) : 999f;
            }
        }

        public static TerrainBuildResult Build(TerrainTheme theme, float size, int resolution, float maxHeight,
            bool mini, Transform parent)
        {
            TerrainBuildResult result = new TerrainBuildResult();
            result.Size = size;
            result.MaxHeight = maxHeight;

            GameObject root = new GameObject(mini ? "MiniTerrain" : "TerrainRoot");
            root.transform.SetParent(parent, false);
            result.Root = root;

            RoadPath path = TrackLayout.Build(size / 600f, mini ? 0.42f : 1f, mini ? 1.5f : 2.3f);
            path.RoadWidth = mini ? 8.5f : 13f;
            result.Path = path;

            int res = resolution;
            float cell = size / (res - 1f);

            TerrainData data = new TerrainData();
            data.name = "Terrain_" + theme.EnglishName;
            data.heightmapResolution = res;
            data.size = new Vector3(size, maxHeight, size);
            data.alphamapResolution = mini ? 128 : 512;
            data.baseMapResolution = mini ? 256 : 1024;

            // Chọn 2 hố đất không nằm trên đường
            if (!mini)
            {
                Vector2[] candidates =
                {
                    new Vector2(0.28f, 0.23f),
                    new Vector2(0.73f, 0.68f),
                    new Vector2(0.18f, 0.63f),
                    new Vector2(0.82f, 0.24f),
                    new Vector2(0.55f, 0.42f)
                };
                for (int i = 0; i < candidates.Length && result.Holes.Count < 2; i++)
                {
                    Vector2 c = candidates[i] * size;
                    int idx;
                    float d = path.DistanceToRoad(new Vector3(c.x, 0f, c.y), -1, out idx);
                    if (d > 42f)
                    {
                        result.Holes.Add(c);
                        result.HoleRadii.Add(24f + i * 2f);
                        result.HoleDepths.Add(5.5f + i * 1.2f);
                    }
                }
            }

            RoadGrid grid = new RoadGrid(path, size);
            float[] distField = new float[res * res];
            float[,] heights = new float[res, res];

            float amp = theme.TerrainAmplitude * (mini ? 0.42f : 1f);
            int seed = 1300 + theme.Index * 91;
            float roadHalf = path.RoadWidth * 0.5f;
            float shoulderOuter = roadHalf + (mini ? 24f : 44f);

            for (int z = 0; z < res; z++)
            {
                float wz = z * cell;
                for (int x = 0; x < res; x++)
                {
                    float wx = x * cell;
                    float u = wx / size;
                    float v = wz / size;

                    float n1 = Mathf.PerlinNoise(u * 2.3f + seed, v * 2.3f + seed);
                    float n2 = Mathf.PerlinNoise(u * 5.6f + seed * 0.37f, v * 5.6f + seed * 0.37f);
                    float n3 = Mathf.PerlinNoise(u * 12f + seed * 0.71f, v * 12f + seed * 0.71f);
                    float h = 3.5f + (n1 * 0.62f + n2 * 0.26f + n3 * 0.12f) * amp;

                    if (theme.Kind == ThemeKind.Mountain)
                        h += Mathf.Pow(n1, 2.4f) * amp * 0.9f;

                    int roadIndex;
                    float d = grid.Nearest(path, wx, wz, out roadIndex);
                    distField[z * res + x] = d;

                    if (roadIndex >= 0 && d < shoulderOuter + 4f)
                    {
                        float roadY = path.Points[roadIndex].y;
                        float w = 1f - Mathf.SmoothStep(roadHalf + 0.4f, shoulderOuter, d);
                        h = Mathf.Lerp(h, roadY, w);
                        if (d < roadHalf + 2f) h = roadY;
                    }

                    for (int hole = 0; hole < result.Holes.Count; hole++)
                    {
                        Vector2 c = result.Holes[hole];
                        float hd = Vector2.Distance(new Vector2(wx, wz), c);
                        float radius = result.HoleRadii[hole];
                        if (hd < radius)
                        {
                            float k = 1f - hd / radius;
                            h -= result.HoleDepths[hole] * k * k * (3f - 2f * k);
                        }
                    }

                    h = Mathf.Clamp(h, 1.2f, maxHeight - 2f);
                    heights[z, x] = h / maxHeight;
                }
            }

            data.SetHeights(0, 0, heights);

            TerrainLayer[] layers = CreateLayers(theme, mini);
            data.terrainLayers = layers;
            PaintSplat(data, heights, distField, res, size, maxHeight, theme, result, mini);

            GameObject terrainGo = Terrain.CreateTerrainGameObject(data);
            terrainGo.name = "Terrain";
            terrainGo.transform.SetParent(root.transform, false);
            Terrain terrain = terrainGo.GetComponent<Terrain>();
            terrain.drawInstanced = true;
            terrain.heightmapPixelError = mini ? 3f : 5f;
            terrain.basemapDistance = mini ? 350f : 650f;
            terrain.Flush();
            result.Terrain = terrain;
            result.Data = data;

            return result;
        }

        private static TerrainLayer[] CreateLayers(TerrainTheme theme, bool mini)
        {
            GroundStyle baseStyle = theme.Kind == ThemeKind.Desert ? GroundStyle.Sand
                : (theme.Kind == ThemeKind.Mud ? GroundStyle.Dirt : GroundStyle.Grass);
            Color baseColor = theme.Kind == ThemeKind.Desert ? theme.SandColor
                : (theme.Kind == ThemeKind.Mud ? theme.GroundColor : theme.GrassColor);

            TerrainLayer baseLayer = new TerrainLayer();
            baseLayer.name = "Base_" + theme.EnglishName;
            baseLayer.diffuseTexture = ProcTex.Ground(theme.Name + "_base", baseColor, baseStyle, 200 + theme.Index);
            baseLayer.tileSize = new Vector2(mini ? 14f : 18f, mini ? 14f : 18f);
            baseLayer.smoothness = theme.Kind == ThemeKind.Mud ? 0.34f : 0.08f;
            baseLayer.normalMapTexture = ProcTex.GroundNormal(theme.Name + "_base", baseColor, baseStyle, 200 + theme.Index);
            baseLayer.normalScale = 0.55f;

            TerrainLayer dirtLayer = new TerrainLayer();
            dirtLayer.name = "Dirt_" + theme.EnglishName;
            dirtLayer.diffuseTexture = ProcTex.Ground(theme.Name + "_dirt", theme.DirtColor, GroundStyle.Dirt, 300 + theme.Index);
            dirtLayer.tileSize = new Vector2(mini ? 12f : 14f, mini ? 12f : 14f);
            dirtLayer.smoothness = theme.Kind == ThemeKind.Mud ? 0.55f : 0.05f;
            dirtLayer.normalMapTexture = ProcTex.GroundNormal(theme.Name + "_dirt", theme.DirtColor, GroundStyle.Dirt, 300 + theme.Index);
            dirtLayer.normalScale = 0.7f;

            TerrainLayer rockLayer = new TerrainLayer();
            rockLayer.name = "Rock_" + theme.EnglishName;
            rockLayer.diffuseTexture = ProcTex.Ground(theme.Name + "_rock", theme.RockColor, GroundStyle.Rock, 400 + theme.Index);
            rockLayer.tileSize = new Vector2(mini ? 16f : 22f, mini ? 16f : 22f);
            rockLayer.smoothness = 0.14f;
            rockLayer.normalMapTexture = ProcTex.GroundNormal(theme.Name + "_rock", theme.RockColor, GroundStyle.Rock, 400 + theme.Index);
            rockLayer.normalScale = 1f;

            TerrainLayer snowLayer = new TerrainLayer();
            snowLayer.name = "Snow_" + theme.EnglishName;
            snowLayer.diffuseTexture = ProcTex.Ground(theme.Name + "_snow", new Color(0.9f, 0.94f, 1f), GroundStyle.Snow, 500 + theme.Index);
            snowLayer.tileSize = new Vector2(24f, 24f);
            snowLayer.smoothness = 0.18f;
            snowLayer.normalMapTexture = ProcTex.GroundNormal(theme.Name + "_snow", new Color(0.9f, 0.94f, 1f), GroundStyle.Snow, 500 + theme.Index);
            snowLayer.normalScale = 0.5f;

            return new[] { baseLayer, dirtLayer, rockLayer, snowLayer };
        }

        private static void PaintSplat(TerrainData data, float[,] heights, float[] distField, int res,
            float size, float maxHeight, TerrainTheme theme, TerrainBuildResult result, bool mini)
        {
            int alphaRes = data.alphamapResolution;
            float[,,] alpha = new float[alphaRes, alphaRes, 4];
            float cell = size / (res - 1f);
            float roadHalf = result.Path.RoadWidth * 0.5f;

            for (int z = 0; z < alphaRes; z++)
            {
                float v = z / (alphaRes - 1f);
                int hz = Mathf.Clamp(Mathf.RoundToInt(v * (res - 1)), 0, res - 1);
                for (int x = 0; x < alphaRes; x++)
                {
                    float u = x / (alphaRes - 1f);
                    int hx = Mathf.Clamp(Mathf.RoundToInt(u * (res - 1)), 0, res - 1);

                    int hx0 = Mathf.Max(0, hx - 1);
                    int hx1 = Mathf.Min(res - 1, hx + 1);
                    int hz0 = Mathf.Max(0, hz - 1);
                    int hz1 = Mathf.Min(res - 1, hz + 1);
                    float dhx = (heights[hz, hx1] - heights[hz, hx0]) * maxHeight / Mathf.Max(0.01f, (hx1 - hx0) * cell);
                    float dhz = (heights[hz1, hx] - heights[hz0, hx]) * maxHeight / Mathf.Max(0.01f, (hz1 - hz0) * cell);
                    float slope = Mathf.Sqrt(dhx * dhx + dhz * dhz);
                    float steep = Mathf.Clamp01(slope / 1.4f);
                    float hn = heights[hz, hx];

                    float d = distField[hz * res + hx];
                    float dirt = 1f - Mathf.SmoothStep(roadHalf + 1.5f, roadHalf + 12f, d);
                    dirt *= 0.75f;

                    float rock = Mathf.SmoothStep(0.16f, 0.55f, steep);
                    if (theme.Kind == ThemeKind.Mountain)
                        rock = Mathf.Max(rock, Mathf.SmoothStep(0.55f, 0.85f, hn) * 0.7f);

                    float snow = 0f;
                    if (theme.Kind == ThemeKind.Mountain)
                        snow = Mathf.SmoothStep(0.62f, 0.8f, hn) * (1f - steep * 0.5f) * 0.9f;

                    for (int hole = 0; hole < result.Holes.Count; hole++)
                    {
                        Vector2 c = result.Holes[hole];
                        float hd = Vector2.Distance(new Vector2(u * size, v * size), c);
                        if (hd < result.HoleRadii[hole] * 1.15f)
                            dirt = Mathf.Max(dirt, 0.9f);
                    }

                    float baseW = Mathf.Max(0f, 1f - dirt - rock - snow);
                    float sum = baseW + dirt + rock + snow;
                    if (sum < 0.001f) { baseW = 1f; sum = 1f; }

                    alpha[z, x, 0] = baseW / sum;
                    alpha[z, x, 1] = dirt / sum;
                    alpha[z, x, 2] = rock / sum;
                    alpha[z, x, 3] = snow / sum;
                }
            }

            data.SetAlphamaps(0, 0, alpha);
        }
    }
}
