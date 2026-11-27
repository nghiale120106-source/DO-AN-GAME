using System.Collections.Generic;
using UnityEngine;

namespace DuaXe3D
{
    public class RoadBuildResult
    {
        public GameObject Root;
        public Mesh RoadMesh;
        public float StartLineDistance;
        public float FinishLineDistance;
        public Vector3 FinishPoint;
        public Vector3 FinishForward;
        public Vector3 StartPoint;
        public Vector3 StartForward;
    }

    /// <summary>
    /// Sinh mặt đường uốn lượn, lề đất, vạch kẻ, vạch xuất phát/đích.
    /// </summary>
    public static class RoadMeshBuilder
    {
        private const float RoadLift = 0.12f;

        public static RoadBuildResult Build(RoadPath path, TerrainTheme theme, Transform parent)
        {
            RoadBuildResult result = new RoadBuildResult();
            GameObject root = new GameObject("Road");
            root.transform.SetParent(parent, false);
            result.Root = root;

            float half = path.RoadWidth * 0.5f;

            // Lề đất rộng hơn mặt đường
            Mesh shoulder = BuildRibbon(path, 0f, path.RoadWidth + 5.5f, 0.065f, 3, 0.02f);
            MeshKit.MeshObject("Shoulder", shoulder, MaterialLibrary.Dirt(theme), root.transform,
                Vector3.zero, Quaternion.identity, Vector3.one);

            // Mặt đường chính (5 cột để có độ vồng nhẹ)
            Mesh road = BuildRibbon(path, 0f, path.RoadWidth, RoadLift, 5, 0.05f);
            road.name = "RoadSurface";
            GameObject roadGo = MeshKit.MeshObject("Surface", road, MaterialLibrary.Road(theme), root.transform,
                Vector3.zero, Quaternion.identity, Vector3.one);
            MeshCollider collider = roadGo.AddComponent<MeshCollider>();
            collider.sharedMesh = road;
            collider.convex = false;
            result.RoadMesh = road;

            // Vạch trắng hai bên
            float edgeOffset = half - 0.62f;
            Mesh edgeL = BuildStrip(path, -edgeOffset, 0.32f, RoadLift + 0.035f, 0f);
            MeshKit.MeshObject("EdgeLineL", edgeL, MaterialLibrary.RoadLine(), root.transform,
                Vector3.zero, Quaternion.identity, Vector3.one);
            Mesh edgeR = BuildStrip(path, edgeOffset, 0.32f, RoadLift + 0.035f, 0f);
            MeshKit.MeshObject("EdgeLineR", edgeR, MaterialLibrary.RoadLine(), root.transform,
                Vector3.zero, Quaternion.identity, Vector3.one);

            // Vạch vàng đứt quãng giữa đường
            Mesh center = BuildDashedStrip(path, 0.22f, RoadLift + 0.045f, 4.6f, 4.4f, 2.5f);
            MeshKit.MeshObject("CenterLine", center, MaterialLibrary.CenterLine(), root.transform,
                Vector3.zero, Quaternion.identity, Vector3.one);

            // Vạch xuất phát / đích kẻ caro
            result.StartLineDistance = 21.5f;
            result.FinishLineDistance = Mathf.Max(10f, path.Length - 7f);

            Mesh startStrip = BuildCheckerStrip(path, result.StartLineDistance, 2.6f);
            MeshKit.MeshObject("StartLine", startStrip, MaterialLibrary.CheckerStrip(), root.transform,
                Vector3.zero, Quaternion.identity, Vector3.one);

            Mesh finishStrip = BuildCheckerStrip(path, result.FinishLineDistance, 3.2f);
            MeshKit.MeshObject("FinishLine", finishStrip, MaterialLibrary.CheckerStrip(), root.transform,
                Vector3.zero, Quaternion.identity, Vector3.one);

            result.StartPoint = path.GetPointAtDistance(0f);
            result.StartForward = path.GetDirectionAtDistance(0f);
            result.FinishPoint = path.GetPointAtDistance(result.FinishLineDistance);
            result.FinishForward = path.GetDirectionAtDistance(result.FinishLineDistance);

            StaticBatchingUtility.Combine(root);
            return result;
        }

        /// <summary>Dải băng theo đường, có thể vồng nhẹ ở giữa.</summary>
        private static Mesh BuildRibbon(RoadPath path, float lateralOffset, float width, float lift,
            int columns, float crown)
        {
            int samples = path.Count;
            columns = Mathf.Max(2, columns);
            Vector3[] verts = new Vector3[samples * columns];
            Vector2[] uvs = new Vector2[verts.Length];
            int[] tris = new int[(samples - 1) * (columns - 1) * 6];

            for (int i = 0; i < samples; i++)
            {
                Vector3 p = path.Points[i];
                Vector3 right = path.GetRight(i);
                float v = path.Distances[i] / 9f;
                for (int c = 0; c < columns; c++)
                {
                    float t = columns == 1 ? 0.5f : c / (float)(columns - 1);
                    float lat = (t - 0.5f) * width + lateralOffset;
                    float bulge = Mathf.Sin(t * Mathf.PI) * crown;
                    verts[i * columns + c] = p + right * lat + Vector3.up * (lift + bulge);
                    uvs[i * columns + c] = new Vector2(t, v);
                }
            }

            int ti = 0;
            for (int i = 0; i < samples - 1; i++)
            {
                for (int c = 0; c < columns - 1; c++)
                {
                    int a = i * columns + c;
                    int b = a + 1;
                    int d = (i + 1) * columns + c;
                    int e = d + 1;
                    tris[ti++] = a; tris[ti++] = d; tris[ti++] = b;
                    tris[ti++] = b; tris[ti++] = d; tris[ti++] = e;
                }
            }

            Mesh mesh = new Mesh();
            mesh.name = "RoadRibbon";
            mesh.vertices = verts;
            mesh.uv = uvs;
            mesh.triangles = tris;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        private static Mesh BuildStrip(RoadPath path, float lateralOffset, float width, float lift, float uvScale)
        {
            int samples = path.Count;
            Vector3[] verts = new Vector3[samples * 2];
            Vector2[] uvs = new Vector2[verts.Length];
            int[] tris = new int[(samples - 1) * 6];

            for (int i = 0; i < samples; i++)
            {
                Vector3 p = path.Points[i];
                Vector3 right = path.GetRight(i);
                float v = path.Distances[i] / Mathf.Max(1f, uvScale);
                verts[i * 2] = p + right * (lateralOffset - width * 0.5f) + Vector3.up * lift;
                verts[i * 2 + 1] = p + right * (lateralOffset + width * 0.5f) + Vector3.up * lift;
                uvs[i * 2] = new Vector2(0f, v);
                uvs[i * 2 + 1] = new Vector2(1f, v);
            }

            int ti = 0;
            for (int i = 0; i < samples - 1; i++)
            {
                int a = i * 2;
                int b = a + 1;
                int d = (i + 1) * 2;
                int e = d + 1;
                tris[ti++] = a; tris[ti++] = d; tris[ti++] = b;
                tris[ti++] = b; tris[ti++] = d; tris[ti++] = e;
            }

            Mesh mesh = new Mesh();
            mesh.name = "RoadStrip";
            mesh.vertices = verts;
            mesh.uv = uvs;
            mesh.triangles = tris;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        private static Mesh BuildDashedStrip(RoadPath path, float width, float lift,
            float dashLength, float gapLength, float startDistance)
        {
            List<Vector3> verts = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            List<int> tris = new List<int>();

            for (float d = startDistance; d < path.Length - 3f; d += dashLength + gapLength)
            {
                float d2 = Mathf.Min(path.Length - 2f, d + dashLength);
                if (d2 <= d + 0.5f) break;

                Vector3 p1 = path.GetPointAtDistance(d);
                Vector3 p2 = path.GetPointAtDistance(d2);
                Vector3 r1 = Vector3.Cross(Vector3.up, path.GetDirectionAtDistance(d)).normalized;
                Vector3 r2 = Vector3.Cross(Vector3.up, path.GetDirectionAtDistance(d2)).normalized;

                int baseIndex = verts.Count;
                verts.Add(p1 - r1 * (width * 0.5f) + Vector3.up * lift);
                verts.Add(p1 + r1 * (width * 0.5f) + Vector3.up * lift);
                verts.Add(p2 - r2 * (width * 0.5f) + Vector3.up * lift);
                verts.Add(p2 + r2 * (width * 0.5f) + Vector3.up * lift);
                uvs.Add(new Vector2(0f, 0f));
                uvs.Add(new Vector2(1f, 0f));
                uvs.Add(new Vector2(0f, 1f));
                uvs.Add(new Vector2(1f, 1f));

                tris.Add(baseIndex); tris.Add(baseIndex + 2); tris.Add(baseIndex + 1);
                tris.Add(baseIndex + 1); tris.Add(baseIndex + 2); tris.Add(baseIndex + 3);
            }

            Mesh mesh = new Mesh();
            mesh.name = "RoadDashes";
            mesh.SetVertices(verts);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        private static Mesh BuildCheckerStrip(RoadPath path, float distance, float length)
        {
            const int lateralCells = 10;
            const int rows = 2;
            float half = path.RoadWidth * 0.5f;

            Vector3 center = path.GetPointAtDistance(distance);
            Vector3 forward = path.GetDirectionAtDistance(distance);
            Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;

            List<Vector3> verts = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            List<int> tris = new List<int>();

            for (int r = 0; r <= rows; r++)
            {
                float f = (r / (float)rows - 0.5f) * length;
                for (int c = 0; c <= lateralCells; c++)
                {
                    float lat = (c / (float)lateralCells - 0.5f) * (half * 2f);
                    verts.Add(center + right * lat + forward * f + Vector3.up * (RoadLift + 0.055f));
                    uvs.Add(new Vector2(c / (float)lateralCells, r / (float)rows));
                }
            }

            int stride = lateralCells + 1;
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < lateralCells; c++)
                {
                    int a = r * stride + c;
                    int b = a + 1;
                    int d = (r + 1) * stride + c;
                    int e = d + 1;
                    tris.Add(a); tris.Add(d); tris.Add(b);
                    tris.Add(b); tris.Add(d); tris.Add(e);
                }
            }

            Mesh mesh = new Mesh();
            mesh.name = "CheckerStrip";
            mesh.SetVertices(verts);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }
    }
}
