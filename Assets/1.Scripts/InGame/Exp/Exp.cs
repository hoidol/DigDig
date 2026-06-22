using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Exp : MonoBehaviour, IPickable
{
    public string Key => "Exp";
    public static ExpPoolingSystem poolingSystem = new();

    public bool IsTaken { get; set; }
    public Transform Transform => transform;

    public static void Instantiate(Vector2 pos, int count, float size)
    {
        for(int i = 0; i < count; i++)
        {
            Vector2 position = pos + Random.insideUnitCircle * size;
            poolingSystem.Get(position);
        }
    }

    public void Droped(Vector2 pos)
    {
        transform.position = pos;
        IsTaken = false;
    }

    public void PickedUp()
    {
        Player.Instance.AddExp(1);
        poolingSystem.Return(this);
    }

    public void Take(IPicker picker)
    {
        IsTaken = true;
        transform.DOMove(picker.Transform.position, 0.1f).OnComplete(() =>
        {
            picker.PickUp(this);
        });
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (IsTaken)
            return;

        if (collision.CompareTag("Player"))
        {
            Take(Player.Instance);

        }
    }
}
