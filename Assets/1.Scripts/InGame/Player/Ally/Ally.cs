using UnityEngine;

public class Ally : MonoBehaviour
{
    public int level;
    public virtual void Spawn(Vector2 pos, int lv)
    {
        transform.position = pos;
        level = lv;
    }

}