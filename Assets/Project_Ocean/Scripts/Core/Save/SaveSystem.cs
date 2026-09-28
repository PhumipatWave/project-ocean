// SaveSystem.cs — core read/write of save files, now with multi-slot support
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Game.Core
{
    /// <summary>
    /// Lightweight per-slot info for populating a save-list UI (Stardew-style)
    /// without fully applying a save into the game.
    /// </summary>
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
        // Bump when SaveData structure changes; add a step in Migrate().
        // v2 -> v3: added killedEnemySpawnIds / hasDivedToday, which older
        // saves don't have on disk.
        public const int SAVE_VERSION = 3;

        private const string FILE_PREFIX = "gamesave_slot";
        private const string FILE_EXT    = ".json";
        private const string LEGACY_FILE = "gamesave.json"; // old single-file save name

        private static string GetPath(int slot) =>
            Path.Combine(Application.persistentDataPath, $"{FILE_PREFIX}{slot}{FILE_EXT}");

        private static string LegacyPath =>
            Path.Combine(Application.persistentDataPath, LEGACY_FILE);

        // Runs once per session: if an old single-slot save exists and slot 0
        // doesn't yet, move it into the new slot-based scheme so existing
        // players don't lose progress when this update ships.
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

        /// <summary>
        /// Loads a save slot. Returns null if missing or unreadable —
        /// the caller must null-check before use.
        /// </summary>
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

        /// <summary>
        /// Upgrades an older SaveData in place to the current version.
        /// v0/v1 → v2: the 'quests' list did not exist; JsonUtility leaves it
        ///   null (not empty) when the field is absent from the file, which
        ///   would NRE on the first foreach unless guarded.
        /// v2 → v3: 'killedEnemySpawnIds' did not exist for the same reason;
        ///   'hasDivedToday' safely defaults to false with no guard needed.
        /// </summary>
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

        /// <summary>
        /// Scans all slot files and returns lightweight metadata for each
        /// occupied slot — for populating a Stardew-style save-list menu
        /// (day, gold, save time) without applying any data into the game.
        /// </summary>
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
