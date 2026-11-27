using UnityEngine;

namespace DuaXe3D
{
    public class CarSpec
    {
        public int Index;
        public string Name;
        public string EnglishName;
        public string Tagline;
        public string EngineNote;

        public Color BodyColor;
        public Color AccentColor;
        public Color SeatColor;

        public float MaxSpeedKmh;
        public float AccelerationStat;
        public float HandlingStat;

        public float WheelRadius;
        public float WheelWidth;
        public float TrackWidth;
        public float WheelBase;
        public float BodyLength;
        public float BodyWidth;
        public float BodyHeight;

        public float Mass;
        public float AccelForce;
        public float SteerAngle;
        public float Grip;
    }

    public static class CarLibrary
    {
        public static readonly CarSpec[] All =
        {
            new CarSpec
            {
                Index = 0,
                Name = "JEEP ĐỊA HÌNH",
                EnglishName = "JEEP OFFROAD",
                Tagline = "Bền bỉ · Cân bằng · Đa địa hình",
                EngineNote = "Động cơ 4.0L · 6 máy",
                BodyColor = UITheme.Hex("6E7C45"),
                AccentColor = UITheme.Hex("FFC53D"),
                SeatColor = UITheme.Hex("3B2A1E"),
                MaxSpeedKmh = 148f,
                AccelerationStat = 66f,
                HandlingStat = 73f,
                WheelRadius = 0.55f,
                WheelWidth = 0.42f,
                TrackWidth = 1.88f,
                WheelBase = 2.62f,
                BodyLength = 4.05f,
                BodyWidth = 1.98f,
                BodyHeight = 1.55f,
                Mass = 1350f,
                AccelForce = 12.5f,
                SteerAngle = 38f,
                Grip = 11.5f
            },
            new CarSpec
            {
                Index = 1,
                Name = "BUGGY BÃO TỐ",
                EnglishName = "STORM BUGGY",
                Tagline = "Nhẹ · Nhanh · Bám đường",
                EngineNote = "Động cơ 2.0L Turbo",
                BodyColor = UITheme.Hex("FF6A1A"),
                AccentColor = UITheme.Hex("22D3EE"),
                SeatColor = UITheme.Hex("1D2430"),
                MaxSpeedKmh = 162f,
                AccelerationStat = 83f,
                HandlingStat = 86f,
                WheelRadius = 0.62f,
                WheelWidth = 0.44f,
                TrackWidth = 1.94f,
                WheelBase = 2.55f,
                BodyLength = 3.85f,
                BodyWidth = 1.92f,
                BodyHeight = 1.22f,
                Mass = 980f,
                AccelForce = 14.5f,
                SteerAngle = 44f,
                Grip = 13.5f
            },
            new CarSpec
            {
                Index = 2,
                Name = "MONSTER TRUCK",
                EnglishName = "TITAN MONSTER",
                Tagline = "Uy lực · Gầm cao · Vượt địa hình",
                EngineNote = "Động cơ V8 siêu nạp",
                BodyColor = UITheme.Hex("2563EB"),
                AccentColor = UITheme.Hex("38BDF8"),
                SeatColor = UITheme.Hex("151A24"),
                MaxSpeedKmh = 139f,
                AccelerationStat = 71f,
                HandlingStat = 58f,
                WheelRadius = 0.93f,
                WheelWidth = 0.62f,
                TrackWidth = 2.32f,
                WheelBase = 3.02f,
                BodyLength = 4.35f,
                BodyWidth = 2.32f,
                BodyHeight = 1.92f,
                Mass = 1780f,
                AccelForce = 13.5f,
                SteerAngle = 35f,
                Grip = 10.0f
            }
        };

        public static CarSpec Get(int index)
        {
            return All[Mathf.Clamp(index, 0, All.Length - 1)];
        }
    }

    public class CharacterSpec
    {
        public int Index;
        public string Name;
        public string Nickname;
        public string Tagline;
        public string Specialty;
        public Color SuitColor;
        public Color SuitAccent;
        public Color HelmetColor;
        public Color VisorColor;
        public Color SkinColor;
        public int Number;
    }

    public static class CharacterLibrary
    {
        public static readonly CharacterSpec[] All =
        {
            new CharacterSpec
            {
                Index = 0,
                Name = "TAY ĐUA 1",
                Nickname = "BÃO LỬA",
                Tagline = "Lì lợm, thích tốc độ và không bao giờ bỏ cuộc.",
                Specialty = "CÂN BẰNG",
                SuitColor = UITheme.Hex("D93A2B"),
                SuitAccent = UITheme.Hex("FFB020"),
                HelmetColor = UITheme.Hex("C1271D"),
                VisorColor = UITheme.Hex("FFC53D"),
                SkinColor = UITheme.Hex("C98C5E"),
                Number = 7
            },
            new CharacterSpec
            {
                Index = 1,
                Name = "TAY ĐUA 2",
                Nickname = "SÓNG THẦN",
                Tagline = "Tay lái trẻ, phản xạ nhanh và cực kỳ chính xác.",
                Specialty = "TỐC ĐỘ",
                SuitColor = UITheme.Hex("0E7490"),
                SuitAccent = UITheme.Hex("67E8F9"),
                HelmetColor = UITheme.Hex("E2F7FF"),
                VisorColor = UITheme.Hex("22D3EE"),
                SkinColor = UITheme.Hex("B97A50"),
                Number = 21
            },
            new CharacterSpec
            {
                Index = 2,
                Name = "TAY ĐUA 3",
                Nickname = "BÓNG ĐÊM",
                Tagline = "Bí ẩn, điềm tĩnh và luôn chọn đường cua hoàn hảo.",
                Specialty = "BÁM ĐƯỜNG",
                SuitColor = UITheme.Hex("4C1D95"),
                SuitAccent = UITheme.Hex("E879F9"),
                HelmetColor = UITheme.Hex("1E1B2E"),
                VisorColor = UITheme.Hex("C084FC"),
                SkinColor = UITheme.Hex("8D5A3B"),
                Number = 99
            }
        };

        public static CharacterSpec Get(int index)
        {
            return All[Mathf.Clamp(index, 0, All.Length - 1)];
        }
    }
}
