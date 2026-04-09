using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Gold : MonoBehaviour, IPickable
{
    public string Key => key;
    public string key;
    public bool IsTaken { get; set; }

    static readonly Stack<Gold> pool = new();
    static Gold prefab;

    public static Gold Dropped(Vector2 pos, string key)
    {
        Gold gold = Get();
        gold.gameObject.SetActive(true);
        gold.Droped(pos, key);
        return gold;
    }

    static Gold Get()
    {
        if (pool.Count > 0)
            return pool.Pop();

        if (prefab == null)
            prefab = Resources.Load<Gold>("Object/Gold");

        return Instantiate(prefab);
    }

    public int exp = 1;
    public int gold = 1;

    public void Droped(Vector2 pos, string key)
    {
        transform.position = pos;
        this.key = key;
        IsTaken = false;
    }

    public void Take(IPicker picker)
    {
        IsTaken = true;
        transform.DOMove(picker.Transform.position, 0.35f).OnComplete(() =>
        {
            picker.PickUp(this);
        });
    }

    public void PickedUp()
    {
        gameObject.SetActive(false);
        pool.Push(this);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (IsTaken)
            return;

        if (other.gameObject.name == "Player")
        {
            Take(Player.Instance);

        }
    }

}
