using UnityEngine;
public class Util
{
    //public static LayerMask materialLayerMask = LayerMask.GetMask("Material");
    public static LayerMask pickableLayerMask = LayerMask.GetMask("Pickable");
    public static LayerMask pickUpableLayerMask = LayerMask.GetMask("PickUpable");
    public static int pickUpableLayer = LayerMask.NameToLayer("PickUpable");
    public static int PickableLayer = LayerMask.NameToLayer("Pickable");

}

