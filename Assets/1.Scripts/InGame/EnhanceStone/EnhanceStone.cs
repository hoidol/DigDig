using UnityEngine;

public class EnhanceStone : MonoBehaviour
{
    public static EnhanceStone Instantiate()
    {
        EnhanceStone prefab = Resources.Load<EnhanceStone>("Prefabs/EnhanceStone");
        return GameObject.Instantiate(prefab);
    }
    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            EnhanceStoneCanvas.Instance.OpenCanvas();
        }
    }
}