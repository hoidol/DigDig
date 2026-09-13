using TMPro;
using UnityEngine;

public class SlimeEnhanceAbilityPanel : MonoBehaviour
{
    public int lv;
    public int idx;
    public TMP_Text enhanceAbilityDescText;
    public GameObject lockImageObject;
    public void SetSlimeData(SlimeData slimeData, UserSlime userSlime)
    {
        if (userSlime.enhanceLevel >= lv)
        {

        }
        enhanceAbilityDescText.text = slimeData.enhanceAbilityDescs[idx];
    }
}
