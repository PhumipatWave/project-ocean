// SaveSystem.cs — core read/write of save files, now with multi-slot support
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Game.Core
{
    /// <summary>Per-slot summary (day, money, time) for the save menu, read without loading the save.</summary>
    [Serializable]
    public class SaveSlotMeta
    {
        public int slot;
        public string saveTime;
        public int currentDay;
        public int money;
        public GameState gameState;
    }

    public static class SaveSystem
    {
        // Bump when SaveData changes, and add a step to Migrate().
        public const int SAVE_VERSION = 3;

        private const string FILE_PREFIX = "gamesave_slot";
        private const string FILE_EXT    = ".json";
        private const string LEGACY_FILE = "gamesave.json"; // old single-file save name

        private static string GetPath(int slot) =>
            Path.Combine(Application.persistentDataPath, $"{FILE_PREFIX}{slot}{FILE_EXT}");

        private static string LegacyPath =>
            Path.Combine(Application.persistentDataPath, LEGACY_FILE);

        // Once per session, move an old single-file save into slot 0 so players keep their progress.
        private static bool _checkedLegacy = false;
        private static void MigrateLegacyFileIfNeeded()
        {
            if (_checkedLegacy) return;
            _checkedLegacy = true;

            if (File.Exists(LegacyPath) && !File.Exists(GetPath(0)))
            {
                try
                {
                    File.Move(LegacyPath, GetPath(0));
                    GameLog.Info("[SaveSystem] Migrated legacy gamesave.json → slot 0");
                }
                catch (Exception e)
                {
                    Debug.LogError($"[SaveSystem] Legacy file migration failed: {e.Message}");
                }
            }
        }

        // -------------------------------------------------- //

        public static void Save(SaveData data, int slot = 0)
        {
            MigrateLegacyFileIfNeeded();
            try
            {
                // Invariant: a Thai/other-calendar Windows locale would otherwise write e.g. year 2569.
                data.saveTime    = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
                data.saveVersion = SAVE_VERSION;

                string json = JsonUtility.ToJson(data, prettyPrint: true);
                string enc  = EncryptionHelper.Encrypt(json);
                File.WriteAllText(GetPath(slot), enc);
                GameLog.Info($"[SaveSystem] Saved slot {slot} → {GetPath(slot)}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] Save failed (slot {slot}): {e.Message}");
                throw; // rethrow so caller knows the save failed
            }
        }

        /// <summary>Loads a slot; null if it is missing or unreadable.</summary>
        public static SaveData Load(int slot = 0)
        {
            MigrateLegacyFileIfNeeded();
            string path = GetPath(slot);

            if (!File.Exists(path))
            {
                Debug.LogWarning($"[SaveSystem] No save file for slot {slot} — new game");
                return null;
            }

            try
            {
                string enc  = File.ReadAllText(path);
                string json = EncryptionHelper.Decrypt(enc);
                var data    = JsonUtility.FromJson<SaveData>(json);

                if (data == null)
                {
                    Debug.LogError($"[SaveSystem] Deserialized data is null (slot {slot})");
                    return null;
                }

                // Migrate older saves up to the current version.
                if (data.saveVersion < SAVE_VERSION)
                {
                    GameLog.Info($"[SaveSystem] Migrating slot {slot}: file={data.saveVersion} → current={SAVE_VERSION}");
                    Migrate(data);
                }

                return data;
            }
            catch (FormatException e)
            {
                // Base64 / decrypt failure — file corrupted
                Debug.LogError($"[SaveSystem] Save file corrupted (slot {slot}, decrypt failed): {e.Message}");
                return null;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] Load failed unexpectedly (slot {slot}): {e.Message}");
                return null;
            }
        }

        /// <summary>Upgrades an older save in place to SAVE_VERSION.</summary>
        // JsonUtility leaves a list missing from the file as null, not empty, so each added list needs a guard here.
        private static void Migrate(SaveData data)
        {
            if (data.quests == null)
                data.quests = new List<QuestSaveData>();

            if (data.killedEnemySpawnIds == null)
                data.killedEnemySpawnIds = new List<string>();

            data.saveVersion = SAVE_VERSION;
        }

        public static bool HasSave(int slot = 0)
        {
            MigrateLegacyFileIfNeeded();
            return File.Exists(GetPath(slot));
        }

        /// <summary>Deletes a save slot, backing it up first by default.</summary>
        public static void Delete(int slot = 0, bool backupFirst = true)
        {
            string path = GetPath(slot);
            if (!File.Exists(path)) return;

            if (backupFirst)
            {
                string backup = path + ".bak";
                try   { File.Copy(path, backup, overwrite: true); }
                catch { /* backup failure is non-fatal */ }
            }

            File.Delete(path);
            GameLog.Info($"[SaveSystem] Save deleted (slot {slot})");
        }

        /// <summary>Attempts to restore a save slot from its backup.</summary>
        public static bool RestoreBackup(int slot = 0)
        {
            string path   = GetPath(slot);
            string backup = path + ".bak";
            if (!File.Exists(backup)) return false;

            try
            {
                File.Copy(backup, path, overwrite: true);
                GameLog.Info($"[SaveSystem] Backup restored (slot {slot})");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] Restore backup failed (slot {slot}): {e.Message}");
                return false;
            }
        }

        /// <summary>Summaries of every occupied slot, for the save menu.</summary>
        public static List<SaveSlotMeta> GetAllSlotMeta(int maxSlots = 10)
        {
            MigrateLegacyFileIfNeeded();
            var result = new List<SaveSlotMeta>();
            var writeTimes = new Dictionary<int, DateTime>();

            for (int slot = 0; slot < maxSlots; slot++)
            {
                if (!File.Exists(GetPath(slot))) continue;

                var data = Load(slot); // files are small; full deserialize is cheap
                if (data == null) continue;

                writeTimes[slot] = File.GetLastWriteTimeUtc(GetPath(slot));
                result.Add(new SaveSlotMeta
                {
                    slot       = slot,
                    saveTime   = data.saveTime,
                    currentDay = data.currentDay,
                    money      = data.money,
                    gameState  = data.gameState,
                });
            }

            // Most recent first. File time, not the saveTime text, which old saves wrote in the local calendar.
            result.Sort((a, b) => writeTimes[b.slot].CompareTo(writeTimes[a.slot]));
            return result;
        }
    }
}
