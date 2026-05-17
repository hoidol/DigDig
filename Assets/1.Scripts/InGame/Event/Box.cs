using UnityEngine;

public class Box : EventObject
{
    public Grade grade;

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Time.timeScale = 0;
            SelectItemCanvas.Instance.OpenCanvas(grade, () =>
            {
                Time.timeScale = 1;
                Player.Instance.UpdatePlayer();
                Destroy();
            });
        }
    }
}