using UnityEngine;
using TMPro;
//현재 얼마나 있는지
public class MemoryFragmentPanel : MonoBehaviour 
{
    public TMP_Text memoryFragmentText;
    void Start()
    {
        GameEventBus.Subscribe<ChangedMemoryFragmentEvent>(OnChangedMemoryFragmentEvent);
        UpdatePanel();
    }

    void OnChangedMemoryFragmentEvent(ChangedMemoryFragmentEvent e)
    {
        UpdatePanel();
    }
    void UpdatePanel()
    {
        memoryFragmentText.text = UserManager.Instance.userData.memoryFragmentCount.ToString();
    }
}