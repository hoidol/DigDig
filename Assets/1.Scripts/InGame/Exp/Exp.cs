using DG.Tweening;
using UnityEngine;

public class Exp : MonoBehaviour, IPickable
{
    public string Key => "Exp";


    public bool IsTaken { get; set; }
    public Transform Transform => transform;

    public void Droped(Vector2 pos)
    {
        transform.position = pos;
        IsTaken = false;
    }

    public void PickedUp()
    {
        gameObject.SetActive(false);
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
        if (collision.CompareTag("Player"))
        {

        }
    }
}
