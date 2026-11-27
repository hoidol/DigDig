using System.Collections.Generic;
using UnityEngine;

public class RuneListContainer : MonoBehaviour
{
    public RuneNotifyEffectPanel[] runeNotifyEffectPanels;
    void Start()
    {
        for (int i = 0; i < runeNotifyEffectPanels.Length; i++)
        {
            runeNotifyEffectPanels[i].gameObject.SetActive(false);
        }
        GameEventBus.Subscribe<AddedRuneEvent>(AddedRuneEvent);
        GameEventBus.Subscribe<StartGameEvent>(OnStartGameEvent);
    }
    void OnStartGameEvent(StartGameEvent e)
    {

        UpdateContainer();
    }
    void AddedRuneEvent(AddedRuneEvent e)
    {
        UpdateContainer();
    }

    void UpdateContainer()
    {
        List<Rune> runes = Slime.Instance.runeInventory.curRunes;
        for (int i = 0; i < runeNotifyEffectPanels.Length; i++)
        {
            if (i < runes.Count)
            {

                runeNotifyEffectPanels[i].SetRune(runes[i]);
                runeNotifyEffectPanels[i].gameObject.SetActive(true);
            }
            else
            {
                runeNotifyEffectPanels[i].gameObject.SetActive(false);
            }

        }
    }
}
