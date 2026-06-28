using UnityEngine;

public class TapPanel : MonoBehaviour
{
    public int idx;
    public void UpdatePanel()
    {
        if (GetComponentInParent<TapContainer>().selectedIdx == idx)
        {
            gameObject.SetActive(true);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}
