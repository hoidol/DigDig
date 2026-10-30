# DigDig 기획 노트

Claude가 기획 의도를 유지하기 위한 문서. 기획이 바뀌거나 사용자가 새 의도를 설명하면 이 파일을 갱신한다.
코드에서 바로 알 수 있는 구현 세부는 적지 않고, **왜 그렇게 하는지(의도)** 위주로 적는다.

## 핵심 루프
- 적 처치 → 코인 획득 → 슬라임 소환(뽑기) → 칸이 차면 합성 → 빈 칸에 다시 소환
- 돈이 계속 쌓이는 만큼 계속 강해질 수 있어야 한다.
- **2026-10-27 문제의식: 강해지는 과정이 일방향이라 고민할 게 없다.** 목표는 **고민하고 전략적으로 배치하는 재미**.
  - 새 기능은 "이걸로 플레이어가 무엇을 고민하게 되는가(배치·선택의 트레이드오프)"를 기준으로 판단한다. 단순히 수치만 올리는 성장은 지양.
- **2026-10-28 의도: 골드 소비에 확률(약 50%) 포인트를 둔다.** 50% 확률 보상의 짜릿함(도파민). 단 순수 동전 던지기가 아니라 "안전하게 쓸지 / 걸어볼지"를 플레이어가 고르는 선택이어야 한다.

## 적 이동 (2026-10-16 기획 변경)
- 적은 플레이어를 쫓지 않고 `EnemySpawner.paths` 경로를 따라 이동 (`Enemy.pathIdx` = 다음 목표 지점).
- 경로 **마지막 지점에 도착하면 캐릭터 체력 감소** 후 사라짐 (보상 없음). → 디펜스 구조: 경로 위에서 처치해야 함.
- 스폰 위치는 `paths[0]`. `paths`가 비어 있으면 이전 방식(플레이어 추적)으로 동작.

## 슬라임 소환 / 합성
- 소환 버튼: **10코인**, 장착한 **5종 중 1종이 랜덤**으로 mergeLv 0으로 **16칸** 중 빈 칸에 등장.
- 합성 조건: **같은 key + 같은 mergeLv** 끼리만 (ice 0 + ice 0 O / ice + flame X / ice 0 + ice 1 X).
- mergeLv는 0부터 시작. 최대치는 `SlimeData.MAX_MERGE_LEVEL` (3단계 0~2 → 5~6까지 확장 중).
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
- Ice / Flame / Wacky / Bounce: mergeLv가 오를수록 한 번에 발사하는 탄 수 증가(`shootCounts`), 탄마다 **서로 다른 타겟**을 노림.
- Orbit: 발사가 없으므로 **공속 = 회전 속도**(2026-10-23). `orbitRotationSpeed × AttackSpeed / GameSetting.ATTACK_SPEED_PER_SECOND`(=50, 초당 1회 공격 기준. 슬라임·캐릭터 공격 타이머도 같은 값 사용) → 공속 강화/버프(타일 아이템, 킹 오라)가 회전 속도로 반영됨.
- 랜덤 데미지(Wacky 등): 데이터에는 **중간값만** 입력, SlimeData의 `attackPowerRandomRatio`로 `중간값 × (1 ± 비율)` 범위 결정. `3~15` 같은 범위 직접 입력은 쓰지 않는다.

## 머지 코인 강화 (2026-10-23 기획 중)
- 슬라임을 **머지 3회마다 머지 코인 1개** 획득 → 머지 코인을 소모해 인게임 강화.
- 강화 대상은 **슬라임 개체가 아니라 종(key) 단위**. 한 종을 강화하면 필드의 그 종 전체(+이후 소환분)에 적용 → 합성으로 투자가 사라지지 않음.
- 검토 중인 방향: 킹슬라임이 슬라임을 **흡수**해 그 종의 특성/강화를 얻는 형태, 머지 코인 **저축형**(모아서 쓸수록 이득).
- **2026-10-23 방향: 킹슬라임(캐릭터)의 필요성을 높인다.** 머지 코인 강화 = **킹 자신 강화 + 킹 주변 슬라임 강화(오라)**.
  - 투자는 킹에 쌓이고 주변 버프는 위치로 결정 → 슬라임 개체에 투자가 묶이지 않아 합성과 충돌 없음.
  - (Grant 아이템은 폐기됐지만, 주변 강화는 아이템이 아니라 킹 고유 능력으로 들어오는 것)

## 아이템 공통 (2026-10-23)
- **보유 개수에 따른 능력치 증가는 없다.** `Item.count`/`GetCount()` 삭제. 아이템 효과는 개수와 무관하게 고정 수치(개수 1 기준). 보유 개수는 `statMgr.itemStatDic`에서 뽑기 상한(MaxCount) 판단에만 쓴다.

## 타일 선택 / 정보 표시 (2026-10-26)
- 선택 단위는 **유닛이 아니라 Tile**. 슬라임·캐릭터·아이템 타일을 클릭하면 정보가 뜬다. 타일 밖을 누르면 해제.
- `TilePointer`는 선택된 타일 위치 표시만 담당(입력 판정은 `SlimeSpawner` 한 곳). **빈 타일(유닛·아이템 없음)은 반응 없음**: 포인터·정보 패널(`TileItemInfoPanel` 등) 모두 숨기고 버튼은 그대로(선택 해제와 같은 화면). `TileItemInfoPanel`은 **아이템이 설치된 타일에서만** 뜬다(유닛만 있는 타일은 유닛 정보만). 드래그 중엔 숨기고, 선택은 옮긴 유닛을 따라간다(머지 시 결과 슬라임 선택).

## 타일 아이템 (2026-10-22)
- 아이템 획득: `DrawItemButton`을 누르면 **TileItem 중에서만** 랜덤 1개를 뽑아 바로 랜덤 타일에 설치한다(선택 UI 없음).
  - 뽑기 가격(2026-10-25): **처음 10코인, 뽑을 때마다 +2** (`INIT_DRAW_ITEM_PRICE`/`INCREASE_DRAW_ITEM_PRICE`, 횟수는 `GameManager.drawItemCount`).
  - 성공 확률(2026-10-28): **처음 100%, 성공할 때마다 -5%** (최소 10%, `INIT/DECREASE/MIN_DRAW_ITEM_CHANCE`). 코인은 먼저 내고 실패하면 코인만 소모, 가격·확률은 그대로. → 골드 소비의 확률 포인트(뒤로 갈수록 "걸어볼지" 고민).
- 타일 아이템(`TileItem`)은 획득하면 랜덤 타일에 설치되고, 그 타일에 슬라임이나 캐릭터가 올라와 있는 동안만 효과가 적용된다(`OnEnter`/`OnExit`).
- 아이템 제거(2026-10-28): 아이템 타일을 선택하면 `TileItemInfoPanel`의 `SellTileItemButton`으로 그 타일 아이템 객체 하나만 판매 → **10코인 고정**(`SELL_TILE_ITEM_PRICE`). 같은 key의 다른 타일 객체는 유지.
- **타일 하나에 아이템은 하나만** 설치한다. 설치할 때 위에 오브젝트가 있어도 되고, 그러면 바로 적용된다.
- 수치는 **타일(객체)마다 고정**이다. 같은 아이템을 또 얻으면 count를 올리지 않고 **객체를 하나 더** 만들어 다른 타일에 설치한다.
- 스탯 버프형(공격력, 공속, 사거리)과 발사 방식형(BurstShot, MultiTarget, CycleRandomShot) 모두 슬라임과 캐릭터에 적용된다.
  - BurstShot: 공격 후 **같은 방향으로 연발**(`COMBO_ATTACK_INTERVAL_MS` 간격). `BurstShotTileItem.OnFired`가 쏜 탄과 같은 탄(스펙·위치·방향·데미지)을 지연 후 다시 생성한다. 다중 타겟 탄도 탄마다 각각 연발. 추가 탄은 OnFired를 안 거쳐 연발이 연발을 만들지 않음.
  - MultiTarget: **다중 타겟** = `Slime.ShootCount`에 탄 수를 더한다(탄마다 서로 다른 타겟).
  - CycleRandomShot: **3번째 공격마다 랜덤 타겟**.
- 2026-10-23 목록 (Key = 클래스명에서 `Item` 뺀 것, ItemData.csv에 행 추가):
  AttackPowerTile(공 +50%), AttackSpeedTile(공속 +30%), BurstShotTile, MultiTargetTile, RangeUpTile(사거리 +2),
  SlowTile(10% 슬로우), StunTile(2% 0.3초 스턴), CriticalTile(10% 2배), TrueDamageTile(방어 배율 무시 → 항상 100%),
  GoldTile(처치 시 코인 +1), PioneerTile(획득 즉시 막힌 칸 1개 해제, 설치 안 함, 막힌 칸 없으면 안 나옴), BerserkTile(공 ×2, 사거리 ×0.5), GiantSlayerTile(엘리트·보스에게 데미지 ×2, 방어 배율과 곱연산).
- 공격 타입 변경 타일 (2026-10-27): PhysicTile / MagicTile / VibrationalTile = 올라간 유닛의 공격 타입을 해당 속성으로 바꿈(`AttackTypeTileItem`).
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
- wave 0~19 (총 20), 19 = 보스 (`GameSetting.BOSS_PHASE`). 각 wave의 WaveType은 WaveData에 **하나로 고정**, 선택 없이 바로 진행.
- WaveType: Normal, Hard, Elite, Boss. wave마다 `Wave` 인스턴스를 새로 생성(`GameManager.accWaves`에 누적).
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
- **보스 wave**: 시간으로 끝나지 않는다. **보스 처치 시 `GameManager.EndGame(true)`** → 스테이지 클리어.
- **부활(Resume)**: 필드 적 전부 제거(처치 보상 없음) → 체력 가득 → **마지막으로 종료된 wave의 다음 wave부터 다시 시작**. 그 이후 wave(스킵으로 겹쳐 진행 중이던 것 포함)는 모두 제거.
  - 예) 0 종료, 1 진행 중 스킵 → 2 진행 중 사망 → 1, 2 제거 후 **1을 처음부터**.
- Normal vs Hard: 스포너 설정(인스펙터 값)과 배율만 다르게, 코드는 공유하는 방향.

### 난이도 의도
- **소환량은 고정: 1초마다 1마리.** 소환 수를 늘리는 방식(PeakCountMul 곡선)은 폐기.
- **난이도는 적 체력으로만 올린다.** (`Enemy.GetHp`: wave별 상승만. wave 안에서 시간에 따른 체력 상승은 2026-10-17 주석 처리)
- 스폰되는 적의 체력/방어 타입은 **자기를 스폰한 wave 기준** (`Enemy.Spawn(pos, waveIdx)`). 스킵 후 이전 wave가 계속 스폰하는 적은 이전 wave 체력으로 나옴.
- 전투 웨이브의 특수 스폰(BundleWave 등 랜덤 스포너 호출)은 2026-10-16 제거. 일반 스폰만 사용.

### 적 방어 타입 (2026-10-16)
- StageData.csv `DefenceTypes`: 그 스테이지에 나오는 방어 타입 목록. `DefenceWaveTypes`: wave마다 순서대로 적용(`wave % 개수`).
- **한 wave 안에서는 모든 일반 적이 같은 방어 타입.** wave가 바뀌면 다음 칸. `Random`은 DefenceTypes 중 하나를 wave 시작 시 한 번 뽑음.
- 적의 방어 타입 기준은 **프리팹의 `Enemy.defenceType`** (2026-10-30, 이전엔 EnemyData.defenceType). 적 프리팹은 `Stage/{order}/Enemy` 폴더(하위 Elite/BonusElite 포함)에 종류×방어 타입별로 두고, EnemyData는 (종류, 방어 타입) 데이터가 없으면 같은 종류의 Normal 데이터를 공유(엘리트는 Normal 하나).
- 공격 속성(Physic/Magic/Vibrational) × 방어 타입 배율은 `Resources/DefenceTable` 에셋 한 곳에서 관리 (인스펙터 표). 방어형 클래스마다 계산을 따로 두지 않는다.
  - (2026-10-30) 적 프리팹의 `EnemyDefence` 컴포넌트(방어 타입별 하위 클래스)가 `CalculateDamage`로 최종 피해 배율(표 배율·만능 무시·거인 사냥 포함)을 반환, `Enemy.TakeDamage`는 이것만 호출. 수치는 여전히 표에서 가져오고, 하위 클래스는 방어형별 특수 처리 자리. 프리팹에 컴포넌트가 없으면 `Enemy.Awake`에서 `enemyData.defenceType`에 맞는 것을 붙인다.
  - 기본값(2026-10-27): 갑옷 물0.2/마2.5/진1 · 정령 2.5/0.2/1 · 중갑 0.2/0.2/2.5 · 생체 1.5/0.5/0.5 · 일반 1/1/1
  - 방어 타입 설명(`DefenceInfo.Desc`)은 배율 표에서 자동 생성 → 표만 고치면 UI 설명도 맞춰짐. 수치 대신 `약점 : 마법형`(배율 >1, 잘 통함) / `내성 : 물리형`(배율 <1, 안 통함) 두 줄로 표기(1배는 생략).
  - **2026-10-27 의도: 방어 타입을 크게 체감되는 판단 요소로 만든다.** wave마다 "어떤 공격 속성 슬라임을 좋은 칸/킹 오라/아이템 칸에 둘지" 다시 고민하게 하는 장치. (현재 슬라임 에셋 attackType이 전부 AllRounder라 효과 없음 → 속성 부여 필요)
  - **만능(`AttackType.AllRounder`)은 방어 타입 배율 무시(항상 100%)** (2026-10-25). TrueDamageTile의 `AllyUnitDamageData.ignoreDefence`도 같은 취급 → 판정은 `DamageData.IgnoreDefence` 한 곳.

## 데이터 파이프라인
- **SlimeData 배열 필드(bounces, shootCounts 등)는 에셋에 직렬화된 값이 코드 초기값보다 우선.** 코드 기본값만 바꾸면 기존 에셋엔 반영 안 됨 → 에셋도 같이 수정.
- 기획 데이터는 `Assets/Json/**/*.csv`(엑셀 → TSV). 에디터에서 `LoadData`(DataTableImporter)를 실행해야 에셋(`AddressableResources`)에 반영됨.
  - WaveData/{난이도}.csv: Phase, WaveType(phase당 1개)
  - (EnemyPatternData/{난이도}.csv는 2026-10-16부터 코드에서 사용 안 함. 일반 스폰은 1초마다 1마리 고정)
  - SlimeData.csv: Key, Desc, Grade, AttackType(enum 이름 그대로: Physic/Magic/Vibrational/AllRounder)
  - SlimeEnhance/{key}.csv: 강화 레벨별 AttackPower/AttackSpeed/AttackRange(`/`로 mergeLv 값) + 고유 스탯 컬럼
