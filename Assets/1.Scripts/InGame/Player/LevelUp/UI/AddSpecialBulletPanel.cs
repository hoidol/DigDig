using UnityEngine;

public class AddSpecialBulletPanel : LevelUpBonusPanel
{
    public GameObject selectButton;
    public GameObject noEnoghButton;
    public void CanSelect(bool b)
    {
        if (b == false)
        {
            noEnoghButton.gameObject.SetActive(true);
            selectButton.gameObject.SetActive(false);
        }
        else
        {

            noEnoghButton.gameObject.SetActive(false);
            selectButton.gameObject.SetActive(true);
        }
    }
}