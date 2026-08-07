using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StackPoolingSystem<T> where T : MonoBehaviour
{
    public Stack<T> pool = new Stack<T>();
    [SerializeField] public T prefab;

    public void SetPrefab(string path)
    {
        if (prefab == null)
            prefab = Resources.Load<T>(path);
    }


    public virtual T Get(Vector3 pos, Transform parent =null)
    {
        T t = pool.Count > 0 ? pool.Pop() : GameObject.Instantiate(prefab, parent);
        t.transform.SetParent(parent);
        t.transform.position = pos;
        t.gameObject.SetActive(true);
        return t;
    }


    public void Return(T t)
    {
        t.gameObject.SetActive(false);
        pool.Push(t);
    }
}
