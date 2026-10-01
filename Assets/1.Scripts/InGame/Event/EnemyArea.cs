using UnityEngine;

public class EnemyArea : EventObject
{
    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            interacting = true;
            
        }
    }
}