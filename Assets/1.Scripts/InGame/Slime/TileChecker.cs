using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class TileChecker : MonoBehaviour
{
    BoxCollider2D boxCollider2D;

    void Awake()
    {
        boxCollider2D = GetComponent<BoxCollider2D>();
        boxCollider2D.enabled = false;
    }
    public int tileCount;
    public ITile tile; //TileCount 시도 이후 획득한 랜덤한 타일
    public int TileCount()
    {
        var hits = Physics2D.OverlapBoxAll(
            boxCollider2D.bounds.center,
            boxCollider2D.bounds.size,
            0f
        );
        tileCount = 0;
        tile = null;
        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<ITile>(out ITile t))
            {
                tileCount++;
                if (hit.TryGetComponent<Enemy>(out Enemy e))
                {
                    if (tile == null)
                        tile = t;
                    else
                    {
                        if (Random.Range(0f, 100f) < 30)
                        {
                            tile = t;
                        }
                    }

                }

            }
        }




        return tileCount;
    }

}
