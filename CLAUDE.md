# DigDig 기획 노트

Claude가 기획 의도를 유지하기 위한 문서. 기획이 바뀌거나 사용자가 새 의도를 설명하면 이 파일을 갱신한다.
코드에서 바로 알 수 있는 구현 세부는 적지 않고, **왜 그렇게 하는지(의도)** 위주로 적는다.
- **사용자에게 답할 때는 항상 한글로 답한다.**

## 클래스 이름 규칙 (2026-11-09)
- `Manager` = 씬을 오가며 살아 있음(데이터·유저). `Controller` = 한 씬에 배치된 싱글톤이 그 씬의 한 영역을 맡음(`SlimeFieldController`, `EnemyFieldController`, 이전 이름 SlimeSpawner/EnemySpawner). `Spawner` = 생성만 하는 부품. `System` = 풀링 또는 static 규칙 묶음(`PoolingSystem`, `AuthoritySystem`).

## 핵심 루프
- 적 처치 → 코인 획득 → 슬라임 소환(뽑기) → 칸이 차면 합성 → 빈 칸에 다시 소환
- 돈이 계속 쌓이는 만큼 계속 강해질 수 있어야 한다.
- **2026-10-27 문제의식: 강해지는 과정이 일방향이라 고민할 게 없다.** 목표는 **고민하고 전략적으로 배치하는 재미**.
  - 새 기능은 "이걸로 플레이어가 무엇을 고민하게 되는가(배치·선택의 트레이드오프)"를 기준으로 판단한다. 단순히 수치만 올리는 성장은 지양.
- **2026-10-28 의도: 골드 소비에 확률(약 50%) 포인트를 둔다.** 50% 확률 보상의 짜릿함(도파민). 단 순수 동전 던지기가 아니라 "안전하게 쓸지 / 걸어볼지"를 플레이어가 고르는 선택이어야 한다.

## 적 이동 (2026-10-16 기획 변경)
- 적은 플레이어를 쫓지 않고 `EnemyFieldController.paths` 경로를 따라 이동 (`Enemy.pathIdx` = 다음 목표 지점).
- 경로 **마지막 지점에 도착하면 캐릭터 체력 감소** 후 사라짐 (보상 없음). → 디펜스 구조: 경로 위에서 처치해야 함.
- 스폰 위치는 `paths[0]`. `paths`가 비어 있으면 이전 방식(플레이어 추적)으로 동작.
- (2026-11-08) **적 애니메이션**: 적 프리팹 자식의 스프라이트 프리팹(Layer Lab) Animator. **Walk가 기본**(스폰·풀 재사용 시 Walk부터). **처치되면 Dead 재생 → 한 번 끝나면 비활성화 후 풀로**(Dead 클립이 루프여도 1회, 최대 `Enemy.DEAD_ANIM_MAX_TIME`). Dead 중엔 충돌·이동·피해 없음, `EnemyDeadEvent`(보상·처치 감지)는 죽는 즉시 발행되고 활성 적 수에서도 바로 빠진다. **경로 끝 도착은 Dead 없이 바로 비활성화.** Dead 상태가 없는 프리팹(현재 보스)은 바로 비활성화. 테스트 `EnemyAnimPlayTest`.

## 슬라임 소환 / 합성
- 소환 버튼: **10코인**, 장착한 **5종 중 1종이 랜덤**으로 mergeLv 0으로 **16칸** 중 빈 칸에 등장.
- 합성 조건: **같은 key + 같은 mergeLv** 끼리만 (ice 0 + ice 0 O / ice + flame X / ice 0 + ice 1 X).
- (2026-11-09) **머지가 안 되는 슬라임 위에 놓으면 두 슬라임 자리 교체**(`SlimeFieldController.SwapSlimes`). 타일 아이템 효과도 칸을 따라 바뀜. 이전엔 원위치. 테스트 `MergeSwapPlayTest`.
- mergeLv는 0부터 시작. 최대치는 `SlimeData.MAX_MERGE_LEVEL` = 4 (0~4, 5단계).
- 합성 결과 key: 현재 코드는 장착 5종 중 **랜덤** (`Slime.Merge`). 기획 의도 확인 필요.
- 슬라임 판매/버리기 기능 없음.
- 게임 시작 시 타일 중 **랜덤 5칸은 사용 불가**(`BLOCKED_TILE_COUNT`, 2026-10-18). 소환/드래그/캐릭터 배치 모두 막힌 칸에는 못 놓음.

### 의도
- **소환 가격은 올리지 않는다.** 싸게 계속 뽑는 뽑기 재미가 핵심.
- **합성은 스탯 합계로 보면 약간 손해**여야 한다. 대신 칸이 비어서 다시 소환할 수 있다는 게 합성의 이득.
  - 칸이 남아 있으면 소환이 이득, 칸이 차면 손해를 감수하고 합성 → 이 순환이 의도.
- 강화 레벨(`userSlime.enhanceLevel`, 로비에서 영구 성장)과 머지 레벨(`mergeLevel`, 인게임 합성)은 별개.
  - 슬라임 에셋의 `level: N` 블록 = 강화 레벨, 그 안의 `values` 배열 = mergeLv별 값.
  - `values` 2개 = `기본/증가량`(선형), 그 외 = mergeLv별 값(모자라면 마지막 값).
- Ice / Flame / Random / Bounce: mergeLv가 오를수록 한 번에 발사하는 탄 수 증가(`shootCounts`), 탄마다 **서로 다른 타겟**을 노림.
- Orbit: 발사가 없으므로 **공속 = 회전 속도**(2026-10-23). `orbitRotationSpeed × AttackSpeed / GameSetting.ATTACK_SPEED_PER_SECOND`(=50, 초당 1회 공격 기준. 슬라임·캐릭터 공격 타이머도 같은 값 사용) → 공속 강화/버프(타일 아이템, 킹 오라)가 회전 속도로 반영됨.
- 랜덤 데미지(Random 등, 2026-11-05 Wacky에서 key 변경. 저장 데이터 마이그레이션은 안 함: 필요하면 데이터 초기화): 데이터에는 **중간값만** 입력, SlimeData의 `attackPowerRandomRatio`로 `중간값 × (1 ± 비율)` 범위 결정. `3~15` 같은 범위 직접 입력은 쓰지 않는다.
- (2026-11-05) 슬라임 스크립트는 **등급별 폴더**(`Slime/Unit/{C,B,A,S}/{Key}Slime/`, 데이터 스크립트는 `Slime/SlimeData/{C,B,A,S}/`, 프리팹은 `3.Prefabs/Slime/{C,B,A,S}/` — `SlimeData.LoadData`가 grade로 찾음). 신규 key: B = Shield(철벽)·Dopamine(광란)·Mine·Breeze·Rage·Stigma(낙인), A = Mud·Keen(설의, 2026-11-06 B→A)·Beam(광선 링크)·Berserker·Hail(빙석)·Inferno(지옥)·Mortar(박격포, 2026-11-06 Boom에서 key 변경), S = Sacrifice·GrowUp·Scavenger(시체수집)·Joker·Hunter(사냥꾼, 2026-11-06). 대부분 아직 뼈대(일반 탄)만 있음.
- (2026-11-09) **강화 능력(EnhanceAbility) 폐지.** 강화 레벨(0~14, 로비 영구 성장)은 유지하되 **스탯(공격력·공속, 슬라임 특성 수치)만 올린다.** 가시방패·포위 진형·화상 같은 레벨 해금 특수 능력(각성 포함)은 없다. 강화 데이터 = `EnhanceSlimeData`(이전 EnhanceAbilitySlimeData) + SlimeEnhance CSV(Key/Level/AttackPower). 공속·특성 수치의 레벨별 증가는 아직 미구현(시트 형식 정해서 추가 예정).
- (2026-11-09) **[각성] = 진화로 발현.** 최대 머지 레벨 슬라임이 진화 타일(`EvolutionTile`)에 올라가면 `Slime.Evolve` → `isEvolved` + `OnEvolve`로 그 슬라임의 각성 능력이 켜진다(이전 강화 8 능력). 수치는 각 슬라임 클래스 `EVOLVE_*` const(임시). 합성·교체로 새 슬라임이 되면 사라짐.
  - C: 탄성·관통 = 탄 피해 +20%, 냉기 = 탄 1회 튕김, 번개 = 맞은 적 1초 감속, 천둥 = 10% 확률 1초 감속, 강철 = 엘리트·보스 배율 +0.2, 회전 = 3% 확률 0.1초 스턴, 랜덤 = 6번째 공격마다 4방향 원형 발사, 화염 = 5초마다 사거리 안 적 최대 7기에 공격력 피해 + 화상.
  - B: 철벽 = 가시방패(막힌 적 1초마다 최대 체력 1%), 광란 = 추가 탄 2발, 지뢰 = 2개씩 설치, 낙인 = 최대 5중첩, 십자 버프(산들바람·격노·설의) = **포위 진형**(같은 그룹 안 범위 4칸이 모두 슬라임이면 버프 증가분 +50%).
  - A: 광선 = 레이저 감속 0.5초, 버서커 = 최대 중첩 시 공격력 ×1.2, 빙석 = 눈 2개, 지옥 = 화상(2초, 초당 공격력 30%), 박격포 = 슬로우 확률 50%, 진흙 = 구슬 최대 +1.
  - S: 사냥꾼 = 2회마다 저격, 시체수집 = 엘리트·보스 +30코인. **희생·성장·조커는 진화하지 않음**(`CanEvolve` false: 머지 레벨을 올리는 각성이라 최대 레벨에선 의미 없음).
- (2026-11-05) C등급 신규: **Iron(강철, 물리)** = 엘리트/보스에게 데미지 ×2(`IronSlimeData.eliteBossDamageMul`). 배율은 탄 스펙의 `BigEnemyDamageForce`(IBulletForce, 2026-11-06)로 실어 연발 탄에도 유지, **거인 사냥 타일과 곱연산**(×4: force가 피해량을 키우고 거인 사냥은 `DamageData.eliteBossDamageMul`로 EnemyDefence에서 곱함). 다른 IBulletForce와는 합연산. 엘리트 wave용 배치 카드.
  **Thunder(천둥, 마법)** = 탄이 날아가지 않고 발사 방향 첫 적 위치에 낙뢰 → 주변 범위 피해(최대 5기). 머지 레벨이 오를수록 범위 증가(`strikeRadiuses`). 발사 0.1초 뒤 피해(타일 효과가 OnFired로 붙은 다음).
- (2026-11-06) **Mine(지뢰, B)** = 슬라임이 있는 타일의 `TileGroup.enemyRoots`(순서대로 이은 선 = 그 그룹 근처 적 경로) 위 랜덤 지점에 지뢰를 던져 설치. 적이 사거리에 없어도 미리 깔고, 밟히면 범위 폭발. **동시에 깔린 지뢰 수 상한이 머지 레벨로 증가**(`maxMineCounts`), 상한이면 대기했다가 터지면 바로 다음 지뢰. 어느 그룹에 두느냐 = 어느 경로 구간을 막느냐가 배치 포인트. 슬라임이 합성돼 사라져도 깔린 지뢰는 남는다. 지뢰는 **탄이 아니라 독립 오브젝트 `Mine`**(MineSlime이 던짐, 자체 풀링): 폭발 범위·최대 피해 적 수·유지 시간은 MineSlimeData, 트리거 범위는 고정(`Mine.TRIGGER_RANGE`). 탄이 아니라서 발사 훅(`TileItem.OnFired`)의 타일 효과는 안 붙고, 피해량·치명타 확률은 던질 때 슬라임 스탯으로 고정.
- (2026-11-06) **Stigma(낙인, B)** = 일반 탄 + 적중 시 확률(`stigmaChances`, 머지 레벨로 증가)로 낙인 1중첩(`StigmaEffect`). 중첩당 그 적이 받는 **모든 아군 피해** +5%(최대 3중첩, `Enemy.TakeDamage`에서 `StatusEffectHandler.StigmaDamageMul` 곱). 낙인은 적이 죽을 때까지 유지. 딜러 보조형 → 다른 슬라임과 같은 적을 때리게 배치하는 게 포인트. 여러 낙인 슬라임이 섞이면 최대 중첩/중첩당 피해는 큰 값.
- (2026-11-06) **Berserker(버서커, A)** = 기본 탄 + 주변(`detectRanges`, mergeLv별)에서 적/돌이 죽을 때마다 **공속 중첩**(중첩당 `attackSpeedPerStacks`%, 최대 10). 광란(Dopamine)과 같은 처치 감지 구조. **마지막 처치 후 `stackDuration`(5초) 동안 처치가 없으면 중첩 초기화** → 적이 계속 죽는 자리(다른 딜러 옆, 경로 가까이)에 두는 배치 포인트.
- (2026-11-06) **Inferno(지옥, A)** = 탄 없음. 자기 TileGroup `enemyRoots` 중 **슬라임과 가장 가까운 2개 사이**에 지옥불(`Hellfire`, 길이 = 두 점 거리, 폭 = `hellfireWidth`) → **공격 주기마다 그 안에 있는 적 전부** 피해, 벗어나면 바로 피해 없음. **TileGroup 3*3 가운데 칸이면 지옥불 없음**(index % 3 == 1). 같은 그룹 안에서도 칸 위치가 덮는 경로 구간을 정함 → 배치 포인트. 드래그 중엔 숨김.
- (2026-11-06) **Mud(진흙, A)** = 탄 없음. **공속 = 진흙 구슬 생성 주기**(공격마다 1개, 최대 `maxOrbCounts` 머지 레벨로 증가, OrbitMachine으로 회전, 회전 속도는 고정). 구슬이 적에 닿으면 **스플래시**(`splashRadiuses`) 피해 + 진흙 1중첩 후 **구슬은 사라짐** → 최대치면 대기했다 터지면 바로 재생성. **진흙 중첩당 이동속도 15% 감소, 최대 80%(엘리트·보스 60%)**, 멈추지는 않음(`MudEffect`, 마지막 적중 후 `mudDuration` 지나면 전부 해제). 슬로우·빙결 중첩과는 별개로 곱함(`StatusEffectHandler.SlowRate`). 4중첩 굳음(스턴)은 다수 배치 시 보스까지 영구 속박돼 폐기(2026-11-06).
- (2026-11-06) B등급 **Dopamine(광란)** = 기본 탄 + 슬라임 주변 `detectRanges`(mergeLv별) 안에서 적/돌이 죽으면 **추가 탄**(공격력 × `extraShotDamageMuls`, mergeLv별) `extraShotCount`발. 추가 발사 후 `extraShotCoolTime` 쿨타임, 일반 공격 타이머는 리셋하지 않음. → 적 경로 근처(처치가 잦은 칸)에 둘지 고민.
- (2026-11-06) B등급 **Shield(철벽)** = 방패(`ShieldOrbitOrb`, `SeparateOrbitMachine`으로 방패마다 따로 회전)가 닿은 적의 **이동을 멈춤**(`Enemy.AddBlocker`). 방패당 최대 `blockCountPerShield`기(기본 3), 초과분은 통과. **엘리트/보스는 막기 무시**(`Enemy.CanBeBlocked`, 슬롯도 안 차지). 막는 동안 그 방패는 정지, 막던 적이 모두 죽거나 빠지면 다시 회전. → 경로 근처에 두면 적을 붙잡아 다른 슬라임이 처리.
- (2026-11-06) A등급 **Beam(광선 링크)** = 필드(타일 위)의 광선 슬라임끼리 **모두 레이저 연결**(`BeamLinkNetwork`, n마리 → n(n-1)/2개). 레이저 두께 = 머지 레벨별 `lineWidths`(두 슬라임 평균). 레이저에 닿은 적에게 **0.25초마다(고정)** 피해(두 슬라임 공격력 평균 × 배율, 배율은 전체 레이저 수가 늘수록 감소). **연결된 슬라임은 기본 공격 안 함**, 혼자면 기본 탄. → 광선 슬라임 둘을 적 경로를 가로지르게 배치하는 고민. 타일 아이템 발사 효과는 레이저에 적용 안 됨.
- (2026-11-06) A등급 **Hail(빙석)** = 탄 없이 **내 TileGroup의 `enemyRoots`를 순서대로 빙빙 도는 눈**(`HailStone`, 마지막 지점 → 첫 지점으로 이어 고리로 순환, 여러 개면 같은 간격). Orbit과 다른 방식(회전체 아님). 닿으면 피해 + 머지 레벨별 확률(30~50%)로 얼음, 같은 적은 0.5초 쿨. 눈 1개. 이동 속도 = 공속 비례. → 적 경로가 지나는 그룹에 둘지 고민.
- (2026-11-06) A등급 **Mortar(박격포, 이전 key Boom. 저장 데이터 마이그레이션 안 함)** = 드래그로 옮기는 **깃발(`MortarFlag`) 자리에 폭탄을 포물선으로 떨어뜨림**(비행 중 충돌 없음, 착탄 지점 범위 피해, 최대 `maxHitCount`기). 높은 공격력 + 느린 공속. 깃발 폭발 범위 안에 적이 있을 때만 발사. 맞은 적 **25% 확률 슬로우(머지 레벨과 무관, 고정)**. 깃발은 슬라임 사거리 안으로만 옮길 수 있고, 처음엔 내 TileGroup 적 경로에서 가장 가까운 지점. 깃발 드래그는 `SlimeFieldController`에서 처리(공용 `DragHandle`): 선택된 슬라임의 깃발은 유닛보다 우선, 아닌 깃발은 유닛이 없는 자리에서만 잡힘. → 깃발을 어디 꽂을지(경로 꺾이는 곳, 철벽이 막는 곳)가 배치 포인트. 낙뢰처럼 타일 발사 효과(치명타·둔화·연발 등)가 적용됨.
- (2026-11-06) S등급 **Hunter(사냥꾼)** = 평소 일반 탄, **3회 공격마다 저격탄**(관통 감쇠 없음 + 스턴 0.2초 + 확정 치명타 `BulletSpec.mustCrit`). 한 공격의 탄이 여러 발이면 전부 저격탄. 저격탄은 Pierce 탄 프리팹을 같이 씀.
- (2026-11-06) S등급 4종 (모두 평소엔 일반 탄 공격):
  - **Sacrifice(희생)** = 같은 머지 레벨의 **아무 종과 합성 가능 → 결과 = 상대 종, 머지 레벨 +1**(일반 합성은 결과 종 랜덤). 끌어온 쪽/놓인 쪽 상관없음(`SlimeFieldController.Merge`), 희생끼리는 일반 규칙. → 키우고 싶은 종을 골라 올리는 카드.
  - **GrowUp(성장)** = 타일 위에서 게임 진행 중 **25초 지나면 장착 슬라임 중 랜덤 종(성장 제외)으로 성장, 머지 레벨 +1**. 드래그 중이면 놓은 뒤 성장, 옮겨도 시간 유지.
  - **Scavenger(시체수집)** = 공격 사거리 안에서 적이 **처치되면(누가 잡든) 1코인**. 경로 끝 도착은 제외(체력 0일 때만). → 처치가 몰리는 자리에 두는 배치 포인트.
  - **Joker(조커)** = 필드의 조커 선택 → 변신 버튼(`JokerTransformButton`) → **장착 슬라임 중 원하는 종으로 변신**(`JokerSelectCanvas`, 일시정지). 머지 레벨 유지, 단 **30% 확률로 실패 → 머지 레벨 -1, 가장 낮은 단계(별 1)면 조커가 사라짐**. 아껴 뒀다가 필요한 종이 생기면 쓰는 카드(걸어볼지 고민).
  - (2026-11-09) **확률 연속 실패 보정**(`SlimeChancePity`, `GameManager.slimeChancePity`): 조커 변신 성공 판정은 슬라임 key별로 연속 실패를 세고, **4연속 실패하면 다음 시도부터 성공 확률 +50%p**(성공할 때까지 유지), **성공하면 초기화**(4회 전 성공도). 한 판 동안 유지. 의도: 확률에 거는 재미는 두되 연속 실패로 짜증나지 않게. 전투 중 적중 확률(낙인·빙석 등)은 대상 아님.
  - 성장·변신은 공용 `SlimeFieldController.ReplaceSlime`(같은 타일에 교체, 선택 유지).
- (2026-11-06) **십자 버프 슬라임**(공용 부모 `CrossBuffSlime` / `CrossBuffSlimeData.buffValues`, mergeLv별): 기본 탄 + 내 타일 기준 **십자 4칸 슬라임에게 버프**. 범위는 **같은 TileGroup 안에서만**(Grant 기획과 같은 규칙). 타일 오브젝트가 바뀔 때마다 대상 재계산, 여러 개가 겹치면 각각 적용(Multiply는 곱). → 배치 고민 포인트.
  - 범위 8방향은 `ExpandBuffRangeTile` 타일. 포위 진형은 진화(각성)로 발현, 고른 모양(십자/대각)의 4칸 기준.
  - B등급 Breeze(산들바람) = 공속 ×, Rage(격노) = 공격력 ×. A등급 Keen(설의) = 치명타 확률 +%(Add) (2026-11-06 B에서 승급).
  - 슬라임 `CritChance` 스탯(기본 0, 버프로만 오름)은 `AllyUnitDamageData.Init`에서 탄 데미지 데이터로 들어간다(연발·Orbit 포함). 타일 치명타는 그 위에 더함.

### 장착 조합 프리셋 (2026-11-09)
- 로비 슬라임 관리 화면에 **조합 프리셋 버튼 5개**(`SlimePresetButton`, presetIdx 0~4, `SlimeManageCanvas` 하위에 두면 자동 연결). 기본 선택 = 1번.
- 버튼을 누르면 그 프리셋 조합으로 장착(`UserSlimeManager.LoadPreset`). **선택된 프리셋은 장착을 바꿀 때마다 자동 저장**(따로 저장 버튼 없음). 저장은 `UserSlimeData.presets`/`selectedPresetIdx`.
- 빈 프리셋은 처음 로드할 때 현재 장착 조합으로 채움. 프리셋 key가 미보유·중복이면 현재 장착 → 보유 슬라임으로 대체(`ResolvePresetKeys`, `SlimePresetTest`).
- 이미 장착된 슬라임을 다른 칸에 장착하면 두 칸을 맞바꾼다(5칸 항상 채움).

### 유닛 설정 버튼 (2026-11-09)
- 선택한 유닛 패널(`AllySettingPanel`)의 설정 버튼은 **유닛이 쓸 수 있는 설정만 노출**(공용 `AllyOptionButton.IsAvailable`). 누를 때마다 다음 값으로 순환.
  - 타겟 방식(`SetFindTargetButton`): 타겟을 노리지 않는 슬라임(`Slime.UseFindTarget = false`: Orbit·Mud·Shield·Hail)엔 숨김.
  - 버프 범위(`SetBuffRangeButton`, `IBuffRangeUnit`): 십자 버프 슬라임 = **십자 4칸 ↔ 대각 4칸**. 버프 범위 확장 타일 위면 모양과 상관없이 8방향.
  - 회전 방향(`SetRotationDirectionButton`, `IRotationDirectionUnit`): 회전체(Orbit·Mud·Shield, `OrbitMachine.rotationDirection`, 기본 반시계) / 경로 순환(Hail, 기본은 데이터) = **시계 ↔ 반시계**.
  - 의도: 같은 슬라임이라도 **버프가 닿는 칸·회전체가 경로를 쓸고 지나가는 방향**을 플레이어가 골라 배치 고민을 늘린다.
  - 설정은 소환(InitSlime) 때 기본값으로 초기화 → 합성·성장으로 새 슬라임이 되면 다시 골라야 함.

## 인게임 직접 강화 (2026-11-04)
- 코인으로 **종(key) 단위** 강화(`ReinforcePanel`, 가격 30×(lv+1)). 레벨은 `KingSlimeStatManager.slimeDic`의 `reinforceLevel`.
- 효과는 주로 **공격력 %**: 레벨당 `SlimeData.reinforceAttackPowerPercent`(SlimeData.csv `ReinforceAttackPower`, 기본 10) 합연산. **%는 종마다 다르게** 준다.
- **슬라임 특성의 시각적 변화(탄 수, Orbit 체인 수 등)는 강화가 아니라 머지 레벨업으로** 준다. 강화는 수치 담당.
- 그 종이 필드에서 잠깐 0마리가 돼도(머지 등) 강화 레벨은 유지(`slimeDic` 항목을 지우지 않음).

## 머지 코인 강화 (2026-10-23 기획 중)
- 슬라임을 **머지 3회마다 머지 코인 1개** 획득 → 머지 코인을 소모해 인게임 강화.
- 강화 대상은 **슬라임 개체가 아니라 종(key) 단위**. 한 종을 강화하면 필드의 그 종 전체(+이후 소환분)에 적용 → 합성으로 투자가 사라지지 않음.
- 검토 중인 방향: 킹슬라임이 슬라임을 **흡수**해 그 종의 특성/강화를 얻는 형태, 머지 코인 **저축형**(모아서 쓸수록 이득).
- **2026-10-23 방향: 킹슬라임(캐릭터)의 필요성을 높인다.** 머지 코인 강화 = **킹 자신 강화 + 킹 주변 슬라임 강화(오라)**.
  - 투자는 킹에 쌓이고 주변 버프는 위치로 결정 → 슬라임 개체에 투자가 묶이지 않아 합성과 충돌 없음.
  - (Grant 아이템은 폐기됐지만, 주변 강화는 아이템이 아니라 킹 고유 능력으로 들어오는 것)

## 권능 (2026-11-09, 이전 이름 "장비" → "유물", 코드명 `Authority`. 기획 중)
- **권능은 스탯 성장이 아니라 인게임 특수 능력.** 수치 성장은 KS 업그레이드가 담당 → 권능에 스탯 없음(킹 스탯 = 기본 + 업그레이드, `KingSlimeManager.GetTotalStatValue`).
- **부위(헬멧·무기 등) 개념 없음, 킹 외형 변화 없음**(`KingSlimePart`/`KingSlimeBody`·`EquipPartType` 삭제). 장비 비교·능력치 변화 창도 삭제 → 권능 정보 창 하나(`AuthorityInfoCanvas`: 이름·설명·해당 슬라임 수·장착/해제).
- 의도: **장착한 슬라임 5종 구성에 맞춰 어떤 권능을 고를지 고민**하게 한다. 권능은 슬라임의 작동 방식(태그)과 시너지가 나도록 설계.
- **묶음(tier) 6개 × 권능 3개 = 18개.** 유저 레벨 **5·10·15·20·25·30**(`AuthorityData.TIER_UNLOCK_LEVELS`)에 묶음이 하나씩 해금되고, **묶음마다 3개 중 1개를 선택**(2026-11-09 확정).
  - **해금된 묶음에서 고른 권능은 전부 적용**(최대 6개). 장착 개수 제한·장착/해제 개념 없음. 적용 목록 = `UserAuthorityManager.GetActiveAuthorityKeys`(레벨이 모자라 잠긴 묶음의 선택은 저장만 되고 적용 안 됨).
  - **로비에서 언제든 무료로 변경**(같은 묶음을 다시 고르면 교체). 인게임 중엔 변경 없음.
  - **상점 권능 뽑기 삭제**(DrawAuthority·결과 창·상품 key `DrawAuthority_*`). 권능은 레벨 해금으로만 얻음. `AchievementType.DrawAuthority`은 값 순서 때문에 남겨 둠(사용 안 함).
  - 로비 UI: KS 관리 화면 권능 탭 = **3열 그리드, 한 줄 = 한 묶음**(`AuthorityContainer.slotPanels` 18칸, 잠긴 묶음은 `LockPanel`에 `Lv.N`). 칸 → `AuthorityInfoCanvas`(이름·설명·해당 슬라임 수·선택하기). 레벨업으로 묶음이 새로 열렸고 아직 안 골랐으면 레벨업 창을 닫은 뒤 권능 탭을 열고 안내(`LobbyManager.OpenAuthoritySelect`).
  - 저장: `UserAuthorityData.selectedKeys`(index = 묶음 - 1). 이전 장비 저장 데이터는 마이그레이션 안 함(출시 전).
  - 지금 18개는 **임시 데이터**(AuthorityData.csv `Key/Tier/MatchTags/Desc`, **권능은 등급(Grade) 없음**, **설명 원본 = CSV Desc**(아직 번역 안 함), 이름 = Translate `Authority.{Key}`). 
  - (2026-11-09) **18개 능력 구현** (수치는 각 `{Key}Authority` 클래스 const, 임시). 클래스 = `InGame/Authority/Authorities/*.cs`, 생성 = `AuthoritySystem.Begin`(GameManager.Start, 타일·킹 배치 후 게임 시작 전) / `End`(GameManager.OnDestroy). 게임 코드는 `AuthoritySystem.Get<T>()`/`Has<T>()`로 확인.
    - 묶음1 운영: 개척자의 곡괭이(막힌 칸 2칸 해제, 막힌 칸은 실제로 3칸) · 상인의 장부(wave 0 소환 가격 절반, `SlimeSpawnButton`) · 도박꾼의 동전(소환 25% 머지 Lv1 / 25% 코인 2배).
    - 묶음2 공격 형태: 분열의 파편(탄이 처치하면 ±30° 두 발, 피해 50%, 쪼개진 탄은 다시 안 쪼개짐, `AllyBulletObject.Hit`) · 잔불의 화로(지뢰·박격포·낙뢰 폭발 지점 / 지옥불은 맞은 적 1명 위치에 3초 불길, 0.5초마다 공격력 20%/초, 동시 20개 상한, `EmberFire`) · 회오리 구슬(회전체 누적 360°마다 슬라임 위치에서 랜덤 방향 기본 탄, `OrbitMachine.AccumulateRotation`).
    - 묶음3 특성: 전염의 성배(디버프 2종 이상 걸린 적 처치 → 반경 2 안 최대 5기에 같은 디버프 새로 적용) · 중계의 수정(킹 칸 중심 가로·세로 1.5칸 안 처치도 광란·버서커·시체수집이 감지, `RelayCrystalAuthority.InDetectRange`) · 지휘의 깃발(십자 버프 그룹 경계 무시).
    - 묶음4 보드: 측량사의 나침반(**지뢰·지옥불만** 옆 그룹(가로·세로) 경로 사용. 빙석은 자기 그룹 순환, 박격포는 원래 사거리 안 자유라 제외) · 쌍둥이 반지(상하좌우 같은 종 → **공격력 등 데이터 스탯만** 머지 Lv +1 값, `Slime.twinBonus`. 탄 수·회전체 수 같은 모양은 안 바뀜: 회전체 슬라임 InitSlime이 재호출에 안전하지 않음) · 장인의 망치(뽑기 최저 확률 30%, 실패 시 낸 코인 절반 환불).
    - 묶음5 위기: 사냥꾼의 부적(Heavy 슬라임 타겟 = 엘리트·보스 우선, 엘리트 처치 시 타일 아이템 뽑기 확률 100% 무료, 이미 뽑기 창이 열려 있으면 생략) · 혼혈의 문장(장착에 물리·마법 모두 있으면 정령/갑옷 wave 시작 시 약점 속성 장착 슬라임 1마리 무료 소환, 소환 가격 안 오름, `AuthoritySystem.SpawnFreeSlime`) · 모래시계(킹 체력 처음 30% 미만 → 5초간 모든 적 정지, `Enemy.FreezeAll`).
    - 묶음6 완성: 순혈의 왕관(장착 5종 공격 속성 모두 같음(만능 제외) → 아군 약점 배율 ×1.5, `EnemyDefence.GetMultiplier`) · 왕의 인장(장착 5종 형태 모두 다름 → 모든 슬라임 공속 ×1.2, `Slime.UpdateSlime`) · 혈통의 플라스크(합성 결과 종 50% 같은 종 유지, `Slime.Merge`).
    - 조건형(왕관·인장)은 게임 시작 시 장착 슬라임으로 한 번 판정. 이름 = Translate `Authority.{Key}`, 설명 = AuthorityData.csv Desc, 발동 토스트 = Translate `Authority_*`.
    - 테스트 `AuthorityAbilityTest`(능력 클래스 존재·규칙 계산), `AuthorityPlayInGameTest`(18개 모두 켜고 실제 전투: 곡괭이·쌍둥이·잔불·모래시계 확인 + 에러 없음).
- 데이터: `AuthorityData`(Addressables 폴더 항목 `AddressableResources/AuthorityData`, 라벨 `AuthorityData`) → `AuthorityManager`(Resources/Init, `GetTierAuthorities`).
- **슬라임 유형 태그(`SlimeTag`, Flags)**: 권능이 반응하는 기준. SlimeData.csv **`Tags` 열(L열, `Shooter|Status`처럼 |로 구분)** → `SlimeData.tags`. 시트 다운로드 범위 `slimeRange` = A:L. 태그는 **기본 효과 기준**(부가 효과는 안 셈).
  - 형태(정확히 하나): Shooter 발사(Random, Bounce, Flame, Ice, Lightning, Pierce, Iron, Dopamine, Stigma, Berserker, Hunter) / Zone 장판(Mine, Beam, Mortar, Hail, Inferno, **Thunder**) / Orbit 회전(Orbit, Shield, Mud) / Support 버프(Breeze, Rage, Keen) / Special 특수(Sacrifice, GrowUp, Joker, Scavenger).
  - 특성: Status 상태이상(Flame, Ice, Stigma, Mud, Hail) / Chance 확률(Ice, Stigma, Hail, Joker) / KillTrigger 처치 감지(Dopamine, Berserker, Scavenger) / Heavy 강타(Iron, Hunter, Mortar) / Transform 변환(Sacrifice, GrowUp, Joker).
  - 규칙(`SlimeTagTest`): 형태는 슬라임마다 하나, 형태·특성마다 해당 슬라임 **3종 이상**. 새 슬라임 추가 시 Tags도 채울 것.
- 권능이 반응하는 유형 = `AuthorityData.matchTags`. 정보 창에 **장착 슬라임 중 해당 종 수 / 장착 수**(태그 중 하나라도 있으면 해당, `AuthorityData.CountMatchingSlimes`). matchTags가 없으면 숨김.
- 테스트 `AuthorityTest`(해금 레벨·선택 가능 여부·묶음마다 3개·이름 번역·설명), `AuthorityPlayTest`(로비 권능 탭: 18칸·잠김 표시·칸 클릭 → 정보 창 → 선택·같은 묶음 교체·잠긴 묶음 선택 불가·적용 목록).

## 오라 (2026-11-09 폐기)
- **오라는 하지 않는다.** 로비 오라 메뉴·패널(`KSAuraContainer`/`KSAuraStateContainer`)과 `KSMenuType.Aura` 삭제. (이전: 킹 주변 타일 강화, 출시 후 업데이트 예정이었음)

## 킹슬라임 로비 업그레이드 (2026-11-03)
- 로비에서 재화(Gold 또는 Dia)로 킹슬라임 스탯을 **StatType별로** 영구 업그레이드(`UserKingSlimeManager`, Lv 저장).
- 데이터는 `Json/KSUpgradeData.csv`(구글 시트) 한 행 = StatType 하나 → `Resources/KSUpgradeData.asset`. **StatType이 추가돼도 행만 추가**하면 인게임에 반영(UI는 `KSUpgradeContainer` 하위에 그 StatType의 `KSUpgradePanel`을 직접 배치). 행이 없는 StatType은 업그레이드 불가, 패널도 숨김.
  - 재화는 StatType마다 하나로 고정(`CurrencyType`). `MaxLv` 0 = 무제한.
  - 증가는 **더하기**: Lv × `ValuePerLv`.
  - 가격(lv → lv+1) = `(BasePrice + PriceAddPerLv × lv) × PriceMulPerLv^lv` (반올림). 선형/지수 모두 이 식으로 조절.
- 최종 스탯 = 기본값(KingSlimeData) + 업그레이드 (권능은 스탯 없음), 계산은 `KingSlimeManager.GetTotalStatValue` 한 곳(로비 표시·인게임 `KingSlimeStatManager` 공용).
  - StartGold = 게임 시작 시 코인, WaveClearGold = wave 종료마다 코인(보스 wave는 보스 처치 시. 무한 상승 게임이라 보스도 지급), MaxSlimeCount = `SlimeFieldController.slimeMaxCount`(기본 `SLIME_MIN_EXIST_COUNT`)에 더함. **더하는 곳은 GameManager 한 곳**(킹 스탯 값, 2026-11-07 SlimeFieldController.Awake에서도 Lv을 더해 두 번 적용되던 버그 수정).
  - 로비 KSManagementCanvas 능력치 표시(`KSStatStateContainer.currentKSAbilityInfoPanels`)는 업그레이드 StatType 전부(최대 슬라임 수·시작 코인·wave 종료 코인 포함). 표시 단위: 치명타 확률은 인게임과 같은 %(값 그대로), 공속은 `ATTACK_SPEED_PER_SECOND` 기준 10초당 발 수.
  - 테스트 `KSUpgradePlayTest`: StatType별 업그레이드(Lv·재화·스탯·표시, 재화 부족/최대 Lv), 로비에서 올린 값이 인게임 킹 스탯·체력·최대 슬라임 수·시작/wave 종료 코인에 반영.

## 아이템 공통 (2026-10-23)
- **보유 개수에 따른 능력치 증가는 없다.** `Item.count`/`GetCount()` 삭제. 아이템 효과는 개수와 무관하게 고정 수치(개수 1 기준). 보유 개수는 `statMgr.itemStatDic`에서 뽑기 상한(MaxCount) 판단에만 쓴다.

## 타일 선택 / 정보 표시 (2026-10-26)
- 선택 단위는 **유닛이 아니라 Tile**. 슬라임·캐릭터·아이템 타일을 클릭하면 정보가 뜬다. 타일 밖을 누르면 해제.
- `TilePointer`는 선택된 타일 위치 표시만 담당(입력 판정은 `SlimeFieldController` 한 곳). **빈 타일(유닛·아이템 없음)은 반응 없음**: 포인터·정보 패널(`TileItemInfoPanel` 등) 모두 숨기고 버튼은 그대로(선택 해제와 같은 화면). `TileItemInfoPanel`은 **아이템이 설치된 타일에서만** 뜬다(유닛만 있는 타일은 유닛 정보만). 드래그 중엔 숨기고, 선택은 옮긴 유닛을 따라간다(머지 시 결과 슬라임 선택).

## 슬로우 / 빙결 (2026-10-31)
- 둘은 **이펙트만 다르고 감속은 같다**(`IceEffect : SlowEffect`). 적중마다 이동속도 ×0.5, **둘을 합쳐 최대 3중첩**(최저 12.5%). 효과가 끝나면 그 효과가 쌓은 중첩만 풀린다.
- (2026-11-06) **넉백(적 밀어내기, `Enemy.Push`/`PushBehavior`)은 삭제.** 적 이동 방해는 슬로우·빙결·스턴·철벽 막기로만. **스턴은 모든 출처 공통으로 엘리트/보스도 걸리고 시간만 절반**(`StatusEffectHandler.Apply`).

## 타일 아이템 (2026-10-22)
- 아이템 획득(2026-11-01): `DrawItemButton`으로 코인을 내면 `SelectItemCanvas`가 열린다. **Draw**(확률 연출, `drawDuration` 0이면 생략) → **SelectItem**(TileItem 중 겹치지 않는 랜덤 3개, 반드시 하나 선택) → **SelectTile**(눌러서 설치, 이미 아이템이 있는 타일은 불가). **열려 있어도 게임은 멈추지 않는다**(2026-11-07 확인, 이전엔 일시정지). 설치 칸을 직접 고르는 것이 배치 고민 포인트. PioneerTile은 설치가 없어 타일 선택 생략. (이전: 랜덤 1개를 랜덤 타일에 바로 설치)
  - 선택지 갱신(2026-11-04): SelectItem 단계에서 `AdRefreshItemButton`으로 광고를 보면 선택지 3개를 다시 뽑는다(가능하면 지금 보이는 것 제외). **한 판에 2번**(`REFRESH_COUNT_PER_GAME`), 광고 제거 구매자는 광고 없이 갱신.
  - 뽑기 가격(2026-10-25): **처음 10코인, 뽑을 때마다 +2** (`INIT_DRAW_ITEM_PRICE`/`INCREASE_DRAW_ITEM_PRICE`, 횟수는 `GameManager.drawItemCount`).
  - 성공 확률(2026-10-28): **처음 100%, 성공할 때마다 -5%** (최소 10%, `INIT/DECREASE/MIN_DRAW_ITEM_CHANCE`). 코인은 먼저 내고 실패하면 코인만 소모, 가격·확률은 그대로. → 골드 소비의 확률 포인트(뒤로 갈수록 "걸어볼지" 고민).
- (2026-11-09) **타일 아이템 강화**: 아이템 타일 선택 → `TileItemInfoPanel`의 `UpgradeTileItemButton`. **설치된 타일 객체 하나 단위**(같은 key 다른 타일은 그대로, 판매하면 투자도 사라짐).
  - 비용 = **주사위 코인(`KingSlime.diceCoin`, 엘리트 처치 보상) × (현재 레벨 + 1)** — 기본값은 ItemData.csv `DiceCoin`. **Lv1 시작, 최대 = `MaxCount`(3)**. Lv1→2 = DiceCoin×2, Lv2→3 = ×3.
  - **성공 확률 70% → 50%**(`TileItem.UPGRADE_CHANCES`, Lv1→2 / Lv2→3). 코인은 먼저 내고 실패하면 코인만 소모·레벨 유지(뽑기 실패와 같은 방식). 골드 소비 확률 포인트와 같은 의도.
  - 레벨별 수치 = ItemData.csv **`Values`** 열(`1.3/1.5/1.7`, 시트 범위 A:G) → `ItemData.levelValues`. 아이템마다 대표 수치 하나에 들어감(`TileItem.ApplyLevelValue`): 스탯 타일 = 배율/증가량, 광폭 = 공격력 배율, 연발 = 추가 발수, 치명타·둔화·스턴 = 확률, 거인 사냥 = 배율, 코인 = 처치 코인, 다중 타겟 = 탄 수. Values가 비면 프리팹 값 그대로. 지금 값은 임시. **설명(Desc)의 `{0}` = 현재 레벨 수치**(`TileItem.GetDescription`, 배율형은 % 증가량: 1.5 → 50).
  - **DiceCoin 0(카운팅 제외·최대 버프·개척·만능 공격)·진화 타일은 강화 불가**(버튼 숨김). 최대 레벨이어도 숨김. 강화 가능 아이템은 이름 옆에 `Lv.N`.
  - 유닛이 올라가 있으면 효과를 뺐다가 새 수치로 다시 건다(버프 중복 없음). 테스트 `TileItemUpgradePlayTest`.
- 타일 아이템(`TileItem`)은 획득하면 고른 타일에 설치되고, 그 타일에 슬라임이나 캐릭터가 올라와 있는 동안만 효과가 적용된다(`OnEnter`/`OnExit`).
- 아이템 제거(2026-10-28): 아이템 타일을 선택하면 `TileItemInfoPanel`의 `SellTileItemButton`으로 그 타일 아이템 객체 하나만 판매 → **10코인 고정**(`SELL_TILE_ITEM_PRICE`). 같은 key의 다른 타일 객체는 유지. 판매 후에는 타일 선택 해제(TilePointer 숨김, 위에 유닛이 있어도 동일).
- **타일 하나에 아이템은 하나만** 설치한다. 설치할 때 위에 오브젝트가 있어도 되고, 그러면 바로 적용된다.
- 수치는 **타일(객체)마다 고정**이다. 같은 아이템을 또 얻으면 count를 올리지 않고 **객체를 하나 더** 만들어 다른 타일에 설치한다.
- 스탯 버프형(공격력, 공속, 사거리)과 발사 방식형(BurstShot, MultiTarget, CycleRandomShot) 모두 슬라임과 캐릭터에 적용된다.
  - BurstShot: 공격 후 **같은 방향으로 연발**(`COMBO_ATTACK_INTERVAL_MS` 간격). `BurstShotTileItem.OnFired`가 쏜 탄과 같은 탄(스펙·위치·방향·데미지)을 지연 후 다시 생성한다. 다중 타겟 탄도 탄마다 각각 연발. 추가 탄은 OnFired를 안 거쳐 연발이 연발을 만들지 않음.
  - MultiTarget: **다중 타겟** = `Slime.ShootCount`에 탄 수를 더한다(탄마다 서로 다른 타겟).
  - CycleRandomShot: **3번째 공격마다 랜덤 타겟**.
- 2026-10-23 목록 (Key = 클래스명에서 `Item` 뺀 것, ItemData.csv에 행 추가):
  AttackPowerTile(공 +50%), AttackSpeedTile(공속 +30%), BurstShotTile, MultiTargetTile, RangeUpTile(사거리 +2),
  SlowTile(10% 슬로우), StunTile(2% 0.3초 스턴), CriticalTile(10% 2배), TrueDamageTile(방어 배율 무시 → 항상 100%),
  IgnoreCountingTile(2026-11-04, 올라간 슬라임은 최대 슬라임 수에 안 셈 → `SlimeFieldController.ignoreSlimeCount`/`CountedSlimeCount`. 최대치 12마리 막힘을 푸는 수단),
  ExpandBuffRangeTile(2026-11-06, 올라간 십자 버프 슬라임의 범위 십자 4칸 → 8방향. 장착 슬라임에 버프 슬라임이 없으면 안 나옴. 배치 고민 포인트),
  EvolutionTile(2026-11-06, 최대 머지 레벨(4) 슬라임이 올라가면 진화 `Slime.Evolve`, 슬라임당 1회(`isEvolved`, 합성·교체로 새로 생기면 초기화), 타일은 남아 재사용. **뽑기(2026-11-09): 필드의 진화 안 한 레벨 4 슬라임 수 > 설치된 진화 타일 수일 때만, 뽑기 시도마다 40%(`EvolutionTileItem.DRAW_CHANCE`)로 선택지 3개 중 하나로 확정 등장**(실패하면 그 뽑기엔 안 나옴). 같으면 안 나옴(예: 5마리/4개 → 40%, 5마리/5개 → 0%, 타일이 더 많은 경우는 없어야 함). 보유 상한(MaxCount) 대신 이 규칙만 씀(`TileItem.IgnoreMaxCount`/`DrawChance`, `ItemManager.GetDrawTileItems`). 강화 불가. 테스트 `EvolutionTileDrawPlayTest`. 진화 = 슬라임 고유 [각성] 발현, 위 슬라임 섹션 참고),
  GoldTile(처치 시 코인 +1), PioneerTile(획득 즉시 막힌 칸 1개 해제, 설치 안 함, 막힌 칸 없으면 안 나옴), BerserkTile(공 ×2, 사거리 ×0.5), GiantSlayerTile(엘리트·보스에게 데미지 ×2, 방어 배율과 곱연산).
- 공격 타입 변경 타일 (2026-10-27): PhysicTile / MagicTile = 올라간 유닛의 공격 타입을 해당 속성으로 바꿈(`AttackTypeTileItem`).
  - 의도: 방어 타입 상성을 **배치로 맞추는 수단**. 슬라임 속성은 랜덤 소환이라 못 고르지만, 타일 위로 옮겨서 이번 wave 상성에 맞출 수 있다.
  - 범위: 유닛의 주 공격(탄·Orbit 적중, 그 탄에서 이어지는 번개 체인/낙뢰). 폭발 등 고유 속성이 정해진 부가 효과는 그대로.
- 적중형 효과는 **발사 시점에 탄에 붙인다** (`TileItem.OnFired`, 슬라임 `FireBullet`·캐릭터 `Weapon.Shoot`에서 호출). 유닛이 올라간 타일의 아이템은 `ShotModifier.tileItem`(Tile이 연결/해제)으로 찾는다. `ShotModifier`는 발사 방식 값만 두고 가볍게 유지.
  - 아군 데미지 데이터는 `AllyUnitDamageData` 하나로 통일(2026-10-23, CharacterDamageData 삭제). 치명타는 `critChance/critPower`로 적중마다 판정하며, 캐릭터 탄은 발사 시 캐릭터 치명타 스탯을 넣고 타일 치명타는 확률을 더한다.
  - 데미지 수정(치명타, 만능)은 `ModifyDamageData` → `AllyUnitDamageData` 플래그, 적중 후 효과(슬로우, 스턴, 처치 코인)는 `HitBehavior`(IBulletBehavior).
  - 탄 변형(관통, 넉백 등)도 같은 훅으로 추가하는 방향. Orbit은 탄이 없어서 `OrbitOrb.OnHit`에서 적중마다 같은 효과를 직접 적용.

## Grant 아이템 (2026-10-22 / 2026-10-23 사용 안 함)
- **2026-10-23: Grant 아이템은 쓰지 않는다. 아이템은 무조건 타일에 적용(TileItem)으로 통일.** 아래는 이전 기획 기록.
- 캐릭터가 주변 슬라임에게 버프를 주는 아이템이다. **캐릭터 칸 기준 범위 모양**은 아이템마다 다르다: AroundFour = 십자 4칸, AroundEight = 8방향, Diagonal = 대각 4칸.
- 타일 배치: 3*3 `TileGroup` 4개를 2*2로 놓은 전체 6*6 격자. `Tile.index`는 전체 기준이라 그룹 경계를 넘어 이어진다(맨 윗줄 (0,0)~(5,0), y는 아래로 증가). 단 **Grant 범위는 캐릭터와 같은 TileGroup 안에서만** 적용된다(그룹이 다르면 바로 옆 칸이라도 영향 없음).
- 캐릭터나 슬라임을 옮기면 버프 대상이 바로 바뀌어야 한다. 캐릭터를 어디 두느냐가 플레이 포인트다.
- 효과가 직관적으로 보여야 한다(범위 칸 하이라이트, 버프 받는 슬라임 표시 등. 아직 미구현).

## 웨이브 (2026-10-17 phase → wave / 2026-10-18 휴식 10초 부활)
- **2026-10-30: 무한 웨이브.** 끝 wave 없이 계속 진행, 게임은 사망으로만 끝난다. WaveType은 WaveData(CSV)가 아니라 `GameManager.GetWaveType`이 번호로 결정:
  - wave 번호(waveIdx+1)가 **20의 배수 = 보스**(`BOSS_WAVE_INTERVAL`), **5의 배수 = 엘리트**(`ELITE_WAVE_INTERVAL`), 나머지 = 일반 전투(Battle). 보스 우선.
  - (2026-11-07 확정) 보스는 20 wave마다, **보스 wave에는 엘리트가 나오지 않는다**(보스 우선 → 20/40…은 BossWave만).
  - (2026-11-07) **보스는 체력에 따른 페이즈가 없다.** 스킬(EnemyAttackPattern, 보스 자식)은 여러 개 들 수 있고 어떤 스킬을 쓸지는 보스 행동(`StartBehaviour`)이 정한다(`Boss.GetAttackPattern(key)`). BossPhase·BossPhaseChangedEvent 삭제.
  - (2026-11-09) **엘리트 처치 보상** = 주사위 2 + 골드 `(wave 번호 / 5) × 30`(wave 5 = 30, 10 = 60, 15 = 90…, `EliteEnemy.GetEliteRewardGold`). **보너스 엘리트(버튼 소환) = 주사위 5만**(골드 없음). 구분은 프리팹 `Enemy.enemyType`.
  - 엘리트 wave = 일반 전투 + 엘리트 1마리. (2026-11-07) **엘리트·보너스 엘리트·보스는 항상 Normal 방어 타입**(wave 방어 타입은 일반 적에만 적용). 갑옷·정령 엘리트 프리팹·CSV 행 삭제.
  - 보스 wave는 시간으로 안 넘어감(타이머 0에서 대기). **보스 처치 시 바로 다음 wave** (이전엔 스테이지 클리어).
  - 아래 "0~19 / 보스 처치 시 클리어" 내용은 이전 기획.
- wave 0~19 (총 20), 19 = 보스. 각 wave의 WaveType은 WaveData에 **하나로 고정**, 선택 없이 바로 진행.
- WaveType: Battle, Elite, Boss (2026-10-30, 이전 Normal/Hard/Elite/Boss). wave마다 `Wave` 인스턴스를 새로 생성(`GameManager.accWaves`에 누적).
- **2026-10-30: 휴식(break time) 없음.** UI(`WaveTimerPanel`)는 `GameManager.mainWaveTimer` 기준 현재 wave 남은 시간을 `분:초`로만 표시. 아래 휴식 관련 내용은 이전 기획.
  - 적 스폰은 wave 시작 후 **`WAVE_SPAWN_TIME`초 동안만**, 남은 시간(`WAVE_TIME`까지)은 스폰 없이 남은 적 처리.
  - `StartNextWaveButton`은 **현재 wave의 소환이 끝나면**(`Wave.isSpawnEnd`) 노출 → 남은 시간을 기다리지 않고 다음 wave 시작. 보스 wave에서는 안 나옴.
- 전투 웨이브 흐름: **웨이브 30초**(`WAVE_TIME`, 30초 내내 1초마다 1마리 스폰) → **휴식 10초**(`BREAK_TIME`) → 다음 wave.
  - 각 Wave가 자기 타이머를 가지고, 시간이 다 되면 스스로 종료(스폰 중지) → `GameManager.EndWave` → 휴식(`GameManager.isBreak`).
  - 휴식 중에는 TimeBar 숨기고 "다음 웨이브까지 N초" 카운트다운 + 다음 웨이브 시작 버튼 표시.
  - 끝날 때 필드에 남은 적은 제거하지 않고 계속 이동.
- **다음 웨이브 바로 시작(`StartNextWaveButton`, 2026-10-28)**: **한 wave의 소환이 모두 끝나면**(= wave 종료 → 휴식) 버튼이 나온다. 누르면 남은 휴식을 건너뛰고 바로 다음 wave(`GameManager.EndBreak`). 전투 중에는 안 나오므로 wave 스폰이 겹치지 않는다. 스킵 보상 골드 없음.
  - (이전 기획: 전투 wave 시작 5초 후 스킵 → 이전 wave 남은 스폰과 겹쳐 진행 + `SKIP_WAVE_GOLD`. 폐기)
  - 같은 wave의 중복 종료는 무시.
- **보스 wave**: 시간으로 끝나지 않는다. (이전: 보스 처치 시 `GameManager.EndGame(true)` → 스테이지 클리어. 지금은 무한 웨이브라 다음 wave)
- (2026-11-07) **사망 → FailCanvas**: 광고 보고 부활 = **하루 5회**(`FailCanvas.MAX_DAILY_RESURRECTION_COUNT`, PlayerPrefs에 오늘 남은 횟수 + 날짜, 날짜 바뀌면 다시 5). 광고 성공 시에만 차감, 광고 제거 구매자는 광고 없이. 취소 → ResultCanvas.
  - 광고 호출은 `AdManager.Show` 한 곳. **AdManager가 없으면(실제 광고 연결 전, StartSetting에서 생성 안 함) 광고 없이 성공 처리.**
- (2026-11-07) **결과 화면(ResultCanvas)이 나올 때** 도전 횟수(`TryStage`) +1 → 부활해도 한 판. 보상: 골드 = `waveIdx`(최종 wave 번호 - 1, 1 wave당 1골드), 경험치 = waveIdx/2, 뽑기권 = 합성 횟수. 최고 wave 기록(`EndStage`).
  - (2026-11-09) **광고 보고 보상 2배**(`ResultCanvas.OnClickedDoubleReward`, 버튼 `adDoubleRewardButton`): 기본 보상은 화면이 열릴 때 바로 지급, 광고 성공 시 **같은 양을 한 번 더**(경험치·뽑기권·골드 모두) → 패널 값 2배로 갱신(글씨가 커지는 순간 새 값으로 바뀌고 원래 크기로 돌아오는 이펙트, `RewardPanel.PlayValueChange`), 버튼 숨김. 결과 화면당 1번, 받을 보상이 없으면 버튼 없음. 광고 제거 구매자는 광고 없이. 최대 레벨이면 경험치는 0 그대로.
- (2026-11-09) **로비 복귀 시 경험치 바 연출**: 결과 화면에서 받은 경험치(2배 포함, `UserMyInfoManager.unshownGainExp`, 저장 안 함)를 로비 `MyInfoPanel` 바가 **받기 전 값에서 차오르는 모습**으로 보여준다(페이드가 끝난 뒤 시작, 바 한 칸 1초 기준). 바가 가득 차면 레벨업 창 → 닫으면 새 레벨 빈 바에서 남은 경험치만큼 다시 차오름(레벨마다 반복). 연출 중엔 경험치 이벤트로 바가 튀지 않게 막음.
- (2026-11-09) **유저 레벨 최대 30**(`UserMyInfoManager.MAX_LEVEL`, 업데이트로 올릴 수 있음). 최대 레벨이면 경험치를 받지 않고 **0으로 유지**(결과 화면 경험치 보상도 없음, 도달 시 남은 경험치는 버림) → 최대치를 올리면 0부터 다시 쌓인다. 로비 경험치 표시는 `MAX`.
- wave 종료 코인(`WaveClearGold`)·시작 코인(`StartGold`)은 기본 0, **로비 업그레이드로만** 얻는다.
- **부활(Resume)**: 필드 적 전부 제거(처치 보상 없음) → 체력 가득 → **마지막으로 종료된 wave의 다음 wave부터 다시 시작**. 그 이후 wave(스킵으로 겹쳐 진행 중이던 것 포함)는 모두 제거.
  - 예) 0 종료, 1 진행 중 스킵 → 2 진행 중 사망 → 1, 2 제거 후 **1을 처음부터**.
- Normal vs Hard: 스포너 설정(인스펙터 값)과 배율만 다르게, 코드는 공유하는 방향.

### 난이도 의도
- **소환량은 고정: 1초마다 1마리.** 소환 수를 늘리는 방식(PeakCountMul 곡선)은 폐기.
- **난이도는 적 체력으로만 올린다.** (`Enemy.GetHp`: wave별 상승만. wave 안에서 시간에 따른 체력 상승은 2026-10-17 주석 처리)
- 스폰되는 적의 체력/방어 타입은 **자기를 스폰한 wave 기준** (`Enemy.Spawn(pos, waveIdx)`). 스킵 후 이전 wave가 계속 스폰하는 적은 이전 wave 체력으로 나옴.
- 전투 웨이브의 특수 스폰(BundleWave 등 랜덤 스포너 호출)은 2026-10-16 제거. 일반 스폰만 사용.

### 적 방어 타입 (2026-10-16)
- (2026-11-08) **일반 적 방어 타입 = wave 번호가 4의 배수(`DEFENCE_WAVE_INTERVAL`)면 Ethereal → Armored 순서로 번갈아(4=정령, 8=갑옷, 12=정령…), 나머지는 Normal** (`GameManager.GetWaveDefenceType`). WaveData의 DefenceTypes/DefenceWaveTypes는 사용 안 함(아래는 이전 기획).
- (2026-10-30) 방어 타입 설정은 **WaveData/{난이도}.csv의 구간 단위**: 행마다 `StartWave`(waveIdx), `DefenceTypes`(등장 목록), `DefenceWaveTypes`(구간 시작부터 순서대로, `(wave - StartWave) % 개수`). wave에는 `StartWave <= wave`인 가장 늦은 행이 적용(예: 0/9/19 → wave 18은 9 행). 마지막 행은 무한히 반복.
- **한 wave 안에서는 모든 일반 적이 같은 방어 타입.** wave가 바뀌면 다음 칸. `Random`은 DefenceTypes 중 하나를 wave 시작 시 한 번 뽑음.
- 적의 방어 타입 기준은 **프리팹의 `Enemy.defenceType`** (2026-10-30, 이전엔 EnemyData.defenceType). 적 프리팹은 `Stage/{order}/Enemy` 폴더(하위 Elite/BonusElite 포함)에 종류×방어 타입별로 두고(엘리트·보스는 Normal만), EnemyData는 (종류, 방어 타입) 데이터가 없으면 같은 종류의 Normal 데이터를 공유(엘리트는 Normal 하나).
- **2026-10-30: 공격 속성은 물리(Physic)·마법(Magic) 두 가지 + 만능.** 진동(Vibrational) 삭제 → 진동형이던 Bounce/Pierce/Boom은 물리로, 진동 약점뿐이던 **중갑형(HeavyArmor) 방어 타입도 삭제**(enum 번호는 비워 둠: DefenceType 3, DefenceWaveType 4).
- 공격 속성(Physic/Magic) × 방어 타입 배율은 `Resources/DefenceInfo` 에셋 한 곳에서 관리 (인스펙터 표). 방어형 클래스마다 계산을 따로 두지 않는다.
  - (2026-10-30) 적 프리팹의 `EnemyDefence` 컴포넌트(방어 타입별 하위 클래스)가 `CalculateDamage`로 최종 피해 배율(표 배율·만능 무시·거인 사냥 포함)을 반환, `Enemy.TakeDamage`는 이것만 호출. 수치는 여전히 표에서 가져오고, 하위 클래스는 방어형별 특수 처리 자리. 프리팹에 컴포넌트가 없으면 `Enemy.Awake`에서 `enemyData.defenceType`에 맞는 것을 붙인다.
  - 기본값(물/마): 갑옷 0.2/2.5 · 정령 2.5/0.2 · 일반 1/1
  - (2026-11-07) **생체형(Organic) 방어 타입 삭제**(enum 번호 비워 둠: DefenceType 4, DefenceWaveType 5). 프리팹·EnemyData 에셋·CSV 행(EnemyData, WaveData 69 구간)도 삭제.
  - 방어 타입 설명(`DefenceInfo.Desc`)은 배율 표에서 자동 생성 → 표만 고치면 UI 설명도 맞춰짐. 수치 대신 `약점 : 마법형`(배율 >1, 잘 통함) / `내성 : 물리형`(배율 <1, 안 통함) 두 줄로 표기(1배는 생략).
  - **2026-10-27 의도: 방어 타입을 크게 체감되는 판단 요소로 만든다.** wave마다 "어떤 공격 속성 슬라임을 좋은 칸/킹 오라/아이템 칸에 둘지" 다시 고민하게 하는 장치. (현재 슬라임 에셋 attackType이 전부 AllRounder라 효과 없음 → 속성 부여 필요)
  - **만능(`AttackType.AllRounder`)은 방어 타입 배율 무시(항상 100%)** (2026-10-25). TrueDamageTile의 `AllyUnitDamageData.ignoreDefence`도 같은 취급 → 판정은 `DamageData.IgnoreDefence` 한 곳.

## 스테이지 (2026-10-30)
- **난이도(Normal/Hard/Hell)마다 스테이지 하나.** 스테이지 순서(order) 개념 없음, key = 난이도 이름(`Normal`/`Hard`/`Hell`).
- 에셋은 `AddressableResources/StageData/{난이도}.asset`, 파일명으로 StageData.csv 행(Difficulty)을 찾는다. 적 프리팹은 난이도 공용 `3.Prefabs/Stage/0/Enemy`.
- (2026-11-08) 해금: Normal은 항상, 그 외는 **한 단계 낮은 난이도에서 wave 100(표시 번호) 이상 도달**(`StageData.UNLOCK_WAVE`, 저장값 `maxWave`는 waveIdx라 99 이상). Normal 100 → Hard, Hard 100 → Hell. 잠긴 난이도는 시작 버튼을 눌러도 토스트만.
- (2026-11-09) 난이도 선택 창(`SelectDifficultyCanvas`): 버튼마다 이름(`Difficulty.{난이도}`) + 설명(잠김 = 해금 조건 / 열림 = 최고 기록 wave, 기록 없으면 "기록 없음"). 잠긴 난이도도 선택은 가능(시작만 막힘). 닫기 버튼은 코드에서 연결.
- 이전 저장 데이터의 `{난이도}_0` 기록은 새 key로 옮기고, `_1~_4` 기록은 버린다(`UserStageManager.MigrateStageKeys`).

## 로비 상점 (2026-11-07)
- **상품 구분(ProductData key)은 패널이 아니라 버튼마다**(`ProductPurchaseButton.key`). 한 패널에 10회·30회·광고 버튼처럼 상품이 여러 개.
- 버튼 클릭 → `ProductManager.TryToBuy`(재화 = 잔액 확인 후 차감 / 인앱 = 결제 완료 / 광고 = 시청 완료, 광고 제거 구매자는 광고 없이) → **구매 기록(버튼 key)** → `onPurchased`(인스펙터에서 패널 함수 등록, 뽑기는 `Purchased(int)`에 횟수) → 상위 `ProductPanel.UpdatePanel`.
- 하루 구매 제한은 버튼 `dailyLimit`(0 = 없음, 오늘 구매 기록 수로 판단). 광고 뽑기 = 광고 상품 + dailyLimit (이전 Normal…ProductPanel의 광고 전용 코드는 삭제).
- 특별 뽑기 카운트(N회마다 A등급 이상)는 **Unique 뽑기 패널 하위 버튼의 구매 횟수 합만** (`DrawProductPanel.PurchaseCount`, specialDrawInterval > 0인 패널만 사용).
- `ProductContainer.productPanels`를 비워 두면 하위 패널 전부.
- 뽑기(`DrawSO.Draw`): 확률표 등급 중 **아이템이 없는 등급은 빼고 남은 확률로** 뽑는다(지금 SS 슬라임 없음). 슬라임 등급 목록은 `SlimeManager.gradeGroupGrowth1SlimeDic`(로드 시 growth 1 슬라임으로 채움).
- 뽑기 결과 창(DrawSlimeResultCanvas / DrawAuthorityResultCanvas, 공용 `DrawResultCanvas`): 카드(`DrawResultCard`)를 **5열 그리드**(30장이면 스크롤)에 깔고 순서대로 뒤집어 공개(간격 `revealInterval` 0.12초, 뒤집기 0.3초, DOTween·실시간). **S 등급 이상(`revealShakeGrade`)은 뒤집기 전에 0.5초 흔들림**, 흔들리는 동안 다음 카드는 기다림. **닫기 버튼(`closeButton`, 결과 패널 하단)은 전부 공개된 뒤에만 보임.** 카드는 `cardTemplate`(뒷면 + 앞면 = 슬라임 칸 / 권능 썸네일)을 복사해서 씀.
- 결과 창이 씬에 없으면 지급 후 토스트만. 패널 `LastDrawKeys` = 마지막 결과.
- 테스트 `DrawShopPlayTest`: 실제 뽑기 버튼 6개(재화 차감·지급 개수·기록), 광고 뽑기 하루 3회, 고급 슬라임 상자 N회차 S 이상, 슬라임 관리 화면(모든 슬라임 1칸씩·보유 먼저·미보유 표시).
- 출시 전이라 이전 패널 key 구매 기록은 옮기지 않음.
- 재화 상품(`CurrencyProductPanel` → `DiaProductPanel`/`GoldProductPanel`): 패널 `amount`만큼 지급, 제목은 `{재화} {amount}` 자동. 상품 가격은 CSV에서.
- (2026-11-07) 상품 데이터 = `Json/ProductData.csv`(key, productId, currencyType, price) → DataTableImporter(`Tools/Data/Import ProductData`, Play 시 자동) → `Resources/ProductData.asset`(`ProductDataTable`) → `ProductManager.Awake`에서 불러옴(씬 인스펙터 `productDatas`는 무시). price는 float: 재화 상품 = 개수(정수), 인앱 = 스토어 가격을 못 받을 때 표시용 달러. **인앱 상품은 productId가 있어야 결제**(없으면 재화 차감으로 처리돼 구매 불가, `ProductDataTableTest`가 검사).

## 서버 (Firebase RTDB, 2026-11-05)
- 인스턴스 URL은 `FirebaseDatabaseConfig` 한 곳에서 관리(asia-southeast1). 랭킹 = 인스턴스 1개, 유저 데이터 = 여러 인스턴스(샤드, 나중에 확장).
- **유저 샤드는 처음 저장할 때 배정 후 고정**(랭킹 인스턴스 `shardMap/{uid}` = 인덱스). uid 해시 % N 방식은 인스턴스 추가 시 기존 유저 배정이 바뀌어서 안 씀. 새 유저만 `ASSIGNABLE_SHARDS` 안에서 uid 해시로 골고루. `USERDATA_URLS`는 뒤에 추가만(순서 변경·삭제 금지).
- 유저 데이터는 **항목별 저장**: `users/{uid}/data/{항목}` = JSON 문자열(항목 = 각 매니저 `UserDataFileName`), `users/{uid}/savedAt`. 저장 시 바뀐 항목만 올림, 불러오기는 서버에 있는 항목만 덮어씀.
- Dev 서버는 `DEV_SERVER` define으로 같은 프로젝트의 dev 인스턴스 사용(URL 미정).

## 멀티플레이 (2026-11-07 기획 메모, 아직 구현 안 함)
- **싱글은 그대로 유지하고, 멀티는 나중에 추가 기능으로** 붙인다. 지금은 기록만, 작업하지 않는다.
- 방식: **비동기 대전, 최대 8인 참여.** 실시간 서버(Photon 등) 없이 Firebase로 처리하는 방향(Photon 자체 호스팅(EC2)은 초기 규모에선 비용 이득 없음).
- 다른 플레이어의 상태를 UI로 확인할 수 있다. 기록(공유)할 항목:
  - 어떤 타일에 어떤 슬라임을 배치했는지(key, 머지 레벨 등)
  - 어떤 타일에 어떤 타일 아이템을 설치해 두었는지
  - 현재 wave, 킹 체력
  - 생존 / 사망 여부

## 랭킹 (2026-11-05)
- Firebase Realtime Database, **난이도별 랭킹**(`rankings/{난이도}/{uid}` = nickname, wave, updatedAt). 기준은 그 난이도 최고 wave(`UserStage.maxWave + 1`), 같은 wave면 먼저 달성한 사람이 위. 플레이 기록 없는 난이도는 안 올림.
- 화면은 **상위 100명**, 내 순위가 밖이면 `100+`. 닉네임이 기본값(GUID)이면 `Guest#uid앞4자리`로 올린다(닉네임 변경 기능 생기면 그대로 반영).
- RankingButton: 로그인 안 돼 있으면 FirebaseLoginCanvas → 로그인 성공 시 RankingCanvas.
- 갱신: 열 때마다 내 기록을 먼저 올리고(서버보다 높을 때만 씀) 목록은 **난이도별 10분 캐시**(캔버스를 껐다 켜도 유지). 단 내 최고 wave가 올라 업로드한 경우는 바로 다시 불러온다.

## 테스트 (2026-11-07)
- **테스트는 메인 에디터가 아니라 테스트 전용 복사본(`~/KingSlime-Test`, git worktree)에서 돌린다** (2026-11-08). `Tools/run-tests.sh [필터]` = 메인의 커밋 + 작업 중 변경을 복사본에 동기화(`Tools/sync-test-project.sh`) → 복사본 에디터가 열려 있으면 그 에디터에서, 아니면 배치모드로 실행. 백그라운드로 돌리고 끝나면 결과를 알려준다. 메인 씬·저장 파일을 건드리지 않으므로 작업 중에도 가능.
- EditMode 테스트는 `Assets/Tests/Editor/`(asmdef 없음 → `Assembly-CSharp-Editor`라서 게임 코드를 바로 참조). 게임 스크립트에 asmdef가 없으므로 테스트에도 asmdef를 만들지 않는다.
- 기획 규칙은 **씬·싱글톤 없이 계산할 수 있는 static 함수**로 빼서 테스트한다(예: `Slime.CanMerge`, `SacrificeSlime.CanSacrificeMerge`, `DrawItemButton.GetDrawChance(int)`). 기획이 바뀌면 `GameRuleTest.cs`의 기대값도 같이 고친다.
- 플레이 테스트(`SlimePlayTest`): EditMode 테스트에서 InGame 씬을 열고 `EnterPlayMode`로 들어가 실제 게임을 돌린다(PlayMode asmdef 없이). 로비 없이 InGame 씬만으로 시작 가능. 로드된 **모든 SlimeData key × 머지 레벨 0/4/4+진화**를 하나씩 소환해 배속(×4)으로 싸우게 하고 에러·예외가 없는지 본다. 새 슬라임은 SlimeData 에셋만 있으면 자동으로 대상이 된다.
  - 에디터용 Firebase 네이티브 라이브러리가 없어 플레이마다 `DllNotFoundException: FirebaseCppApp`이 난다 → 테스트에서는 무시 목록(`IGNORE_ERRORS`)으로 거른다.
  - 사망 시 `EndGame`에서 유저 데이터가 저장되므로 테스트 중엔 킹 체력을 계속 채워 죽지 않게 한다.
  - 콘솔 Error Pause가 켜져 있으면 에러에서 멈추므로 테스트가 일시정지를 풀면서 진행한다.
  - **테스트 중엔 광고·인앱 결제를 띄우지 않고 바로 성공**(`AdManager.SkipAdForTest`, `ProductManager.SkipInAppPurchaseForTest`). 테스트 기본 클래스가 플레이 진입 후(`WaitGameStart`/`WaitLobbyStart`) 켜고 TearDown에서 끔. 기본값 false라 실제 게임엔 영향 없음.
  - 공용 준비는 `InGamePlayTestBase`(씬 열기, 에러 수집, 저장 파일 `Assets/User*.json` 백업·복원 — 플레이 중 업적 처치 수 등이 저장되기 때문).
  - `TileItemPlayTest`: 타일 index 6*6 중복 없음, 모든 타일 아이템 설치→전투→판매, 스탯 타일 효과, 버프 범위 확장(대각선 버프). `DrawItemButtonPlayTest`: 뽑기 버튼 성공/실패/코인 부족 흐름(실패는 `Random.InitState` 시드로 고정).
  - `WaveResultPlayTest`: wave를 강제로 넘겨(`waveTimer`/`mainWaveTimer` = 0) 엘리트·보스 주기, 보스 wave는 시간으로 안 넘어감, 보스 처치 → 다음 wave + 종료 코인. 사망 → 실패 화면 → 결과 화면 보상(골드 = waveIdx, 경험치 = waveIdx/2, 뽑기권 = 합성 횟수)·최고 wave 기록·홈 → 로비. 부활 흐름.
  - `EvolutionPlayTest`(2026-11-09): 진화 타일 흐름(최대 레벨만 진화·옮겨도 유지·타일 재사용·슬라임 위 설치 시 진화·희생/성장/조커 제외·새 소환은 미진화) + 각성 효과(상태 값, 발사 탄 스펙, 가시방패·화염 스킬 실제 피해, 포위 진형 버프, 시체수집 엘리트 보너스).
  - `GameCyclePlayTest`(2026-11-09): 로비 KS 업그레이드(버튼) → 전투 버튼(에너지 차감) → 인게임 스탯 반영 → 사망·부활(보상 없음) → 사망·포기 → 결과 보상 → 로비 레벨업 창(레벨마다 하나씩, 2레벨 연속) → 재업그레이드 → 2판 wave 0 사망(보상 0, 최고 기록 유지). 단계마다 UserDataManager 메모리 값과 **저장 파일(SaveManager.LoadData)** 을 같이 확인.
  - **도메인 리로드 후 테스트 이터레이터는 직렬화로 복원되는데 람다가 캡처한 지역 변수(클로저)는 null이 된다** → 플레이 모드 진입 뒤 본문은 별도 이터레이터(`XxxBody()`)로 분리.
- 새 데이터 추가 시 주의 (테스트로 잡힌 실수):
  - ItemData 에셋을 **복사해서 만들면 key·itemPrefab이 원본 그대로** 남는다(`ItemData.LoadData`는 기존 key로 CSV 행을 찾고 프리팹은 안 채움) → key와 itemPrefab을 직접 바꾸고 프리팹도 만들 것.
  - 슬라임 데이터 에셋은 **`{Key}SlimeData`(EnhanceSlimeData 계열)** 로 만들어야 LoadData가 SlimeEnhance CSV를 읽는다. 기본 `SlimeData`로 만들면 강화 데이터가 비어 소환 시 NullReference.

## 데이터 파이프라인
- (2026-11-08) **번역**: `Json/Translate.csv`(시트 Key / Kr / En) → DataTableImporter(`Tools/Data/Import TranslateData`, Play 시 자동) → `Resources/TranslateData.asset` → `TranslateManager.GetText`가 처음 호출될 때 로드. 한국어 기기 = Kr, 그 외 = En, 칸이 비면 Kr로 대체, key가 없으면 `*key`. 코드에서 보여주는 문구는 한글 리터럴 대신 key로(조합 key: `Difficulty.{난이도}`, `Slime.{Key}`/`Slime.{Key}.Desc`, `{StatType}_title`, `Grade.{등급}`). `TranslateDataTest`가 코드 key가 시트에 다 있는지 검사. 씬에 박힌 고정 텍스트는 아직 `TranslateText` 미적용. 슬라임 설명 중 CSV Desc를 그대로 쓰는 종(Bounce, Orbit 등)과 아이템 이름·설명(ItemData.csv)은 아직 번역 안 됨.
- **SlimeData 배열 필드(bounces, shootCounts 등)는 에셋에 직렬화된 값이 코드 초기값보다 우선.** 코드 기본값만 바꾸면 기존 에셋엔 반영 안 됨 → 에셋도 같이 수정.
- 기획 데이터는 `Assets/Json/**/*.csv`(엑셀 → TSV). 에디터에서 `LoadData`(DataTableImporter)를 실행해야 에셋(`AddressableResources`)에 반영됨.
  - WaveData/{난이도}.csv: StartWave, DefenceTypes, DefenceWaveTypes (WaveType은 번호로 결정, 위 웨이브 섹션)
  - (EnemyPatternData/{난이도}.csv는 2026-10-16부터 코드에서 사용 안 함. 일반 스폰은 1초마다 1마리 고정)
  - SlimeData.csv: Key, Desc, Grade, AttackType(enum 이름 그대로: Physic/Magic/AllRounder), Growth, **AttackSpeed, AttackRange, CritChance**(2026-11-06 SlimeEnhance에서 이동. **강화 레벨·머지 레벨과 무관한 고정 기본 스탯** → `SlimeData.baseSlimeStats`, CritChance는 %), ReinforceAttackPower, ReinforceAttackSpeed(코드 미반영). 슬라임 치명타 배율은 `GameSetting.INIT_CRIT_POWER`(1.5, 타일 치명타는 2배).
  - SlimeEnhance/SlimeEnhance_{등급}.csv (2026-11-05, 시트도 등급별 탭, SheetDownloadSetting.entries에 각각 등록): `Key`는 블록 첫 행에만 쓰고 아래 행은 비우면 위 Key를 이어받음. 읽을 때는 등급과 상관없이 `SlimeEnhance_*.csv` 전체에서 key 블록을 찾음(같은 key가 두 파일에 있으면 경고). **Key / Level / AttackPower 만** 읽음(시트 범위 A:C, EnhanceAbility 열은 2026-11-09 삭제)(공속·사거리는 SlimeData.csv)(`/`로 mergeLv 값).
  - (2026-11-04) SlimeStatType은 StatType으로 통합. 슬라임 고유 스탯(`uniqueSlimeStats`)은 CSV가 아니라 **각 슬라임 SlimeData의 `LoadData` 재정의**에서 `SetUniqueStat`으로 입력.
