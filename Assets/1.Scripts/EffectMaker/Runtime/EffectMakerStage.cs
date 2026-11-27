using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace EffectMaker
{
    // 칸 하나 = 2x2 타일 + 시전자 + 주변을 도는 적 + 카메라(화면 3x3 중 한 칸).
    // 플레이 모드에서 EffectMakerController가 만들고 돌린다.
    public class EffectMakerStage
    {
        public const int CELL_COUNT = 9;
        const float STAGE_SPACING = 60f;
        static readonly Vector3 STAGE_ORIGIN = new Vector3(1000f, 1000f, 0f);
        const float TILE_SIZE = 1f;
        const float CAST_ANIM_TIME = 0.3f;

        // 정렬: 무대는 아주 낮게 → 이펙트가 항상 위에 보이게
        const int TILE_ORDER = -1000;
        const int ENEMY_ORDER = -900;
        const int CASTER_ORDER = -800;

        public const string TILE_SPRITE_PATH = "Assets/2.Sprites/BaseTile0.png";
        public const string ENEMY_SPRITE_PATH = "Assets/2.Sprites/Minion/Growth1/enemy1.png";
        public const string MINION_SPRITE_FOLDER = "Assets/2.Sprites/Minion/Growth1";
        public const string SLIME_BODY_PATH = "Assets/2.Sprites/Character/Body.png";
        public const string SLIME_EYES_PATH = "Assets/2.Sprites/Character/eyes.png";

        class RunningEffect
        {
            public GameObject go;
            public ParticleSystem[] rootParticles;
            public Transform follow;
            public Vector3 offset;
            public float age;
            public float stopTime;   // 루프 이펙트 방출을 멈출 시각 (없으면 -1)
            public float life;
            public bool stopped;
        }

        public readonly int index;
        public GameObject root;
        public Camera cam;
        public GameObject prefab;

        Vector3 center;
        Transform caster;
        Vector3 casterBaseScale;
        readonly List<Transform> enemies = new List<Transform>();
        readonly List<SpriteRenderer> enemyRenderers = new List<SpriteRenderer>();
        readonly List<float> enemyAngles = new List<float>();
        readonly List<RunningEffect> runnings = new List<RunningEffect>();
        float castAnimTimer;

        static readonly Dictionary<GameObject, (float life, float loopMaxLifetime, bool looping)> lifeCache
            = new Dictionary<GameObject, (float, float, bool)>();

        public EffectMakerStage(int index)
        {
            this.index = index;
        }

        public Rect ViewportRect
        {
            get
            {
                int col = index % 3;
                int row = index / 3;
                return new Rect(col / 3f, (2 - row) / 3f, 1f / 3f, 1f / 3f);
            }
        }

        // ---------------------------------------------------------------- 만들기 / 지우기

        public void Build(Transform parent, EffectMakerSettings s)
        {
            int col = index % 3;
            int row = index / 3;
            center = STAGE_ORIGIN + new Vector3(col * STAGE_SPACING, -row * STAGE_SPACING, 0f);

            root = NewObject($"Stage{index + 1}", parent);
            root.transform.position = center;

            // 카메라 (게임 화면을 3x3으로 나눠 한 칸씩)
            var camGo = NewObject("Camera", root.transform);
            camGo.transform.position = center + new Vector3(0f, 0f, -10f);
            cam = camGo.AddComponent<Camera>();
            cam.orthographic = true;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.nearClipPlane = 0.1f;
            cam.farClipPlane = 50f;
            cam.depth = 10 + index;
            cam.rect = ViewportRect;

            // 2x2 타일
            var tileSprite = LoadSprite(TILE_SPRITE_PATH);
            for (int i = 0; i < 4; i++)
            {
                var tile = NewSprite($"Tile{i}", root.transform, tileSprite, TILE_SIZE * 0.96f, TILE_ORDER);
                tile.transform.position = GetTilePos(i);
            }

            BuildCaster(s);

            // 적
            var enemySprite = LoadSprite(ENEMY_SPRITE_PATH);
            for (int i = 0; i < s.enemyCount; i++)
            {
                var enemy = NewSprite($"Enemy{i}", root.transform, enemySprite, 0.6f, ENEMY_ORDER);
                enemies.Add(enemy.transform);
                enemyRenderers.Add(enemy.GetComponent<SpriteRenderer>());
                enemyAngles.Add(360f / s.enemyCount * i);
            }

            ApplyView(s);
            UpdateEnemies(0f, s);
        }

        void BuildCaster(EffectMakerSettings s)
        {
            caster = NewObject("Caster", root.transform).transform;
            caster.position = GetTilePos(Mathf.Clamp(s.casterTile, 0, 3));

            if (s.casterType == CasterType.Slime)
            {
                var body = NewSprite("Body", caster, LoadSprite(SLIME_BODY_PATH), 0.9f, CASTER_ORDER);
                var eyes = NewSprite("Eyes", caster, LoadSprite(SLIME_EYES_PATH), 1f, CASTER_ORDER + 1);
                eyes.transform.localScale = body.transform.localScale; // 몸과 같은 비율
            }
            else
            {
                NewSprite("Body", caster, LoadSprite($"{MINION_SPRITE_FOLDER}/{s.minionKey}.png"), 0.85f, CASTER_ORDER);
            }
            casterBaseScale = caster.localScale;
        }

        public void Destroy()
        {
            ClearEffects();
            if (root != null)
                Object.Destroy(root);
            enemies.Clear();
            enemyRenderers.Clear();
            enemyAngles.Clear();
        }

        public void ClearEffects()
        {
            foreach (var r in runnings)
                if (r.go != null)
                    Object.Destroy(r.go);
            runnings.Clear();
        }

        // ---------------------------------------------------------------- 매 프레임

        public void ApplyView(EffectMakerSettings s)
        {
            if (cam == null)
                return;
            cam.orthographicSize = s.cameraSize;
            cam.backgroundColor = s.bgColor;
        }

        public void Tick(float dt, EffectMakerSettings s)
        {
            UpdateEnemies(dt, s);
            UpdateCastAnim(dt);
            UpdateEffects(dt);
        }

        void UpdateEnemies(float dt, EffectMakerSettings s)
        {
            for (int i = 0; i < enemies.Count; i++)
            {
                if (enemies[i] == null)
                    continue;
                enemyAngles[i] = Mathf.Repeat(enemyAngles[i] + s.enemySpeed * dt, 360f);
                float rad = enemyAngles[i] * Mathf.Deg2Rad;
                enemies[i].position = center + new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f) * s.enemyRadius;
                // 진행 방향을 바라보게 (반시계 → 접선 x = -sin)
                enemyRenderers[i].flipX = -Mathf.Sin(rad) * Mathf.Sign(s.enemySpeed) < 0f;
            }
        }

        void UpdateCastAnim(float dt)
        {
            if (caster == null)
                return;
            if (castAnimTimer <= 0f)
            {
                caster.localScale = casterBaseScale;
                return;
            }
            castAnimTimer = Mathf.Max(0f, castAnimTimer - dt);
            float t = 1f - castAnimTimer / CAST_ANIM_TIME;
            float punch = Mathf.Sin(t * Mathf.PI) * 0.2f;
            caster.localScale = new Vector3(casterBaseScale.x * (1f + punch), casterBaseScale.y * (1f - punch * 0.5f), casterBaseScale.z);
        }

        void UpdateEffects(float dt)
        {
            for (int i = runnings.Count - 1; i >= 0; i--)
            {
                var r = runnings[i];
                if (r.go == null)
                {
                    runnings.RemoveAt(i);
                    continue;
                }

                r.age += dt;
                if (r.follow != null)
                    r.go.transform.position = r.follow.position + r.offset;

                if (!r.stopped && r.stopTime >= 0f && r.age >= r.stopTime)
                {
                    r.stopped = true;
                    foreach (var ps in r.rootParticles)
                        if (ps != null)
                            ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                }

                if (r.age >= r.life)
                {
                    Object.Destroy(r.go);
                    runnings.RemoveAt(i);
                }
            }
        }

        // ---------------------------------------------------------------- 스킬 발동

        public void Cast(EffectMakerSettings s)
        {
            castAnimTimer = CAST_ANIM_TIME;
            if (prefab == null || caster == null)
                return;

            var offset = new Vector3(0f, s.offsetY, 0f);
            switch (s.spawnTarget)
            {
                case SpawnTarget.Caster:
                    Spawn(s, caster.position + offset, 0f, s.followTarget ? caster : null, offset);
                    break;
                case SpawnTarget.TileCenter:
                    Spawn(s, center + offset, 0f, null, offset);
                    break;
                case SpawnTarget.NearestEnemy:
                {
                    var enemy = FindNearestEnemy();
                    if (enemy != null)
                        Spawn(s, enemy.position + offset, 0f, s.followTarget ? enemy : null, offset);
                    break;
                }
                case SpawnTarget.AllEnemies:
                    foreach (var enemy in enemies)
                        if (enemy != null)
                            Spawn(s, enemy.position + offset, 0f, s.followTarget ? enemy : null, offset);
                    break;
                case SpawnTarget.CasterToEnemy:
                {
                    var enemy = FindNearestEnemy();
                    float angle = 0f;
                    if (enemy != null)
                    {
                        var dir = enemy.position - caster.position;
                        angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                    }
                    Spawn(s, caster.position + offset, angle, s.followTarget ? caster : null, offset);
                    break;
                }
            }
        }

        Transform FindNearestEnemy()
        {
            Transform best = null;
            float bestDist = float.MaxValue;
            foreach (var enemy in enemies)
            {
                if (enemy == null)
                    continue;
                float d = (enemy.position - caster.position).sqrMagnitude;
                if (d < bestDist)
                {
                    bestDist = d;
                    best = enemy;
                }
            }
            return best;
        }

        void Spawn(EffectMakerSettings s, Vector3 pos, float angleZ, Transform follow, Vector3 offset)
        {
            var go = Object.Instantiate(prefab, pos, prefab.transform.rotation * Quaternion.Euler(0f, 0f, angleZ), root.transform);
            go.name = prefab.name;
            go.transform.localScale = prefab.transform.localScale * s.effectScale;

            var roots = new List<ParticleSystem>();
            foreach (var ps in go.GetComponentsInChildren<ParticleSystem>(true))
            {
                // 크기 조절이 파티클에도 먹도록
                if (!Mathf.Approximately(s.effectScale, 1f))
                {
                    var main = ps.main;
                    main.scalingMode = ParticleSystemScalingMode.Hierarchy;
                }
                if (IsRootParticle(ps, go.transform))
                    roots.Add(ps);
            }

            // 게임 이펙트(Effect 상속: ParticleEffect 등)는 Play()를 불러야 타이머가 돌고 바로 꺼지지 않음.
            // 게임 코드를 참조하지 않으려고 이름으로 찾아 호출
            bool startedByEffect = TryPlayGameEffect(go);
            if (!startedByEffect)
                foreach (var ps in roots)
                    ps.Play(true);

            var info = GetLifeInfo(prefab);
            var r = new RunningEffect
            {
                go = go,
                rootParticles = roots.ToArray(),
                follow = follow,
                offset = offset,
                stopTime = -1f,
            };
            if (info.looping)
            {
                r.stopTime = s.loopTime;
                r.life = s.loopTime + info.loopMaxLifetime;
            }
            else
            {
                r.life = info.life;
            }
            r.life = Mathf.Min(r.life, s.maxLife);
            runnings.Add(r);
        }

        static bool IsRootParticle(ParticleSystem ps, Transform effectRoot)
        {
            var parent = ps.transform == effectRoot ? null : ps.transform.parent;
            while (parent != null)
            {
                if (parent.GetComponent<ParticleSystem>() != null)
                    return false;
                if (parent == effectRoot)
                    break;
                parent = parent.parent;
            }
            return true;
        }

        const string GAME_EFFECT_BASE_TYPE = "Effect";
        static readonly Dictionary<Type, MethodInfo> playMethodCache = new Dictionary<Type, MethodInfo>();

        static bool TryPlayGameEffect(GameObject go)
        {
            bool played = false;
            foreach (var mb in go.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (mb == null)
                    continue;
                var method = GetGameEffectPlay(mb.GetType());
                if (method == null)
                    continue;
                try
                {
                    method.Invoke(mb, null);
                    played = true;
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[EffectMaker] {go.name} Play() 실패: {e.InnerException?.Message ?? e.Message}");
                }
            }
            return played;
        }

        static MethodInfo GetGameEffectPlay(Type type)
        {
            if (playMethodCache.TryGetValue(type, out var cached))
                return cached;
            MethodInfo method = null;
            for (var t = type; t != null && t != typeof(MonoBehaviour); t = t.BaseType)
            {
                if (t.Name == GAME_EFFECT_BASE_TYPE)
                {
                    method = type.GetMethod("Play", BindingFlags.Public | BindingFlags.Instance, null, Type.EmptyTypes, null);
                    break;
                }
            }
            playMethodCache[type] = method;
            return method;
        }

        // 이펙트 유지 시간 = 파티클 duration + 지연 + 수명 중 최댓값
        static (float life, float loopMaxLifetime, bool looping) GetLifeInfo(GameObject prefab)
        {
            if (lifeCache.TryGetValue(prefab, out var cached))
                return cached;

            float life = 0f;
            float loopMaxLifetime = 0f;
            bool looping = false;
            foreach (var ps in prefab.GetComponentsInChildren<ParticleSystem>(true))
            {
                var main = ps.main;
                float lifetime = main.startLifetime.constantMax;
                if (main.loop)
                {
                    looping = true;
                    loopMaxLifetime = Mathf.Max(loopMaxLifetime, lifetime);
                }
                life = Mathf.Max(life, main.startDelay.constantMax + main.duration + lifetime);
            }
            if (life <= 0f)
                life = 2f; // 파티클이 없는 이펙트(글씨 등)
            var result = (life, loopMaxLifetime, looping);
            lifeCache[prefab] = result;
            return result;
        }

        public static void ClearLifeCache() => lifeCache.Clear();

        // ---------------------------------------------------------------- 도우미

        Vector3 GetTilePos(int tileIdx)
        {
            int x = tileIdx % 2;
            int y = tileIdx / 2;
            return center + new Vector3((x - 0.5f) * TILE_SIZE, (0.5f - y) * TILE_SIZE, 0f);
        }

        static GameObject NewObject(string name, Transform parent)
        {
            var go = new GameObject(name);
            if (parent != null)
                go.transform.SetParent(parent, false);
            return go;
        }

        static GameObject NewSprite(string name, Transform parent, Sprite sprite, float fitSize, int order)
        {
            var go = NewObject(name, parent);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = order;
            if (sprite != null)
            {
                var size = sprite.bounds.size;
                float max = Mathf.Max(size.x, size.y);
                if (max > 0f)
                    go.transform.localScale = Vector3.one * (fitSize / max);
            }
            return go;
        }

        // 스프라이트 모드가 Multiple이어도 첫 스프라이트를 가져옴
        public static Sprite LoadSprite(string path)
        {
            foreach (var obj in AssetDatabase.LoadAllAssetsAtPath(path))
                if (obj is Sprite sprite)
                    return sprite;
            return null;
        }

        public static GameObject LoadPrefab(string path)
        {
            return string.IsNullOrEmpty(path) ? null : AssetDatabase.LoadAssetAtPath<GameObject>(path);
        }
    }
}
