using UnityEngine;

public class FadeObject : MonoBehaviour
{
    public float fadeTime;

    public bool onceShow;
    public string fadeKey; 
    void Start()
    {
        if (onceShow && !string.IsNullOrEmpty(fadeKey))
        {
            int count = PlayerPrefs.GetInt(fadeKey,0);
            if(count <= 0)
            {
                Invoke("Fade", fadeTime);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
        else
        {
            Invoke("Fade", fadeTime);
        }
        
    }

    void Fade()
    {
        PlayerPrefs.SetInt(fadeKey, 1);
        gameObject.SetActive(false);
    }
}
