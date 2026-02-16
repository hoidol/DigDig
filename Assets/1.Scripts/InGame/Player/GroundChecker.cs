using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GroundChecker : MonoBehaviour
{
    //public Vector2 size;
    //public Player player;
    //public LayerMask ground;
    //private void Start()
    //{
    //    player = GetComponentInParent<Player>();
    //    size = GetComponent<BoxCollider2D>().size;
    //    StartCoroutine(CoGroundCheck());
    //}

    //IEnumerator CoGroundCheck()
    //{
    //    WaitForSeconds wait = new WaitForSeconds(0.15f);
    //    while (true)
    //    {
    //        Collider2D col = Physics2D.OverlapBox(transform.position, size, 0, ground);
    //        player.onGround = col != null;
    //        yield return wait;
    //    }
    //}
}
