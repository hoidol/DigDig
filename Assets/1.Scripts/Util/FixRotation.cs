using UnityEngine;

public class FixRotation : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        transform.localRotation = Quaternion.identity;
    }
}
