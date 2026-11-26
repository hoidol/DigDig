using UnityEngine;

public class Ally : MonoBehaviour
{
    public int level;
    public virtual void Spawn(Vector2 pos)
    {
        transform.position = pos;
    }

    public virtual void SetLevel(int lv)
    {
        level = lv;
    }

}