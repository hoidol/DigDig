using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AbilityInventory : MonoBehaviour
{
    public List<Ability> equippedAbilitys = new List<Ability>();
    public List<SynergyData> currentSynergyDatas = new List<SynergyData>();
    //public readonly int MAX_SKILL_COUNT = 3;

    public List<IPreAttack> preAttackItems = new List<IPreAttack>();
    public List<IAttackItem> attackItems = new List<IAttackItem>();
    public List<IComboAttackItem> comboAttackItems = new List<IComboAttackItem>();
    public List<IBulletItem> bulletItems = new List<IBulletItem>();

    public int abilityCount
    {
        get; set;
    }
    void Start()
    {
#if UNITY_EDITOR
        GameEventBus.Subscribe<StartGameEvent>(OnStartGame);
#endif
    }
#if UNITY_EDITOR
    void OnStartGame(StartGameEvent e)
    {
        currentSynergyDatas.Clear();


    }
#endif


    public void SortingAbility()
    {
        equippedAbilitys = equippedAbilitys.OrderBy(e => e.abilityData.applyOrder).ToList();
        RefreshCache();
    }

    void RefreshCache()
    {
        preAttackItems = equippedAbilitys.OfType<IPreAttack>().ToList();
        attackItems = equippedAbilitys.OfType<IAttackItem>().ToList();
        comboAttackItems = equippedAbilitys.OfType<IComboAttackItem>().ToList();
        bulletItems = equippedAbilitys.OfType<IBulletItem>().ToList();
    }

    public bool HasAbility(string key)
    {
        return equippedAbilitys.Any(s => s.key == key);
    }

    public Ability GetAbility(string key)
    {
        return equippedAbilitys.FirstOrDefault(s => s.key == key);
    }

    public void AddAbility(string key)
    {
        AddAbility(AbilityData.GetAbilityData(key));
    }

    public void AddAbility(AbilityData abilityData)
    {
        Ability ability = equippedAbilitys.FirstOrDefault(e => e.key == abilityData.key);
        if (ability != null)
        {
            ability.LevelUp();
            ability.UpdateEnhancement();
        }
        else
        {
            ability = Instantiate(abilityData.abilityPrefab, transform);
            ability.key = abilityData.key;
            ability.count = 1;
            ability.OnEquip(Player.Instance);
            equippedAbilitys.Add(ability);
        }
        abilityCount++;
        GameEventBus.Publish(new AddedAbilityEvent(abilityData));
        SortingAbility();
        CheckSynergy();
        Player.Instance.UpdatePlayer();
    }
    void CheckSynergy()
    {
        for (int i = 0; i < AbilityManager.Instance.synergyDatas.Length; i++)
        {
            SynergyData synergyData = currentSynergyDatas.FirstOrDefault(e => e.synergyType == AbilityManager.Instance.synergyDatas[i].synergyType);
            if (synergyData != null)
                continue;
            bool canPick = AbilityManager.Instance.synergyDatas[i].CanPickSynergyAbility();
            if (!canPick)
                continue;

            currentSynergyDatas.Add(synergyData); // new 시너지
        }
    }

}


public class AddedAbilityEvent
{
    public AbilityData abilityData;
    public AddedAbilityEvent(AbilityData aData)
    {
        this.abilityData = aData;
    }
}