// SaveSystem.cs — core read/write of the save file
using System;
using System.IO;
using UnityEngine;

namespace Game.Testing
{
    public static class SaveSystem
    {
        // Bump when SaveData structure changes; add a step in Migrate().
        public const int SAVE_VERSION = 2;

        static readonly string PATH =
            Path.Combine(Application.persistentDataPath, "gamesave.json");

        // -------------------------------------------------- //

        public static void Save(SaveData data)
        {
            try
            {
                data.saveTime    = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                data.saveVersion = SAVE_VERSION;

                string json = JsonUtility.ToJson(data, prettyPrint: true);
                string enc  = EncryptionHelper.Encrypt(json);
                File.WriteAllText(PATH, enc);
                Debug.Log($"[SaveSystem] Saved → {PATH}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] Save failed: {e.Message}");
                throw; // rethrow so caller knows the save failed
            }
        }

        /// <summary>
        /// Loads the save file. Returns null if missing or unreadable —
        /// the caller must null-check before use.
        /// </summary>
        public static SaveData Load()
        {
            if (!File.Exists(PATH))
            {
                Debug.LogWarning("[SaveSystem] No save file — new game");
                return null;
            }

            try
            {
                string enc  = File.ReadAllText(PATH);
                string json = EncryptionHelper.Decrypt(enc);
                var data    = JsonUtility.FromJson<SaveData>(json);

                if (data == null)
                {
                    Debug.LogError("[SaveSystem] Deserialized data is null");
                    return null;
                }

                // Migrate older saves up to the current version.
                if (data.saveVersion < SAVE_VERSION)
                {
                    Debug.Log($"[SaveSystem] Migrating save: file={data.saveVersion} → current={SAVE_VERSION}");
                    Migrate(data);
                }

                return data;
            }
            catch (FormatException e)
            {
                // Base64 / decrypt failure — file corrupted
                Debug.LogError($"[SaveSystem] Save file corrupted (decrypt failed): {e.Message}");
                return null;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] Load failed unexpectedly: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// Upgrades an older SaveData in place to the current version.
        /// v0/v1 → v2: the 'quests' list did not exist; JsonUtility leaves it empty,
        /// which QuestManager.LoadSaveState treats as "keep template defaults".
        /// The hook stays here for future structural migrations.
        /// </summary>
        private static void Migrate(SaveData data)
        {
            if (data.quests == null)
                data.quests = new System.Collections.Generic.List<QuestSaveData>();

            data.saveVersion = SAVE_VERSION;
        }

        public static bool HasSave() => File.Exists(PATH);

        /// <summary>Deletes the save file, backing it up first by default.</summary>
        public static void Delete(bool backupFirst = true)
        {
            if (!File.Exists(PATH)) return;

            if (backupFirst)
            {
                string backup = PATH + ".bak";
                try   { File.Copy(PATH, backup, overwrite: true); }
                catch { /* backup failure is non-fatal */ }
            }

            File.Delete(PATH);
            Debug.Log("[SaveSystem] Save deleted");
        }

        /// <summary>Attempts to restore the save from backup.</summary>
        public static bool RestoreBackup()
        {
            string backup = PATH + ".bak";
            if (!File.Exists(backup)) return false;

            try
            {
                File.Copy(backup, PATH, overwrite: true);
                Debug.Log("[SaveSystem] Backup restored");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] Restore backup failed: {e.Message}");
                return false;
            }
        }
    }
}
