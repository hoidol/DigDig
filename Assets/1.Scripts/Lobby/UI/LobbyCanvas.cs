
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyCanvas : CanvasUI<LobbyCanvas>
{
    public LobbyStateButton[] lobbyStateButtons;
    void Awake()
    {
        lobbyStateButtons = GetComponentsInChildren<LobbyStateButton>();
    }
    public void UpdateCanvas()
    {
        foreach (var button in lobbyStateButtons)
        {
            button.UpdateButton();
        }
    }
}