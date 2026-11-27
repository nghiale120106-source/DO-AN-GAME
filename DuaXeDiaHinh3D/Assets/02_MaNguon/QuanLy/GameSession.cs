using System.Collections.Generic;
using UnityEngine;

namespace DuaXe3D
{
    /// <summary>
    /// Lưu / đọc lựa chọn của người chơi bằng PlayerPrefs (Yêu cầu 13).
    /// </summary>
    public static class GameSession
    {
        private const string KeyCar = "DX3D_SelectedCar";
        private const string KeyCharacter = "DX3D_SelectedCharacter";
        private const string KeyTerrain = "DX3D_SelectedTerrain";
        private const string KeyMusic = "DX3D_Music";
        private const string KeySfx = "DX3D_Sfx";

        public static int CarIndex { get; private set; }
        public static int CharacterIndex { get; private set; }
        public static int TerrainIndex { get; private set; }

        public static bool MusicOn
        {
            get { return PlayerPrefs.GetInt(KeyMusic, 1) == 1; }
            set { PlayerPrefs.SetInt(KeyMusic, value ? 1 : 0); PlayerPrefs.Save(); }
        }

        public static bool SfxOn
        {
            get { return PlayerPrefs.GetInt(KeySfx, 1) == 1; }
            set { PlayerPrefs.SetInt(KeySfx, value ? 1 : 0); PlayerPrefs.Save(); }
        }

        public static void Load()
        {
            CarIndex = Mathf.Clamp(PlayerPrefs.GetInt(KeyCar, 0), 0, CarLibrary.All.Length - 1);
            CharacterIndex = Mathf.Clamp(PlayerPrefs.GetInt(KeyCharacter, 0), 0, CharacterLibrary.All.Length - 1);
            TerrainIndex = Mathf.Clamp(PlayerPrefs.GetInt(KeyTerrain, 0), 0, ThemeLibrary.All.Length - 1);

            // Xóa kỷ lục bất thường (< 20s) nếu từng sinh ra do chế độ tự kiểm thử cũ
            for (int i = 0; i < ThemeLibrary.All.Length; i++)
            {
                string key = "DX3D_Best_" + i;
                float best = PlayerPrefs.GetFloat(key, 0f);
                if (best > 0f && best < 20f)
                {
                    PlayerPrefs.DeleteKey(key);
                }
            }
        }

        public static void SetCar(int index)
        {
            CarIndex = Mathf.Clamp(index, 0, CarLibrary.All.Length - 1);
            PlayerPrefs.SetInt(KeyCar, CarIndex);
            PlayerPrefs.Save();
        }

        public static void SetCharacter(int index)
        {
            CharacterIndex = Mathf.Clamp(index, 0, CharacterLibrary.All.Length - 1);
            PlayerPrefs.SetInt(KeyCharacter, CharacterIndex);
            PlayerPrefs.Save();
        }

        public static void SetTerrain(int index)
        {
            TerrainIndex = Mathf.Clamp(index, 0, ThemeLibrary.All.Length - 1);
            PlayerPrefs.SetInt(KeyTerrain, TerrainIndex);
            PlayerPrefs.Save();
        }

        public static float GetBestTime(int terrainIndex)
        {
            return PlayerPrefs.GetFloat("DX3D_Best_" + terrainIndex, 0f);
        }

        /// <summary>Trả về true nếu đây là kỷ lục mới.</summary>
        public static bool SubmitTime(int terrainIndex, float seconds)
        {
            if (seconds <= 0f) return false;
            float best = GetBestTime(terrainIndex);
            if (best <= 0f || seconds < best)
            {
                PlayerPrefs.SetFloat("DX3D_Best_" + terrainIndex, seconds);
                PlayerPrefs.Save();
                return true;
            }
            return false;
        }
    }

    public class RaceResultEntry
    {
        public string Name;
        public string CarName;
        public string CharacterName;
        public int CarIndex;
        public int CharacterIndex;
        public float Time;
        public bool IsPlayer;
        public bool Finished;
        public int Place;
    }

    /// <summary>Dữ liệu chuyển giữa scene 05_DuaXe và 06_KetQua.</summary>
    public static class RaceResults
    {
        public static readonly List<RaceResultEntry> Entries = new List<RaceResultEntry>();
        public static int PlayerPlace = 1;
        public static float PlayerTime;
        public static bool HasData;
        public static bool NewRecord;
        public static int TerrainIndex;

        public static void Clear()
        {
            Entries.Clear();
            PlayerPlace = 1;
            PlayerTime = 0f;
            HasData = false;
            NewRecord = false;
        }
    }
}
