using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolingSystem<T> where T : MonoBehaviour
{
    List<T> list = new List<T>();
    [SerializeField] T prefab;

    public void SetPrefab(string path)
    {
        if (prefab != null)
            prefab = Resources.Load<T>(path);

    }

    public T GetObject<T>() where T : Component
    {
        GameObject obj = GetObject();
        return obj.GetComponent<T>();
    }

    public GameObject GetObject(Transform parent =null)
    {
        for (int i = 0; i < list.Count; i++)
        {
            if (!list[i].gameObject.activeSelf)
            {
                list[i].gameObject.SetActive(true);
                return list[i].gameObject;
            }

        }
        T obj = GameObject.Instantiate(prefab);
        obj.transform.SetParent(parent);
        list.Add(obj);
        return obj.gameObject;
    }

    public void ReturnAll()
    {
        for (int i = 0; i < list.Count; i++)
            list[i].gameObject.SetActive(false);
    }
}
