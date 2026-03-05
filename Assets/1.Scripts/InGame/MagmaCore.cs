using System.Collections;
using UnityEngine;

public class MagmaCore : MonoBehaviour//MonoSingleton<MagmaCore>, IPicker
{
    public int gold;
    CoreCollectingArea coreCollectingArea;
    public GameObject nearCoreCanvas;

    public Transform Transform => transform;
    public Transform topPointTr;
    private void Awake()
    {
        if (coreCollectingArea == null)
            coreCollectingArea = GetComponentInChildren<CoreCollectingArea>();
    }
    public void CollectOre(string oreKey)
    {
        Ore ore = OreManager.Instance.GetOre();
        ore.Droped(Player.Instance.transform.position, oreKey);
        // ore.Take(this);

        AddGold(ore.gold, true);
    }
    public void AddGold(int g, bool effect = false)
    {
        this.gold += g;
        if (effect)
        {
            GoldText goldText = GoldText.Instantiate();
            goldText.SetGoldText(topPointTr.position, g.ToString());
        }

    }


    public void PickUp(IPickable pickable)
    {
        pickable.PickedUp();
    }

}
