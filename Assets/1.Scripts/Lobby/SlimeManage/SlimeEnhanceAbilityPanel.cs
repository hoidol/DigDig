using TMPro;
using UnityEngine;

public class SlimeEnhanceAbilityPanel : MonoBehaviour
{
    public int lv;
    public int idx;
    public TMP_Text levelText;
    public TMP_Text descText;
    public GameObject lockImageObject;
    public GameObject checkImageObject;
    public void SetSlimeData(SlimeData slimeData, UserSlime userSlime)
    {
        if (userSlime.enhanceLevel >= lv)
        {
            lockImageObject.SetActive(false);
            checkImageObject.SetActive(true);
        }
        else
        {
            lockImageObject.SetActive(true);
            checkImageObject.SetActive(false);
        }
        levelText.text = $"Lv.{userSlime.enhanceLevel+1}";

        if(slimeData is EnhanceAbilitySlimeData enhanceAbilitySlimeData)
        {   
            descText.text = enhanceAbilitySlimeData.enhanceAbilityInfos[idx].desc;    
        }
        
    }
}
