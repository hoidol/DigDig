using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

// 2026-11-05~06에 만든 슬라임 기능을 프리팹에 반영하는 메뉴 (Tools/Slime/Setup Slime Prefabs)
// 여러 번 실행해도 안전: 이미 있는 것은 건너뜀 (단 replace 표시된 강화 능력은 교체)
//  1) 없는 프리팹/에셋 생성: HunterSlime(ScavengerSlime 복사 후 스크립트 교체), Hunter.asset, HailStone/MortarFlag 임시 프리팹
//  2) 강화 능력(5/8/11) 컴포넌트 부착: EnhanceAbilities/{level}Ability
//  3) 비어 있는 참조 연결: Hail.hailPrefab, Mortar.flagPrefab, Shield.orbitMachine, Dopamine/Berserker 감지 범위, GrowUp.growBar
// 임시 프리팹(HailStone, MortarFlag, GrowBar)은 기본 스프라이트로 만든 자리표시. 그림은 나중에 교체
public static class SlimePrefabSetup
{
    const string PREFAB_ROOT = "Assets/3.Prefabs/Slime";
    const string HAIL_STONE_PATH = PREFAB_ROOT + "/A/HailStone.prefab";
    const string MORTAR_FLAG_PATH = PREFAB_ROOT + "/A/MortarFlag.prefab";
    const string SHIELD_ORB_PATH = "Assets/3.Prefabs/Item/Level2_Old/Prefabs/ShieldOrb.prefab";
    const string HUNTER_DATA_PATH = "Assets/AddressableResources/SlimeData/S/Hunter.asset";

    class Entry
    {
        public string prefab; // "{등급}/{프리팹 이름}"
        public int level;
        public Type type;
        public Action<SlimeEnhanceAbility> setup;
        public bool replace;  // 같은 level 능력이 있으면 지우고 교체
        public Entry(string prefab, int level, Type type, Action<SlimeEnhanceAbility> setup = null, bool replace = false)
        {
            this.prefab = prefab;
            this.level = level;
            this.type = type;
            this.setup = setup;
            this.replace = replace;
        }
    }

    static Buff Mul(StatType statType, float value) => new Buff(statType, value, StatOpType.Multiply);

    static readonly Entry[] entries =
    {
        // ── B ──
        new Entry("B/ShieldSlime", 5, typeof(ShieldEnhance5Ability)),
        new Entry("B/ShieldSlime", 8, typeof(ShieldEnhance8Ability)),
        new Entry("B/ShieldSlime", 11, typeof(ShieldEnhance11Ability)),

        new Entry("B/DopamineSlime", 5, typeof(DopamineEnhance5Ability)),
        new Entry("B/DopamineSlime", 8, typeof(DopamineEnhance8Ability)),
        new Entry("B/DopamineSlime", 11, typeof(DopamineEnhance11Ability)),

        new Entry("B/MineSlime", 5, typeof(MineEnhance5Ability)),
        new Entry("B/MineSlime", 8, typeof(MineEnhance8Ability)),
        new Entry("B/MineSlime", 11, typeof(MineEnhance11Ability)),

        // Breeze: 공속 버프 +5%p / 포위 진형 / 버프 대상 공격력 5%
        new Entry("B/BreezeSlime", 5, typeof(CrossBuffValueEnhanceAbility), a => ((CrossBuffValueEnhanceAbility)a).addValue = 0.05f),
        new Entry("B/BreezeSlime", 8, typeof(CrossBuffSurroundEnhanceAbility)),
        new Entry("B/BreezeSlime", 11, typeof(CrossBuffExtraBuffEnhanceAbility),
            a => ((CrossBuffExtraBuffEnhanceAbility)a).buffs = new[] { Mul(StatType.AttackPower, 1.05f) }),

        // Rage: 공격력 버프 +5%p / 포위 진형 / 버프 대상 공격속도 5%
        new Entry("B/RageSlime", 5, typeof(CrossBuffValueEnhanceAbility), a => ((CrossBuffValueEnhanceAbility)a).addValue = 0.05f),
        new Entry("B/RageSlime", 8, typeof(CrossBuffSurroundEnhanceAbility)),
        new Entry("B/RageSlime", 11, typeof(CrossBuffExtraBuffEnhanceAbility),
            a => ((CrossBuffExtraBuffEnhanceAbility)a).buffs = new[] { Mul(StatType.AttackSpeed, 1.05f) }),

        // ── A ──
        // Keen: 치명타 확률 버프 +5(Add) / 포위 진형 / 11 미정
        new Entry("A/KeenSlime", 5, typeof(CrossBuffValueEnhanceAbility), a => ((CrossBuffValueEnhanceAbility)a).addValue = 5f),
        new Entry("A/KeenSlime", 8, typeof(CrossBuffSurroundEnhanceAbility)),

        // Beam: 5 미정 / 8 레이저 감속 / 11 레이저 수 피해 감소 완화
        new Entry("A/BeamSlime", 8, typeof(BeamEnhance8Ability)),
        new Entry("A/BeamSlime", 11, typeof(BeamEnhance11Ability)),

        // Hail: 5 미정 / 8 눈 2개 / 11 이동 속도 15%
        new Entry("A/HailSlime", 8, typeof(HailEnhance8Ability)),
        new Entry("A/HailSlime", 11, typeof(HailEnhance11Ability)),

        // ── S ──
        new Entry("S/HunterSlime", 5, typeof(HunterEnhance5Ability)),
        new Entry("S/HunterSlime", 8, typeof(HunterEnhance8Ability)),
        new Entry("S/HunterSlime", 11, typeof(HunterEnhance11Ability)),

        // Sacrifice: 결과 공격력 10% / 30% 확률 레벨 +1 / 결과 공격속도 10%
        new Entry("S/SacrificeSlime", 5, typeof(ResultBuffEnhanceAbility),
            a => ((ResultBuffEnhanceAbility)a).buffs = new[] { Mul(StatType.AttackPower, 1.1f) }),
        new Entry("S/SacrificeSlime", 8, typeof(SacrificeEnhance8Ability)),
        new Entry("S/SacrificeSlime", 11, typeof(ResultBuffEnhanceAbility),
            a => ((ResultBuffEnhanceAbility)a).buffs = new[] { Mul(StatType.AttackSpeed, 1.1f) }),

        // GrowUp: 성장 시간 22초 / 50% 확률 레벨 +1 / 18초
        new Entry("S/GrowUpSlime", 5, typeof(GrowUpTimeEnhanceAbility), a => ((GrowUpTimeEnhanceAbility)a).growTime = 22f),
        new Entry("S/GrowUpSlime", 8, typeof(GrowUpEnhance8Ability)),
        new Entry("S/GrowUpSlime", 11, typeof(GrowUpTimeEnhanceAbility), a => ((GrowUpTimeEnhanceAbility)a).growTime = 18f),

        new Entry("S/ScavengerSlime", 5, typeof(ScavengerEnhance5Ability)),
        new Entry("S/ScavengerSlime", 8, typeof(ScavengerEnhance8Ability)),
        new Entry("S/ScavengerSlime", 11, typeof(ScavengerEnhance11Ability)),

        // Joker: 변신 실패 확률 25 / 20 / 15% (5에 있던 공용 스탯 능력은 교체)
        new Entry("S/JokerSlime", 5, typeof(JokerLevelDownEnhanceAbility), a => ((JokerLevelDownEnhanceAbility)a).levelDownChance = 0.25f, replace: true),
        new Entry("S/JokerSlime", 8, typeof(JokerLevelDownEnhanceAbility), a => ((JokerLevelDownEnhanceAbility)a).levelDownChance = 0.2f),
        new Entry("S/JokerSlime", 11, typeof(JokerLevelDownEnhanceAbility), a => ((JokerLevelDownEnhanceAbility)a).levelDownChance = 0.15f),
    };

    [MenuItem("Tools/Slime/Setup Slime Prefabs")]
    static void Setup()
    {
        CreateHunterPrefab();
        CreateHailStonePrefab();
        CreateMortarFlagPrefab();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        int added = 0;
        foreach (var group in entries.GroupBy(e => e.prefab))
            added += ModifyPrefab(group.Key, root => AddAbilities(root, group.Key, group));

        ModifyPrefab("A/HailSlime", LinkHail);
        ModifyPrefab("A/MortarSlime", LinkMortar);
        ModifyPrefab("B/ShieldSlime", LinkShield);
        ModifyPrefab("B/DopamineSlime", root => LinkDetectRange<DopamineSlime>(root, s => s.dopamineRange, (s, t) => s.dopamineRange = t));
        ModifyPrefab("A/BerserkerSlime", root => LinkDetectRange<BerserkerSlime>(root, s => s.berserkerRange, (s, t) => s.berserkerRange = t));
        ModifyPrefab("S/GrowUpSlime", LinkGrowBar);

        CreateHunterData();
        AssetDatabase.SaveAssets();
        Debug.Log($"[SlimePrefabSetup] 완료. 강화 능력 {added}개 추가");
    }

    // ── 공통 ──────────────────────────────────────────────

    // 프리팹을 열어 수정. modify가 바꾼 게 있으면(>0 또는 true) 저장. 반환 = modify 결과(정수)
    static int ModifyPrefab(string prefab, Func<GameObject, int> modify)
    {
        string path = $"{PREFAB_ROOT}/{prefab}.prefab";
        if (AssetDatabase.LoadAssetAtPath<GameObject>(path) == null)
        {
            Debug.LogWarning($"[SlimePrefabSetup] 프리팹 없음: {path}");
            return 0;
        }

        GameObject root = PrefabUtility.LoadPrefabContents(path);
        try
        {
            int changed = modify(root);
            if (changed > 0)
                PrefabUtility.SaveAsPrefabAsset(root, path);
            return changed;
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    static int ModifyPrefab(string prefab, Func<GameObject, bool> modify) => ModifyPrefab(prefab, root => modify(root) ? 1 : 0);

    // 슬라임 사거리 표시(rangeTr)의 스프라이트 (원). 임시 프리팹 재료
    static SpriteRenderer FindRangeRenderer()
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PREFAB_ROOT}/C/IceSlime.prefab");
        Slime slime = prefab != null ? prefab.GetComponent<Slime>() : null;
        return slime != null && slime.rangeTr != null ? slime.rangeTr.GetComponentInChildren<SpriteRenderer>(true) : null;
    }

    static Sprite WhiteSprite => AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");

    // 스프라이트가 월드 크기 size(가로·세로)가 되도록 하는 scale
    static Vector3 ScaleToSize(Sprite sprite, float width, float height)
    {
        Vector2 b = sprite != null ? (Vector2)sprite.bounds.size : Vector2.one;
        return new Vector3(width / Mathf.Max(b.x, 0.0001f), height / Mathf.Max(b.y, 0.0001f), 1f);
    }

    // ── 강화 능력 ──────────────────────────────────────────

    static int AddAbilities(GameObject root, string prefab, IEnumerable<Entry> group)
    {
        Transform container = root.transform.Find("EnhanceAbilities");
        if (container == null)
        {
            container = new GameObject("EnhanceAbilities").transform;
            container.SetParent(root.transform, false);
        }

        int added = 0;
        foreach (Entry e in group)
        {
            SlimeEnhanceAbility[] existing = root.GetComponentsInChildren<SlimeEnhanceAbility>(true);
            SlimeEnhanceAbility same = existing.FirstOrDefault(a => a.level == e.level);
            if (same != null)
            {
                if (!e.replace || same.GetType() == e.type)
                {
                    Debug.Log($"[SlimePrefabSetup] {prefab} lv{e.level} 이미 있음 ({same.GetType().Name}) → 건너뜀");
                    continue;
                }
                Debug.Log($"[SlimePrefabSetup] {prefab} lv{e.level} {same.GetType().Name} → {e.type.Name} 교체");
                GameObject old = same.gameObject;
                UnityEngine.Object.DestroyImmediate(same, true);
                if (old.GetComponents<Component>().Length == 1) // Transform만 남으면 오브젝트도 삭제
                    UnityEngine.Object.DestroyImmediate(old, true);
            }

            GameObject go = new GameObject($"{e.level}Ability");
            go.transform.SetParent(container, false);
            SlimeEnhanceAbility ability = (SlimeEnhanceAbility)go.AddComponent(e.type);
            ability.level = e.level;
            e.setup?.Invoke(ability);
            added++;
        }
        return added;
    }

    // ── 새 프리팹 / 에셋 ───────────────────────────────────

    // HunterSlime: ScavengerSlime 프리팹 구조(Root, StarContainer, 사거리 등)를 복사하고 슬라임 스크립트만 교체
    static void CreateHunterPrefab()
    {
        string path = $"{PREFAB_ROOT}/S/HunterSlime.prefab";
        if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null)
            return;

        string srcPath = $"{PREFAB_ROOT}/S/ScavengerSlime.prefab";
        GameObject root = PrefabUtility.LoadPrefabContents(srcPath);
        try
        {
            ScavengerSlime old = root.GetComponent<ScavengerSlime>();
            HunterSlime hunter = root.AddComponent<HunterSlime>();
            EditorJsonUtility.FromJsonOverwrite(EditorJsonUtility.ToJson(old), hunter); // 공통 필드(rangeTr, rootTr 등) 복사
            hunter.key = "Hunter";
            UnityEngine.Object.DestroyImmediate(old, true);

            // 시체수집 고유 강화 능력(5/8/11)이 있으면 제거 (공용 2/14는 유지)
            foreach (SlimeEnhanceAbility a in root.GetComponentsInChildren<SlimeEnhanceAbility>(true))
            {
                if (a.level == 5 || a.level == 8 || a.level == 11)
                    UnityEngine.Object.DestroyImmediate(a.gameObject, true);
            }

            root.name = "HunterSlime";
            PrefabUtility.SaveAsPrefabAsset(root, path);
            Debug.Log($"[SlimePrefabSetup] {path} 생성 (ScavengerSlime 복사)");
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    // Hunter.asset: 없으면 만들고 LoadData로 CSV·프리팹 연결 (S 폴더는 Addressable SlimeData 라벨 폴더라 자동 포함)
    static void CreateHunterData()
    {
        HunterSlimeData data = AssetDatabase.LoadAssetAtPath<HunterSlimeData>(HUNTER_DATA_PATH);
        if (data == null)
        {
            data = ScriptableObject.CreateInstance<HunterSlimeData>();
            data.key = "Hunter";
            AssetDatabase.CreateAsset(data, HUNTER_DATA_PATH);
            Debug.Log($"[SlimePrefabSetup] {HUNTER_DATA_PATH} 생성");
        }
        data.LoadData();
        EditorUtility.SetDirty(data);
    }

    // 빙석 눈: 원 스프라이트 + 트리거 콜라이더 + Kinematic 리지드바디 + HailStone
    static void CreateHailStonePrefab()
    {
        if (AssetDatabase.LoadAssetAtPath<GameObject>(HAIL_STONE_PATH) != null)
            return;

        GameObject go = new GameObject("HailStone");
        try
        {
            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            SpriteRenderer rangeRenderer = FindRangeRenderer();
            sr.sprite = rangeRenderer != null ? rangeRenderer.sprite : WhiteSprite;
            go.transform.localScale = ScaleToSize(sr.sprite, 0.4f, 0.4f); // 지름 0.4
            if (rangeRenderer != null)
                sr.sortingLayerID = rangeRenderer.sortingLayerID;
            sr.sortingOrder = 50;
            sr.color = new Color(0.7f, 0.9f, 1f, 1f);

            CircleCollider2D col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            go.AddComponent<HailStone>();

            PrefabUtility.SaveAsPrefabAsset(go, HAIL_STONE_PATH);
            Debug.Log($"[SlimePrefabSetup] {HAIL_STONE_PATH} 생성 (임시 그림)");
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(go);
        }
    }

    // 박격포 깃발: 깃발(작은 빨간 사각) + 폭발 범위 원(슬라임 사거리 표시를 복사, scale = 반지름 규칙 그대로)
    static void CreateMortarFlagPrefab()
    {
        if (AssetDatabase.LoadAssetAtPath<GameObject>(MORTAR_FLAG_PATH) != null)
            return;

        GameObject go = new GameObject("MortarFlag");
        try
        {
            GameObject flagSprite = new GameObject("Flag");
            flagSprite.transform.SetParent(go.transform, false);
            flagSprite.transform.localPosition = new Vector3(0f, 0.2f, 0f);
            SpriteRenderer sr = flagSprite.AddComponent<SpriteRenderer>();
            sr.sprite = WhiteSprite;
            flagSprite.transform.localScale = ScaleToSize(sr.sprite, 0.25f, 0.4f);
            sr.color = new Color(1f, 0.25f, 0.2f, 1f);
            sr.sortingOrder = 60;

            MortarFlag flag = go.AddComponent<MortarFlag>();
            GameObject rangePrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PREFAB_ROOT}/C/IceSlime.prefab");
            Slime slime = rangePrefab != null ? rangePrefab.GetComponent<Slime>() : null;
            if (slime != null && slime.rangeTr != null)
            {
                GameObject range = UnityEngine.Object.Instantiate(slime.rangeTr.gameObject, go.transform);
                range.name = "BoomRange";
                range.transform.localPosition = Vector3.zero;
                range.SetActive(false);
                foreach (SpriteRenderer r in range.GetComponentsInChildren<SpriteRenderer>(true))
                    r.color = new Color(1f, 0.4f, 0.2f, r.color.a);
                flag.rangeTr = range.transform;
            }

            if (SpriteRendererSortingFrom(slime) is SpriteRenderer src)
                sr.sortingLayerID = src.sortingLayerID;

            PrefabUtility.SaveAsPrefabAsset(go, MORTAR_FLAG_PATH);
            Debug.Log($"[SlimePrefabSetup] {MORTAR_FLAG_PATH} 생성 (임시 그림)");
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(go);
        }
    }

    static SpriteRenderer SpriteRendererSortingFrom(Slime slime) =>
        slime != null && slime.rangeTr != null ? slime.rangeTr.GetComponentInChildren<SpriteRenderer>(true) : null;

    // ── 참조 연결 ──────────────────────────────────────────

    static bool LinkHail(GameObject root)
    {
        HailSlime hail = root.GetComponent<HailSlime>();
        if (hail == null || hail.hailPrefab != null)
            return false;
        hail.hailPrefab = AssetDatabase.LoadAssetAtPath<HailStone>(HAIL_STONE_PATH);
        return hail.hailPrefab != null;
    }

    static bool LinkMortar(GameObject root)
    {
        MortarSlime mortar = root.GetComponent<MortarSlime>();
        if (mortar == null || mortar.flagPrefab != null)
            return false;
        mortar.flagPrefab = AssetDatabase.LoadAssetAtPath<MortarFlag>(MORTAR_FLAG_PATH);
        return mortar.flagPrefab != null;
    }

    // 철벽: 자식 "OrbitMachine"(SeparateOrbitMachine) + 방패 오브 프리팹(ShieldOrb, ShieldOrbitOrb)
    static bool LinkShield(GameObject root)
    {
        ShieldSlime shield = root.GetComponent<ShieldSlime>();
        if (shield == null || shield.orbitMachine != null)
            return false;

        SeparateOrbitMachine machine = root.GetComponentInChildren<SeparateOrbitMachine>(true);
        if (machine == null)
        {
            GameObject go = new GameObject("OrbitMachine");
            go.transform.SetParent(root.transform, false);
            machine = go.AddComponent<SeparateOrbitMachine>();
        }
        if (machine.orbitOrbPrefab == null)
        {
            GameObject orb = AssetDatabase.LoadAssetAtPath<GameObject>(SHIELD_ORB_PATH);
            machine.orbitOrbPrefab = orb != null ? orb.GetComponent<OrbitOrb>() : null;
            if (machine.orbitOrbPrefab == null)
                Debug.LogWarning($"[SlimePrefabSetup] 방패 오브 프리팹 없음: {SHIELD_ORB_PATH} → ShieldSlime/OrbitMachine.orbitOrbPrefab 직접 연결 필요");
        }
        shield.orbitMachine = machine;
        return true;
    }

    // 광란/버서커 감지 범위 표시: 사거리 표시(rangeTr)를 복사해 색만 다르게
    static bool LinkDetectRange<T>(GameObject root, Func<T, Transform> get, Action<T, Transform> set) where T : Slime
    {
        T slime = root.GetComponent<T>();
        if (slime == null || get(slime) != null || slime.rangeTr == null)
            return false;

        GameObject range = UnityEngine.Object.Instantiate(slime.rangeTr.gameObject, slime.rangeTr.parent);
        range.name = "DetectRange";
        range.SetActive(false);
        foreach (SpriteRenderer r in range.GetComponentsInChildren<SpriteRenderer>(true))
            r.color = new Color(1f, 0.85f, 0.2f, r.color.a);
        set(slime, range.transform);
        return true;
    }

    // 성장 진행도 막대: 왼쪽 기준으로 늘어나도록 부모(growBar) 위치를 왼쪽 끝에 두고 채움은 오른쪽으로
    static bool LinkGrowBar(GameObject root)
    {
        GrowUpSlime growUp = root.GetComponent<GrowUpSlime>();
        if (growUp == null || growUp.growBar != null)
            return false;

        const float width = 0.8f;
        GameObject bar = new GameObject("GrowBar");
        bar.transform.SetParent(root.transform, false);
        bar.transform.localPosition = new Vector3(-width * 0.5f, -0.6f, 0f);

        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(bar.transform, false);
        fill.transform.localPosition = new Vector3(width * 0.5f, 0f, 0f);
        SpriteRenderer sr = fill.AddComponent<SpriteRenderer>();
        sr.sprite = WhiteSprite;
        fill.transform.localScale = ScaleToSize(sr.sprite, width, 0.08f);
        sr.color = new Color(0.4f, 1f, 0.4f, 1f);
        sr.sortingOrder = 60;

        growUp.growBar = bar.transform;
        return true;
    }
}
