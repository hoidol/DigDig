using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class Inventory : MonoBehaviour
{
    public List<InventoryData> inventoryDatas = new List<InventoryData>();

    public void AddItem(string key, int count = 1)
    {
        InventoryData data = inventoryDatas.Where(x => x.key == key).FirstOrDefault() as InventoryData;
        if (data == null)
        {
            data = new InventoryData();
            inventoryDatas.Add(data);
        }
        data.key = key;
        data.count += count;
    }

    public string TakeOut()
    {
        if (inventoryDatas.Count <= 0)
            return null;
        string key = inventoryDatas[0].key;
        inventoryDatas[0].count--;
        if (inventoryDatas[0].count == 0)
        {
            inventoryDatas.RemoveAt(0);
        }

        return key;
    }
}

[System.Serializable]
public class InventoryData
{
    public string key;
    public int count;
}