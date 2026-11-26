using UnityEngine.Serialization;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SelectRunePanel : MonoBehaviour
{
    [SerializeField] [FormerlySerializedAs("itemData")] protected RuneData runeData;

    public Image thumImage;
    public TMP_Text descText;

    public void SetRuneData(RuneData runeData)
    {
        this.runeData = runeData;
        thumImage.sprite = runeData.thum;

        descText.text = runeData.GetDescription();
    }

    public void OnClickedButton()
    {
        GetComponentInParent<SelectRuneCanvas>().Selected(runeData);


    }
}
