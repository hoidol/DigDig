using UnityEngine;
using UnityEngine.UI;
public class AngerPanel : MonoBehaviour
{
    public Image progressBar;

    void Update()
    {
        progressBar.fillAmount = Player.Instance.angerAmount / 100f;
    }
}
