using System;
using System.IO;
using UnityEngine;

namespace HolyMackerel.Core
{
    /// <summary>
    /// Persistent singleton that owns <see cref="PlayerData"/> and round-trips it
    /// to disk as JSON. Place a GameManager GameObject in the StartScreen scene
    /// manually — there is no auto-bootstrap. Survives scene loads via
    /// DontDestroyOnLoad and enforces a single instance in Awake.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public PlayerData Data { get; private set; }

        private static string SavePath => Path.Combine(Application.persistentDataPath, "save.json");

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            Load();
        }

        /// <summary>
        /// Serializes <see cref="Data"/> to JSON and writes it to the save file.
        /// </summary>
        public void Save()
        {
            try
            {
                string json = JsonUtility.ToJson(Data);
                File.WriteAllText(SavePath, json);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[GameManager] Save failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Adds coins to the current save and immediately persists.
        /// </summary>
        public void AddCoins(int amount)
        {
            Data.coins += amount;
            Save();
        }

        private void Load()
        {
            if (!File.Exists(SavePath))
            {
                Debug.Log("[GameManager] No save found — starting with fresh PlayerData.");
                Data = new PlayerData();
                return;
            }

            try
            {
                string json = File.ReadAllText(SavePath);
                PlayerData loaded = JsonUtility.FromJson<PlayerData>(json);
                if (loaded == null)
                {
                    Debug.Log("[GameManager] Save corrupted (deserialized null) — resetting.");
                    Data = new PlayerData();
                    return;
                }
                Data = loaded;
            }
            catch (Exception ex)
            {
                Debug.Log($"[GameManager] Save corrupted, resetting. ({ex.Message})");
                Data = new PlayerData();
            }
        }
    }
}
