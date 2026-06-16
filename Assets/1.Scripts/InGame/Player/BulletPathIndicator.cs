using System.Collections.Generic;
using UnityEngine;

public class BulletPathIndicator : MonoBehaviour
{
    public float pathLength = 10;
    public LineRenderer lineRenderer;
Player player;
public LayerMask layerMask;
    void Awake()
    {
        
    }
    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        player =GetComponent<Player>();
    }
    List<Vector2> paths = new List<Vector2>();
    void Update()
    {
        paths.Clear();
        if(player.attackJoystick.Direction.magnitude <= 0.1f)
        {
            lineRenderer.positionCount = 0;
            return;
        }

        float length = pathLength;
        Vector2 point = player.weapon.attackPoint.position;
        Vector2 direction =player.attackJoystick.Direction; 
        paths.Add(point);
        while(length > 0)
        {
            RaycastHit2D hit = Physics2D.Raycast(point, direction,length,layerMask);
            if (hit)
            {
                float distance = Vector2.Distance(point, hit.point);
                if (distance < 0.001f) break;
                paths.Add(hit.point);
                point = hit.point;
                length -= distance;
                direction = Vector2.Reflect(direction, hit.normal);
            }
            else
            {
                paths.Add(point + direction * length);

                length = 0;
            }
            
        }

        lineRenderer.positionCount = paths.Count;
        for(int i =0;i<paths.Count; i++)
        {
            lineRenderer.SetPosition(i, paths[i]);
        }


     
        

    }
}
