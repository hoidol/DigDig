using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace EffectMaker
{
    // 고른 이펙트 목록 (git에 올라가도록 Editor 폴더 안 JSON)
    [Serializable]
    public class EffectFavorite
    {
        public string usage;
        public string path;
        public float scale = 1f;
        public SpawnTarget spawnTarget;
        public string memo = "";
    }

    [Serializable]
    public class EffectFavoriteList
    {
        public const string FILE_PATH = "Assets/1.Scripts/EffectMaker/Editor/Data/EffectMakerFavorites.json";

        public List<EffectFavorite> items = new List<EffectFavorite>();

        public static EffectFavoriteList Load()
        {
            var list = new EffectFavoriteList();
            if (File.Exists(FILE_PATH))
            {
                try { JsonUtility.FromJsonOverwrite(File.ReadAllText(FILE_PATH), list); }
                catch (Exception e) { Debug.LogWarning($"[EffectMaker] 즐겨찾기 파일을 읽지 못함: {e.Message}"); }
            }
            if (list.items == null)
                list.items = new List<EffectFavorite>();
            return list;
        }

        public void Save()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(FILE_PATH));
            File.WriteAllText(FILE_PATH, JsonUtility.ToJson(this, true));
            AssetDatabase.ImportAsset(FILE_PATH);
        }

        public EffectFavorite Find(string usage, string path)
        {
            return items.Find(f => f.usage == usage && f.path == path);
        }

        public void Toggle(string usage, string path, float scale, SpawnTarget spawnTarget)
        {
            var found = Find(usage, path);
            if (found != null)
                items.Remove(found);
            else
                items.Add(new EffectFavorite { usage = usage, path = path, scale = scale, spawnTarget = spawnTarget });
            Save();
        }
    }
}
