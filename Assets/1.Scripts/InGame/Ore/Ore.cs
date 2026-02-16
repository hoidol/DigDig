
using UnityEngine;
using DG.Tweening;

public class Ore : MonoBehaviour, IPickable
{
    public string Key
    {
        get { return key; }
    }
    public string key;
    public bool IsTaken
    {
        get;
        set;
    }

    public int exp = 1;// 임시 경험치량
    public void Droped(Vector2 pos, string key)
    {
        transform.position = pos;
        this.key = key;
        IsTaken = false;
    }

    public void Take(IPicker picker)
    {
        IsTaken = true;
        transform.DOMove(picker.Transform.position, 0.3f).OnComplete(() =>
        {
            picker.PickUp(this);
            //Player.Instance.PickUp(this); //나중에 고치기 - 플레이어 획득하는게 아니게, 범용적으로 

        });
    }

    public void PickedUp()
    {
        gameObject.SetActive(false);
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
