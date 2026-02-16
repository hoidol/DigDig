using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolingSystem : MonoBehaviour
{
    List<GameObject> list = new List<GameObject>();
    [SerializeField] GameObject prefab;

    public void SetObject(GameObject _prefab)
    {
        if (prefab != null)
            prefab = _prefab;

    }
    public void SetObject(Component component)
    {
        prefab = component.gameObject;
    }


    public T GetObject<T>() where T : Component
    {
        GameObject obj = GetObject();
        return obj.GetComponent<T>();
    }

    public GameObject GetObject()
    {
        for (int i = 0; i < list.Count; i++)
        {
            if (!list[i].gameObject.activeSelf)
            {
                list[i].gameObject.SetActive(true);
                return list[i];
            }

        }
        GameObject obj = Instantiate(prefab);
        obj.transform.SetParent(transform);
        list.Add(obj);
        return obj;
    }
}
