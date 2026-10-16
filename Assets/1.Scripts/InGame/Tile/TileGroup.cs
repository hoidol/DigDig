using UnityEngine;

public class TileGroup : MonoBehaviour
{
    public Tile[] tiles;
    void Awake()
    {
        tiles = GetComponentsInChildren<Tile>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
