using UnityEngine;
using System.Collections;
using TMPro;

[RequireComponent(typeof(TMP_Text))]
public class TranslateText : MonoBehaviour
{
    public TMP_Text text;
    public string key;

    //private void OnEnable()
    //{

    //}
    //private void OnDestroy()
    //{

    //}
    // Use this for initialization
    private void Awake()
    {
        if (string.IsNullOrEmpty(key))
            return;
        text = GetComponent<TMP_Text>();
    }

    void Start()
    {
        if (string.IsNullOrEmpty( key))
            return;
        UpdatePanel();
    }


    public void UpdatePanel()
    {
        if (string.IsNullOrEmpty(key))
            return;
        if (text == null)
            return;
        text.text = TranslateManager.GetText(key);
    }

    
}
