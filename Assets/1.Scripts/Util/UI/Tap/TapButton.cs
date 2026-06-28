using UnityEngine;
using UnityEngine.UI;

public class TapButton : ButtonUI
{
    public int idx;
    public Image image;
    
    //public Sprite activeSprtie;
    //public Sprite inactiveSprtie;
    public override void Awake()
    {
        base.Awake();
        image = GetComponent<Image>();
    }

    public void UpdateButton()
    {
        if(GetComponentInParent<TapContainer>().selectedIdx == idx)
        {
            //if (activeSprtie == null)
            //    image.sprite = Resources.Load<Sprite>("UI/btn_rectangle_pressed_purple");
            //else
            
            image.color = Color.white;
        }
        else
        {
            //image.sprite = Resources.Load<Sprite>("UI/btn_rectangle_night");

            image.color = Color.gray;
        }
    }
    public override void OnClickedBtn()
    {
        SoundMgr.Instance.PlaySound(SFXType.Click);
        GetComponentInParent<TapContainer>().Switch(idx);
    }
}
