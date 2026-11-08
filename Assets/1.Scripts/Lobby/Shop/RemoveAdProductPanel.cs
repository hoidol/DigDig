using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;


public class RemoveAdProductPanel : ProductPanel 
{
    public GameObject adRemovedObject;
    public override void UpdatePanel()
    {
        base.UpdatePanel();
        adRemovedObject.gameObject.SetActive(UserDataManager.Instance.isAdRemoved);
    }
}
