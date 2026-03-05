using UnityEngine;
using System.Collections;
public class CoreCollectingArea : MonoBehaviour
{
    Coroutine collectingCor;
    public bool isNear;
    public GameObject nearCoreCanvas;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (collectingCor != null)
            {
                StopCoroutine(collectingCor);
            }
            collectingCor = StartCoroutine(CoCollecting());
            isNear = true;
            nearCoreCanvas.SetActive(isNear);
        }
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {

            if (collectingCor != null)
            {
                StopCoroutine(collectingCor);
                collectingCor = null;
            }
            isNear = false;
            nearCoreCanvas?.SetActive(isNear);
        }
    }

    IEnumerator CoCollecting()
    {
        float timer = 0.2f;
        WaitForSeconds sec = new WaitForSeconds(1);
        while (true)
        {
            if (timer <= 0)
            {
                string oreKey = Player.Instance.inventory.TakeOut();
                if (oreKey == null)
                {
                    yield return sec;
                    continue;
                }


                // MagmaCore.Instance.CollectOre(oreKey);
                timer = 0.2f;
            }
            timer -= Time.deltaTime;
            yield return null;
        }
    }
}
