using UnityEngine;

public class LifeFountain : EventObject
{
    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // ItemStoreCanvas.Instance.OpenCanvas(transform, maxGrade, () =>
            // {
            //     Player.Instance.UpdatePlayer();
            // });
        }
    }
}
