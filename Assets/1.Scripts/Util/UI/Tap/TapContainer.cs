using UnityEngine;

public class TapContainer : MonoBehaviour
{
    [SerializeField] TapButton[] tapButtons;
    [SerializeField] TapPanel[] tapPanels;
    public int selectedIdx;
    private void Awake()
    {
        //tapButtons = GetComponentsInChildren<TapButton>(true);
        //tapPanels = GetComponentsInChildren<TapPanel>(true);
    }
    private void Start()
    {
        Switch(0);
    }

    public void Switch(int idx)
    {
        selectedIdx = idx;

        for (int i = 0; i < tapButtons.Length; i++)
        {
            tapButtons[i].UpdateButton();
        }

        for (int i = 0; i < tapPanels.Length; i++)
        {
            tapPanels[i].UpdatePanel();
        }
    }

    //TapButton GetTapButton(int idx)
    //{
    //    for (int i = 0; i < tapButtons.Length; i++)
    //    {
    //        if (tapButtons[i].idx == idx)
    //            return tapButtons[i];
    //    }
    //    return null;
    //}
    //TapPanel GetTapPanel(int idx)
    //{
    //    for (int i = 0; i < tapPanels.Length; i++)
    //    {
    //        if (tapButtons[i].idx == idx)
    //            return tapPanels[i];
    //    }
    //    return null;
    //}
}
