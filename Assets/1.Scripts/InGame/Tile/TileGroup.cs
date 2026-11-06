using UnityEngine;

public class TileGroup : MonoBehaviour
{
    public Tile[] tiles;
    public Transform[] enemyRoots;
    void Awake()
    {
        tiles = GetComponentsInChildren<Tile>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
