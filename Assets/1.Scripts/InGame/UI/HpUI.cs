using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class HpUI : MonoBehaviour
{
    static readonly Stack<HpUI> pool = new();
    static HpUI prefab;

    public Image hpImage;
    public IHittable hittable;
    public float timer;

    public static HpUI Get(IHittable hittable)
    {
        HpUI ui = pool.Count > 0 ? pool.Pop() : Instantiate(GetPrefab(), GameWorldCanvas.Instance.transform);
        ui.gameObject.SetActive(true);
        ui.hittable = hittable;
        ui.timer = 5f;
        return ui;
    }

    static HpUI GetPrefab()
    {
        if (prefab == null)
            prefab = Resources.Load<HpUI>("UI/HpUI");
        return prefab;
    }

    public bool IsOwn(Transform owner)
    {
        return gameObject.activeSelf && hittable.Transform == owner;
    }

    public void SetRate(float rate)
    {
        hpImage.fillAmount = rate;
        timer = 5f;
    }

    public void Release()
    {
        hittable = null;
        gameObject.SetActive(false);
        pool.Push(this);
    }

    void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0f)
            Release();
    }
}
