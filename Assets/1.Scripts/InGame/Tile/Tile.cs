using UnityEngine;

public class Tile : MonoBehaviour
{
    public ITileObject tileObject;

    public bool IsEmpty => tileObject == null;

    public void SetTileObject(ITileObject tileObject)
    {
        this.tileObject = tileObject;
        if (tileObject != null)
            tileObject.Transform.position = transform.position;
    }

    public void Clear()
    {
        tileObject = null;
    }
}
