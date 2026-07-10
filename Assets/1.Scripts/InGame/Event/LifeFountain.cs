using UnityEngine;

public class LifeFountain : EventObject
{
    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Time.timeScale= 0;
            LifeFountainCanvas.Instance.OpenCanvas(this, () =>
            {
                Time.timeScale= 1;
            });
        }
    }
}
