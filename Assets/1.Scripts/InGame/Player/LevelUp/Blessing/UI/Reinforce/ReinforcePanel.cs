
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ReinforcePanel : MonoBehaviour
{
    public ReinforceType reinforceType;
    public TMP_Text preLvText;
    public TMP_Text nextLvText;
    public Image thumImage;
    public TMP_Text titleText;
    public virtual void SetReinforce(IReinforce reinforce, int preLv, int nextLv)
    {
        preLvText.text = preLv.ToString();
        nextLvText.text = nextLv.ToString();
    }
}