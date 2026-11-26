using UnityEngine.Serialization;
using UnityEngine;
using UnityEngine.UI;
public class RunePanelOnlyImage : MonoBehaviour
{
    // public Image bgImage;
    [SerializeField] [FormerlySerializedAs("itemData")] public RuneData runeData;
    public Image thumImage;
    public virtual void SetRuneData(RuneData runeData)
    {
        this.runeData = runeData;
        if(runeData != null)
        {
            thumImage.sprite = runeData.thum;
            // thumImage.color = itemData.color;
            thumImage.enabled = true;
        }
        else
        {
            
            thumImage.enabled = false;
        }
        
    }
}
