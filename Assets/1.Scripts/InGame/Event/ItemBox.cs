using UnityEngine;

public class ItemBox : EventObject
{


    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            interacting = true;
            Time.timeScale = 0;
            SelectItemCanvas.Instance.OpenCanvas(() =>
            {
                Time.timeScale = 1;
                OnDestroy();
            });
        }
    }
}