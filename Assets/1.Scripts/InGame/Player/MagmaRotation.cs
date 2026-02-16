using UnityEngine;

public class MagmaRotation : MonoBehaviour
{
    public float rotationSpeed = 10f;
    public void SetRotationSpeed(float speed)
    {
        rotationSpeed = speed;
    }
    // Update is called once per frame
    void Update()
    {
        transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
    }
}
