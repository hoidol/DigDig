using TMPro;
using UnityEngine;

public class OrePiecePanel : MonoBehaviour
{
    public TMP_Text orePieceText;
    public void Update()
    {
        orePieceText.text = Character.Instance.coin.ToString();
    }
}