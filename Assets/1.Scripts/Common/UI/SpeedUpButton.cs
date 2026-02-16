using UnityEngine;
using UnityEngine.EventSystems;
public class SpeedUpButton : MonoBehaviour,IPointerDownHandler,IPointerUpHandler,IPointerExitHandler
{
    public float speed;
    private void OnDisable()
    {
        Time.timeScale = 1;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Time.timeScale = speed;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Time.timeScale = 1;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Time.timeScale = 1;
    }

    
}
