using UnityEngine;

namespace DuaXe3D
{
    /// <summary>Tạo tay đua stylized từ các khối hình học cơ bản.</summary>
    public static class CharacterFactory
    {
        private static Material Suit(CharacterSpec s)
        {
            return MaterialLibrary.Paint("char_suit_" + s.Index, s.SuitColor);
        }

        private static Material Pants(CharacterSpec s)
        {
            return MaterialLibrary.Standard("char_pants_" + s.Index, s.SuitColor * 0.55f, 0.05f, 0.45f);
        }

        private static Material Accent(CharacterSpec s)
        {
            return MaterialLibrary.Paint("char_accent_" + s.Index, s.SuitAccent);
        }

        private static Material Helmet(CharacterSpec s)
        {
            return MaterialLibrary.Paint("char_helmet_" + s.Index, s.HelmetColor);
        }

        private static Material Visor(CharacterSpec s)
        {
            return MaterialLibrary.Standard("char_visor_" + s.Index, s.VisorColor, 0.7f, 0.98f,
                null, null, s.VisorColor, 1.1f);
        }

        private static Material Skin(CharacterSpec s)
        {
            return MaterialLibrary.Standard("char_skin_" + s.Index, s.SkinColor, 0f, 0.32f);
        }

        private static Material Dark()
        {
            return MaterialLibrary.Standard("char_dark", new Color(0.055f, 0.06f, 0.075f), 0.15f, 0.35f);
        }

        public static GameObject Build(CharacterSpec spec, Transform parent, Vector3 localPos, float scale = 1f, bool idle = true)
        {
            GameObject root = new GameObject("Racer_" + spec.Nickname);
            root.transform.SetParent(parent, false);
            root.transform.localPosition = localPos;
            root.transform.localScale = Vector3.one * scale;

            Transform t = root.transform;
            Material suit = Suit(spec);
            Material pants = Pants(spec);
            Material accent = Accent(spec);
            Material dark = Dark();

            // Chân & giày
            MeshKit.Capsule("LegL", t, new Vector3(-0.15f, 0.5f, 0f), new Vector3(0.2f, 0.43f, 0.21f), new Vector3(0f, 0f, 2f), pants);
            MeshKit.Capsule("LegR", t, new Vector3(0.15f, 0.5f, 0f), new Vector3(0.2f, 0.43f, 0.21f), new Vector3(0f, 0f, -2f), pants);
            MeshKit.Box("BootL", t, new Vector3(-0.15f, 0.08f, 0.055f), new Vector3(0.22f, 0.16f, 0.36f), Vector3.zero, dark);
            MeshKit.Box("BootR", t, new Vector3(0.15f, 0.08f, 0.055f), new Vector3(0.22f, 0.16f, 0.36f), Vector3.zero, dark);
            MeshKit.Box("SoleL", t, new Vector3(-0.15f, 0.015f, 0.06f), new Vector3(0.24f, 0.035f, 0.38f), Vector3.zero, accent);
            MeshKit.Box("SoleR", t, new Vector3(0.15f, 0.015f, 0.06f), new Vector3(0.24f, 0.035f, 0.38f), Vector3.zero, accent);

            // Hông, đai, thân
            MeshKit.Box("Pelvis", t, new Vector3(0f, 0.93f, 0f), new Vector3(0.5f, 0.24f, 0.32f), Vector3.zero, pants);
            MeshKit.Box("Belt", t, new Vector3(0f, 1.035f, 0f), new Vector3(0.53f, 0.075f, 0.34f), Vector3.zero, accent);
            MeshKit.Capsule("Torso", t, new Vector3(0f, 1.29f, 0f), new Vector3(0.5f, 0.33f, 0.33f), Vector3.zero, suit);
            MeshKit.Box("ChestStripe", t, new Vector3(0f, 1.40f, 0.145f), new Vector3(0.34f, 0.06f, 0.045f), Vector3.zero, accent);
            MeshKit.Box("SideStripeL", t, new Vector3(-0.245f, 1.24f, 0f), new Vector3(0.025f, 0.22f, 0.2f), Vector3.zero, accent);
            MeshKit.Box("SideStripeR", t, new Vector3(0.245f, 1.24f, 0f), new Vector3(0.025f, 0.22f, 0.2f), Vector3.zero, accent);

            // Vai, tay, găng
            MeshKit.Sphere("ShoulderL", t, new Vector3(-0.29f, 1.47f, 0f), new Vector3(0.25f, 0.2f, 0.27f), accent);
            MeshKit.Sphere("ShoulderR", t, new Vector3(0.29f, 1.47f, 0f), new Vector3(0.25f, 0.2f, 0.27f), accent);
            MeshKit.Capsule("ArmL", t, new Vector3(-0.355f, 1.28f, 0f), new Vector3(0.135f, 0.24f, 0.135f), new Vector3(0f, 0f, 13f), suit);
            MeshKit.Capsule("ArmR", t, new Vector3(0.355f, 1.28f, 0f), new Vector3(0.135f, 0.24f, 0.135f), new Vector3(0f, 0f, -13f), suit);
            MeshKit.Capsule("ForeL", t, new Vector3(-0.405f, 1.03f, 0.015f), new Vector3(0.12f, 0.22f, 0.12f), new Vector3(0f, 0f, 5f), suit);
            MeshKit.Capsule("ForeR", t, new Vector3(0.405f, 1.03f, 0.015f), new Vector3(0.12f, 0.22f, 0.12f), new Vector3(0f, 0f, -5f), suit);
            MeshKit.Sphere("GloveL", t, new Vector3(-0.425f, 0.86f, 0.03f), new Vector3(0.17f, 0.17f, 0.17f), dark);
            MeshKit.Sphere("GloveR", t, new Vector3(0.425f, 0.86f, 0.03f), new Vector3(0.17f, 0.17f, 0.17f), dark);

            // Đầu + mũ bảo hiểm
            BuildHead(spec, t, new Vector3(0f, 1.55f, 0f), 0.96f);

            if (idle)
            {
                CharacterIdle anim = root.AddComponent<CharacterIdle>();
                anim.Phase = spec.Index * 1.7f;
            }
            return root;
        }

        public static GameObject BuildHead(CharacterSpec spec, Transform parent, Vector3 localPos, float scale = 1f)
        {
            GameObject root = new GameObject("Head");
            root.transform.SetParent(parent, false);
            root.transform.localPosition = localPos;
            root.transform.localScale = Vector3.one * scale;
            Transform t = root.transform;

            Material skin = Skin(spec);
            Material helmetMat = Helmet(spec);
            Material visor = Visor(spec);
            Material accent = Accent(spec);
            Material dark = Dark();

            MeshKit.Cylinder("Neck", t, new Vector3(0f, 0.045f, 0f), new Vector3(0.17f, 0.07f, 0.17f), Vector3.zero, dark);
            MeshKit.Sphere("Face", t, new Vector3(0f, 0.17f, 0.015f), new Vector3(0.31f, 0.32f, 0.30f), skin);
            MeshKit.Sphere("Helmet", t, new Vector3(0f, 0.195f, -0.01f), new Vector3(0.40f, 0.38f, 0.41f), helmetMat);

            // Vành mũ + kính
            MeshKit.Box("Visor", t, new Vector3(0f, 0.19f, 0.185f), new Vector3(0.285f, 0.115f, 0.055f), new Vector3(-6f, 0f, 0f), visor);
            MeshKit.Box("Brim", t, new Vector3(0f, 0.29f, 0.155f), new Vector3(0.33f, 0.035f, 0.12f), new Vector3(0f, 0f, 0f), accent);
            MeshKit.Box("Chin", t, new Vector3(0f, 0.075f, 0.14f), new Vector3(0.20f, 0.09f, 0.10f), Vector3.zero, helmetMat);

            // Sọc mũ + tai nghe
            MeshKit.Box("HelmetStripe", t, new Vector3(0f, 0.365f, 0.0f), new Vector3(0.075f, 0.025f, 0.36f), Vector3.zero, accent);
            MeshKit.Cylinder("EarL", t, new Vector3(-0.195f, 0.17f, 0f), new Vector3(0.09f, 0.03f, 0.09f), new Vector3(0f, 0f, 90f), dark);
            MeshKit.Cylinder("EarR", t, new Vector3(0.195f, 0.17f, 0f), new Vector3(0.09f, 0.03f, 0.09f), new Vector3(0f, 0f, 90f), dark);

            return root;
        }
    }

    public class CharacterIdle : MonoBehaviour
    {
        public float Phase;
        public float Bob = 0.028f;
        public float Sway = 5f;

        private Vector3 _basePos;
        private Quaternion _baseRot;

        private void Start()
        {
            _basePos = transform.localPosition;
            _baseRot = transform.localRotation;
        }

        private void Update()
        {
            float t = Time.unscaledTime + Phase;
            transform.localPosition = _basePos + Vector3.up * Mathf.Sin(t * 1.5f) * Bob;
            transform.localRotation = _baseRot * Quaternion.Euler(0f, Mathf.Sin(t * 0.75f) * Sway, 0f);
        }
    }
}
