using System.Collections.Generic;
using UnityEngine;

// (2026-11-26) 5성 진화 수치를 한곳에서: Resources/MinionEvolutionSetting 에셋 인스펙터 (종마다 한 칸)
// 에셋이 없으면 코드 기본값 (EvolutionConfigs.cs 초기값). 메뉴 Tools/Data/Create MinionEvolutionSetting 으로 에셋 생성
// 여기에 칸이 있는 종만 진화 (5성이 되면 진화 선택 창). (2026-11-27) 발비(Mine)도 추가 → 지금 게임에서 쓰는 12종 모두 진화
[CreateAssetMenu(menuName = "KingSlime/MinionEvolutionSetting", fileName = "MinionEvolutionSetting")]
public class MinionEvolutionSetting : ScriptableObject
{
    public const string RESOURCE_PATH = "MinionEvolutionSetting";

    [Header("불똥이 (Flame) → 마그미")] public FlameEvolutionConfig flame = new FlameEvolutionConfig();
    [Header("뭉치 (Random) → 털뭉치")] public RandomEvolutionConfig random = new RandomEvolutionConfig();
    [Header("송송 (Ice) → 빙빙")] public IceEvolutionConfig ice = new IceEvolutionConfig();
    [Header("전디 (Lightning) → 전도니")] public LightningEvolutionConfig lightning = new LightningEvolutionConfig();
    [Header("도나 (Orbit) → 돌다다")] public OrbitEvolutionConfig orbit = new OrbitEvolutionConfig();
    [Header("기니 (Bounce) → 티기니")] public BounceEvolutionConfig bounce = new BounceEvolutionConfig();
    [Header("처럴 (Iron) → 처령")] public IronEvolutionConfig iron = new IronEvolutionConfig();
    [Header("깨모 (Pierce) → 깨두")] public PierceEvolutionConfig pierce = new PierceEvolutionConfig();
    [Header("할키 (Berserker) → 할키니")] public BerserkerEvolutionConfig berserker = new BerserkerEvolutionConfig();
    [Header("쏘나 (Mortar) → 쏘다")] public MortarEvolutionConfig mortar = new MortarEvolutionConfig();
    [Header("묵천 (Thunder) → 묵묵천천")] public ThunderEvolutionConfig thunder = new ThunderEvolutionConfig();
    [Header("발비 (Mine) → 발비마")] public MineEvolutionConfig mine = new MineEvolutionConfig();

    // 미니언 key → Config (칸이 없으면 null = 진화 없음)
    public EvolutionConfig Get(string key)
    {
        switch (key)
        {
            case "Flame": return flame;
            case "Random": return random;
            case "Ice": return ice;
            case "Lightning": return lightning;
            case "Orbit": return orbit;
            case "Bounce": return bounce;
            case "Iron": return iron;
            case "Pierce": return pierce;
            case "Berserker": return berserker;
            case "Mortar": return mortar;
            case "Thunder": return thunder;
            case "Mine": return mine;
            default: return null;
        }
    }

    public static readonly string[] EVOLVING_KEYS = { "Flame", "Random", "Ice", "Lightning", "Orbit", "Bounce", "Iron", "Pierce", "Berserker", "Mortar", "Thunder", "Mine" };

    static MinionEvolutionSetting instance;
    public static MinionEvolutionSetting Instance
    {
        get
        {
            if (instance == null)
                instance = Resources.Load<MinionEvolutionSetting>(RESOURCE_PATH);
            if (instance == null)
            {
                instance = CreateInstance<MinionEvolutionSetting>();
                instance.hideFlags = HideFlags.DontSave;
            }
            return instance;
        }
    }

#if UNITY_EDITOR
    [UnityEditor.MenuItem("Tools/Data/Create MinionEvolutionSetting")]
    static void CreateAsset()
    {
        const string path = "Assets/Resources/" + RESOURCE_PATH + ".asset";
        var existing = UnityEditor.AssetDatabase.LoadAssetAtPath<MinionEvolutionSetting>(path);
        if (existing != null)
        {
            UnityEditor.Selection.activeObject = existing;
            return;
        }
        var asset = CreateInstance<MinionEvolutionSetting>();
        asset.ice.hailPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<HailStone>("Assets/3.Prefabs/Minion/A/HailStone.prefab");
        UnityEditor.AssetDatabase.CreateAsset(asset, path);
        UnityEditor.AssetDatabase.SaveAssets();
        UnityEditor.Selection.activeObject = asset;
    }
#endif
}
