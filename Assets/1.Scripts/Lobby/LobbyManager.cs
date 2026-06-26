using System.Linq;
using UnityEngine;

public class LobbyManager : MonoSingleton<LobbyManager>
{    

    void Awake()
    {
        GameEventBus.Clear();
        
        
    }
    private void Start() 
    {
        
    }
   
}

