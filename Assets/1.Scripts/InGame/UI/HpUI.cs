using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class HpUI : MonoBehaviour
{
    static readonly Stack<HpUI> pool = new();
    static HpUI prefab;

    public Image hpImage;
    public IHpUI target;
    public float timer;

    public static HpUI Get(IHpUI target)
    {
        HpUI ui = pool.Count > 0 ? pool.Pop() : Instantiate(GetPrefab(), GameWorldCanvas.Instance.transform);
        ui.gameObject.SetActive(true);
        ui.target = target;
        ui.timer = 5f;
        return ui;
    }

    static HpUI GetPrefab()
    {
        if (prefab == null)
            prefab = Resources.Load<HpUI>("UI/HpUI");
        return prefab;
    }

    public bool IsOwn(IHpUI owner) => gameObject.activeSelf && target == owner;

    public void Release()
    {
        target = null;
        gameObject.SetActive(false);
        pool.Push(this);
    }
    public void UpdateTime()
    {
        timer = 5;
    }

    void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0f) { Release(); return; }
        transform.position = target.HpUIPosition;
        hpImage.fillAmount = target.CurHp / target.MaxHp;
    }
}
