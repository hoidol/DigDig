using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    //[SerializeField] Piece picePrefab;

    [SerializeField] Transform startTr;
    [SerializeField] Transform endTr;

    PoolingSystem pooingSystem;
    private void Start()
    {
        pooingSystem = GetComponentInChildren<PoolingSystem>();
        if (pooingSystem == null)
        {
            pooingSystem = gameObject.AddComponent<PoolingSystem>();
            //pooingSystem.SetObject(picePrefab);
        }
            
    }
    public Transform parent;
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            Debug.Log("Test Udpate S");
            for(int i =0;i< 10; i++)
            {
                BezierSprite piece = pooingSystem.GetObject<BezierSprite>();
                piece.transform.SetParent(parent);
                piece.gameObject.SetActive(true);

                Bezier.Instance.MoveTo(piece.transform, startTr.position, endTr.position, () =>
                {
                    piece.gameObject.SetActive(false);
                    Debug.Log("되는겨 ");
                });
            }
            
        }
    }

}
