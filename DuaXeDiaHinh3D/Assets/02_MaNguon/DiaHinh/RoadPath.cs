using System.Collections.Generic;
using UnityEngine;

namespace DuaXe3D
{
    /// <summary>Đường đua uốn lượn được sinh từ các điểm điều khiển Catmull-Rom.</summary>
    public class RoadPath
    {
        public Vector3[] Points;
        public float[] Distances;
        public float Length;
        public float RoadWidth = 13f;

        public int Count { get { return Points != null ? Points.Length : 0; } }

        public Vector3 GetPoint(int index)
        {
            return Points[Mathf.Clamp(index, 0, Count - 1)];
        }

        public Vector3 GetDirection(int index)
        {
            int a = Mathf.Clamp(index - 1, 0, Count - 1);
            int b = Mathf.Clamp(index + 1, 0, Count - 1);
            Vector3 d = Points[b] - Points[a];
            d.y = 0f;
            return d.sqrMagnitude > 0.0001f ? d.normalized : Vector3.forward;
        }

        public Vector3 GetRight(int index)
        {
            return Vector3.Cross(Vector3.up, GetDirection(index)).normalized;
        }

        public int NearestIndex(Vector3 position, int hint = -1)
        {
            if (Count == 0) return 0;
            int best = 0;
            float bestD = float.MaxValue;

            if (hint >= 0)
            {
                int a = Mathf.Max(0, hint - 45);
                int b = Mathf.Min(Count - 1, hint + 45);
                for (int i = a; i <= b; i++)
                {
                    float dx = Points[i].x - position.x;
                    float dz = Points[i].z - position.z;
                    float d = dx * dx + dz * dz;
                    if (d < bestD) { bestD = d; best = i; }
                }
            }

            if (hint < 0 || bestD > 70f * 70f)
            {
                bestD = float.MaxValue;
                for (int i = 0; i < Count; i += 3)
                {
                    float dx = Points[i].x - position.x;
                    float dz = Points[i].z - position.z;
                    float d = dx * dx + dz * dz;
                    if (d < bestD) { bestD = d; best = i; }
                }
                int lo = Mathf.Max(0, best - 4);
                int hi = Mathf.Min(Count - 1, best + 4);
                for (int i = lo; i <= hi; i++)
                {
                    float dx = Points[i].x - position.x;
                    float dz = Points[i].z - position.z;
                    float d = dx * dx + dz * dz;
                    if (d < bestD) { bestD = d; best = i; }
                }
            }

            return best;
        }

        public float DistanceToRoad(Vector3 position, int hint, out int index)
        {
            index = NearestIndex(position, hint);
            Vector3 p = Points[index];
            float dx = p.x - position.x;
            float dz = p.z - position.z;
            return Mathf.Sqrt(dx * dx + dz * dz);
        }

        public float Progress01(Vector3 position, int hint, out int index)
        {
            index = NearestIndex(position, hint);
            return Count <= 1 ? 0f : index / (float)(Count - 1);
        }

        public Vector3 GetPointAtDistance(float distance)
        {
            if (Count == 0) return Vector3.zero;
            if (distance <= 0f) return Points[0];
            if (distance >= Length) return Points[Count - 1];

            int lo = 0;
            int hi = Count - 1;
            while (lo < hi - 1)
            {
                int mid = (lo + hi) / 2;
                if (Distances[mid] <= distance) lo = mid;
                else hi = mid;
            }
            float span = Distances[hi] - Distances[lo];
            float t = span > 0.0001f ? (distance - Distances[lo]) / span : 0f;
            return Vector3.Lerp(Points[lo], Points[hi], t);
        }

        public Vector3 GetDirectionAtDistance(float distance)
        {
            Vector3 a = GetPointAtDistance(Mathf.Max(0f, distance - 2f));
            Vector3 b = GetPointAtDistance(Mathf.Min(Length, distance + 2f));
            Vector3 d = b - a;
            d.y = 0f;
            return d.sqrMagnitude > 0.0001f ? d.normalized : Vector3.forward;
        }

        public Vector3 GetLanePoint(float distance, float lateral)
        {
            Vector3 p = GetPointAtDistance(distance);
            Vector3 dir = GetDirectionAtDistance(distance);
            Vector3 right = Vector3.Cross(Vector3.up, dir).normalized;
            return p + right * lateral;
        }

        public float DistanceFromIndex(int index)
        {
            return Distances[Mathf.Clamp(index, 0, Count - 1)];
        }

        public float RemainingDistance(Vector3 position, int hint)
        {
            int idx = NearestIndex(position, hint);
            return Mathf.Max(0f, Length - Distances[idx]);
        }
    }

    public static class TrackLayout
    {
        // Đường đua quanh co, có khúc cua tóc + nhiều đoạn dốc.
        private static readonly Vector2[] ControlPoints =
        {
            new Vector2(72f, 78f),
            new Vector2(96f, 196f),
            new Vector2(178f, 292f),
            new Vector2(132f, 402f),
            new Vector2(226f, 502f),
            new Vector2(382f, 524f),
            new Vector2(506f, 438f),
            new Vector2(482f, 328f),
            new Vector2(368f, 302f),
            new Vector2(302f, 220f),
            new Vector2(390f, 144f),
            new Vector2(532f, 108f)
        };

        private static readonly float[] Heights =
        {
            7f, 19f, 10f, 24f, 13f, 27f, 17f, 31f, 16f, 25f, 12f, 8f
        };

        public static RoadPath Build(float scale = 1f, float heightScale = 1f, float sampleStep = 2.3f)
        {
            List<Vector3> pts = new List<Vector3>();
            int n = ControlPoints.Length;

            for (int i = 0; i < n - 1; i++)
            {
                Vector2 p0 = ControlPoints[Mathf.Max(0, i - 1)];
                Vector2 p1 = ControlPoints[i];
                Vector2 p2 = ControlPoints[i + 1];
                Vector2 p3 = ControlPoints[Mathf.Min(n - 1, i + 2)];

                float h0 = Heights[Mathf.Max(0, i - 1)];
                float h1 = Heights[i];
                float h2 = Heights[i + 1];
                float h3 = Heights[Mathf.Min(n - 1, i + 2)];

                int steps = Mathf.Max(4, Mathf.CeilToInt(Vector2.Distance(p1, p2) / sampleStep));
                for (int s = 0; s < steps; s++)
                {
                    float t = s / (float)steps;
                    Vector2 p = Catmull(p0, p1, p2, p3, t);
                    float h = Catmull1D(h0, h1, h2, h3, t);
                    pts.Add(new Vector3(p.x * scale, h * heightScale, p.y * scale));
                }
            }

            Vector2 last = ControlPoints[n - 1];
            pts.Add(new Vector3(last.x * scale, Heights[n - 1] * heightScale, last.y * scale));

            // Làm phẳng khu vực xuất phát / đích để lưới xe đứng cân bằng
            int flat = Mathf.Min(22, pts.Count / 5);
            float startY = pts[0].y;
            float endY = pts[pts.Count - 1].y;
            for (int i = 0; i < flat; i++)
            {
                float k = Mathf.SmoothStep(0f, 1f, Mathf.Max(0f, (i - 10f) / Mathf.Max(1f, flat - 10f)));
                pts[i] = new Vector3(pts[i].x, Mathf.Lerp(startY, pts[i].y, k), pts[i].z);
                int j = pts.Count - 1 - i;
                pts[j] = new Vector3(pts[j].x, Mathf.Lerp(endY, pts[j].y, k), pts[j].z);
            }

            RoadPath path = new RoadPath();
            path.Points = pts.ToArray();
            path.Distances = new float[path.Points.Length];
            float total = 0f;
            path.Distances[0] = 0f;
            for (int i = 1; i < path.Points.Length; i++)
            {
                total += Vector3.Distance(path.Points[i - 1], path.Points[i]);
                path.Distances[i] = total;
            }
            path.Length = total;
            return path;
        }

        private static Vector2 Catmull(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
        {
            float t2 = t * t;
            float t3 = t2 * t;
            return 0.5f * (
                2f * p1 +
                (-p0 + p2) * t +
                (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 +
                (-p0 + 3f * p1 - 3f * p2 + p3) * t3);
        }

        private static float Catmull1D(float p0, float p1, float p2, float p3, float t)
        {
            float t2 = t * t;
            float t3 = t2 * t;
            return 0.5f * (
                2f * p1 +
                (-p0 + p2) * t +
                (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 +
                (-p0 + 3f * p1 - 3f * p2 + p3) * t3);
        }
    }
}
