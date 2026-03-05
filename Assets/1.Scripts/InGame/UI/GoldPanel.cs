using UnityEngine;
using TMPro;
public class GoldPanel : MonoBehaviour
{
    public TMP_Text goldText;

    public void Update()
    {
        goldText.text = Player.Instance.gold.ToString();
    }
}
