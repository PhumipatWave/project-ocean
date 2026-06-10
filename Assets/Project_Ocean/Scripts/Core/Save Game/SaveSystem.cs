// SaveSystem.cs — core เขียน/อ่านไฟล์
using System;
using System.IO;
using UnityEngine;

namespace Game.Testing
{
    public static class SaveSystem
    {
        // ✅ เพิ่ม version — ถ้า SaveData เปลี่ยน structure ให้ bump ขึ้น
        public const int SAVE_VERSION = 1;

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
                throw; // ✅ โยนต่อให้ caller รู้ว่า save ล้มเหลว
            }
        }

        /// <summary>
        /// โหลด save file — ถ้าไม่มีหรืออ่านไม่ได้จะ return null
        /// ผู้เรียกต้องเช็ค null เองก่อนใช้งาน
        /// </summary>
        public static SaveData Load()
        {
            if (!File.Exists(PATH))
            {
                Debug.LogWarning("[SaveSystem] No save file — new game");
                return null; // ✅ return null แทน new SaveData() เพื่อให้ caller แยกกรณีได้
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

                // ✅ Version check — warn ถ้า save เก่ากว่า current version
                if (data.saveVersion < SAVE_VERSION)
                    Debug.LogWarning(
                        $"[SaveSystem] Save version mismatch: file={data.saveVersion}, " +
                        $"current={SAVE_VERSION}. Some fields may use default values.");

                return data;
            }
            catch (FormatException e)
            {
                // Base64 / decrypt ผิดพลาด — ไฟล์เสียหาย
                Debug.LogError($"[SaveSystem] Save file corrupted (decrypt failed): {e.Message}");
                return null;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] Load failed unexpectedly: {e.Message}");
                return null;
            }
        }

        public static bool HasSave() => File.Exists(PATH);

        /// <summary>ลบ save file พร้อม backup ไว้ก่อนเผื่อเรียกคืน</summary>
        public static void Delete(bool backupFirst = true)
        {
            if (!File.Exists(PATH)) return;

            if (backupFirst)
            {
                string backup = PATH + ".bak";
                try   { File.Copy(PATH, backup, overwrite: true); }
                catch { /* backup ล้มเหลวก็ไม่เป็นไร */ }
            }

            File.Delete(PATH);
            Debug.Log("[SaveSystem] Save deleted");
        }

        /// <summary>ลองกู้ save จาก backup</summary>
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
