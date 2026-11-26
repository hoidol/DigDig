using UnityEngine.Serialization;
using UnityEngine;
using System.Collections.Generic;

public class DroppedRune : MonoBehaviour
{
    static readonly Stack<DroppedRune> pool = new();
    static DroppedRune prefab;

    public static DroppedRune Instantiate()
    {
        if (pool.Count > 0)
        {
            DroppedRune rune = pool.Pop();
            rune.gameObject.SetActive(true);
            return rune;
        }
        if (prefab == null)
            prefab = Resources.Load<DroppedRune>("UI/DroppedItem");
        return Instantiate(prefab);
    }

    public void Release()
    {
        gameObject.SetActive(false);
        pool.Push(this);
    }

    [FormerlySerializedAs("itemData")] public RuneData runeData;
    public void Drop(RuneData iData)
    {
        runeData = iData;
        // GetComponentInChildren<SpriteRenderer>().sprite = itemData.thumbnail;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // PickUpItemCanvas.Instance.OpenCanvas(new NearDropItemEvent(itemData), (r) =>
            // {

            // });
        }
    }

    public void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // if (PickUpItemCanvas.Instance.curItemData == itemData)
            // {
            //     PickUpItemCanvas.Instance.CloseCanvas(itemData.key);
            // }
        }
    }

}
