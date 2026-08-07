using System.Collections.Generic;
using UnityEngine;

public class BulletPathIndicator : MonoBehaviour
{
    public float pathLength = 10;
    public LineRenderer lineRenderer;

    public LayerMask layerMask;
    [SerializeField] List<Vector2> paths = new List<Vector2>();
    void Update()
    {
        paths.Clear();

        float length = pathLength;
        Vector2 point = Character.Instance.weapon.attackPoint.position;
        Vector2 direction = Character.Instance.weapon.dirTr.up;
        paths.Add(point);
        while (length > 0)
        {
            RaycastHit2D hit = Physics2D.Raycast(point, direction, length, layerMask);
            if (hit)
            {
                float distance = Vector2.Distance(point, hit.point);
                Debug.Log($"부딪힘 {hit.collider.gameObject.name}");
                if (distance < 0.001f) break;

                paths.Add(hit.point);
                length -= distance;
                direction = Vector2.Reflect(direction, hit.normal);
                point = hit.point + hit.normal * 0.05f;

            }
            else
            {
                paths.Add(point + direction * length);
                length = 0;
            }
        }

        lineRenderer.positionCount = paths.Count;
        for (int i = 0; i < paths.Count; i++)
        {
            lineRenderer.SetPosition(i, paths[i]);
        }





    }
}
