using UnityEngine;

public class Tile : MonoBehaviour
{
    public Slime slime;

    public bool IsEmpty => slime == null;

    public void SetSlime(Slime s)
    {
        slime = s;
        if (slime != null)
            slime.transform.position = transform.position;
    }

    public void Clear()
    {
        slime = null;
    }
}
