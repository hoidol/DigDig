using UnityEngine;

public class User : MonoSingleton<User>
{
    public string stageKey;
    void Start()
    {
        Application.targetFrameRate = 60;
    }
}
