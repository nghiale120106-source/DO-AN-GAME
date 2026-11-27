using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace DuaXe3D
{
    public static class MeshKit
    {
        public static GameObject Primitive(PrimitiveType type, string name, Transform parent,
            Vector3 localPos, Vector3 localScale, Vector3 localEuler, Material material, bool keepCollider = false)
        {
            GameObject go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localRotation = Quaternion.Euler(localEuler);
            go.transform.localScale = localScale;

            Renderer r = go.GetComponent<Renderer>();
            if (r != null)
            {
                r.sharedMaterial = material;
                r.shadowCastingMode = ShadowCastingMode.On;
                r.receiveShadows = true;
            }

            if (!keepCollider)
            {
                Collider c = go.GetComponent<Collider>();
                if (c != null)
                {
                    if (Application.isPlaying) Object.Destroy(c);
                    else Object.DestroyImmediate(c);
                }
            }
            return go;
        }

        public static GameObject Box(string name, Transform parent, Vector3 pos, Vector3 scale, Vector3 euler, Material mat, bool keepCollider = false)
        {
            return Primitive(PrimitiveType.Cube, name, parent, pos, scale, euler, mat, keepCollider);
        }

        public static GameObject Sphere(string name, Transform parent, Vector3 pos, Vector3 scale, Material mat, bool keepCollider = false)
        {
            return Primitive(PrimitiveType.Sphere, name, parent, pos, scale, Vector3.zero, mat, keepCollider);
        }

        public static GameObject Cylinder(string name, Transform parent, Vector3 pos, Vector3 scale, Vector3 euler, Material mat, bool keepCollider = false)
        {
            return Primitive(PrimitiveType.Cylinder, name, parent, pos, scale, euler, mat, keepCollider);
        }

        public static GameObject Capsule(string name, Transform parent, Vector3 pos, Vector3 scale, Vector3 euler, Material mat, bool keepCollider = false)
        {
            return Primitive(PrimitiveType.Capsule, name, parent, pos, scale, euler, mat, keepCollider);
        }

        public static GameObject Quad(string name, Transform parent, Vector3 pos, Vector3 scale, Vector3 euler, Material mat)
        {
            return Primitive(PrimitiveType.Quad, name, parent, pos, scale, euler, mat);
        }

        public static GameObject Beam(string name, Transform parent, Vector3 a, Vector3 b, float radius, Material mat)
        {
            Vector3 mid = (a + b) * 0.5f;
            Vector3 dir = b - a;
            float len = dir.magnitude;
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = mid;
            go.transform.localScale = new Vector3(radius * 2f, len * 0.5f, radius * 2f);
            if (len > 0.0001f)
                go.transform.localRotation = Quaternion.FromToRotation(Vector3.up, dir.normalized);

            Renderer r = go.GetComponent<Renderer>();
            if (r != null)
            {
                r.sharedMaterial = mat;
                r.shadowCastingMode = ShadowCastingMode.On;
                r.receiveShadows = true;
            }
            Collider c = go.GetComponent<Collider>();
            if (c != null)
            {
                if (Application.isPlaying) Object.Destroy(c);
                else Object.DestroyImmediate(c);
            }
            return go;
        }

        public static GameObject MeshObject(string name, Mesh mesh, Material mat, Transform parent,
            Vector3 localPos, Quaternion localRot, Vector3 localScale)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localRotation = localRot;
            go.transform.localScale = localScale;

            MeshFilter filter = go.AddComponent<MeshFilter>();
            filter.sharedMesh = mesh;
            MeshRenderer renderer = go.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = mat;
            renderer.shadowCastingMode = ShadowCastingMode.On;
            renderer.receiveShadows = true;
            return go;
        }

        public static Mesh Ring(float innerRadius, float outerRadius, int segments = 64)
        {
            Mesh mesh = new Mesh();
            mesh.name = "RingMesh";

            Vector3[] verts = new Vector3[(segments + 1) * 2];
            Vector2[] uvs = new Vector2[verts.Length];
            int[] tris = new int[segments * 6];

            for (int i = 0; i <= segments; i++)
            {
                float a = i / (float)segments * Mathf.PI * 2f;
                float cos = Mathf.Cos(a);
                float sin = Mathf.Sin(a);
                verts[i * 2] = new Vector3(cos * innerRadius, 0f, sin * innerRadius);
                verts[i * 2 + 1] = new Vector3(cos * outerRadius, 0f, sin * outerRadius);
                uvs[i * 2] = new Vector2(i / (float)segments, 0f);
                uvs[i * 2 + 1] = new Vector2(i / (float)segments, 1f);
            }

            for (int i = 0; i < segments; i++)
            {
                int b = i * 2;
                tris[i * 6 + 0] = b;
                tris[i * 6 + 1] = b + 3;
                tris[i * 6 + 2] = b + 1;
                tris[i * 6 + 3] = b;
                tris[i * 6 + 4] = b + 2;
                tris[i * 6 + 5] = b + 3;
            }

            mesh.vertices = verts;
            mesh.uv = uvs;
            mesh.triangles = tris;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        public static Mesh Mountain(int seed, float radius, float height, int sides = 11, int rings = 5)
        {
            System.Random rnd = new System.Random(seed);
            List<Vector3> verts = new List<Vector3>();
            List<int> tris = new List<int>();

            float angleOffset = (float)rnd.NextDouble() * 360f;
            float[] ringRadii = new float[rings];
            for (int r = 0; r < rings; r++)
            {
                float t = r / (float)(rings - 1);
                ringRadii[r] = Mathf.Lerp(radius * 0.08f, radius, Mathf.Pow(t, 0.72f)) * (0.9f + (float)rnd.NextDouble() * 0.2f);
            }

            int apex = 0;
            verts.Add(new Vector3((float)rnd.NextDouble() * 0.2f - 0.1f, height, 0f));

            for (int r = 0; r < rings; r++)
            {
                float t = r / (float)(rings - 1);
                float y = height * (1f - t);
                for (int s = 0; s < sides; s++)
                {
                    float a = angleOffset * Mathf.Deg2Rad + s / (float)sides * Mathf.PI * 2f;
                    float rad = ringRadii[r] * (0.82f + (float)rnd.NextDouble() * 0.36f);
                    verts.Add(new Vector3(Mathf.Cos(a) * rad, y, Mathf.Sin(a) * rad));
                }
            }

            int firstRing = 1;
            for (int s = 0; s < sides; s++)
            {
                int s2 = (s + 1) % sides;
                tris.Add(apex);
                tris.Add(firstRing + s2);
                tris.Add(firstRing + s);
            }

            for (int r = 0; r < rings - 1; r++)
            {
                int r0 = firstRing + r * sides;
                int r1 = firstRing + (r + 1) * sides;
                for (int s = 0; s < sides; s++)
                {
                    int s2 = (s + 1) % sides;
                    tris.Add(r0 + s);
                    tris.Add(r1 + s2);
                    tris.Add(r1 + s);
                    tris.Add(r0 + s);
                    tris.Add(r0 + s2);
                    tris.Add(r1 + s2);
                }
            }

            Mesh mesh = new Mesh();
            mesh.name = "Mountain";
            mesh.SetVertices(verts);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        public static Mesh Cone(float radius, float height, int sides = 16)
        {
            Vector3[] verts = new Vector3[sides + 1];
            Vector2[] uvs = new Vector2[sides + 1];
            int[] tris = new int[sides * 3];

            verts[0] = new Vector3(0f, height, 0f);
            uvs[0] = new Vector2(0.5f, 1f);
            for (int i = 0; i < sides; i++)
            {
                float a = i / (float)sides * Mathf.PI * 2f;
                verts[i + 1] = new Vector3(Mathf.Cos(a) * radius, 0f, Mathf.Sin(a) * radius);
                uvs[i + 1] = new Vector2(i / (float)sides, 0f);
                tris[i * 3] = 0;
                tris[i * 3 + 1] = (i + 1) % sides + 1;
                tris[i * 3 + 2] = i + 1;
            }

            Mesh mesh = new Mesh();
            mesh.name = "Cone";
            mesh.vertices = verts;
            mesh.uv = uvs;
            mesh.triangles = tris;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        public static void SetShadows(GameObject root, bool cast, bool receive)
        {
            foreach (Renderer r in root.GetComponentsInChildren<Renderer>(true))
            {
                r.shadowCastingMode = cast ? ShadowCastingMode.On : ShadowCastingMode.Off;
                r.receiveShadows = receive;
            }
        }
    }
}
