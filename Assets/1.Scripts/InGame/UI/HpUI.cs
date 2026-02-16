using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine.UI;
public class HpUI : MonoBehaviour
{
    public static List<HpUI> list = new List<HpUI>();
    static HpUI hpUIPrefab;
    public Image hpImage;
    public IHittable hittable;
    public static HpUI GetHpUI(IHittable hittable)
    {
        HpUI hUI = GetHpUIInPooling();
        hUI.hittable = hittable;
        hUI.timer = 5;
        return hUI;
    }
    static HpUI GetHpUIInPooling()
    {
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i].gameObject.activeSelf)
                continue;
            list[i].gameObject.SetActive(true);
            return list[i];
        }
        if (hpUIPrefab == null)
        {
            hpUIPrefab = Resources.Load<HpUI>("UI/HpUI");
        }

        HpUI hUI = Instantiate(hpUIPrefab, GameWorldCanvas.Instance.transform);
        list.Add(hUI);
        return hUI;
    }

    private void OnDestroy()
    {
        list.Remove(this);
    }
    public void SetRate(float rate)
    {
        hpImage.fillAmount = rate;
        timer = 5;
    }
    public float timer = 5;
    void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            Release();
        }
    }

    //반환하다
    public void Release()
    {
        hittable = null;
        gameObject.SetActive(false);
    }
}
