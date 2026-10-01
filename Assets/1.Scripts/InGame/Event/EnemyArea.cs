using System.Collections.Generic;
using UnityEngine;

public class EnemyArea : MonoBehaviour
{
    public MapRoom room;
    public List<Enemy> enemiesInArea = new List<Enemy>();
    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            
        }
    }

}