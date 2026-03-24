using UnityEngine;

public class LobbyManager : MonoBehaviour
{
    public void Start()
    {
        GameEventBus.Clear();
    }
}