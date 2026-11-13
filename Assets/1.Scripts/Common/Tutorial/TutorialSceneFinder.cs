using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// 튜토리얼 프리팹은 씬 오브젝트를 직접 참조할 수 없으므로 이름으로 찾는다 (꺼져 있는 오브젝트 포함)
// 이름 = 오브젝트 이름 하나("DrawItemButton") 또는 경로("GameCanvas/Bottom/DrawItemButton", 중간부터 써도 됨)
public static class TutorialSceneFinder
{
    public static GameObject Find(string nameOrPath)
    {
        if (string.IsNullOrEmpty(nameOrPath))
            return null;

        string[] parts = nameOrPath.Split('/');
        GameObject found = null;
        int count = 0;
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (!scene.isLoaded)
                continue;
            foreach (GameObject root in scene.GetRootGameObjects())
                Search(root.transform, parts, ref found, ref count);
        }

        if (found == null)
            Debug.LogWarning($"[Tutorial] 씬에서 찾을 수 없음: {nameOrPath}");
        else if (count > 1)
            Debug.LogWarning($"[Tutorial] 같은 이름이 {count}개: {nameOrPath} → 첫 번째 사용 (경로로 지정하면 구분됨)");
        return found;
    }

    public static List<GameObject> FindAll(IList<string> namesOrPaths)
    {
        List<GameObject> result = new List<GameObject>();
        if (namesOrPaths == null)
            return result;
        for (int i = 0; i < namesOrPaths.Count; i++)
        {
            GameObject go = Find(namesOrPaths[i]);
            if (go != null)
                result.Add(go);
        }
        return result;
    }

    static void Search(Transform tr, string[] parts, ref GameObject found, ref int count)
    {
        if (tr.name == parts[parts.Length - 1] && MatchParents(tr, parts))
        {
            count++;
            if (found == null)
                found = tr.gameObject;
        }
        for (int i = 0; i < tr.childCount; i++)
            Search(tr.GetChild(i), parts, ref found, ref count);
    }

    // 경로의 앞부분이 실제 부모 순서와 맞는지 (끝에서부터 비교)
    static bool MatchParents(Transform tr, string[] parts)
    {
        Transform parent = tr.parent;
        for (int i = parts.Length - 2; i >= 0; i--)
        {
            if (parent == null || parent.name != parts[i])
                return false;
            parent = parent.parent;
        }
        return true;
    }
}
