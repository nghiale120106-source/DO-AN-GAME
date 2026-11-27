using UnityEngine;

namespace DuaXe3D
{
    public enum ThemeKind
    {
        Desert,
        Forest,
        Mountain,
        Mud
    }

    /// <summary>Bộ màu + thông số cho 4 địa hình (Yêu cầu 6 &amp; 7).</summary>
    public class TerrainTheme
    {
        public int Index;
        public ThemeKind Kind;
        public string Name;
        public string EnglishName;
        public string Description;
        public string Difficulty;

        public Color SkyTint;
        public Color GroundColor;
        public Color GrassColor;
        public Color RockColor;
        public Color DirtColor;
        public Color SandColor;
        public Color FogColor;
        public Color SunColor;
        public Color AccentColor;
        public Color PropColor;
        public Color PropColor2;

        public float FogDensity = 0.002f;
        public float TerrainAmplitude = 30f;
        public float SunIntensity = 1.2f;
        public float SunPitch = 42f;
        public float SunYaw = 35f;
        public float SurfaceGrip = 1f;

        public int TreeCount = 80;
        public int RockCount = 40;
        public int BushCount = 40;
        public int CloudCount = 0;

        public bool SnowCaps;
        public bool Cacti;
        public bool Fences;
        public bool MudPuddles;
    }

    public static class ThemeLibrary
    {
        public static readonly TerrainTheme[] All =
        {
            new TerrainTheme
            {
                Index = 0, Kind = ThemeKind.Desert,
                Name = "SA MẠC",
                EnglishName = "DESERT",
                Description = "Nắng cháy, cát mềm và những cồn cát uốn lượn. Đường đất dễ trượt bánh.",
                Difficulty = "TRUNG BÌNH",
                SkyTint = UITheme.Hex("F5A65B"),
                GroundColor = UITheme.Hex("D9B36C"),
                GrassColor = UITheme.Hex("C9A45E"),
                RockColor = UITheme.Hex("9A5B3A"),
                DirtColor = UITheme.Hex("A97845"),
                SandColor = UITheme.Hex("E7C98F"),
                FogColor = UITheme.Hex("E5B27A"),
                SunColor = UITheme.Hex("FFE0B0"),
                AccentColor = UITheme.Hex("FF7A1A"),
                PropColor = UITheme.Hex("7E9B52"),
                PropColor2 = UITheme.Hex("8C6440"),
                FogDensity = 0.0017f,
                TerrainAmplitude = 26f,
                SunIntensity = 1.35f,
                SunPitch = 30f,
                SunYaw = 28f,
                TreeCount = 55,
                RockCount = 60,
                BushCount = 26,
                Cacti = true
            },
            new TerrainTheme
            {
                Index = 1, Kind = ThemeKind.Forest,
                Name = "RỪNG GIÀ",
                EnglishName = "FOREST",
                Description = "Cây cối rậm rạp, ánh sáng lọc qua tán lá và những khúc cua khuất tầm nhìn.",
                Difficulty = "KHÓ",
                SkyTint = UITheme.Hex("7FC8FF"),
                GroundColor = UITheme.Hex("4F7A36"),
                GrassColor = UITheme.Hex("3F6B2C"),
                RockColor = UITheme.Hex("6B7065"),
                DirtColor = UITheme.Hex("6A4B31"),
                SandColor = UITheme.Hex("8E7C4D"),
                FogColor = UITheme.Hex("A9C9AE"),
                SunColor = UITheme.Hex("FFF4DA"),
                AccentColor = UITheme.Hex("34D399"),
                PropColor = UITheme.Hex("2F5E2A"),
                PropColor2 = UITheme.Hex("4A7D3A"),
                FogDensity = 0.0028f,
                TerrainAmplitude = 36f,
                SunIntensity = 1.18f,
                SunPitch = 48f,
                SunYaw = 40f,
                TreeCount = 190,
                RockCount = 46,
                BushCount = 110
            },
            new TerrainTheme
            {
                Index = 2, Kind = ThemeKind.Mountain,
                Name = "ĐỒI NÚI",
                EnglishName = "MOUNTAINS",
                Description = "Đèo dốc đá sỏi, đỉnh núi phủ tuyết và những đoạn đường cheo leo.",
                Difficulty = "KHÓ",
                SkyTint = UITheme.Hex("8FB9EA"),
                GroundColor = UITheme.Hex("777A72"),
                GrassColor = UITheme.Hex("5C6B4C"),
                RockColor = UITheme.Hex("77736B"),
                DirtColor = UITheme.Hex("6E5D4B"),
                SandColor = UITheme.Hex("9A958A"),
                FogColor = UITheme.Hex("C3D2E2"),
                SunColor = UITheme.Hex("EEF6FF"),
                AccentColor = UITheme.Hex("60A5FA"),
                PropColor = UITheme.Hex("3C5A44"),
                PropColor2 = UITheme.Hex("8B8D86"),
                FogDensity = 0.0022f,
                TerrainAmplitude = 58f,
                SunIntensity = 1.22f,
                SunPitch = 36f,
                SunYaw = 55f,
                TreeCount = 95,
                RockCount = 120,
                BushCount = 42,
                SnowCaps = true
            },
            new TerrainTheme
            {
                Index = 3, Kind = ThemeKind.Mud,
                Name = "ĐƯỜNG BÙN",
                EnglishName = "MUDDY ROAD",
                Description = "Bùn đất lầy lội sau mưa, vũng nước sâu và những đoạn đường trơn trượt đầy thử thách.",
                Difficulty = "TRUNG BÌNH",
                SkyTint = UITheme.Hex("8CA3B0"),
                GroundColor = UITheme.Hex("5A4A35"),
                GrassColor = UITheme.Hex("4D5E32"),
                RockColor = UITheme.Hex("595248"),
                DirtColor = UITheme.Hex("4A3522"),
                SandColor = UITheme.Hex("6B5439"),
                FogColor = UITheme.Hex("9EA9A2"),
                SunColor = UITheme.Hex("E8E2D0"),
                AccentColor = UITheme.Hex("D97706"),
                PropColor = UITheme.Hex("3B5228"),
                PropColor2 = UITheme.Hex("5C6B32"),
                FogDensity = 0.0026f,
                TerrainAmplitude = 24f,
                SunIntensity = 1.08f,
                SunPitch = 34f,
                SunYaw = 24f,
                SurfaceGrip = 0.78f,
                TreeCount = 115,
                RockCount = 48,
                BushCount = 95,
                Fences = true,
                MudPuddles = true
            }
        };

        public static TerrainTheme Get(int index)
        {
            return All[Mathf.Clamp(index, 0, All.Length - 1)];
        }
    }
}
