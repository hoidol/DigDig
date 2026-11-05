using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

public class BulletShortInfoPanel : MonoSingleton<BulletShortInfoPanel>
{

    public Image thumImage;
    public TMP_Text titleText;
    public TMP_Text descriptionText;

    List<string> alreadyShowBulletKeys = new List<string>();
    List<BulletData> bulletDatas = new List<BulletData>();

    public void AddShortInfo(BulletData bulletData, bool checkAlreadyShow)
    {
        if (checkAlreadyShow)
        {
            if (alreadyShowBulletKeys.Contains(bulletData.key))
                return;
        }

        if (!alreadyShowBulletKeys.Contains(bulletData.key))
        {
            alreadyShowBulletKeys.Add(bulletData.key);
        }
        bulletDatas.Add(bulletData);

        TryShow();
    }
    bool showing;
    void TryShow()
    {
        if (bulletDatas.Count <= 0)
            return;
        if (showing)
            return;

        showing = true;

        BulletData data = bulletDatas[0];
        bulletDatas.RemoveAt(0);
        thumImage.sprite = data.thumbnail;
        titleText.text = data.Title;
        descriptionText.text = data.desc;
        gameObject.SetActive(true);
        Show(data).Forget();

    }

    async UniTaskVoid Show(BulletData data)
    {
        await UniTask.Delay(4000);
        gameObject.SetActive(false);
        showing = false;
        TryShow();
    }


    public void OnClickedSelect()
    {
        // PickedBulletCanvas.Instance.CloseCanvas();
    }
}
