using UnityEngine;
using UnityEngine.UI;
public class ExpBar : MonoBehaviour
{
    public Image image;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        image = GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        image.fillAmount = (float)MagmaCore.Instance.exp / (float)MagmaCore.Instance.GetMaxExp();
    }
}
