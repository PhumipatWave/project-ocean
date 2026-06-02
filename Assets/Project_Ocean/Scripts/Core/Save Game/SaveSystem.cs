// SaveSystem.cs — core เขียน/อ่านไฟล์
using System.IO;
using UnityEngine;

namespace Game.Testing
{
    public static class SaveSystem
    {
        static readonly string PATH = $"{Application.persistentDataPath}/gamesave.json";

        public static void Save(SaveData data)
        {
            data.saveTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm");
            string json = JsonUtility.ToJson(data, prettyPrint: true);
            string enc = EncryptionHelper.Encrypt(json);
            File.WriteAllText(PATH, enc);
            Debug.Log($"[SaveSystem] Saved → {PATH}");
        }

        public static SaveData Load()
        {
            if (!File.Exists(PATH))
            {
                Debug.LogWarning("[SaveSystem] No save file — new game");
                return new SaveData();
            }
            string enc = File.ReadAllText(PATH);
            string json = EncryptionHelper.Decrypt(enc);
            return JsonUtility.FromJson<SaveData>(json);
        }

        public static bool HasSave() => File.Exists(PATH);

        public static void Delete()
        {
            if (File.Exists(PATH)) File.Delete(PATH);
        }
    }
}