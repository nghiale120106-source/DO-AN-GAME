using UnityEngine;

namespace DuaXe3D
{
    public class CarVisual
    {
        public GameObject Root;
        public Transform[] WheelPivots = new Transform[4];
        public float[] WheelRadii = new float[4];
        public Transform DriverHead;

        public void ApplyWheels(float spinAngle, float steerAngle)
        {
            for (int i = 0; i < WheelPivots.Length; i++)
            {
                if (WheelPivots[i] == null) continue;
                float steer = i < 2 ? steerAngle : 0f;
                WheelPivots[i].localRotation = Quaternion.Euler(0f, steer, 0f) * Quaternion.Euler(spinAngle, 0f, 0f);
            }
        }
    }

    /// <summary>Tạo 3 mẫu xe: Jeep, Buggy, Monster Truck.</summary>
    public static class CarFactory
    {
        private static Material Paint(CarSpec spec)
        {
            return MaterialLibrary.Paint("car_paint_" + spec.Index, spec.BodyColor);
        }

        private static Material Accent(CarSpec spec)
        {
            return MaterialLibrary.Paint("car_accent_" + spec.Index, spec.AccentColor);
        }

        private static Material Seat(CarSpec spec)
        {
            return MaterialLibrary.Standard("car_seat_" + spec.Index, spec.SeatColor, 0.05f, 0.4f);
        }

        private static Material Rubber()
        {
            return MaterialLibrary.Rubber();
        }

        private static Material Dark()
        {
            return MaterialLibrary.DarkMetal();
        }

        private static Material Chrome()
        {
            return MaterialLibrary.Chrome();
        }

        private static Material Glass()
        {
            return MaterialLibrary.Glass();
        }

        private static Material Headlight()
        {
            return MaterialLibrary.Emissive("car_headlight", new Color(1f, 0.86f, 0.55f), 2.4f);
        }

        private static Material TailLight()
        {
            return MaterialLibrary.Emissive("car_taillight", new Color(1f, 0.08f, 0.06f), 2.2f);
        }

        public static CarVisual BuildVisual(CarSpec spec, CharacterSpec character, Transform parent, bool detailed = true)
        {
            GameObject root = new GameObject("Visual_" + spec.EnglishName);
            root.transform.SetParent(parent, false);
            Transform t = root.transform;

            CarVisual visual = new CarVisual();
            visual.Root = root;
            visual.WheelRadii[0] = spec.WheelRadius;
            visual.WheelRadii[1] = spec.WheelRadius;
            visual.WheelRadii[2] = spec.WheelRadius;
            visual.WheelRadii[3] = spec.WheelRadius;

            if (spec.Index == 0) BuildJeep(spec, character, t, visual, detailed);
            else if (spec.Index == 1) BuildBuggy(spec, character, t, visual, detailed);
            else BuildMonster(spec, character, t, visual, detailed);

            return visual;
        }

        private static void BuildJeep(CarSpec s, CharacterSpec ch, Transform t, CarVisual v, bool detailed)
        {
            Material paint = Paint(s);
            Material accent = Accent(s);
            Material dark = Dark();
            Material chrome = Chrome();
            Material glass = Glass();
            Material tire = Rubber();

            float bw = s.BodyWidth;
            float bl = s.BodyLength;
            float wr = s.WheelRadius;
            float wb = s.WheelBase;

            MeshKit.Box("Chassis", t, new Vector3(0f, wr + 0.20f, 0f), new Vector3(bw * 0.92f, 0.28f, bl * 0.88f), Vector3.zero, dark);
            MeshKit.Box("Body", t, new Vector3(0f, wr + 0.42f, 0f), new Vector3(bw, 0.38f, bl), Vector3.zero, paint);
            MeshKit.Box("Hood", t, new Vector3(0f, wr + 0.66f, bl * 0.27f), new Vector3(bw * 0.88f, 0.20f, bl * 0.34f), Vector3.zero, paint);
            MeshKit.Box("HoodScoop", t, new Vector3(0f, wr + 0.78f, bl * 0.25f), new Vector3(bw * 0.32f, 0.07f, bl * 0.16f), Vector3.zero, dark);
            MeshKit.Box("Cabin", t, new Vector3(0f, wr + 0.84f, -bl * 0.12f), new Vector3(bw * 0.82f, 0.54f, bl * 0.38f), Vector3.zero, paint);
            MeshKit.Box("Windshield", t, new Vector3(0f, wr + 1.02f, bl * 0.055f), new Vector3(bw * 0.74f, 0.42f, 0.055f), new Vector3(-13f, 0f, 0f), glass);
            MeshKit.Box("RearGlass", t, new Vector3(0f, wr + 1.02f, -bl * 0.305f), new Vector3(bw * 0.70f, 0.36f, 0.05f), new Vector3(10f, 0f, 0f), glass);
            MeshKit.Box("SideGlassL", t, new Vector3(-bw * 0.41f, wr + 1.04f, -bl * 0.12f), new Vector3(0.035f, 0.34f, bl * 0.28f), Vector3.zero, glass);
            MeshKit.Box("SideGlassR", t, new Vector3(bw * 0.41f, wr + 1.04f, -bl * 0.12f), new Vector3(0.035f, 0.34f, bl * 0.28f), Vector3.zero, glass);

            // Lưới tản nhiệt + đèn
            MeshKit.Box("Grille", t, new Vector3(0f, wr + 0.58f, bl * 0.5f + 0.01f), new Vector3(bw * 0.52f, 0.25f, 0.07f), Vector3.zero, dark);
            for (int i = -2; i <= 2; i++)
                MeshKit.Box("GrilleBar", t, new Vector3(i * 0.11f, wr + 0.58f, bl * 0.5f + 0.055f), new Vector3(0.028f, 0.2f, 0.03f), Vector3.zero, chrome);
            MeshKit.Sphere("HeadlightL", t, new Vector3(-bw * 0.34f, wr + 0.64f, bl * 0.5f + 0.02f), new Vector3(0.17f, 0.17f, 0.09f), Headlight());
            MeshKit.Sphere("HeadlightR", t, new Vector3(bw * 0.34f, wr + 0.64f, bl * 0.5f + 0.02f), new Vector3(0.17f, 0.17f, 0.09f), Headlight());
            MeshKit.Box("TailL", t, new Vector3(-bw * 0.37f, wr + 0.58f, -bl * 0.5f - 0.01f), new Vector3(0.17f, 0.12f, 0.05f), Vector3.zero, TailLight());
            MeshKit.Box("TailR", t, new Vector3(bw * 0.37f, wr + 0.58f, -bl * 0.5f - 0.01f), new Vector3(0.17f, 0.12f, 0.05f), Vector3.zero, TailLight());

            // Cản, bậc, fender
            MeshKit.Box("BumperF", t, new Vector3(0f, wr + 0.38f, bl * 0.5f + 0.06f), new Vector3(bw * 1.03f, 0.16f, 0.18f), Vector3.zero, dark);
            MeshKit.Box("BumperR", t, new Vector3(0f, wr + 0.38f, -bl * 0.5f - 0.06f), new Vector3(bw * 1.03f, 0.16f, 0.18f), Vector3.zero, dark);
            MeshKit.Box("StepL", t, new Vector3(-bw * 0.52f, wr + 0.2f, 0f), new Vector3(0.12f, 0.07f, bl * 0.32f), Vector3.zero, dark);
            MeshKit.Box("StepR", t, new Vector3(bw * 0.52f, wr + 0.2f, 0f), new Vector3(0.12f, 0.07f, bl * 0.32f), Vector3.zero, dark);
            MeshKit.Box("FenderFL", t, new Vector3(-bw * 0.5f, wr + 0.28f, wb * 0.5f), new Vector3(0.16f, 0.16f, wr * 2.4f), Vector3.zero, dark);
            MeshKit.Box("FenderFR", t, new Vector3(bw * 0.5f, wr + 0.28f, wb * 0.5f), new Vector3(0.16f, 0.16f, wr * 2.4f), Vector3.zero, dark);
            MeshKit.Box("FenderRL", t, new Vector3(-bw * 0.5f, wr + 0.28f, -wb * 0.5f), new Vector3(0.16f, 0.16f, wr * 2.4f), Vector3.zero, dark);
            MeshKit.Box("FenderRR", t, new Vector3(bw * 0.5f, wr + 0.28f, -wb * 0.5f), new Vector3(0.16f, 0.16f, wr * 2.4f), Vector3.zero, dark);

            // Sọc thể thao
            MeshKit.Box("StripeL", t, new Vector3(-bw * 0.42f, wr + 0.46f, 0.1f), new Vector3(0.025f, 0.16f, bl * 0.55f), Vector3.zero, accent);
            MeshKit.Box("StripeR", t, new Vector3(bw * 0.42f, wr + 0.46f, 0.1f), new Vector3(0.025f, 0.16f, bl * 0.55f), Vector3.zero, accent);

            // Lốp dự phòng
            MeshKit.Cylinder("SpareTire", t, new Vector3(0f, wr + 0.68f, -bl * 0.5f - 0.18f), new Vector3(wr * 1.9f, 0.12f, wr * 1.9f), new Vector3(90f, 0f, 0f), tire);
            MeshKit.Cylinder("SpareRim", t, new Vector3(0f, wr + 0.68f, -bl * 0.5f - 0.19f), new Vector3(wr * 1.1f, 0.125f, wr * 1.1f), new Vector3(90f, 0f, 0f), chrome);

            // Khung bảo vệ (roll cage) mở
            float topY = wr + 1.62f;
            Vector3 fl = new Vector3(-bw * 0.39f, wr + 0.62f, bl * 0.12f);
            Vector3 fr = new Vector3(bw * 0.39f, wr + 0.62f, bl * 0.12f);
            Vector3 rl = new Vector3(-bw * 0.39f, wr + 0.62f, -bl * 0.32f);
            Vector3 rr = new Vector3(bw * 0.39f, wr + 0.62f, -bl * 0.32f);
            Vector3 tfl = new Vector3(-bw * 0.36f, topY, bl * 0.02f);
            Vector3 tfr = new Vector3(bw * 0.36f, topY, bl * 0.02f);
            Vector3 trl = new Vector3(-bw * 0.36f, topY, -bl * 0.30f);
            Vector3 trr = new Vector3(bw * 0.36f, topY, -bl * 0.30f);
            MeshKit.Beam("CageA_L", t, fl, tfl, 0.035f, dark);
            MeshKit.Beam("CageA_R", t, fr, tfr, 0.035f, dark);
            MeshKit.Beam("CageB_L", t, rl, trl, 0.035f, dark);
            MeshKit.Beam("CageB_R", t, rr, trr, 0.035f, dark);
            MeshKit.Beam("CageTopF", t, tfl, tfr, 0.035f, dark);
            MeshKit.Beam("CageTopB", t, trl, trr, 0.035f, dark);
            MeshKit.Beam("CageTopL", t, tfl, trl, 0.035f, dark);
            MeshKit.Beam("CageTopR", t, tfr, trr, 0.035f, dark);
            MeshKit.Beam("CageCross", t, tfl, trr, 0.03f, accent);

            // Gương + ống xả
            MeshKit.Box("MirrorL", t, new Vector3(-bw * 0.55f, wr + 0.98f, bl * 0.08f), new Vector3(0.15f, 0.1f, 0.06f), Vector3.zero, dark);
            MeshKit.Box("MirrorR", t, new Vector3(bw * 0.55f, wr + 0.98f, bl * 0.08f), new Vector3(0.15f, 0.1f, 0.06f), Vector3.zero, dark);
            MeshKit.Cylinder("Exhaust", t, new Vector3(-bw * 0.28f, wr + 0.18f, -bl * 0.5f), new Vector3(0.09f, 0.16f, 0.09f), new Vector3(90f, 0f, 0f), chrome);

            // Người lái
            v.DriverHead = CharacterFactory.BuildHead(ch, t, new Vector3(0f, wr + 1.05f, -bl * 0.11f), 0.78f).transform;

            MakeWheels(s, t, v, tire, chrome, accent, detailed);
        }

        private static void BuildBuggy(CarSpec s, CharacterSpec ch, Transform t, CarVisual v, bool detailed)
        {
            Material paint = Paint(s);
            Material accent = Accent(s);
            Material dark = Dark();
            Material chrome = Chrome();
            Material seat = Seat(s);
            Material tire = Rubber();

            float bw = s.BodyWidth;
            float bl = s.BodyLength;
            float wr = s.WheelRadius;
            float wb = s.WheelBase;

            MeshKit.Box("Chassis", t, new Vector3(0f, wr + 0.12f, 0f), new Vector3(bw * 0.78f, 0.2f, bl * 0.78f), Vector3.zero, dark);
            MeshKit.Box("Floor", t, new Vector3(0f, wr + 0.23f, 0f), new Vector3(bw * 0.9f, 0.06f, bl * 0.86f), Vector3.zero, dark);
            MeshKit.Box("Hood", t, new Vector3(0f, wr + 0.34f, bl * 0.3f), new Vector3(bw * 0.68f, 0.18f, bl * 0.28f), new Vector3(-5f, 0f, 0f), paint);
            MeshKit.Box("Nose", t, new Vector3(0f, wr + 0.30f, bl * 0.46f), new Vector3(bw * 0.55f, 0.22f, 0.16f), Vector3.zero, accent);

            // Động cơ phía sau
            MeshKit.Box("Engine", t, new Vector3(0f, wr + 0.42f, -bl * 0.3f), new Vector3(0.8f, 0.38f, 0.6f), Vector3.zero, dark);
            MeshKit.Cylinder("Intake", t, new Vector3(0f, wr + 0.64f, -bl * 0.28f), new Vector3(0.32f, 0.09f, 0.32f), Vector3.zero, chrome);
            MeshKit.Cylinder("Exhaust1", t, new Vector3(-0.28f, wr + 0.5f, -bl * 0.52f), new Vector3(0.09f, 0.22f, 0.09f), new Vector3(90f, 0f, 0f), chrome);
            MeshKit.Cylinder("Exhaust2", t, new Vector3(0.28f, wr + 0.5f, -bl * 0.52f), new Vector3(0.09f, 0.22f, 0.09f), new Vector3(90f, 0f, 0f), chrome);

            // Ghế
            MeshKit.Box("SeatL", t, new Vector3(-0.3f, wr + 0.38f, -0.02f), new Vector3(0.4f, 0.1f, 0.44f), Vector3.zero, seat);
            MeshKit.Box("SeatBackL", t, new Vector3(-0.3f, wr + 0.62f, -0.22f), new Vector3(0.38f, 0.44f, 0.1f), new Vector3(-10f, 0f, 0f), seat);
            MeshKit.Box("SeatR", t, new Vector3(0.3f, wr + 0.38f, -0.02f), new Vector3(0.4f, 0.1f, 0.44f), Vector3.zero, seat);
            MeshKit.Box("SeatBackR", t, new Vector3(0.3f, wr + 0.62f, -0.22f), new Vector3(0.38f, 0.44f, 0.1f), new Vector3(-10f, 0f, 0f), seat);
            MeshKit.Cylinder("WheelSteer", t, new Vector3(-0.3f, wr + 0.62f, 0.2f), new Vector3(0.26f, 0.025f, 0.26f), new Vector3(62f, 0f, 0f), dark);

            // Khung bảo vệ
            float top = wr + 1.28f;
            Vector3[] cage =
            {
                new Vector3(-0.62f, wr + 0.3f, 0.55f), new Vector3(0.62f, wr + 0.3f, 0.55f),
                new Vector3(-0.58f, wr + 0.3f, -0.45f), new Vector3(0.58f, wr + 0.3f, -0.45f)
            };
            Vector3[] cageTop =
            {
                new Vector3(-0.52f, top, 0.18f), new Vector3(0.52f, top, 0.18f),
                new Vector3(-0.5f, top, -0.42f), new Vector3(0.5f, top, -0.42f)
            };
            MeshKit.Beam("CageFL", t, cage[0], cageTop[0], 0.045f, accent);
            MeshKit.Beam("CageFR", t, cage[1], cageTop[1], 0.045f, accent);
            MeshKit.Beam("CageRL", t, cage[2], cageTop[2], 0.045f, accent);
            MeshKit.Beam("CageRR", t, cage[3], cageTop[3], 0.045f, accent);
            MeshKit.Beam("CageTopF", t, cageTop[0], cageTop[1], 0.04f, accent);
            MeshKit.Beam("CageTopB", t, cageTop[2], cageTop[3], 0.04f, accent);
            MeshKit.Beam("CageTopL", t, cageTop[0], cageTop[2], 0.04f, accent);
            MeshKit.Beam("CageTopR", t, cageTop[1], cageTop[3], 0.04f, accent);
            MeshKit.Beam("CageX", t, cageTop[0], cageTop[3], 0.035f, dark);

            // Cánh gió sau
            MeshKit.Box("Spoiler", t, new Vector3(0f, wr + 1.05f, -bl * 0.47f), new Vector3(bw * 0.92f, 0.06f, 0.34f), new Vector3(-8f, 0f, 0f), accent);
            MeshKit.Box("SpoilerL", t, new Vector3(-0.55f, wr + 0.9f, -bl * 0.45f), new Vector3(0.05f, 0.26f, 0.12f), Vector3.zero, dark);
            MeshKit.Box("SpoilerR", t, new Vector3(0.55f, wr + 0.9f, -bl * 0.45f), new Vector3(0.05f, 0.26f, 0.12f), Vector3.zero, dark);

            // Đèn
            MeshKit.Box("LightBar", t, new Vector3(0f, wr + 0.52f, bl * 0.47f), new Vector3(bw * 0.7f, 0.12f, 0.1f), Vector3.zero, dark);
            for (int i = -2; i <= 2; i++)
                MeshKit.Sphere("Light", t, new Vector3(i * 0.18f, wr + 0.52f, bl * 0.47f + 0.06f), new Vector3(0.11f, 0.11f, 0.06f), Headlight());

            // Fender nhỏ
            MeshKit.Box("FenderFL", t, new Vector3(-bw * 0.52f, wr + 0.2f, wb * 0.5f), new Vector3(0.12f, 0.1f, wr * 1.9f), Vector3.zero, dark);
            MeshKit.Box("FenderFR", t, new Vector3(bw * 0.52f, wr + 0.2f, wb * 0.5f), new Vector3(0.12f, 0.1f, wr * 1.9f), Vector3.zero, dark);
            MeshKit.Box("FenderRL", t, new Vector3(-bw * 0.52f, wr + 0.2f, -wb * 0.5f), new Vector3(0.12f, 0.1f, wr * 1.9f), Vector3.zero, dark);
            MeshKit.Box("FenderRR", t, new Vector3(bw * 0.52f, wr + 0.2f, -wb * 0.5f), new Vector3(0.12f, 0.1f, wr * 1.9f), Vector3.zero, dark);

            v.DriverHead = CharacterFactory.BuildHead(ch, t, new Vector3(-0.3f, wr + 0.66f, -0.04f), 0.8f).transform;

            MakeWheels(s, t, v, tire, chrome, accent, detailed, 1f, 1.12f);
        }

        private static void BuildMonster(CarSpec s, CharacterSpec ch, Transform t, CarVisual v, bool detailed)
        {
            Material paint = Paint(s);
            Material accent = Accent(s);
            Material dark = Dark();
            Material chrome = Chrome();
            Material glass = Glass();
            Material tire = Rubber();

            float bw = s.BodyWidth;
            float bl = s.BodyLength;
            float wr = s.WheelRadius;
            float wb = s.WheelBase;

            // Khung gầm cao
            MeshKit.Box("Chassis", t, new Vector3(0f, wr + 0.22f, 0f), new Vector3(bw * 0.8f, 0.26f, bl * 0.78f), Vector3.zero, dark);
            MeshKit.Box("Body", t, new Vector3(0f, wr + 0.56f, 0f), new Vector3(bw * 0.92f, 0.5f, bl * 0.8f), Vector3.zero, paint);
            MeshKit.Box("Hood", t, new Vector3(0f, wr + 0.88f, bl * 0.27f), new Vector3(bw * 0.8f, 0.28f, bl * 0.3f), Vector3.zero, paint);
            MeshKit.Box("Cabin", t, new Vector3(0f, wr + 1.02f, -bl * 0.1f), new Vector3(bw * 0.72f, 0.5f, bl * 0.34f), Vector3.zero, paint);
            MeshKit.Box("Windshield", t, new Vector3(0f, wr + 1.16f, bl * 0.075f), new Vector3(bw * 0.62f, 0.36f, 0.05f), new Vector3(-14f, 0f, 0f), glass);
            MeshKit.Box("SideGlassL", t, new Vector3(-bw * 0.36f, wr + 1.18f, -bl * 0.1f), new Vector3(0.03f, 0.3f, bl * 0.24f), Vector3.zero, glass);
            MeshKit.Box("SideGlassR", t, new Vector3(bw * 0.36f, wr + 1.18f, -bl * 0.1f), new Vector3(0.03f, 0.3f, bl * 0.24f), Vector3.zero, glass);

            // Cản hầm hố
            MeshKit.Box("BumperF", t, new Vector3(0f, wr + 0.35f, bl * 0.5f + 0.08f), new Vector3(bw * 1.06f, 0.3f, 0.22f), Vector3.zero, dark);
            MeshKit.Box("BumperR", t, new Vector3(0f, wr + 0.35f, -bl * 0.5f - 0.08f), new Vector3(bw * 1.06f, 0.3f, 0.22f), Vector3.zero, dark);
            MeshKit.Box("SkidF", t, new Vector3(0f, wr + 0.12f, bl * 0.5f), new Vector3(bw * 0.8f, 0.12f, 0.3f), Vector3.zero, chrome);

            // Lưới + đèn
            MeshKit.Box("Grille", t, new Vector3(0f, wr + 0.68f, bl * 0.4f + 0.02f), new Vector3(bw * 0.56f, 0.3f, 0.08f), Vector3.zero, dark);
            for (int i = -2; i <= 2; i++)
                MeshKit.Box("GrilleBar", t, new Vector3(i * 0.12f, wr + 0.68f, bl * 0.4f + 0.07f), new Vector3(0.03f, 0.24f, 0.03f), Vector3.zero, chrome);
            MeshKit.Sphere("HeadlightL", t, new Vector3(-bw * 0.33f, wr + 0.92f, bl * 0.4f + 0.02f), new Vector3(0.18f, 0.18f, 0.09f), Headlight());
            MeshKit.Sphere("HeadlightR", t, new Vector3(bw * 0.33f, wr + 0.92f, bl * 0.4f + 0.02f), new Vector3(0.18f, 0.18f, 0.09f), Headlight());
            MeshKit.Box("TailL", t, new Vector3(-bw * 0.34f, wr + 0.66f, -bl * 0.4f - 0.02f), new Vector3(0.18f, 0.12f, 0.05f), Vector3.zero, TailLight());
            MeshKit.Box("TailR", t, new Vector3(bw * 0.34f, wr + 0.66f, -bl * 0.4f - 0.02f), new Vector3(0.18f, 0.12f, 0.05f), Vector3.zero, TailLight());

            // Sọc + bậc
            MeshKit.Box("StripeL", t, new Vector3(-bw * 0.44f, wr + 0.6f, 0f), new Vector3(0.025f, 0.2f, bl * 0.6f), Vector3.zero, accent);
            MeshKit.Box("StripeR", t, new Vector3(bw * 0.44f, wr + 0.6f, 0f), new Vector3(0.025f, 0.2f, bl * 0.6f), Vector3.zero, accent);
            MeshKit.Box("StepL", t, new Vector3(-bw * 0.53f, wr + 0.24f, 0f), new Vector3(0.14f, 0.08f, bl * 0.34f), Vector3.zero, chrome);
            MeshKit.Box("StepR", t, new Vector3(bw * 0.53f, wr + 0.24f, 0f), new Vector3(0.14f, 0.08f, bl * 0.34f), Vector3.zero, chrome);

            // Fender lớn
            MeshKit.Box("FenderFL", t, new Vector3(-bw * 0.53f, wr + 0.38f, wb * 0.5f), new Vector3(0.2f, 0.2f, wr * 2.2f), Vector3.zero, dark);
            MeshKit.Box("FenderFR", t, new Vector3(bw * 0.53f, wr + 0.38f, wb * 0.5f), new Vector3(0.2f, 0.2f, wr * 2.2f), Vector3.zero, dark);
            MeshKit.Box("FenderRL", t, new Vector3(-bw * 0.53f, wr + 0.38f, -wb * 0.5f), new Vector3(0.2f, 0.2f, wr * 2.2f), Vector3.zero, dark);
            MeshKit.Box("FenderRR", t, new Vector3(bw * 0.53f, wr + 0.38f, -wb * 0.5f), new Vector3(0.2f, 0.2f, wr * 2.2f), Vector3.zero, dark);

            // Ống xả đứng
            MeshKit.Cylinder("StackL", t, new Vector3(-bw * 0.32f, wr + 1.05f, -bl * 0.28f), new Vector3(0.1f, 0.42f, 0.1f), Vector3.zero, chrome);
            MeshKit.Cylinder("StackR", t, new Vector3(bw * 0.32f, wr + 1.05f, -bl * 0.28f), new Vector3(0.1f, 0.42f, 0.1f), Vector3.zero, chrome);
            MeshKit.Cylinder("StackTipL", t, new Vector3(-bw * 0.32f, wr + 1.49f, -bl * 0.28f), new Vector3(0.13f, 0.06f, 0.13f), Vector3.zero, dark);
            MeshKit.Cylinder("StackTipR", t, new Vector3(bw * 0.32f, wr + 1.49f, -bl * 0.28f), new Vector3(0.13f, 0.06f, 0.13f), Vector3.zero, dark);

            // Khung bảo vệ
            float top = wr + 1.45f;
            Vector3[] cageTop =
            {
                new Vector3(-bw * 0.32f, top, bl * 0.02f), new Vector3(bw * 0.32f, top, bl * 0.02f),
                new Vector3(-bw * 0.32f, top, -bl * 0.26f), new Vector3(bw * 0.32f, top, -bl * 0.26f)
            };
            MeshKit.Beam("CageFL", t, new Vector3(-bw * 0.34f, wr + 0.9f, bl * 0.02f), cageTop[0], 0.04f, dark);
            MeshKit.Beam("CageFR", t, new Vector3(bw * 0.34f, wr + 0.9f, bl * 0.02f), cageTop[1], 0.04f, dark);
            MeshKit.Beam("CageRL", t, new Vector3(-bw * 0.34f, wr + 0.9f, -bl * 0.26f), cageTop[2], 0.04f, dark);
            MeshKit.Beam("CageRR", t, new Vector3(bw * 0.34f, wr + 0.9f, -bl * 0.26f), cageTop[3], 0.04f, dark);
            MeshKit.Beam("CageTopF", t, cageTop[0], cageTop[1], 0.04f, accent);
            MeshKit.Beam("CageTopB", t, cageTop[2], cageTop[3], 0.04f, accent);
            MeshKit.Beam("CageTopL", t, cageTop[0], cageTop[2], 0.04f, accent);
            MeshKit.Beam("CageTopR", t, cageTop[1], cageTop[3], 0.04f, accent);

            v.DriverHead = CharacterFactory.BuildHead(ch, t, new Vector3(0f, wr + 1.05f, -bl * 0.09f), 0.8f).transform;

            MakeWheels(s, t, v, tire, chrome, accent, detailed);
        }

        private static void MakeWheels(CarSpec s, Transform t, CarVisual v, Material tire, Material rim, Material accent, bool detailed,
            float frontScale = 1f, float rearScale = 1f)
        {
            float halfTrack = s.TrackWidth * 0.5f;
            float halfBase = s.WheelBase * 0.5f;
            float fw = s.WheelRadius * frontScale;
            float rw = s.WheelRadius * rearScale;

            v.WheelPivots[0] = MakeWheel(t, new Vector3(-halfTrack, fw, halfBase), fw, s.WheelWidth, tire, rim, accent, detailed);
            v.WheelPivots[1] = MakeWheel(t, new Vector3(halfTrack, fw, halfBase), fw, s.WheelWidth, tire, rim, accent, detailed);
            v.WheelPivots[2] = MakeWheel(t, new Vector3(-halfTrack, rw, -halfBase), rw, s.WheelWidth * 1.05f, tire, rim, accent, detailed);
            v.WheelPivots[3] = MakeWheel(t, new Vector3(halfTrack, rw, -halfBase), rw, s.WheelWidth * 1.05f, tire, rim, accent, detailed);

            v.WheelRadii[0] = fw;
            v.WheelRadii[1] = fw;
            v.WheelRadii[2] = rw;
            v.WheelRadii[3] = rw;
        }

        private static Transform MakeWheel(Transform parent, Vector3 localPos, float radius, float width,
            Material tireMat, Material rimMat, Material accentMat, bool detailed)
        {
            GameObject pivotGo = new GameObject("WheelPivot");
            pivotGo.transform.SetParent(parent, false);
            pivotGo.transform.localPosition = localPos;
            Transform pivot = pivotGo.transform;

            MeshKit.Cylinder("Tire", pivot, Vector3.zero, new Vector3(radius * 2f, width * 0.5f, radius * 2f), new Vector3(0f, 0f, 90f), tireMat);
            MeshKit.Cylinder("Rim", pivot, Vector3.zero, new Vector3(radius * 1.24f, width * 0.52f, radius * 1.24f), new Vector3(0f, 0f, 90f), rimMat);
            MeshKit.Cylinder("Hub", pivot, Vector3.zero, new Vector3(radius * 0.55f, width * 0.56f, radius * 0.55f), new Vector3(0f, 0f, 90f), accentMat);

            if (detailed)
            {
                for (int i = 0; i < 5; i++)
                {
                    float a = i * 72f;
                    MeshKit.Box("Spoke", pivot, Vector3.zero, new Vector3(radius * 0.06f, radius * 1.1f, width * 0.5f),
                        new Vector3(0f, 0f, a), rimMat);
                }

                int treads = 9;
                for (int i = 0; i < treads; i++)
                {
                    float a = i * 360f / treads;
                    float rad = a * Mathf.Deg2Rad;
                    MeshKit.Box("Tread", pivot,
                        new Vector3(0f, Mathf.Cos(rad) * radius * 0.94f, Mathf.Sin(rad) * radius * 0.94f),
                        new Vector3(width * 0.98f, 0.055f, radius * 0.22f),
                        new Vector3(a, 0f, 0f), tireMat);
                }
            }

            return pivot;
        }

        public static RacerInfo SpawnRaceCar(CarSpec spec, CharacterSpec character, Vector3 position, Quaternion rotation,
            bool isPlayer, string displayName, RoadPath path)
        {
            GameObject root = new GameObject("Car_" + spec.EnglishName + (isPlayer ? "_Player" : "_AI"));
            root.transform.SetPositionAndRotation(position, rotation);

            CarVisual visual = BuildVisual(spec, character, root.transform, true);

            BoxCollider box = root.AddComponent<BoxCollider>();
            float colHeight = spec.WheelRadius + spec.BodyHeight * 0.72f;
            box.size = new Vector3(spec.BodyWidth * 0.96f, colHeight, spec.BodyLength * 0.96f);
            box.center = new Vector3(0f, colHeight * 0.5f + 0.02f, 0f);

            PhysicsMaterial slick = new PhysicsMaterial("CarSlick");
            slick.dynamicFriction = 0f;
            slick.staticFriction = 0f;
            slick.bounciness = 0f;
            slick.frictionCombine = PhysicsMaterialCombine.Minimum;
            slick.bounceCombine = PhysicsMaterialCombine.Minimum;
            box.material = slick;

            Rigidbody rb = root.AddComponent<Rigidbody>();
            rb.mass = spec.Mass;
            rb.linearDamping = 0.08f;
            rb.angularDamping = 3.5f;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            rb.centerOfMass = new Vector3(0f, spec.WheelRadius * 0.55f, 0f);
            rb.useGravity = true;

            ArcadeCarController controller = root.AddComponent<ArcadeCarController>();
            controller.Configure(spec, visual, isPlayer, path);

            RacerInfo info = root.AddComponent<RacerInfo>();
            info.CarIndex = spec.Index;
            info.CharacterIndex = character.Index;
            info.DisplayName = displayName;
            info.IsPlayer = isPlayer;
            info.Controller = controller;
            info.Visual = visual;
            info.Path = path;

            if (isPlayer)
            {
                root.AddComponent<EngineAudio>();
            }

            int carLayer = SceneSetup.CarLayer;
            SceneSetup.SetLayerRecursive(root, carLayer);
            return info;
        }
    }
}
