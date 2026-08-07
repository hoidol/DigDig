using UnityEngine;
using System.Collections;
using UnityEngine.UI;
[RequireComponent(typeof(Button))]
public abstract class ButtonUI : MonoBehaviour
{
    public Button button;
    // Use this for initialization
    public virtual void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnClickedBtn);
    }

    // Update is called once per frame
    public abstract void OnClickedBtn();
}
