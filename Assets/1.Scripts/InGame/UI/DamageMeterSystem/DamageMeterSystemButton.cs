using UnityEngine;
using UnityEngine.UI;

// 피해 집계 창(DamageMeterSystemContainer) 열기/닫기
public class DamageMeterSystemButton : MonoBehaviour
{
    public DamageMeterSystemContainer container;

    void Awake()
    {
        GetComponent<Button>().onClick.AddListener(OnClick);
        if (container != null)
            container.gameObject.SetActive(false);
    }

    void OnClick()
    {
        if (container != null)
            container.gameObject.SetActive(!container.gameObject.activeSelf);
    }
}
