# DigDig 기획 노트

Claude가 기획 의도를 유지하기 위한 문서. 기획이 바뀌거나 사용자가 새 의도를 설명하면 이 파일을 갱신한다.
코드에서 바로 알 수 있는 구현 세부는 적지 않고, **왜 그렇게 하는지(의도)** 위주로 적는다.
- **사용자에게 답할 때는 항상 한글로 답한다.**

## 클래스 이름 규칙 (2026-11-09)
- `Manager` = 씬을 오가며 살아 있음(데이터·유저). `Controller` = 한 씬에 배치된 싱글톤이 그 씬의 한 영역을 맡음(`MinionFieldController`, `EnemyFieldController`, 이전 이름 MinionSpawner/EnemySpawner). `Spawner` = 생성만 하는 부품. `System` = 풀링 또는 static 규칙 묶음(`PoolingSystem`, `AuthoritySystem`).

- (2026-11-13) **소환 유닛의 코드 이름 = `Minion`(이전 `Slime`).** 소환 유닛을 슬라임으로 한정하지 않고 귀여운 악마·귀신·도깨비 등 여러 형태로 낼 예정이라 형태를 가리키지 않는 이름으로 바꿈. 층: `Ally` = 아군 전체(킹 + 소환 유닛), `KingSlime` = 킹(이름 그대로 유지), `Minion` = 소환·합성하는 유닛. 클래스·파일·폴더·씬 오브젝트 이름·데이터 key(`Minion.{Key}`, `DrawMinion_*`, `MaxMinionCount`, 업적 `SpawnMinion` 등)·저장 파일(`UserMinionData`)까지 전부 바꿈. **이 문서의 한글 "슬라임"과 화면에 보이는 글씨(번역 Kr/En)는 아직 그대로**(표시 이름은 따로 정함). 적 몬스터 프리팹 `Slime_Green`/`Slime_Purple`과 Layer Lab 에셋은 대상 아님.
- (2026-11-19) **미니언 표시 이름 = 캐릭터 이름(애칭)**: 종족·능력에서 따온 부르기 쉬운 이름(Random = 뭉치, Bounce = 콩콩이, Hunter = 저승이). "주사위 그렘린" 같은 설명형 이름은 쓰지 않음. 종족은 `Assets/ImageList.xlsx` 미니언 시트 '추천 종류' 기준이되 **실제 그림이 다르면 그림 기준**(조커 = 카드 광대, 다크니스 = 그림자, 지옥 = 용암 망령, 수확 = 동전 도깨비, 성장 = 알). 화면 이름은 번역 `Minion.{Key}`(Kr/En), MinionData.csv `Name` 열에도 같은 한글 이름(코드에서는 안 읽음).

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
- (2026-11-08) **적 애니메이션**: 적 프리팹 자식의 스프라이트 프리팹(Layer Lab) Animator. **Walk가 기본**(스폰·풀 재사용 시 Walk부터). **처치되면 Dead 재생 → 끝나기 0.1초 전(`DEAD_ANIM_END_EARLY_TIME`, 최소 1프레임 전)에 비활성화 후 풀로**(Dead 클립이 루프여도 1회, 최대 `Enemy.DEAD_ANIM_MAX_TIME`). Dead 중엔 충돌·이동·피해 없음, `EnemyDeadEvent`(보상·처치 감지)는 죽는 즉시 발행되고 활성 적 수에서도 바로 빠진다. **경로 끝 도착은 Dead 없이 바로 비활성화.** Dead 상태가 없는 프리팹(현재 보스)은 바로 비활성화. 테스트 `EnemyAnimPlayTest`.
- (2026-11-10) **적 머리 위 체력바**: InGame 씬 `EnemyWorldCanvas`(World Space) 하위의 `EnemyHpUI`(HpBarBg, `thumImage` = Filled 바) 하나를 템플릿으로 적마다 복사·풀링. 스폰 시 붙고 `HpPoint` 위치를 따라감, 피해마다 갱신, **Dead 시작·경로 끝·회수 시 숨김**. 테스트 `EnemyHpUIPlayTest`.
  - (2026-11-20) **BossCanvas 삭제 → 보스도 머리 위 체력바.** 엘리트·보너스 엘리트·보스(`Enemy.UseBigHpUI`)는 **큰 체력바 `Resources/UI/BigEnemyHpUI`**(`EnemyWorldCanvas.bigHpUITemplate`, 셋 다 같은 크기, 체력 숫자 글씨 `hpText` 표시, 따로 풀링). 일반 적은 `Resources/UI/EnemyHpUI`(글씨 없음).

## 슬라임 소환 / 합성
- 소환 버튼: **10코인**, 장착한 **5종 중 1종이 랜덤**으로 mergeLv 0으로 **16칸** 중 빈 칸에 등장.
- (2026-11-16) **등장 연출**: 미니언이 생길 때마다(소환·합성 결과·성장/변신 교체 등 `Minion.Spawn`) 몸(`Root`)이 0.6 위에서 떨어져 착지하며 납작해졌다 통통 돌아옴(DOTween, `PlaySpawnDrop`). 미니언 위치가 아니라 몸만 움직여 칸 이동·드래그와 겹쳐도 어긋나지 않음. **머지 결과는 떨어지지 않고 제자리에서 0.35 살짝 뛰었다 착지**(`PlayMergeHop`, `MinionFieldController.Merge`).
- 합성 조건: **같은 key + 같은 mergeLv** 끼리만 (ice 0 + ice 0 O / ice + flame X / ice 0 + ice 1 X).
- (2026-11-15) **머지 이펙트**: 머지할 때마다 결과 미니언 위치에 `Resources/Effect/MergedEffect`(EffectType.Merge, `MinionFieldController.Merge`). 자주 나오는 연출이라 화려하지 않게: 파티클은 원·스파크 하나만, 대신 **귀여운 글씨 "레벨업!"**(`LevelUpText`, 힙피 폰트 + `Font/hippie SDF Outline` 머티리얼, `PopTextEffect` = DOTween으로 튀어나옴 → 출렁 → 둥실 → 사라짐).
- (2026-11-09) **머지가 안 되는 슬라임 위에 놓으면 두 슬라임 자리 교체**(`MinionFieldController.SwapMinions`). 타일 아이템 효과도 칸을 따라 바뀜. 이전엔 원위치. 테스트 `MergeSwapPlayTest`.
- (2026-11-11) **킹슬라임도 슬라임과 자리 교체**: 킹을 슬라임 위에 놓거나 슬라임을 킹 위에 놓으면 맞바꿈(`MinionFieldController.SwapTileObjects`, `SwapMinions`도 이걸 씀). 테스트 `MergeSwapPlayTest.KingSlime_SwapWithMinion`.
- mergeLv는 0부터 시작. 최대치는 `MinionData.MAX_MERGE_LEVEL` = 4 (0~4, 5단계).
- 합성 결과 key: 현재 코드는 장착 5종 중 **랜덤** (`Minion.Merge`). 기획 의도 확인 필요.
- (2026-11-20) **미니언·룬 제거는 보상 없음**(골드 개념 삭제). 미니언 제거 버튼(`SellMinionButton`) 글씨 = "제거", 가격 표시 삭제. 룬 제거(`SellTileItemButton`)도 코인 없음. **골드 소환 버튼(`MinionSpawnButton`)·인게임 강화(`InGame/Reinforce` 전체)·룬 뽑기 버튼(`DrawItemButton`) 코드·씬 오브젝트 삭제**(GameSetting의 관련 가격 상수도 주석 처리). `SpawnMinionEvent`/`RemoveMinionEvent`는 `Minion/MinionEvents.cs`로 이동. 룬 뽑기 창(`SelectItemCanvas.OpenDraw(확률)`)은 남김(사냥꾼의 부적 등). 튜토리얼 소환 블록은 버튼 없이 바로 소환(소환 버튼 보이기 블록 삭제). 권능 상인의 장부·도박꾼의 동전·장인의 망치는 지금 효과 없음(다시 기획 필요). MultiGame 씬에는 지운 스크립트가 Missing으로 남음(쓰지 않는 씬). 아래 판매 코인·소환 버튼·인게임 강화·룬 뽑기 가격 내용은 이전 기획.
- (2026-11-18) **미니언 판매**: 미니언을 선택하면 `AllySettingPanel`의 `SellMinionButton`으로 판매 → **5 + 5 × mergeLevel 코인**(mergeLevel 0부터: 0 = 5, 4 = 25, `GameSetting.SELL_MINION_*`). 킹 선택 시엔 숨김. 판매 후 선택 해제. (이전: 판매/버리기 없음)
- 게임 시작 시 타일 중 **랜덤 5칸은 사용 불가**(`BLOCKED_TILE_COUNT`, 2026-10-18). 소환/드래그/캐릭터 배치 모두 막힌 칸에는 못 놓음. (2026-11-10) 막힌 칸은 숨기지 않고 **깨진 타일 이미지**(`TileManager.breakTile`)로 보여줌, 해제되면 `baseTile`.

### 의도
- **소환 가격은 올리지 않는다.** 싸게 계속 뽑는 뽑기 재미가 핵심.
- **합성은 스탯 합계로 보면 약간 손해**여야 한다. 대신 칸이 비어서 다시 소환할 수 있다는 게 합성의 이득.
  - 칸이 남아 있으면 소환이 이득, 칸이 차면 손해를 감수하고 합성 → 이 순환이 의도.
- 강화 레벨(`userMinion.enhanceLevel`, 로비에서 영구 성장)과 머지 레벨(`mergeLevel`, 인게임 합성)은 별개.
  - 슬라임 에셋의 `level: N` 블록 = 강화 레벨, 그 안의 `values` 배열 = mergeLv별 값.
  - `values` 2개 = `기본/증가량`(선형), 그 외 = mergeLv별 값(모자라면 마지막 값).
  - (2026-11-15) **MinionEnhance CSV의 AttackPower가 값 하나면 머지 레벨에 비례**: 표시 머지 레벨 1 = 값, 2 = 2배, 3 = 3배 …(`EnhanceMinionData.ToMergeLinear`가 `값/값`으로 저장). 시트 범위는 A:C(Key/Level/AttackPower).
- (2026-11-14) **미니언 머리 위 상태(`MinionGameStatePanel`)**: 미니언마다 Canvas를 두지 않고 `GameWorldCanvas` 하위 템플릿 하나를 미니언마다 복사·풀링(`Minion.Spawn` → 붙음, `OnDisable` → 반납), LateUpdate에서 `Minion.statePoint`(프리팹에서 지정하는 기준 위치, 적 `HpPoint`와 같은 역할. 비우면 미니언 위치 + 0.9) + 패널 `offset`을 따라감. **쿨타임 스킬이 있는 미니언(`MinionData.hasSkill`, 에셋 인스펙터) = `mergeLvForCooltime`(머지 레벨 + 쿨타임 Slider `cooltimeSlider`, `Minion.SkillProgress`), 없으면 `mergeLv`(머지 레벨만)**. 머지 레벨 표시는 mergeLevel + 1. 미니언 프리팹의 별 표시(`StarContainer`)는 삭제, 프리팹 구조 = `StateTr`(y 1.5, statePoint) + `Root > BodyRoot(Animator) > Body`(SpriteRenderer, 이미지 = `2.Sprites/Minion/Growth1/{Key}`).
- (2026-11-15) **미니언 몸 애니메이션**: 미니언 프리팹 공용 컨트롤러 `5.Animation/Minion/Minion.controller`(BodyRoot Animator, 대상 = `Body`). `Minion_Idle`(기본, 스케일만 살짝 움찔 루프) · `Minion_Attack`(트리거 `Attack`, 바라보는 방향으로 아주 살짝 나갔다 복귀 0.2초, 공격 성공 시 `Minion.PlayAttackAnim`. 회전체·경로 순환 미니언(`UseFindTarget` false)은 안 함) · `Minion_Skill`(트리거 `Skill`, 회전 없이 1.2배로 살짝 커졌다 돌아옴 0.35초(2026-11-16), `PlaySkillAnim`. **지금은 Random 스킬만 사용**, 다른 스킬은 기획 후).
- (2026-11-15) **미니언 스킬** (MinionData.csv `HasSkill`(TRUE/FALSE → `MinionData.hasSkill`) + `SkillInfo` 열이 기획 원본, 수치는 각 클래스 `SKILL_*` const). 스킬 공속 버프는 각 미니언이 `AddBuff`(공속 Multiply)로 걸고 자기 `skillBuffTimer`가 끝나면 `RemoveBuff`(Random·Hunter·Dopamine), 공용 도우미 = `FindClosestTargetInRange`·`FireCircle(At)`, 진행도 = `SkillProgress`(머리 위 바). **사거리에 적이 없으면 쿨타임이 찬 채로 대기**(낭비 방지).
  - Bounce = 7초마다 5방향 도탄 원형 발사(첫 방향 = 가장 가까운 적) · Flame = 8초마다 사거리 안 적 전부 화상(**쿨타임이 차면 적이 없어도 바로 발동**, Random과 함께 대기 규칙 예외, 발동 시 미니언 위치에 `Resources/Effect/FlameEffect`(EffectType.Flame, 이전엔 씬에 놓여 있던 파티클)) · **Random = 9초마다 2초간 공속 ×3**(이전 10초 5방향 발사는 폐기, 버프 중엔 쿨타임 멈춤, **쿨타임이 차면 적이 없어도 바로 발동**) · Hunter = 공격 15회마다 4초간 공속 ×3 + 타겟 3명(**버프 중엔 저격탄 없음**, 그동안 공격은 저격 횟수에 안 셈, 2026-11-16) · Scavenger = 처치 코인 조건의 처치 20마리마다 10 + 2×머지 레벨 코인 · GrowUp = 성장 타이머(기존).
  - **Inferno = 평소 기본 탄, 12초 차면 지옥불 10초**(그동안 공격 주기마다 지옥불 피해), 지옥불이 사라지면 쿨타임 다시 참. 놓을 수 없는 칸(가운데 칸 등)이면 대기. (이전: 지옥불 항상, 탄 없음)
  - **Hail = 평소 기본 탄, 10초마다 눈 소환 → 12초 동안 경로 순환 후 사라짐**, 눈이 있어도 쿨타임은 계속 참(겹쳐서 여럿 가능). 첫 눈은 미니언과 가장 가까운 경로 지점부터, 각성 = 한 번에 2개. **눈 피해 = 공격력 × 머지 레벨별 배율 150%·170%·190%·210%·230%**(`HailMinionData.stoneDamageMuls`, 2026-11-16). 탄을 쏘므로 `UseFindTarget` true(타겟 방식 버튼·공격 모션 있음). (이전: 눈 항상, 탄 없음)
  - (2026-11-15) **스킬 강화 이펙트**: 스킬 공속 버프(Random·Hunter·Dopamine)가 켜지면 버프 시간 동안 `Resources/Effect/PowerUpEffect`(EffectType.PowerUp)가 미니언을 따라다님(`Minion.PlayPowerUpEffect(시간)`/`StopPowerUpEffect`, 미니언이 꺼지면 같이 끝남). 새로 스킬로 강화되는 미니언도 이걸 호출.
  - Berserker = 머리 위 글씨 **"중첩 N"**(`Minion.StateText` → `MinionGameStatePanel.stateText`, 번역 `Minion_Stack`) + 바에 중첩 비율(stack / 최대).
  - **Dopamine = 감지 범위 안 처치(경로 끝 제외) 30번 → 5초간 공속 ×3**(버프 중 처치는 안 셈, 추가 탄 기본 효과는 그대로) · **Stigma = 10초마다 공격 사거리 안 적 전부 낙인**(확률 없음, 각성 피해 증가량 적용, 발동 시 미니언 위치에 `Resources/Effect/StigmaEffect`(EffectType.Stigma, 이전엔 씬에 놓여 있던 파티클), 2026-11-16). (2026-11-16) **낙인이 걸린 적은 몸에 보라색 `Stigma` 표시**(적 공용 `StatusEffect.prefab` 하위, Flame과 같은 `StatusEffectView`, `StigmaEffect.EffectKey`), 적이 죽을 때까지 유지. **크기는 프리팹 = 일반 적 기준, `StatusEffectView.eliteScale`(엘리트·보너스 엘리트)/`bossScale` 배율**로 키움(값은 사용자가 조정). 보스 프리팹(`3.Prefabs/Boss/Boss`)에도 `StatusEffect.prefab`을 붙여 다른 상태이상 표시도 보스에 나옴. **MinionData.csv Evolution 열은 아직 기획 전 → 구현하지 않는다**(2026-11-15). 미니언은 돌 파괴(`DestroyedStoneEvent`)를 감지하지 않음(돌 없음, 2026-11-15), 광란 각성(추가 탄 2발) 삭제.
- Ice / Flame / Random / Bounce: mergeLv가 오를수록 한 번에 발사하는 탄 수 증가(`shootCounts`), 탄마다 **서로 다른 타겟**을 노림.
- Orbit: 발사가 없으므로 **공속 = 회전 속도**(2026-10-23). `orbitRotationSpeed × AttackSpeed / GameSetting.ATTACK_SPEED_PER_SECOND`(=50, 초당 1회 공격 기준. 슬라임·캐릭터 공격 타이머도 같은 값 사용) → 공속 강화/버프(타일 아이템, 킹 오라)가 회전 속도로 반영됨.
- 랜덤 데미지(Random 등, 2026-11-05 Wacky에서 key 변경. 저장 데이터 마이그레이션은 안 함: 필요하면 데이터 초기화): 데이터에는 **중간값만** 입력, MinionData의 `attackPowerRandomRatio`로 `중간값 × (1 ± 비율)` 범위 결정. `3~15` 같은 범위 직접 입력은 쓰지 않는다.
- (2026-11-05) 슬라임 스크립트는 **등급별 폴더**(`Minion/Unit/{C,B,A,S}/{Key}Minion/`, 데이터 스크립트는 `Minion/MinionData/{C,B,A,S}/`, 프리팹은 `3.Prefabs/Minion/{C,B,A,S}/` — `MinionData.LoadData`가 grade로 찾음). 신규 key: B = Shield(철벽)·Dopamine(광란)·Mine·Breeze·Rage·Stigma(낙인), A = Mud·Keen(설의, 2026-11-06 B→A)·Beam(광선 링크)·Berserker·Hail(빙석)·Inferno(지옥)·Mortar(박격포, 2026-11-06 Boom에서 key 변경), S = Sacrifice·GrowUp·Scavenger(시체수집)·Darkness(다크니스, 이전 Joker)·Hunter(사냥꾼, 2026-11-06)·Joker(카드, 2026-11-16 신규). 대부분 아직 뼈대(일반 탄)만 있음.
- (2026-11-09) **강화 능력(EnhanceAbility) 폐지.** 강화 레벨(0~14, 로비 영구 성장)은 유지하되 **스탯(공격력·공속, 슬라임 특성 수치)만 올린다.** 가시방패·포위 진형·화상 같은 레벨 해금 특수 능력(각성 포함)은 없다. 강화 데이터 = `EnhanceMinionData`(이전 EnhanceAbilityMinionData) + MinionEnhance CSV(Key/Level/AttackPower). 공속·특성 수치의 레벨별 증가는 아직 미구현(시트 형식 정해서 추가 예정).
- (2026-11-09) **[각성] = 진화로 발현.** 최대 머지 레벨 슬라임이 진화 타일(`EvolutionTile`, 1회용)에 올라가면 `Minion.Evolve` → `isEvolved` + `OnEvolve`로 그 슬라임의 각성 능력이 켜진다(이전 강화 8 능력). 수치는 각 슬라임 클래스 `EVOLVE_*` const(임시). 합성·교체로 새 슬라임이 되면 사라짐.
  - C: 탄성·관통 = 탄 피해 +20%, 냉기 = 탄 1회 튕김, 번개 = 맞은 적 1초 감속, 천둥 = 10% 확률 1초 감속, 강철 = 엘리트·보스 배율 +0.2, 회전 = 3% 확률 0.1초 스턴, 랜덤 = 6번째 공격마다 4방향 원형 발사, 화염 = 5초마다 사거리 안 적 최대 7기에 공격력 피해 + 화상.
  - B: 철벽 = 가시방패(막힌 적 1초마다 최대 체력 1%), 지뢰 = 2개씩 설치, 낙인 = 피해 증가 25%(2026-11-16, 이전 최대 5중첩), 십자 버프(산들바람·격노·설의) = **포위 진형**(같은 그룹 안 범위 4칸이 모두 슬라임이면 버프 증가분 +50%).
  - A: 광선 = 레이저 감속 0.5초, 버서커 = 최대 중첩 시 공격력 ×1.2, 빙석 = 눈 2개, 지옥 = 화상(2초, 초당 공격력 30%), 박격포 = 슬로우 확률 50%, 진흙 = 구슬 최대 +1.
  - S: 사냥꾼 = 2회마다 저격, 시체수집 = 엘리트·보스 +30코인. **희생·성장·다크니스는 진화하지 않음**(`CanEvolve` false: 머지 레벨을 올리는 각성이라 최대 레벨에선 의미 없음).
- (2026-11-05) C등급 신규: **Iron(강철, 물리)** = 엘리트/보스에게 데미지 ×2(`IronMinionData.eliteBossDamageMul`). 배율은 탄 스펙의 `BigEnemyDamageForce`(IBulletForce, 2026-11-06)로 실어 연발 탄에도 유지, **거인 사냥 타일과 곱연산**(×4: force가 피해량을 키우고 거인 사냥은 `DamageData.eliteBossDamageMul`로 EnemyDefence에서 곱함). 다른 IBulletForce와는 합연산. 엘리트 wave용 배치 카드.
  **Thunder(천둥, 마법)** = 탄이 날아가지 않고 발사 방향 첫 적 위치에 낙뢰 → 주변 범위 피해(최대 5기). 머지 레벨이 오를수록 범위 증가(`strikeRadiuses`). 발사 0.1초 뒤 피해(타일 효과가 OnFired로 붙은 다음).
- (2026-11-06) **Mine(지뢰, B)** = 슬라임이 있는 타일의 `TileGroup.enemyRoots`(순서대로 이은 선 = 그 그룹 근처 적 경로) 위 랜덤 지점에 지뢰를 던져 설치. 적이 사거리에 없어도 미리 깔고, 밟히면 범위 폭발. **동시에 깔린 지뢰 수 상한이 머지 레벨로 증가**(`maxMineCounts`), 상한이면 대기했다가 터지면 바로 다음 지뢰. 어느 그룹에 두느냐 = 어느 경로 구간을 막느냐가 배치 포인트. 슬라임이 합성돼 사라져도 깔린 지뢰는 남는다. 지뢰는 **탄이 아니라 독립 오브젝트 `Mine`**(MineMinion이 던짐, 자체 풀링): 폭발 범위·최대 피해 적 수·유지 시간은 MineMinionData, 트리거 범위는 고정(`Mine.TRIGGER_RANGE`). 탄이 아니라서 발사 훅(`TileItem.OnFired`)의 타일 효과는 안 붙고, 피해량·치명타 확률은 던질 때 슬라임 스탯으로 고정.
- (2026-11-06) **Stigma(낙인, B)** = 일반 탄 + 적중 시 확률(`stigmaChances`, 머지 레벨로 증가)로 낙인(`StigmaEffect`). 낙인 걸린 적이 받는 **모든 아군 피해** +`damageBonus`(15%, `Enemy.TakeDamage`에서 `StatusEffectHandler.StigmaDamageMul` 곱). **(2026-11-16) 중첩 없음**(이전 중첩당 5%·최대 3중첩). 낙인은 적이 죽을 때까지 유지. 딜러 보조형 → 다른 슬라임과 같은 적을 때리게 배치하는 게 포인트. 여러 낙인 슬라임이 섞이면 피해 증가량은 큰 값.
- (2026-11-06) **Berserker(버서커, A)** = (2026-11-16) **근접 공격(탄 없음)**: 공격 사거리(AttackRange 1.5) 안 타겟을 공격 주기마다 바로 때림(타일 적중형 효과는 타격마다 직접 적용, 연발 등 발사형은 적용 안 됨). **처치 감지 범위 = 공격 사거리**(이전 `detectRanges`·감지 범위 표시 삭제). 사거리 안에서 적이 죽을 때마다 **공속 중첩**(중첩당 `attackSpeedPerStacks`%, 최대 10). 광란(Dopamine)과 같은 처치 감지 구조. **마지막 처치 후 `stackDuration`(5초) 동안 처치가 없으면 중첩 초기화** → 적이 계속 죽는 자리(다른 딜러 옆, 경로 가까이)에 두는 배치 포인트.
- (2026-11-06) **Inferno(지옥, A)** = 탄 없음. 자기 TileGroup `enemyRoots` 중 **슬라임과 가장 가까운 2개 사이**에 지옥불(`Hellfire`, 길이 = 두 점 거리, 폭 = `hellfireWidth`) → **공격 주기마다 그 안에 있는 적 전부** 피해, 벗어나면 바로 피해 없음. **TileGroup 3*3 가운데 칸이면 지옥불 없음**(index % 3 == 1). 같은 그룹 안에서도 칸 위치가 덮는 경로 구간을 정함 → 배치 포인트. 드래그 중엔 숨김.
- (2026-11-06) **Mud(진흙, A)** = 탄 없음. **공속 = 진흙 구슬 생성 주기**(공격마다 1개, 최대 `maxOrbCounts` 머지 레벨로 증가, OrbitMachine으로 회전, 회전 속도는 고정). 구슬이 적에 닿으면 **스플래시**(`splashRadius`) 피해 + 진흙 1중첩, **(2026-11-16) 구슬은 사라지지 않고 계속 회전**(같은 적은 그 구슬에 0.5초마다 한 번, 최대 개수면 더 안 만듦). **진흙 중첩당 이동속도 5% 감소, 최대 15중첩(75%), 엘리트·보스도 같음**(이전 15%·최대 80%/60%, 구슬 소멸), 멈추지는 않음(`MudEffect`, 마지막 적중 후 `mudDuration` 지나면 전부 해제). 슬로우·빙결 중첩과는 별개로 곱함(`StatusEffectHandler.SlowRate`). 4중첩 굳음(스턴)은 다수 배치 시 보스까지 영구 속박돼 폐기(2026-11-06).
- (2026-11-06) B등급 **Dopamine(광란)** = 기본 탄 + 슬라임 주변 `detectRanges`(mergeLv별) 안에서 적이 죽으면 **추가 탄**(공격력 × `extraShotDamageMuls`, mergeLv별) `extraShotCount`발. 추가 발사 후 `extraShotCoolTime` 쿨타임, 일반 공격 타이머는 리셋하지 않음. → 적 경로 근처(처치가 잦은 칸)에 둘지 고민.
- (2026-11-06) B등급 **Shield(철벽)** = 방패(`ShieldOrbitOrb`, `SeparateOrbitMachine`으로 방패마다 따로 회전)가 닿은 적의 **이동을 멈춤**(`Enemy.AddBlocker`). 방패당 최대 `blockCountPerShield`기(기본 3), 초과분은 통과. **엘리트/보스는 막기 무시**(`Enemy.CanBeBlocked`, 슬롯도 안 차지). (2026-11-15) **피해는 막은 적에게만**: 막는 순간 1번 + 막혀 있는 동안 공격 주기(공속)마다 공격력 피해. 닿기만 하고 지나가는 적(정원 초과·엘리트·보스)은 피해 없음. 막는 동안 그 방패는 정지, 막던 적이 모두 죽거나 빠지면 다시 회전. → 경로 근처에 두면 적을 붙잡아 다른 슬라임이 처리.
- (2026-11-06) A등급 **Beam(광선 링크)** = 필드(타일 위)의 광선 슬라임끼리 **모두 레이저 연결**(`BeamLinkNetwork`, n마리 → n(n-1)/2개). 레이저 두께 = 머지 레벨별 `lineWidths`(두 슬라임 평균). (2026-11-15) 레이저 모양 = 전기 이펙트 `LightningLine_Yellow`(`BeamMinion.linkEffectPrefab`, 비우면 단색 LineRenderer). (2026-11-16) **레이저 양 끝 = 프리팹 자식 `BeamPoint`(`BeamMinion.beamPoint`, 비우면 미니언 위치)**, 이펙트 크기는 판정 폭과 분리해 **머지 레벨이 높을수록 약간 크게**(`BeamMinionData.effectScales` 1~1.25, 판정 폭은 `lineWidths`). 번개(Lightning, C급) 연쇄는 낮은 등급이라 약한 변형 `LightningLine_Weak`(번개 선만 가늘게 + 끝점 작은 빛) — **등급이 높을수록 화려하게**. 레이저에 닿은 적에게 **0.25초마다(고정)** 피해(두 슬라임 공격력 평균 × 배율, 배율은 전체 레이저 수가 늘수록 감소). **연결된 슬라임은 기본 공격 안 함**, 혼자면 기본 탄. → 광선 슬라임 둘을 적 경로를 가로지르게 배치하는 고민. 타일 아이템 발사 효과는 레이저에 적용 안 됨.
- (2026-11-06) A등급 **Hail(빙석)** = 탄 없이 **내 TileGroup의 `enemyRoots`를 순서대로 빙빙 도는 눈**(`HailStone`, 마지막 지점 → 첫 지점으로 이어 고리로 순환, 여러 개면 같은 간격). Orbit과 다른 방식(회전체 아님). 닿으면 피해 + 머지 레벨별 확률(30~50%)로 얼음, 같은 적은 0.5초 쿨. 눈 1개. 이동 속도 = 공속 비례. → 적 경로가 지나는 그룹에 둘지 고민.
- (2026-11-06) A등급 **Mortar(박격포, 이전 key Boom. 저장 데이터 마이그레이션 안 함)** = 드래그로 옮기는 **깃발(`MortarFlag`) 자리에 폭탄을 포물선으로 떨어뜨림**(비행 중 충돌 없음, 착탄 지점 범위 피해, 최대 `maxHitCount`기). 높은 공격력 + 느린 공속. 깃발 폭발 범위 안에 적이 있을 때만 발사. 맞은 적 **25% 확률 슬로우(머지 레벨과 무관, 고정)**. 깃발은 슬라임 사거리 안으로만 옮길 수 있고, 처음엔 내 TileGroup 적 경로에서 가장 가까운 지점. 깃발 드래그는 `MinionFieldController`에서 처리(공용 `DragHandle`): 선택된 슬라임의 깃발은 유닛보다 우선, 아닌 깃발은 유닛이 없는 자리에서만 잡힘. → 깃발을 어디 꽂을지(경로 꺾이는 곳, 철벽이 막는 곳)가 배치 포인트. 낙뢰처럼 타일 발사 효과(치명타·둔화·연발 등)가 적용됨. (2026-11-16) 착탄 이펙트 = `Resources/Effect/MortarExplosion`(EffectType.MortarExplosion, 크기 0.7).
- (2026-11-06) S등급 **Hunter(사냥꾼)** = 평소 일반 탄, **5회 공격마다 저격탄**(2026-11-16, 이전 3회)(관통 감쇠 없음 + 스턴 0.2초 + 확정 치명타 `BulletSpec.mustCrit`). 한 공격의 탄이 여러 발이면 전부 저격탄. (2026-11-16) 저격탄은 **부적을 던짐** = 전용 탄 프리팹 `AddressableResources/BulletObject/Level1/Amulet`(관통 탄 동작 그대로 + `Amulet.png` 그림이 빙글빙글 도는 `BulletSpin`, key `Amulet`).
- (2026-11-06) S등급 4종 (모두 평소엔 일반 탄 공격):
  - **Sacrifice(희생)** = 같은 머지 레벨의 **아무 종과 합성 가능 → 결과 = 상대 종, 머지 레벨 +1**(일반 합성은 결과 종 랜덤). 끌어온 쪽/놓인 쪽 상관없음(`MinionFieldController.Merge`), 희생끼리는 일반 규칙. → 키우고 싶은 종을 골라 올리는 카드.
  - **GrowUp(성장)** = 타일 위에서 게임 진행 중 **25초 지나면 장착 슬라임 중 랜덤 종(성장 제외)으로 성장, 머지 레벨 +1**. 드래그 중이면 놓은 뒤 성장, 옮겨도 시간 유지. (2026-11-14) 성장까지 진행도는 머리 위 `MinionGameStatePanel` 쿨타임 바(`hasSkill` 켬, `SkillProgress`), 프리팹 GrowBar 삭제.
  - **Scavenger(시체수집)** = 공격 사거리 안에서 적이 **처치되면(누가 잡든) 1코인**. 경로 끝 도착은 제외(체력 0일 때만). → 처치가 몰리는 자리에 두는 배치 포인트.
  - **Darkness(다크니스)** (2026-11-16 스킬형, **이전 key Joker → 이름 변경**, 이미지 `Darkness.png`. 저장 데이터 마이그레이션 안 함) = 필드(타일 위)에서 게임 진행 중 **15초 쿨타임**(`DarknessMinion.SKILL_COOLTIME`, 머리 위 쿨타임 바)이 차면, 다크니스를 선택했을 때 **머리 위에 변신 목록**(`DarknessTransformPopup`, GameWorldCanvas 하위, 선택지 = `DarknessTransformOption` 템플릿 복사) → **장착 미니언 중 원하는 종으로 변신, 머지 레벨 유지(증가 없음)**. (2026-11-16) **성공 확률 70%**(`DarknessMinion.TRANSFORM_SUCCESS_CHANCE`, 연속 실패 보정 `MinionChancePity` 적용): 성공 = 변신 + 위에 "변환 성공!", **성공 시 10%(`TRANSFORM_GROW_CHANCE`)로 머지 레벨 +1**(최대 레벨이면 그대로, "변환 성공! 성장!"), 실패 = 다크니스 그대로·쿨타임만 처음부터 + "변환 실패..."(공용 글씨 이펙트 `EffectManager.PlayPopText` → `Resources/Effect/PopTextEffect`, EffectType.PopText). 게임은 멈추지 않고, 선택 해제·드래그 중엔 목록 숨김. 쿨타임이 찬 뒤엔 쓸 때까지 대기(아껴 두는 카드). **충전된 동안 몸 뒤에 검은 연기**(`Resources/Effect/DarknessReadyEffect`, EffectType.DarknessReady, CartoonVFX 연기 머티리얼 + 보라 스파크 + 보라 빛 번짐으로 조립, 정렬 UnderDefault). (이전: 변신 버튼 + `JokerSelectCanvas`(일시정지), 30% 확률 실패 → 머지 레벨 -1 / 별 1이면 사라짐 — 스크립트 삭제)
  - **Joker(조커, 2026-11-16 신규 S)** = 카드를 쓰는 미니언. 평소 일반 탄, **5초마다 카드 한 장(4종 같은 확률)**: **스페이드** = 공격 사거리 안 적 전부 공격력 300% 가시 피해 · **다이아** = 5초간 공속 ×5·공격력 ×0.5(PowerUpEffect) · **클로버(임시)** = 코인 5 + 2×머지 레벨 · **하트** = 꽝. 뽑은 카드는 머리 위에 **카드 그림 + 글씨**(그림 = `JokerMinion.cardSprites`(스페이드·다이아·클로버·하트 순) → 이펙트 `LevelUpText/CardImage`, 글씨와 같이 튀어나오고 사라짐(`PopTextEffect.fadeSprites`), `Resources/Effect/JokerCardEffect`, EffectType.JokerCard, 번역 `Joker_Card_*`, 하트 = "꽝!"). 사거리에 적이 없으면 대기, 다이아 중엔 쿨타임 멈춤. 수치는 `JokerMinion` const(임시). 의도: 꽝이 섞인 확률의 재미. 기본 탄 = **카드를 던짐**(전용 탄 프리팹 `AddressableResources/BulletObject/Level1/Card`, 일반 탄 동작 + 발사마다 `2.Sprites/Minion/Growth1/Weapon/Card`의 4장 중 랜덤 그림(`BulletRandomSprite`) + 회전(`BulletSpin`), key `Card`). 각성은 아직 없음.
  - (2026-11-09) **확률 연속 실패 보정**(`MinionChancePity`, `GameManager.minionChancePity`, 2026-11-16 다크니스 변환 성공 판정에 사용): 조커 변신 성공 판정은 슬라임 key별로 연속 실패를 세고, **4연속 실패하면 다음 시도부터 성공 확률 +50%p**(성공할 때까지 유지), **성공하면 초기화**(4회 전 성공도). 한 판 동안 유지. 의도: 확률에 거는 재미는 두되 연속 실패로 짜증나지 않게. 전투 중 적중 확률(낙인·빙석 등)은 대상 아님.
  - 성장·변신은 공용 `MinionFieldController.ReplaceMinion`(같은 타일에 교체, 선택 유지).
- (2026-11-06) **십자 버프 슬라임**(공용 부모 `CrossBuffMinion` / `CrossBuffMinionData.buffValues`, mergeLv별): 기본 탄 + 내 타일 기준 **십자 4칸 슬라임에게 버프**. (2026-11-15) **킹슬라임도 범위 안이면 버프를 받는다**(대상 = `IBuffable`, 킹에도 버프 이펙트). 포위 진형 판정에는 킹을 세지 않음(4칸 모두 슬라임). 범위는 **같은 TileGroup 안에서만**(Grant 기획과 같은 규칙). 타일 오브젝트가 바뀔 때마다 대상 재계산, 여러 개가 겹치면 각각 적용(Multiply는 곱). → 배치 고민 포인트.
  - 범위 8방향은 `ExpandBuffRangeTile` 타일. 포위 진형은 진화(각성)로 발현, 고른 모양(십자/대각)의 4칸 기준.
  - B등급 Breeze(산들바람) = 공속 ×, Rage(격노) = 공격력 ×. A등급 Keen(설의) = 치명타 확률 +%(Add) (2026-11-06 B에서 승급).
  - (2026-11-15) **버프 받는 미니언 표시 이펙트**: 버프를 받는 동안 그 미니언을 따라다님 — 공속 `AttackSpeedUpEffect`(파랑) · 공격력 `AttackPowerUpEffect`(빨강) · 치명타 `CritChanceUpEffect`(노랑)(`Resources/Effect`, PowerUpEffect 복사). `Buff.effectType`에 이펙트를 달면 받는 쪽이 버프 목록 기준으로 켜고 끔(공용 `BuffEffectDisplay`, 미니언·킹, 같은 종류는 하나만) → 범위에서 빠지거나 버프 미니언이 사라지면 같이 꺼짐. 매핑 = `CrossBuffMinion.GetBuffEffectType`.
  - 슬라임 `CritChance` 스탯(기본 0, 버프로만 오름)은 `AllyUnitDamageData.Init`에서 탄 데미지 데이터로 들어간다(연발·Orbit 포함). 타일 치명타는 그 위에 더함.

### 미니언 구성 개편 (2026-11-19, 진행 중)
- (2026-11-20) **미니언 등급(Grade) 없음, 미니언 뽑기 없음.** `MinionData.grade`·등급별 시작 강화 레벨·등급 카드 색·등급 라벨(`MinionGradePanel`)·상점 미니언 뽑기(`Lobby/Shop/DrawProduct` 전체, 상품 `DrawMinion_*`, 누적 업적 DrawMinion 행) 삭제. 미니언은 초기 지급 + 레벨 지급으로만 얻음. **표시 순서 = 지급 순서**(`GameSetting.GetMinionOrder`: 초기 지급 → 레벨 지급). 강화 경험치 표는 모든 미니언 공용 = EnhanceLevelInfo.csv **C 열**(`MinionManager.ENHANCE_EXP_GRADE`), 강화 레벨은 0부터. 프리팹은 `3.Prefabs/Minion` 하위 어디든 `{Key}Minion.prefab`(예전 등급 폴더는 그대로 둠, 새로 만들면 루트). **MinionData.csv `Coin` 열 = 카드 소환 가격 `offerDiceCoinPrice`**(Grade 열 자리). 아래 글의 C/B/A/S 등급 표기는 이전 기록. 아이템·KS 업그레이드의 등급(`GradeType`)은 그대로.
- **게임에서 쓰는 미니언 = 13종**(특성이 뚜렷한 애들만): 탄성(Bounce)·화염·얼음·강철(Iron)·관통(Pierce)·Orbit·랜덤·라이트닝·버서커·박격포(Mortar)·수확(Scavenger)·지뢰(Mine)·팔랑이(Breeze). 나머지 미니언(Hail·Joker 등)은 데이터는 남기되 뽑기·미니언 관리 화면에서 제외(`GameSetting.IsUsedMinion`).
- (2026-11-20) **수확(Scavenger)·팔랑이(Breeze) 제외 → 게임에서 쓰는 미니언 11종.** **초기 지급·레벨 지급은 `Resources/MinionUnlockSetting` 에셋 인스펙터에서 설정**(`initMinionKeys` + `levelUnlocks`(레벨·key), GameSetting.INIT_MINION_KEYS/MINION_LEVEL_UNLOCKS가 여기서 읽음, 에셋이 없으면 코드 기본값). 지금: 탄성 1·강철 2·관통 3·지뢰 4·버서커 6·박격포 7(빈자리를 당겨 채움). 이전 저장에서 빠진 미니언이 장착돼 있으면 로드 때 다른 보유 미니언으로 교체(빠진 미니언은 장착 후보에서 제외). 테스트 `MinionUnlockSettingTest`. 아래 13종·레벨 목록은 이전 값.
- **초기 지급 5종** = 라이트닝·화염·얼음·Orbit·랜덤(`INIT_MINION_KEYS`). **나머지 8종은 유저 레벨에 따라 순서대로 지급**(`GameSetting.MINION_LEVEL_UNLOCKS`, 레벨 = 표시 LV.N, 임시: 탄성 1·강철 2·팔랑이 3·관통 4·지뢰 6·버서커 7·박격포 8·수확 9. 권능 해금 레벨 5·10과 겹치지 않게). 지급 = `UserMinionManager.GrantLevelUnlockMinions`(데이터 로드 후 지난 레벨분도 채움 + `UserMyInfoManager.LevelUp`), 로비 레벨업 창을 닫으면 토스트 `Minion_LevelUnlocked`.
- (2026-11-19) **wave 카드 소환(`MinionOfferSystem`, `GameSetting.USE_WAVE_MINION_OFFER`)**: 골드 소환 대신 **wave가 시작될 때마다 장착 5종 중 랜덤 3장**(중복 가능, **같은 종은 최대 2장** `MAX_SAME_OFFER` → 같은 종 3장은 안 나옴)을 하단 카드로 지급. wave 중엔 새 카드 없음. **카드를 타일로 드래그** → 빈 칸 = 0레벨 소환, 같은 종·0레벨 미니언 위 = 바로 합성. **새 카드를 줄 때 남은 카드는 지움**(`KEEP_UNUSED_OFFERS` false, 2026-11-19). **카드 자리는 고정**: `CardContainer`의 HorizontalLayoutGroup은 카드를 지급할 때만 켜서 자리를 잡고 바로 끔(`ArrangeCards`), 쓴 카드는 그 자리만 비움 → 드래그·사용해도 다른 카드가 밀리지 않음. **wave 중엔 `MinionOfferPanel`(카드 줄)을 끄고(카드 사용 불가) 준비 단계가 되면 다시 켬**(`WaveStartEvent`/`BreakTimeStartEvent`). **한 번 놓은 미니언은 자리 고정**(`MinionFieldController.LockMinionPosition`): 드래그는 머지용으로만, 빈 칸 이동·머지 안 되는 미니언과 자리 교체·킹과 자리 교체 없음(원위치). 킹은 빈 칸으로 자유롭게 이동. **골드 개념 삭제**: 소환 버튼·인게임 강화(`ReinforceContainer`)·코인 표시(`CoinPanel`)는 실행 중 숨김(씬은 그대로). **합성 결과 = 같은 종 +1**(이전: 장착 중 랜덤). 튜토리얼 소환 블록·골드 권능(상인의 장부·도박꾼의 동전·혈통의 플라스크)은 아직 새 방식에 안 맞춤.
- (2026-11-19) **카드 소환 비용 = 주사위 코인(`KingSlime.diceCoin`)**: **브레이크타임마다 5개로 초기화**(2026-11-20, 남은 코인 이월 없음. `GameSetting.WAVE_DICE_COIN`), 카드 1장을 놓을 때 **미니언마다 정한 가격만큼 소비**(`MinionData.offerDiceCoinPrice`, 에셋 인스펙터, 기본 2 = `OFFER_MINION_DICE_COIN_PRICE`. 나중에 비싼 유닛은 더 높게). 카드 아래에 가격 표시(`MinionOfferCard_Template/PricePanel`, 판매 버튼 가격 표시와 같은 모양: 주사위 코인 아이콘 + 숫자, 모자라면 빨간 숫자) → 한 wave에 3장 중 2장만 놓을 수 있어 "무엇을 놓을지" 고르게 함. (아이템 뽑기와 같은 주머니). **코인을 다 쓰지 않고 Next를 누르면 확인 팝업**(남은 카드 중 지금 코인으로 살 수 있는 카드가 있을 때 = `MinionOfferSystem.HasAffordableCard`): `YesOrNoCanvas` "주사위 코인이 남았습니다. 그래도 웨이브를 시작할까요?"(번역 `NextWave_DiceCoinLeft_Title/Body`) → Yes면 wave 시작. **인게임 중 다른 획득처 없음**: 일반 적 10% 확률·엘리트·보너스 엘리트 처치 주사위 코인은 카드 소환 모드에서 끔(골드 보상은 그대로). 코인이 모자라면 카드가 어둡게 보이고 놓으면 토스트 `Not enough dice coin`. 표시는 `BottomInfoPanel/DiceCoinPanel`(골드 `CoinPanel`은 실행 중 숨김).
- (2026-11-20) **인게임 킹 레벨(한 판 동안)**: 처치 경험치로 레벨업(`KingSlime.AddExp`, 넘친 경험치 이월). **필요 경험치 = 25 + 8 × 레벨**(Lv0 25, Lv1 33, Lv2 41 …, `KingSlime.GetMaxExp`), **Lv6부터는 레벨당 증가량 8 → 4**(Lv6 73, Lv7 77, Lv8 81 …, `KING_LEVEL_EXP_SLOW_LEVEL`/`KING_LEVEL_EXP_PER_LEVEL_SLOW`, 2026-11-20). 경험치: 일반 적 1 · **엘리트 = 10 + 5 × (엘리트 순번 - 1)**(wave 5 = 10, 10 = 15, 15 = 20, `EliteEnemy.GetEliteExp`) · **보너스 엘리트 = 10 고정 + 다음 브레이크타임 주사위 코인 +1**(`KingSlime.bonusDiceCoinNextBreak` → `MinionOfferSystem`이 5 + 보너스로 지급, 처치 위치에 "다음 웨이브 +1"). 보스는 경험치 없음. 수치 = `GameSetting.KING_LEVEL_*`/`*_EXP`. **레벨업 효과·`LevelUpCanvas`는 아직 없음** — 화면 상단 경험치바만(`ExpBar` = **InGame 씬 `GameCanvas/Panel/ExpBar`**(맨 위, 높이 26, 가로 전체, 하위 `ExpSlider`·`LevelText`·`ExpText`). 씬에서 빠지면 `GameCanvas.EnsureExpBar`가 실행 중 같은 자리에 만듦. `slider`를 연결하지 않으면 실행 중 기본 Slider + `Lv.N` + `exp/max` 글씨를 만듦).
- (2026-11-19) **최대 5성 = mergeLevel 4**(`MinionData.MAX_MERGE_LEVEL` 4, 표시 = mergeLevel + 1. 잠시 4성(3)으로 줄였다가 되돌림).
- (2026-11-19) **용어: wave가 진행되지 않는 시간(wave 클리어 후 ~ Next 누르기 전, 게임 시작 직후 포함) = "브레이크타임"**. 코드 이름도 같음: `GameManager.isBreakTime`/`breakTimeNextWaveIdx`/`StartBreakTime`/`EndBreakTime`(Next), `BreakTimeStartEvent`(이전 isPreparing·WavePrepareEvent). 문서의 "준비 단계"도 같은 뜻. (아래 웨이브 섹션의 옛 "휴식(break time, `isBreak`)"은 폐기된 이전 기획)
- (2026-11-19) **wave 진행 = 클리어 → 준비 → Next**(카드 소환 모드): wave의 적을 모두 소환했고(`Wave.isSpawnEnd`, 보스는 보스 처치) 필드에 살아 있는 적이 0(처치 또는 경로 끝)이면 wave 클리어 → **준비 단계**(`GameManager.isBreakTime`, `BreakTimeStartEvent`) → 카드 3장 지급 + **Next 버튼**(카드·유닛·깃발 드래그 중엔 숨김) → 누르면 다음 wave(`EndBreakTime`). (Wave 타이머 종료·자동 다음 wave·기존 스킵 버튼 끔). **(2026-11-19) wave 시간 90초(`WAVE_TIME`)**: 90초 안에 적을 다 처리하지 못하면 **필드에 남은 적 1마리당 킹 피해 1**(`GameManager.OnWaveTimeOver`, `TIME_OVER_DAMAGE_PER_ENEMY`) → 남은 적은 보상 없이 제거·남은 소환 취소 → 킹이 살아 있으면 wave 클리어(브레이크타임). **보스 wave는 시간 제한 없음**(보스 처치까지). 게임 시작 때도 wave 1 전에 준비 단계. 부활 = 그 wave 준비 단계(카드는 그 wave 몫을 이미 받았으면 다시 안 줌). 마지막 wave 클리어 = 게임 클리어. UI는 InGame 씬 `BottomGameCanvas/Panel/OfferButtons` 하위(**기존 버튼 줄 `Buttons_Hor`(아이템 뽑기·강화 등)는 카드 소환 모드에서 게임 내내 숨김**, 타일 선택 중 숨김/표시는 `BottomGameCanvas.offerButtonObject` = OfferButtons가 대신함): `BottomInfoPanel`(주사위 코인) + **`MinionOfferPanel`**(MinionOfferSystem, 버튼 줄 위 카드 줄. `CardContainer` + 꺼 둔 `MinionOfferCard_Template`을 장수만큼 복사) + **`NextWaveButton`**(숨긴 소환 버튼 자리). 연결이 비어 있으면 실행 중 기본 모양 생성(`BuildUI`).
- (2026-11-20) **킹 레벨업 창**: 인게임 킹 경험치(`KingSlime.AddExp`)로 레벨이 오를 때마다 `LevelUpCanvas`(InGame 네임스페이스) 열기 + **시간 정지(timeScale 0)**, 닫으면 **열기 전 timeScale로 복구**(배속 유지). 한 번에 여러 레벨이 오르면 닫을 때마다 남은 만큼 다시 열림(`RequestOpen`/대기 수). 카드 = 킹 성장 능력(아래 구조). 레벨업 패널은 씬에 1개만 있어도 모자라면 복사(`LevelUpCanvas.EnsurePanels`), 제목 글씨가 없으면 설명 위에 굵게.
  - (2026-11-20) **능력 구조 = 공용 부모 `Ability`(key·OnEquip·OnUnequip·GetDescription)**: 타일 아이템(룬)은 `Item : Ability` → TileItem, 킹 성장은 `KingAbility : Ability`. **데이터 SO가 프리팹의 Ability 컴포넌트를 참조 → 참조만 바꾸면 능력이 바뀜**(TileItem = `ItemData.itemPrefab`, 킹 = `AbilityData.prefab`). 옛 Ability 계열(AbilityManager·Inventory·Canvas·Synergy·CoreAbility 스크립트·`3.Prefabs/Ability`)과 옛 레벨업 데이터(LevelUpStatManager·LevelUpStatType)는 삭제.
    - 킹 성장 = `AbilityData`(SO: key·종류 `AbilityCategory`·이름·설명 `{0}`·아이콘·최대 레벨·prefab, 이름·설명은 번역 전이라 에셋에 직접) → 킹의 `KingAbilityInventory`(한 판, 같은 카드 다시 고르면 level +1, prefab을 킹 아래 생성). 탄 훅 = `KingAbility.OnKingFired`(Weapon.Shoot). 카드 풀 = `Resources/KingGrowthSetting`(`statAbilities`), 카드 구성 규칙 = `LevelUpChoiceRule`(static, 테스트 `KingGrowthRuleTest`).
    - **1단계(지금)**: 모든 레벨업에서 킹 능력치 카드 3장(최대 레벨 제외): 미니언 소환 수(필드 최대 미니언 수 +1, 최대 3, `MaxMinionCountKingAbility`) · 공격력 +20%/Lv(최대 5) · 공속 +15%(5) · 공격 범위 +0.5(3) · 최대 체력 +3·회복(5) · 치명타 +5%(5) · 연속탄 +1발(3, `BurstShotKingAbility`) · 바운드 +1(3, `BounceKingAbility`). 수치 임시, 에셋 = `Resources/KingGrowth/Stat`, 프리팹 = `3.Prefabs/KingAbility/Stat`. 직업·패시브·발동 스킬은 다음 단계.
    - (2026-11-20) **2단계 종족(구현)**: 종족이 없으면 레벨업 창 = 종족 카드(5종 중 랜덤 3, 한 번에 여러 레벨이 올라도 첫 창), 이후 = 능력치 카드(`LevelUpChoiceRule.GetChoices`). `RaceKingAbility`(`KingRaceType` 뒤에만 추가): 버프 칸 `tileOffsets`(Tile.index 기준, y 아래 +) + 칸 미니언 버프 `tileBuffs` + 킹 스탯 `kingBuffs` + 칸 표시 색, 외형 변화용 `appearanceSprite`/`appearanceAnimator`(아직 안 씀). 버프 칸 처리 = 킹에 붙는 `KingTileBuffArea`(출처별 버프 등록 → 직업도 같은 칸에 버프 추가 예정, 타일 변경마다 재계산, 버프 칸은 타일 위 반투명 색 표시, 받는 미니언엔 버프 이펙트). 설명 `{0}` = 3×3 모양(■ 버프 칸, K 킹).
      - 인간(오른위·오른·오른아래, 칸 공격력 +5%, 킹 공격력·공속 +5%) · 오크(위·왼아래·오른아래, 칸 공격력 +8%, 킹 공격력 +15%/공속 -10%) · 엘프(위쪽 가로줄, 칸 공속 +8%, 킹 공속 +15%/최대 체력 -10%) · 드워프(왼·오른·아래, 칸 엘리트·보스 피해 +10%, 킹 최대 체력 +20%/사거리 -0.5) · 언데드(왼위·왼아래·오른아래, 칸 치명타 +5%, 킹 치명타 +10%/최대 체력 -10%). 에셋 `Resources/KingGrowth/Race`, 프리팹 `3.Prefabs/KingAbility/Race`, 수치 임시.
      - **`StatType.EliteBossDamage`**(엘리트·보스 피해 +%, Add, 버프로만) 추가 → `AllyUnitDamageData.Init`에서 `eliteBossDamageMul = 1 + 값/100`(미니언·킹, 거인 사냥 룬은 그 위에 곱). 킹 최대 체력이 줄면 현재 체력도 잘림.
      - (2026-11-20) **레벨업 카드 UI = 로비 패널 계열**(KSUpgradePanel·AuthoritySlotPanel: 어두운 바탕 + `Button_01_White_Bg` 슬라이스 + 아이콘 + `Main_Regular Outline` 흰 글씨 + `Popup_Box` 테두리). 카드(`LevelUpStatPanel`, 360×800) = 분류 글씨 · 아이콘 바탕(카드 색) + 아이콘 + 레벨 배지(NEW/Lv.N) · 이름 · (종족만) 버프 칸 3×3 그림(`shapeGrid`, 가운데 킹 아이콘) · 설명 · 선택 버튼. **카드 색 = 종류별**(능력치 파랑 등), 종족은 종족 색. 창 위 `headerText`("레벨 업! Lv.N")·`subText`(종족을/능력을 선택하세요). 아이콘: 능력치 = `Resources/Icons/Stat`, 연속탄 = 탄 그림(Pierce), 바운드 = Icon_Reset, **종족 = Layer Lab 헬멧**(인간 035_SilverBlue · 오크 031_Green · 엘프 011_Green · 드워프 015_Brown · 언데드 039_Green). 종족 설명엔 모양 글씨 없음(그림으로 보여줌).
      - 테스트 `KingGrowthRuleTest`(규칙·모양 글씨·설정 에셋), `KingRacePlayTest`(첫 레벨업 = 종족 3장·오크 선택 → 킹 스탯·버프 칸 미니언 +8%·칸 밖 없음·킹 이동 시 해제).
  - (2026-11-20 기획 중) **레벨업 카드 = 킹 강화 · 미니언 진화 · 룬을 섞어 제시**(주로 킹 강화·진화). **진화 = 5성(mergeLevel 4) 미니언에게 적용**, 진화 안 한 5성 미니언이 필드에 있으면 **진화 카드가 반드시 1장 나옴**. **룬 카드를 고르면 타일 선택 화면 → 칸을 누르면 바로 새김**(룬 뽑기 버튼 대체). **타일 선택 화면 = 타일 캔버스(`TileCanvas`)를 잠깐 UI 위로 올림**(미니언이 가려져도 괜찮음), **부서진(막힌) 칸에도 룬 설치 가능**. 킹 강화 = **자연스러운 킹 성장(스탯) + 배치 연계(킹 위치에 따라 주변 미니언 강화)**, 킹 역할(딜러/보조)은 고정하지 않고 유저가 고르게(테스트하며 조정). 같은 강화를 다시 고르면 단계 상승, 일부는 Lv.3까지. 세부 목록은 논의 중.
  - **의도: 레벨업 카드는 기능을 분산하지 말고 집중** → "예상(다음에 뭐가 올지 보임) → 기대(그걸 노리고 고름) → 적중(완성되는 순간의 보상)"이 이뤄지게. 잡다한 스탯 카드를 많이 깔지 않는다.
  - (2026-11-20 사용자 구상, 논의 중) **레벨업 = 킹 강화만**: Lv.1~4 = **능력 뽑기 Panel**(킹 공격력·공속·사거리·체력·연속탄·바운드 등) → **Lv.5 = 각성 Panel**(전직/강성 느낌의 특별 능력, 배치 연계 시작: 킹 중심 8칸 중 2칸 선택 → 킹과 인접한 그 칸 미니언에게 버프) → Lv.6~9 = 능력 뽑기(배치 연계 능력 + 킹 능력 랜덤) → **Lv.10 = 각성 다시**(칸 2개 추가, 중복 불가) → Lv.15까지 능력 뽑기. 각성의 "칸 2개 선택" 방식은 한계가 있어 대안 검토 중. (진화·룬 카드를 레벨업에 섞는 안은 보류)
  - (2026-11-20 갱신) **각성 = 종족 → 직업 단계 성장**: Lv.5 **종족**(5종 중 랜덤 3개 제시) → Lv.10 종족 스킬(2개 중 1) → Lv.15 **직업**(5종 중 랜덤 3개 제시) → Lv.20 직업 스킬(2개 중 1). **종족이 버프 받을 칸 3개(킹 기준 8칸 중)를 정함**(예: 인간 = 오른·오른위·오른아래, 오크 = 위·오른아래·왼아래). 종족마다 잘 어울리는 직업이 있음. 마일스톤 도달 시점(경험치 곡선) 조정 필요. 세부 종족·직업 목록 논의 중.
  - (2026-11-20 갱신, 위 단계를 대체) **Lv.1 = 종족 선택**(종족에 맞게 킹 스탯 약한 폭의 버프·너프) → **Lv.2~4 = 킹 능력치 성장 카드만** → **Lv.5 = 직업 선택**(5개: 기사·궁수·마법사·사제 등, **특징은 같고 이름이 종족마다 다름**: 오크 + 기사 = 광전사, 엘프 + 궁수 = 레인저). **킹 버프 칸은 TileGroup 경계 무시**, **모양은 회전 없이 고정**. 그 이후 단계 논의 중.
    - **종족이 버프 칸 3개 모양을 정하고, Lv.1부터 그 칸에 약한 종족 버프**. 직업(Lv.5)이 버프 칸에 주는 버프 종류를 더함(기사 = 공격력, 궁수 = 공속, **마법사 = 스킬 쿨타임 감소**, 사제 = 공격력·공속 조금씩, 도적 = 치명타). 종족·직업 모두 **5개 중 랜덤 3개 제시**.
    - **Lv.10 = 패시브 스킬 3개 중 1택**: 구성은 버프형 2 + 공격형 1 또는 공격형 2 + 버프형 1. **Lv.15 = 발동 스킬 2개 중 1택, 모두 적에게 적용되는 스킬**. **발동 = 킹을 선택하면 하단에 스킬 버튼(쿨타임이 차면 활성) → 누르면 발동**, 위치 지정 스킬(메테오 등)은 버튼을 누른 뒤 "원하는 위치를 누르세요" → 누른 곳에 발동.
  - (메모, 아직 구현 안 함) **광고 시청 보상 선택지**: "다음 브레이크타임 주사위 코인 +2", "다음 브레이크타임 미니언 카드 4장 제시" — 레벨업 카드가 아니라 광고를 보고 고르는 보상.
- (2026-11-20) **다음 wave 적 정보(`WaveEnemyInfoPanel`, 방어 타입)**: 브레이크타임이 되면 **다음 wave** 정보를 띄움(방어 타입은 wave 번호로 정해져 시작 전에도 표시, `GameManager.GetWaveDefenceType`) → **X 버튼(`closeButton`, ReinforceCanvas 닫기 버튼 복사)으로 닫기**, **wave 시작 시 자동으로 닫힘**. **필드의 일반 적(NormalEnemy, 엘리트 포함)을 누르면 다시 보임**(브레이크타임 = 다음 wave, wave 중 = 지금 wave, `MinionFieldController.BeginDrag` → `ShowByEnemyClick`). 카드 소환 모드가 아니면 이전처럼 wave 시작 때 10초 표시.
- (2026-11-19) **서로 다른 종 합체는 하지 않는다**(한 번 구현했다가 삭제). 지금 방향 = **wave 전 제시된 3장을 타일에 배치 → wave가 끝나면 다시 3장**, 합성은 같은 종·같은 레벨끼리만. 상점 미니언 뽑기와 레벨 지급이 겹치는 문제(뽑기로 미리 얻음) 미정.

### 5성 진화 (2026-11-20 기획 중)
- **5성(mergeLevel 4) 미니언 = 진화 2갈래 중 하나 선택(B안)**, 진화한 미니언은 **발동형 스킬**을 가짐(킹 발동 스킬처럼 선택 → 하단 버튼 → 발동, 위치 지정형은 위치 클릭).
- 시뮬레이션(카드 3장·코인 5·가격 2): 첫 5성은 한 종을 밀면 wave 22 전후(빠르면 18), 아무 카드나 놓으면 wave 30 전후. 그때 킹 Lv.13~17. 한 판 카드 약 120장 → 5성은 많아야 3~5마리 → 진화는 드문 "적중" 순간.
- 갈래는 **발동 스킬 또는 패시브**(종마다 다름). 확정된 것(2026-11-20):
  - 라이트닝 A = (패시브) **광선 링크**(옛 Beam 능력): 필드의 모든 라이트닝이 진화한 5성 라이트닝과 레이저로 연결 → 선에 닿은 적 범위 피해 / B = **과충전**(발동: 다음 5번 공격 연쇄 수 ×2).
  - 탄성 A = (패시브) **탄이 화면 끝에 부딪혀 튕김** / B = **대왕 공**(발동: 지정 위치에 큰 공이 떨어져 튕기며 피해 + 스턴).
  - 지뢰 A = **지뢰밭**(발동: 경로에 지뢰 6개) / B = **굴러가는 폭탄**(빙석 `HailStone`처럼 적 경로를 한 바퀴 돌면서 닿는 곳마다 폭발).
  - **수확(Scavenger)·팔랑이(Breeze)는 진화 갈래 없음.**
  - 나머지(화염·얼음·강철·관통·회전·랜덤·버서커·박격포)는 초안 단계.
  - 지뢰 B는 발동형(버튼을 누르면 한 바퀴). 나머지 8종(화염·얼음·강철·관통·회전·랜덤·버서커·박격포)은 초안대로: 화염 불바다/연소 · 얼음 블리자드/얼음 감옥 · 강철 강철 포탄/강철 벽 · 관통 관통 광선/천 개의 창 · 회전 회전 폭주/구슬 발사 · 랜덤 잭팟/행운 고정 · 버서커 광폭화/회오리 베기 · 박격포 집중 포격/융단 폭격.
- **진화 시점 = 합성으로 5성이 되는 순간**: "와우!" 같은 글씨 이펙트 + 사운드 → 진화 갈래 팝업. **5성은 미니언 그림도 바뀜(갈래가 달라도 그림은 같음).**
- **발동 스킬 쿨타임 표시 = 미니언 머리 위 정보(MinionGameStatePanel) 오른쪽 살짝 위.** 사용은 미니언 선택 → 하단 버튼.
- **진화 타일 없음**(새 진화로 대체, 기존 종별 각성 `OnEvolve`도 이 진화로 교체 예정).

### 장착 조합 프리셋 (2026-11-09)
- 로비 슬라임 관리 화면에 **조합 프리셋 버튼 5개**(`MinionPresetButton`, presetIdx 0~4, `MinionManageCanvas` 하위에 두면 자동 연결). 기본 선택 = 1번.
- 버튼을 누르면 그 프리셋 조합으로 장착(`UserMinionManager.LoadPreset`). **선택된 프리셋은 장착을 바꿀 때마다 자동 저장**(따로 저장 버튼 없음). 저장은 `UserMinionData.presets`/`selectedPresetIdx`.
- 빈 프리셋은 처음 로드할 때 현재 장착 조합으로 채움. 프리셋 key가 미보유·중복이면 현재 장착 → 보유 슬라임으로 대체(`ResolvePresetKeys`, `MinionPresetTest`).
- 이미 장착된 슬라임을 다른 칸에 장착하면 두 칸을 맞바꾼다(5칸 항상 채움).

- (2026-11-12) 슬라임 관리 화면의 슬라임 칸(`Growth1MinionEntryPanel`)은 **씬에 1개만 두고 슬라임 수만큼 코드가 복사**(`MinionManageCanvas.EnsureEntryPanels`). (2026-11-19) **칸은 한 컨테이너(`ownContainer`, unownContainer 삭제), 순서 = 보유(등급 낮은 순 C→S) → 미보유(등급 낮은 순)**. 경험치 표시(`MinionExpPanel`)는 Slider + 글씨. **카드 틀(Bg(Mask)·InnerBorder·HightLight) 색 = 등급**(`Grade.GetCardColor`, `MinionPanel.bgImage`/`innerBorder`/`hightLightImage`가 연결된 패널만. 등급 라벨 `MinionGradePanel`과 같은 계열 색). (2026-11-13) **칸 어디를 눌러도 정보 창**(`MinionEntryPanel.OnPointerClick`, 이전엔 썸네일 버튼만 반응).

- (2026-11-14) **장착 슬롯(`MinionEquipedSlotPanel`) 클릭**: 미니언 관리 화면 = 정보 창(`MinionInfoCanvas`), 장착 창(`MinionEquipCanvas`) 안 = 그 칸에 장착. 버튼은 코드에서 연결(`slotButton`, 비우면 썸네일 Button). 장착 창 슬롯은 `slotPanels`(비우면 하위 전부).
- (2026-11-14, **2026-11-20 등급 삭제로 시작 레벨 보정 없음 = 0부터**) ~~미니언 강화 레벨은 등급별 시작 레벨부터~~: C 0 · B 2 · A 4 · S 6 · SS 8 · SSS 10(`MinionData.GetStartEnhanceLevel`). 읽을 때는 `UserMinion.EnhanceLevel`(저장값이 더 낮으면 시작 레벨). **경험치는 레벨업 시 소모**: 표시 = 모인 exp / 이번 레벨 필요량(EnhanceLevelInfo.csv), 최대 레벨이면 `MAX`(`EnhanceExpInfo.IsMaxLevel` = 마지막 칸). 레벨업 = `UserMinionManager.LevelUp`(필요량 차감).

### 유닛 설정 버튼 (2026-11-09)
- 선택한 유닛 패널(`AllySettingPanel`)의 설정 버튼은 **유닛이 쓸 수 있는 설정만 노출**(공용 `AllyOptionButton.IsAvailable`). 누를 때마다 다음 값으로 순환.
  - 타겟 방식(`SetFindTargetButton`): 타겟을 노리지 않는 슬라임(`Minion.UseFindTarget = false`: Orbit·Mud·Shield)엔 숨김. (Hail은 2026-11-15부터 기본 탄을 쏴서 표시)
  - 버프 범위(`SetBuffRangeButton`, `IBuffRangeUnit`): 십자 버프 슬라임 = **십자 4칸 ↔ 대각 4칸**. 버프 범위 확장 타일 위면 모양과 상관없이 8방향.
  - 회전 방향(`SetRotationDirectionButton`, `IRotationDirectionUnit`): 회전체(Orbit·Mud·Shield, `OrbitMachine.rotationDirection`, 기본 반시계) / 경로 순환(Hail, 기본은 데이터) = **시계 ↔ 반시계**.
  - 의도: 같은 슬라임이라도 **버프가 닿는 칸·회전체가 경로를 쓸고 지나가는 방향**을 플레이어가 골라 배치 고민을 늘린다.
  - 설정은 소환(InitMinion) 때 기본값으로 초기화 → 합성·성장으로 새 슬라임이 되면 다시 골라야 함.

## 인게임 직접 강화 (2026-11-04)
- 코인으로 **종(key) 단위** 강화(`ReinforcePanel`, 가격 30×(lv+1)). 레벨은 `KingSlimeStatManager.minionDic`의 `reinforceLevel`.
- (2026-11-14) **강화 칸은 InGame 화면에 항상 보인다**: `ReinforceContainer`가 장착 5종을 `reinforcePanels[]`(비우면 하위 전부)에 장착 슬롯 순서대로 채움(킹 `statMgr` 준비 후). 이전의 강화 버튼(`ReinforceOpenButton`) → `ReinforceCanvas` 창 방식은 사용 안 함(씬 정리 후 삭제).
- (2026-11-11) `ReinforceCanvas` = **장착 5종을 `reinforcePanels[]`에 장착 슬롯 순서대로** 보여주고 바로 강화. **필드에 그 종이 없어도 강화 가능**(이후 소환분에 적용). 슬롯머신(릴 3개로 대상·종류·수치 결정) 기능은 폐기, 코드 주석으로 남김. 테스트 `ReinforcePlayTest`.
- (2026-11-15) **강화 이펙트**: 강화하면 그 종의 필드(타일 위) 미니언마다 `Resources/Effect/ReinforcedEffect`(EffectType.Reinforce, `MergedEffect`를 복사해 글씨만 **"강화됨!"**) — 머지 "레벨업!"과 같은 연출로 어느 미니언이 강해졌는지 보여줌. 필드에 그 종이 없으면 이펙트 없음.
- 효과는 주로 **공격력 %**: 레벨당 `MinionData.reinforceAttackPowerPercent`(MinionData.csv `ReinforceAttackPower`, 기본 10) 합연산. **%는 종마다 다르게** 준다.
- **슬라임 특성의 시각적 변화(탄 수, Orbit 체인 수 등)는 강화가 아니라 머지 레벨업으로** 준다. 강화는 수치 담당.
- 그 종이 필드에서 잠깐 0마리가 돼도(머지 등) 강화 레벨은 유지(`minionDic` 항목을 지우지 않음).

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
  - 로비 UI: KS 관리 화면 권능 탭 = **3열 그리드, 한 줄 = 한 묶음**(`AuthorityContainer.slotPanels` 18칸, 잠긴 묶음은 `LockPanel`에 `Lv.N`). 칸 → `AuthorityInfoCanvas`(이름·설명·해당 슬라임 수·선택하기). (2026-11-13) **칸에 설명(`descText`)을 바로 보여 주고, 고른 권능은 강조 테두리(`highlightObject`)** → 정보 창을 열지 않아도 묶음 3개를 비교하고 지금 적용 중인 권능을 한눈에 본다. 18칸은 `AuthoritySlotPanel_T1_1`을 복사한 것(모양을 바꾸면 T1_1 기준으로 다시 복사). 레벨업으로 묶음이 새로 열렸고 아직 안 골랐으면 레벨업 창을 닫은 뒤 권능 탭을 열고 안내(`LobbyManager.OpenAuthoritySelect`).
  - 저장: `UserAuthorityData.selectedKeys`(index = 묶음 - 1). 이전 장비 저장 데이터는 마이그레이션 안 함(출시 전).
  - 지금 18개는 **임시 데이터**(AuthorityData.csv `Key/Tier/MatchTags/Desc`, **권능은 등급(Grade) 없음**, **설명 원본 = CSV Desc**(아직 번역 안 함), 이름 = Translate `Authority.{Key}`). 
  - (2026-11-09) **18개 능력 구현** (수치는 각 `{Key}Authority` 클래스 const, 임시). 클래스 = `InGame/Authority/Authorities/*.cs`, 생성 = `AuthoritySystem.Begin`(GameManager.Start, 타일·킹 배치 후 게임 시작 전) / `End`(GameManager.OnDestroy). 게임 코드는 `AuthoritySystem.Get<T>()`/`Has<T>()`로 확인.
    - 묶음1 운영: 개척자의 곡괭이(막힌 칸 2칸 해제, 막힌 칸은 실제로 3칸) · 상인의 장부(wave 0 소환 가격 절반, `MinionSpawnButton`) · 도박꾼의 동전(소환 25% 머지 Lv1 / 25% 코인 2배).
    - 묶음2 공격 형태: 분열의 파편(탄이 처치하면 ±30° 두 발, 피해 50%, 쪼개진 탄은 다시 안 쪼개짐, `AllyBulletObject.Hit`) · 잔불의 화로(지뢰·박격포·낙뢰 폭발 지점 / 지옥불은 맞은 적 1명 위치에 3초 불길, 0.5초마다 공격력 20%/초, 동시 20개 상한, `EmberFire`) · 회오리 구슬(회전체 누적 360°마다 슬라임 위치에서 랜덤 방향 기본 탄, `OrbitMachine.AccumulateRotation`).
    - 묶음3 특성: 전염의 성배(디버프 2종 이상 걸린 적 처치 → 반경 2 안 최대 5기에 같은 디버프 새로 적용) · 중계의 수정(킹 칸 중심 가로·세로 1.5칸 안 처치도 광란·버서커·시체수집이 감지, `RelayCrystalAuthority.InDetectRange`) · 지휘의 깃발(십자 버프 그룹 경계 무시).
    - 묶음4 보드: 측량사의 나침반(**지뢰·지옥불만** 옆 그룹(가로·세로) 경로 사용. 빙석은 자기 그룹 순환, 박격포는 원래 사거리 안 자유라 제외) · 쌍둥이 반지(상하좌우 같은 종 → **공격력 등 데이터 스탯만** 머지 Lv +1 값, `Minion.twinBonus`. 탄 수·회전체 수 같은 모양은 안 바뀜: 회전체 슬라임 InitMinion이 재호출에 안전하지 않음) · 장인의 망치(뽑기 최저 확률 30%, 실패 시 낸 코인 절반 환불).
    - 묶음5 위기: 사냥꾼의 부적(Heavy 슬라임 타겟 = 엘리트·보스 우선, 엘리트 처치 시 타일 아이템 뽑기 확률 100% 무료, 이미 뽑기 창이 열려 있으면 생략) · 혼혈의 문장(장착에 물리·마법 모두 있으면 정령/갑옷 wave 시작 시 약점 속성 장착 슬라임 1마리 무료 소환, 소환 가격 안 오름, `AuthoritySystem.SpawnFreeMinion`) · 모래시계(킹 체력 처음 30% 미만 → 5초간 모든 적 정지, `Enemy.FreezeAll`).
    - 묶음6 완성: 순혈의 왕관(장착 5종 공격 속성 모두 같음(만능 제외) → 아군 약점 배율 ×1.5, `EnemyDefence.GetMultiplier`) · 왕의 인장(장착 5종 형태 모두 다름 → 모든 슬라임 공속 ×1.2, `Minion.UpdateMinion`) · 혈통의 플라스크(합성 결과 종 50% 같은 종 유지, `Minion.Merge`).
    - 조건형(왕관·인장)은 게임 시작 시 장착 슬라임으로 한 번 판정. 이름 = Translate `Authority.{Key}`, 설명 = AuthorityData.csv Desc, 발동 토스트 = Translate `Authority_*`.
    - 테스트 `AuthorityAbilityTest`(능력 클래스 존재·규칙 계산), `AuthorityPlayInGameTest`(18개 모두 켜고 실제 전투: 곡괭이·쌍둥이·잔불·모래시계 확인 + 에러 없음).
- 데이터: `AuthorityData`(Addressables 폴더 항목 `AddressableResources/AuthorityData`, 라벨 `AuthorityData`) → `AuthorityManager`(Resources/Init, `GetTierAuthorities`).
- **슬라임 유형 태그(`MinionTag`, Flags)**: 권능이 반응하는 기준. MinionData.csv **`Tags` 열(L열, `Shooter|Status`처럼 |로 구분)** → `MinionData.tags`. 시트 다운로드 범위 `minionRange` = A:L. 태그는 **기본 효과 기준**(부가 효과는 안 셈).
  - 형태(정확히 하나): Shooter 발사(Random, Bounce, Flame, Ice, Lightning, Pierce, Iron, Dopamine, Stigma, Berserker, Hunter, Joker) / Zone 장판(Mine, Beam, Mortar, Hail, Inferno, **Thunder**) / Orbit 회전(Orbit, Shield, Mud) / Support 버프(Breeze, Rage, Keen) / Special 특수(Sacrifice, GrowUp, Darkness, Scavenger).
  - 특성: Status 상태이상(Flame, Ice, Stigma, Mud, Hail) / Chance 확률(Ice, Stigma, Hail, Joker(카드). 다크니스 변환 70%는 태그 미반영) / KillTrigger 처치 감지(Dopamine, Berserker, Scavenger) / Heavy 강타(Iron, Hunter, Mortar) / Transform 변환(Sacrifice, GrowUp, Darkness).
  - 규칙(`MinionTagTest`): 형태는 슬라임마다 하나, 형태·특성마다 해당 슬라임 **3종 이상**. 새 슬라임 추가 시 Tags도 채울 것.
- 권능이 반응하는 유형 = `AuthorityData.matchTags`. 정보 창에 **장착 슬라임 중 해당 종 수 / 장착 수**(태그 중 하나라도 있으면 해당, `AuthorityData.CountMatchingMinions`). matchTags가 없으면 숨김.
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
  - StartGold = 게임 시작 시 코인, WaveClearGold = wave 종료마다 코인(보스 wave는 보스 처치 시. 무한 상승 게임이라 보스도 지급), MaxMinionCount = `MinionFieldController.minionMaxCount`(기본 `MINION_MIN_EXIST_COUNT`)에 더함. **더하는 곳은 GameManager 한 곳**(킹 스탯 값, 2026-11-07 MinionFieldController.Awake에서도 Lv을 더해 두 번 적용되던 버그 수정).
  - 로비 KSManagementCanvas 능력치 표시(`KSStatStateContainer.currentKSAbilityInfoPanels`)는 업그레이드 StatType 전부(최대 슬라임 수·시작 코인·wave 종료 코인 포함). 표시 단위: 치명타 확률은 인게임과 같은 %(값 그대로), 공속은 `ATTACK_SPEED_PER_SECOND` 기준 10초당 발 수.
  - 테스트 `KSUpgradePlayTest`: StatType별 업그레이드(Lv·재화·스탯·표시, 재화 부족/최대 Lv), 로비에서 올린 값이 인게임 킹 스탯·체력·최대 슬라임 수·시작/wave 종료 코인에 반영.

## 아이템 공통 (2026-10-23)
- **보유 개수에 따른 능력치 증가는 없다.** `Item.count`/`GetCount()` 삭제. 아이템 효과는 개수와 무관하게 고정 수치(개수 1 기준). 보유 개수는 `statMgr.itemStatDic`에서 뽑기 상한(MaxCount) 판단에만 쓴다.

## 타일 선택 / 정보 표시 (2026-10-26)
- 선택 단위는 **유닛이 아니라 Tile**. 슬라임·캐릭터·아이템 타일을 클릭하면 정보가 뜬다. 타일 밖을 누르면 해제.
- `TilePointer`는 선택된 타일 위치 표시만 담당(입력 판정은 `MinionFieldController` 한 곳). **빈 타일(유닛·아이템 없음)은 반응 없음**: 포인터·정보 패널(`TileItemInfoPanel` 등) 모두 숨기고 버튼은 그대로(선택 해제와 같은 화면). `TileItemInfoPanel`은 **아이템이 설치된 타일에서만** 뜬다(유닛만 있는 타일은 유닛 정보만). 드래그 중엔 숨기고, 선택은 옮긴 유닛을 따라간다(머지 시 결과 슬라임 선택).

## 피해 숫자 연출 (2026-11-15)
- 의도: 타격감 + **방어 타입 상성을 숫자로 체감**(약점·내성). 연출은 `WorldTextBase.Play` 한 곳(피해·치명타·킹 피해·회복 글씨 공용), 옵션 = `WorldTextStyle`(size·crit·type).
- 공통: 처음 0.08초에 최종 크기의 1.4배로 튀었다 돌아옴 + 흰색에서 원래 색으로 번쩍 + 좌우 랜덤으로 살짝 튀었다 떨어지는 포물선(`DOJump`) 후 사라짐.
- **치명타(`CRTDamageText`)**: 1.6배 + 좌우 흔들림(`DOPunchRotation`) + 숫자 뒤 `!`, 더 높이 튐.
- **약점/내성**: `Enemy.TakeDamage`가 방어 타입 표 배율(`DefenceInfo.Get`, 만능은 1)을 `ApplyDamage(typeMultiplier)`로 넘김 → >1 약점 = 1.25배 + 위에 작게 `DamageText_Weak`("약점!") + `ColorSetting.weakDamageColor`(치명타면 치명타 색 유지) / <1 내성 = 0.8배 + 회색(`resistDamageColor`) + 번쩍임 없이 아래로 힘없이.
- **피해량 비례 크기**: 대상 최대 체력 대비 피해 비율로 1 ~ 1.5배(최대 체력 절반 이상이면 1.5, `AllyUnitDamageData.GetDamageTextSize`).
- 같은 적 피해 합치기(숫자 누적)는 아직 안 함(보류).

## 피해 집계 (2026-11-17)
- 미니언이 적에게 준 피해를 **종(key)별로 한 판 누적**(`DamageMeterSystem`, static). **킹 피해는 세지 않음.** 초기화 = `GameManager.StartGame`(부활해도 이어서 셈).
- 집계 = `Enemy.TakeDamage` 한 곳, **실제로 줄어든 체력**(마무리 일격의 넘친 피해 제외). 출처 = `DamageData.Attacker`(`AllyUnitDamageData.allyUnit`, 아니면 `DamageData.source`). **새 피해 경로를 만들 때 출처를 꼭 넣을 것**: 화상(`FlameEffect` 생성자 source)·번개 연쇄(`chainDamageData.source`)는 넣어 둠. 권능 잔불(`EmberFire`)·`InGameUtil.DamageEnemies`(옛 아이템용)는 출처 없음 → 집계 안 됨.
- UI: InGame `GameCanvas/Panel/DamageMeterSystemButton`(열기/닫기) 바로 아래 `DamageMeterSystemContainer`(기본 꺼짐) → 장착 5종 = `DamageMeterSystemPanel` 5줄(썸네일·1위 대비 바·피해량 `12.3K`·전체 대비 %), **피해 큰 순서로 정렬**, 켜져 있는 동안 0.5초(실제 시간)마다 갱신.

## 슬로우 / 빙결 (2026-10-31)
- (2026-11-19) **빙결은 슬로우 중첩과 분리**: 이동속도 × `IceEffect.slowRate`(얼음 미니언 = `IceMinionData.slowRate` 머지 레벨별 0.7/0.6/0.5/0.4/0.3 = 30~70% 감속, 빙석·얼음 구슬 = 기본 0.7). **중첩 없음 — 다시 걸리면 배율은 강한 쪽(작은 값) 유지, 남은 시간보다 새 시간이 길면 새 시간으로**(예: 0.3 3초 → 2초 후 0.7 3초 = 0.3 유지, 남은 3초). 슬로우 중첩·진흙과는 곱함(`StatusEffectHandler.IceSlowRate`). 아래는 이전 기획.
- 둘은 **이펙트만 다르고 감속은 같다**(`IceEffect : SlowEffect`). 적중마다 이동속도 ×0.5, **둘을 합쳐 최대 3중첩**(최저 12.5%). 효과가 끝나면 그 효과가 쌓은 중첩만 풀린다.
- (2026-11-06) **넉백(적 밀어내기, `Enemy.Push`/`PushBehavior`)은 삭제.** 적 이동 방해는 슬로우·빙결·스턴·철벽 막기로만. **스턴은 모든 출처 공통으로 엘리트/보스도 걸리고 시간만 절반**(`StatusEffectHandler.Apply`).

## 타일 아이템 (2026-10-22)
- (2026-11-18) **화면에 보이는 이름 = "룬"**("타일 아이템"은 개발용 말). 코드 이름(`TileItem` 등)은 그대로, UI 글씨만: 아이템 이름 `○○ 룬`(ItemData.csv Name), 뽑기 버튼 "룬 뽑기", 설치 = "룬을 새길 칸", 번역 `TileItem_*`·`Achievement.DrawItem`·`Authority_HunterCharmDraw`. 새 문구도 룬으로.
- 아이템 획득(2026-11-01): `DrawItemButton`으로 코인을 내면 `SelectItemCanvas`가 열린다. **Draw**(확률 연출, `drawDuration` 0이면 생략) → **SelectItem**(TileItem 중 겹치지 않는 랜덤 3개, 반드시 하나 선택) → **SelectTile**(눌러서 설치, 이미 아이템이 있는 타일은 불가). **열려 있어도 게임은 멈추지 않는다**(2026-11-07 확인, 이전엔 일시정지). 설치 칸을 직접 고르는 것이 배치 고민 포인트. PioneerTile은 설치 대신 해제할 막힌 칸을 고름. (이전: 랜덤 1개를 랜덤 타일에 바로 설치)
  - 선택지 갱신(2026-11-04): SelectItem 단계에서 `AdRefreshItemButton`으로 광고를 보면 선택지 3개를 다시 뽑는다(가능하면 지금 보이는 것 제외). **한 판에 2번**(`REFRESH_COUNT_PER_GAME`), 광고 제거 구매자는 광고 없이 갱신.
  - (2026-11-14) 뽑기 가격: **주사위 코인(`KingSlime.diceCoin`) 15개 고정**(`GameSetting.DRAW_ITEM_DICE_COIN_PRICE`, 뽑을수록 오르지 않음, 일반 코인은 안 씀). 장인의 망치 환불도 주사위 코인. (이전: 일반 코인 처음 10, 뽑을 때마다 증가)
  - 성공 확률(2026-10-28): **처음 100%, 성공할 때마다 -5%** (최소 10%, `INIT/DECREASE/MIN_DRAW_ITEM_CHANCE`). 주사위 코인은 먼저 내고 실패하면 코인만 소모, 확률은 그대로. → 골드 소비의 확률 포인트(뒤로 갈수록 "걸어볼지" 고민).
- (2026-11-09) **타일 아이템 강화**: 아이템 타일 선택 → `TileItemInfoPanel`의 `UpgradeTileItemButton`. **설치된 타일 객체 하나 단위**(같은 key 다른 타일은 그대로, 판매하면 투자도 사라짐).
  - 비용 = **주사위 코인(`KingSlime.diceCoin`, 엘리트 처치 보상) × (현재 레벨 + 1)** — 기본값은 ItemData.csv `DiceCoin`. **Lv1 시작, 최대 = `MaxCount`(3)**. Lv1→2 = DiceCoin×2, Lv2→3 = ×3.
  - **성공 확률 70% → 50%**(`TileItem.UPGRADE_CHANCES`, Lv1→2 / Lv2→3). 코인은 먼저 내고 실패하면 코인만 소모·레벨 유지(뽑기 실패와 같은 방식). 골드 소비 확률 포인트와 같은 의도.
  - 레벨별 수치 = ItemData.csv **`Values`** 열(`1.3/1.5/1.7`, 시트 범위 A:G) → `ItemData.levelValues`. 아이템마다 대표 수치 하나에 들어감(`TileItem.ApplyLevelValue`): 스탯 타일 = 배율/증가량, 광폭 = 공격력 배율, 연발 = 추가 발수, 치명타·둔화·스턴 = 확률, 거인 사냥 = 배율, 코인 = 처치 코인, 다중 타겟 = 탄 수. Values가 비면 프리팹 값 그대로. 지금 값은 임시. **설명(Desc)의 `{0}` = 현재 레벨 수치**(`TileItem.GetDescription`, 배율형은 % 증가량: 1.5 → 50).
  - **DiceCoin 0(슬라임 수 증가·최대 버프·개척·만능 공격)·진화 타일은 강화 불가**(버튼 숨김). 최대 레벨이어도 숨김. 강화 가능 아이템은 이름 옆에 `Lv.N`.
  - 유닛이 올라가 있으면 효과를 뺐다가 새 수치로 다시 건다(버프 중복 없음). 테스트 `TileItemUpgradePlayTest`.
  - (2026-11-18) **판 위에서 레벨이 보이게 아이템이 설치된 타일(칸) 색 변경**(2026-11-19, 이전엔 아이템 그림 색. `TileItem.LEVEL_COLORS` → `Tile.SetItemColor`, 아이템이 빠지면 원래 색 `ResetItemColor`, 임시: Lv1 연두 · Lv2 하늘 · Lv3 금색, 강화 가능 아이템만). 결과는 토스트 대신 **타일 위 글씨 이펙트 "성공!" / "실패.."**(`EffectManager.PlayPopText(pos, PopTextType)`).
  - 아쉬운 점 논의(2026-11-18): 숫자만 오름·최대 레벨에 질적 변화 없음·확률이 고르는 선택이 아님·실패 보정 없음. 최대 레벨 성질 변화 / 안전·도박 강화 선택 / 실패 보정은 검토 중.
- 타일 아이템(`TileItem`)은 획득하면 고른 타일에 설치되고, 그 타일에 슬라임이나 캐릭터가 올라와 있는 동안만 효과가 적용된다(`OnEnter`/`OnExit`).
- 아이템 제거(2026-10-28): 아이템 타일을 선택하면 `TileItemInfoPanel`의 `SellTileItemButton`으로 그 타일 아이템 객체 하나만 판매 → **10코인 고정**(`SELL_TILE_ITEM_PRICE`). 같은 key의 다른 타일 객체는 유지. 판매 후에는 타일 선택 해제(TilePointer 숨김, 위에 유닛이 있어도 동일).
- **타일 하나에 아이템은 하나만** 설치한다. 설치할 때 위에 오브젝트가 있어도 되고, 그러면 바로 적용된다.
- 수치는 **타일(객체)마다 고정**이다. 같은 아이템을 또 얻으면 count를 올리지 않고 **객체를 하나 더** 만들어 다른 타일에 설치한다.
- 스탯 버프형(공격력, 공속, 사거리)과 발사 방식형(BurstShot, MultiTarget, CycleRandomShot) 모두 슬라임과 캐릭터에 적용된다.
  - BurstShot: 공격 후 **같은 방향으로 연발**(`COMBO_ATTACK_INTERVAL_MS` 간격). `BurstShotTileItem.OnFired`가 쏜 탄과 같은 탄(스펙·위치·방향·데미지)을 지연 후 다시 생성한다. 다중 타겟 탄도 탄마다 각각 연발. 추가 탄은 OnFired를 안 거쳐 연발이 연발을 만들지 않음.
  - MultiTarget: **다중 타겟** = `Minion.ShootCount`에 탄 수를 더한다(탄마다 서로 다른 타겟). (2026-11-17) **타겟이 겹치지 않음: 서로 다른 타겟이 발사 수보다 적으면 남는 탄은 쏘지 않는다**(이전엔 같은 적을 다시 노림). 미니언 공통(`Minion.Fire`, 머지 레벨 탄 수·사냥꾼 스킬 3타겟 포함)·킹(`Weapon`) 모두.
    - (2026-11-17) **번개(Lightning)도 같은 규칙: 타겟 +1 = 그 적부터 시작하는 연쇄 한 줄 더.** 연쇄 첫 타겟 = 발사 방향의 적(`LightningBulletObject.FindFirstTarget`). 이전엔 항상 가장 가까운 적부터라 탄마다 똑같은 연쇄가 겹쳐 이펙트만 수십 개 생겼음.
  - CycleRandomShot: **3번째 공격마다 랜덤 타겟**.
- 2026-10-23 목록 (Key = 클래스명에서 `Item` 뺀 것, ItemData.csv에 행 추가):
  AttackPowerTile(공 +50%), AttackSpeedTile(공속 +30%), BurstShotTile, MultiTargetTile, RangeUpTile(사거리 +2),
  SlowTile(10% 슬로우), StunTile(2% 0.3초 스턴), CriticalTile(10% 2배), TrueDamageTile(방어 배율 무시 → 항상 100%),
  AddMinionSpawnCount(2026-11-11, 이전 IgnoreCountingTile = 올라간 슬라임을 최대 수에 안 세는 타일. **타일에 설치하지 않고 획득 즉시 최대 슬라임 수 +1**(`AddMinionSpawnCountItem`, `NeedTile` false → 타일 선택 단계 없음). 보유 상한 MaxCount 3. 최대치 12마리 막힘을 푸는 수단),
  ExpandBuffRangeTile(2026-11-06, 올라간 십자 버프 슬라임의 범위 십자 4칸 → 8방향. 장착 슬라임에 버프 슬라임이 없으면 안 나옴. 배치 고민 포인트),
  EvolutionTile(2026-11-06, 최대 머지 레벨(4) 슬라임이 올라가면 진화 `Minion.Evolve`, 슬라임당 1회(`isEvolved`, 합성·교체로 새로 생기면 초기화). **(2026-11-10) 1회용: 슬라임을 진화시키면 타일 아이템은 사라짐**(다음 프레임에 `KingSlime.RemoveItem`, 판매 코인 없음). 진화 못 하는 슬라임(최대 레벨 미만·희생/성장/다크니스)이 올라가면 그대로 남음. **뽑기(2026-11-09): 필드의 진화 안 한 레벨 4 슬라임 수 > 설치된 진화 타일 수일 때만, 뽑기 시도마다 40%(`EvolutionTileItem.DRAW_CHANCE`)로 선택지 3개 중 하나로 확정 등장**(실패하면 그 뽑기엔 안 나옴). 같으면 안 나옴(예: 5마리/4개 → 40%, 5마리/5개 → 0%, 타일이 더 많은 경우는 없어야 함). 보유 상한(MaxCount) 대신 이 규칙만 씀(`TileItem.IgnoreMaxCount`/`DrawChance`, `ItemManager.GetDrawTileItems`). 강화 불가. 테스트 `EvolutionTileDrawPlayTest`. 진화 = 슬라임 고유 [각성] 발현, 위 슬라임 섹션 참고),
  GoldTile(처치 시 코인 +1), PioneerTile(2026-11-10 **선택 후 막힌 칸을 눌러 그 칸을 해제**(`TileItem.CanPickTile`, 막히지 않은 칸은 토스트), 설치 안 함, 막힌 칸 없으면 안 나옴. 권능 곡괭이는 랜덤 해제 그대로), BerserkTile(공 ×2, 사거리 ×0.5), GiantSlayerTile(엘리트·보스에게 데미지 ×2, 방어 배율과 곱연산).
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
- **(2026-11-20) 마지막 wave = 60**(`GameSetting.LAST_WAVE`, 보스 = 20·40·60, 60 보스 처치 = 클리어). 아래 80은 이전 값(규칙은 그대로, 숫자만 60).
- **(2026-11-19) 마지막 wave = 80**(`GameSetting.LAST_WAVE`, 보스 wave). **80 보스를 처치하면 게임 클리어**(`GameManager.StartNextWave` → `isCleared` + `EndGame(true)` → ResultCanvas). 클리어 보상 = 80 wave를 모두 넘긴 것으로 계산(골드 80, 경험치 40), 저장 `UserStage.isClear` + 업적 `ClearStage`. **다음 난이도 해금 = 이전 난이도 클리어**(`StageData.IsUnlocked`, 이전: wave 100 도달). 아래 "무한 웨이브"는 이전 기획(wave 종류·보스 주기 규칙은 그대로).
- **2026-10-30: 무한 웨이브.** 끝 wave 없이 계속 진행, 게임은 사망으로만 끝난다. WaveType은 WaveData(CSV)가 아니라 `GameManager.GetWaveType`이 번호로 결정:
  - wave 번호(waveIdx+1)가 **20의 배수 = 보스**(`BOSS_WAVE_INTERVAL`), **5의 배수 = 엘리트**(`ELITE_WAVE_INTERVAL`), 나머지 = 일반 전투(Battle). 보스 우선.
  - (2026-11-07 확정) 보스는 20 wave마다, **보스 wave에는 엘리트가 나오지 않는다**(보스 우선 → 20/40…은 BossWave만).
  - (2026-11-07) **보스는 체력에 따른 페이즈가 없다.** 스킬(EnemyAttackPattern, 보스 자식)은 여러 개 들 수 있고 어떤 스킬을 쓸지는 보스 행동(`StartBehaviour`)이 정한다(`Boss.GetAttackPattern(key)`). BossPhase·BossPhaseChangedEvent 삭제.
  - (2026-11-19) **일반 적 처치 코인 = 1 + 20 wave마다 +1**(`GameSetting.KILL_GOLD_WAVE_INTERVAL`, `NormalEnemy.GetKillGold`: wave 1~20 = 1, 21~40 = 2, 41~60 = 3, 61~80 = 4. 이전 10 wave마다) + 10% 확률 주사위 1.
  - (2026-11-09) **엘리트 처치 보상** = 주사위 2 + 골드 `(wave 번호 / 5) × 30`(wave 5 = 30, 10 = 60, 15 = 90…, `EliteEnemy.GetEliteRewardGold`). **보너스 엘리트(버튼 소환) = 주사위 5만**(골드 없음). 구분은 프리팹 `Enemy.enemyType`.
  - 엘리트 wave = 일반 전투 + 엘리트 1마리. (2026-11-07) **엘리트·보너스 엘리트·보스는 항상 Normal 방어 타입**(wave 방어 타입은 일반 적에만 적용). 갑옷·정령 엘리트 프리팹·CSV 행 삭제.
  - 보스 wave는 시간으로 안 넘어감(타이머 0에서 대기). **보스 처치 시 바로 다음 wave** (이전엔 스테이지 클리어).
  - 아래 "0~19 / 보스 처치 시 클리어" 내용은 이전 기획.
- wave 0~19 (총 20), 19 = 보스. 각 wave의 WaveType은 WaveData에 **하나로 고정**, 선택 없이 바로 진행.
- WaveType: Battle, Elite, Boss (2026-10-30, 이전 Normal/Hard/Elite/Boss). wave마다 `Wave` 인스턴스를 새로 생성(`GameManager.accWaves`에 누적).
- **2026-10-30: 휴식(break time) 없음.** UI(`WaveTimerPanel`)는 `GameManager.mainWaveTimer` 기준 현재 wave 남은 시간을 `분:초`로만 표시. 아래 휴식 관련 내용은 이전 기획.
  - (2026-11-15) 적 스폰은 **시간이 아니라 마리 수 기준: 한 wave에 일반 적 총 `WAVE_ENEMY_COUNT`(40)마리**, `ENEMY_SPAWN_INTERVAL` 간격(시작 즉시 1마리). 다 소환하면 남은 시간(`WAVE_TIME`까지)은 스폰 없이 남은 적 처리. wave 시간이 먼저 끝나면 남은 소환은 취소. 일반 적 소환 위치 = `EnemyFieldController.enemySpawnPointTrs` 순서대로 번갈아(비면 `paths[0]`), 엘리트·보스는 `paths[0]`. 소환 지점에서 나온 일반 적은 `paths[0]`을 건너뛰고 바로 `paths[1]`로 → 각 지점에서 `paths[1]`로 오는 길에 한 줄로 합쳐짐(`NormalStartPathIdx`). (이전: `WAVE_SPAWN_TIME`초 동안)
  - (2026-11-15) `StartNextWaveButton`은 **현재 wave의 적이 모두 소환되면**(`Wave.isSpawnEnd`, 일반 적 `WAVE_ENEMY_COUNT`마리 소환 완료) 노출 → 바로 다음 wave 시작. 필드에 남은 적은 그대로 진행. 보스 wave에서는 안 나옴. 규칙 = `GameManager.CanSkipWave(waveType, isSpawnEnd)`. (이전 2026-11-11: wave 시작 후 20초 `SKIP_ENABLE_TIME`부터 노출, 남은 소환과 겹침 → 삭제)
- 전투 웨이브 흐름: **웨이브 30초**(`WAVE_TIME`, 30초 내내 1초마다 1마리 스폰) → **휴식 10초**(`BREAK_TIME`) → 다음 wave.
  - 각 Wave가 자기 타이머를 가지고, 시간이 다 되면 스스로 종료(스폰 중지) → `GameManager.EndWave` → 휴식(`GameManager.isBreak`).
  - 휴식 중에는 TimeBar 숨기고 "다음 웨이브까지 N초" 카운트다운 + 다음 웨이브 시작 버튼 표시.
  - 끝날 때 필드에 남은 적은 제거하지 않고 계속 이동.
- **다음 웨이브 바로 시작(`StartNextWaveButton`, 2026-10-28)**: **한 wave의 소환이 모두 끝나면**(= wave 종료 → 휴식) 버튼이 나온다. 누르면 남은 휴식을 건너뛰고 바로 다음 wave(`GameManager.EndBreak`). 전투 중에는 안 나오므로 wave 스폰이 겹치지 않는다. 스킵 보상 골드 없음.
  - (이전 기획: 전투 wave 시작 5초 후 스킵 → 이전 wave 남은 스폰과 겹쳐 진행 + `SKIP_WAVE_GOLD`. 폐기)
  - 같은 wave의 중복 종료는 무시.
- **보스 wave**: 시간으로 끝나지 않는다. (이전: 보스 처치 시 `GameManager.EndGame(true)` → 스테이지 클리어. 지금은 무한 웨이브라 다음 wave)
- (2026-11-07) **사망 → FailCanvas**: 광고 보고 부활 = **하루 5회**(`FailCanvas.MAX_DAILY_RESURRECTION_COUNT`, PlayerPrefs에 오늘 남은 횟수 + 날짜, 날짜 바뀌면 다시 5). 광고 성공 시에만 차감, 광고 제거 구매자는 광고 없이. 취소 → ResultCanvas.
  - 광고 호출은 `AdManager.Show` 한 곳. **AdManager가 없으면(실제 광고 연결 전, StartSetting에서 생성 안 함) 광고 없이 성공 처리.** 광고 제거 구매자 처리도 Show 안에서(2026-11-12).
- (2026-11-07) **결과 화면(ResultCanvas)이 나올 때** 도전 횟수(`TryStage`) +1 → 부활해도 한 판. 보상: 골드 = `waveIdx`(최종 wave 번호 - 1, 1 wave당 1골드), 경험치 = waveIdx/2. (2026-11-20 **뽑기권(DrawTicket) 삭제**: 결과 보상·재화·패키지 지급 모두 없음. `CurrencyType.DrawTicket`은 번호 유지용으로만 남김) 최고 wave 기록(`EndStage`).
  - (2026-11-09) **광고 보고 보상 2배**(`ResultCanvas.OnClickedDoubleReward`, 버튼 `adDoubleRewardButton`): 기본 보상은 화면이 열릴 때 바로 지급, 광고 성공 시 **같은 양을 한 번 더**(경험치·골드) → 패널 값 2배로 갱신(글씨가 커지는 순간 새 값으로 바뀌고 원래 크기로 돌아오는 이펙트, `RewardPanel.PlayValueChange`), 버튼 숨김. 결과 화면당 1번, 받을 보상이 없으면 버튼 없음. 광고 제거 구매자는 광고 없이. 최대 레벨이면 경험치는 0 그대로.
- (2026-11-09) **로비 복귀 시 경험치 바 연출**: 결과 화면에서 받은 경험치(2배 포함, `UserMyInfoManager.unshownGainExp`, 저장 안 함)를 로비 `MyInfoPanel` 바가 **받기 전 값에서 차오르는 모습**으로 보여준다(페이드가 끝난 뒤 시작, 바 한 칸 1초 기준). 바가 가득 차면 레벨업 창 → 닫으면 새 레벨 빈 바에서 남은 경험치만큼 다시 차오름(레벨마다 반복). 연출 중엔 경험치 이벤트로 바가 튀지 않게 막음.
- (2026-11-20) **로비 보스 처치 연출(`LobbyReturnController`)**: 의도 = 인게임 보스 처치의 성취감을 로비에서도 보여줌. 스테이지에서 보스(wave 20·40·60)를 **처음** 처치하고 돌아오면(결과 화면에서 저장 전 기록과 비교 → static `unshownDefeatedBossWave`, 한 판에 여럿이면 늦은 보스) 페이드 후·경험치 바 전에 연출. **씬 구성: `BossMover`(`bossMoverTr`, 비우면 bossTr의 부모) 하위에 `Boss` · `NextBoss` · `BossTr` · `NextBossTr`(`nextBossTr`) · `WaveText`(`curWaveText`) · `NextWaveText`(`nextWaveText`) — **글씨는 보스 하위가 아님**(BossMover와 같이 움직이고 보스가 날아가도 그 자리), 킹은 따로 → 보스 쪽은 BossMover만 움직임.** 순서: 킹은 제자리 콩콩 + BossMover가 내려와 보스가 킹 앞(`stopDistance`)까지 → 킹 뒤로 살짝 → 몸통 박치기 → 보스 돌며 날아감(글씨는 남아 BossMover와 같이 내려감) → BossMover가 계속 내려오고(킹 콩콩) **`revealDistance`(3)만큼 더 내려오면 NextBoss·NextWaveText가 보임** → NextBoss가 보스 자리에 도착·출렁 → 평소 화면(BossMover 원위치, Boss = 다음 보스). NextBoss 글씨 = 다음 보스 wave("Wave 40"), **최종 보스를 잡은 연출이면 NextBoss 없이 "Clear" 글씨만**. **평소 화면 = 지금 잡을 보스만(NextBoss·NextWaveText 숨김), 다 잡았으면 최종 보스(Wave 60)**(`GetDisplayBossWave`). **BattleCanvas가 다시 열리면(다른 로비 화면에 갔다 오면) 연출 중이어도 끊고 바로 평소 화면**(`ShowStage`). 움직임은 DOTween 코드, 시간·거리는 인스펙터 "연출" 값. 보스 그림 = `bossSprites`(index = 보스 순번 - 1), 보스 그림은 인게임 Boss 대신 `LobbyBoss`(SpriteRenderer 또는 Image). 테스트 `LobbyReturnRuleTest`.
- (2026-11-09) **유저 레벨 최대 30**(`UserMyInfoManager.MAX_LEVEL`, 업데이트로 올릴 수 있음). 최대 레벨이면 경험치를 받지 않고 **0으로 유지**(결과 화면 경험치 보상도 없음, 도달 시 남은 경험치는 버림) → 최대치를 올리면 0부터 다시 쌓인다. 로비 경험치 표시는 `MAX`.
- wave 종료 코인(`WaveClearGold`)·시작 코인(`StartGold`)은 기본 0, **로비 업그레이드로만** 얻는다.
- **부활(Resume)**: 필드 적 전부 제거(처치 보상 없음) → 체력 가득 → **마지막으로 종료된 wave의 다음 wave부터 다시 시작**. 그 이후 wave(스킵으로 겹쳐 진행 중이던 것 포함)는 모두 제거.
  - 예) 0 종료, 1 진행 중 스킵 → 2 진행 중 사망 → 1, 2 제거 후 **1을 처음부터**.
- Normal vs Hard: 스포너 설정(인스펙터 값)과 배율만 다르게, 코드는 공유하는 방향.

### 난이도 의도
- **소환량은 고정: 1초마다 1마리.** 소환 수를 늘리는 방식(PeakCountMul 곡선)은 폐기.
- **난이도는 적 체력으로만 올린다.** (`Enemy.GetHp`: wave별 상승만. wave 안에서 시간에 따른 체력 상승은 2026-10-17 주석 처리)
- (2026-11-19) 체력 조정: **기본 체력 +10%**(StageData `EnemyHp` 40/50/60 → 44/55/66) + **20 wave(보스 주기) 구간별 체력 배율을 직접 설정**: StageData.csv `CycleHpMuls`(F열, 난이도마다, `1/1.1/1.21/1.33` = wave 1~20 / 21~40 / 41~60 / 61~80, 칸이 모자라면 마지막 값, 누적이 아니라 그 구간의 배율 그대로) → `StageData.cycleHpMuls`·`GetCycleHpMul`. 비우면 `GameSetting.HP_MUL_PER_BOSS_CYCLE`(1.1)을 구간마다 곱함. 보스 wave는 자기 구간. 
- (2026-11-19) **보스 체력 배율 0.6 → 80**(일반 적보다 약했음. 엘리트 15). EnemyData.csv에 Boss 행이 없어 에셋 값(`BossData/TigerWraith`)이 그대로 쓰이던 것 → **Boss 행 추가**(`Boss/Normal/80`). (BossCanvas는 2026-11-20 삭제, 보스 체력은 머리 위 큰 체력바)
- 스폰되는 적의 체력/방어 타입은 **자기를 스폰한 wave 기준** (`Enemy.Spawn(pos, waveIdx)`). 스킵 후 이전 wave가 계속 스폰하는 적은 이전 wave 체력으로 나옴.
- 전투 웨이브의 특수 스폰(BundleWave 등 랜덤 스포너 호출)은 2026-10-16 제거. 일반 스폰만 사용.

### 적 방어 타입 (2026-10-16)
- (2026-11-20) **일반 적 방어 타입 = StageData.csv `DefenceWaveTypes`(H열, 스테이지·난이도마다)를 wave 1부터 한 칸씩 반복**(`Normal/Normal/Ethereal/Normal/Normal/Armored` → wave 3 정령, 6 갑옷 …). **wave 구간별로 바뀌는 것 없음. WaveData/{난이도}.csv·`WaveData` 클래스·`DEFENCE_WAVE_INTERVAL` 삭제.** 비우면 `StageData.DEFAULT_DEFENCE_WAVE_TYPES`(위와 같은 순서). `Random` = wave 번호로 고정된 랜덤(Normal/Armored/Ethereal, 다음 wave 미리보기와 실제가 같음). 계산 = `StageData.GetDefenceType`, 게임 코드는 `GameManager.GetWaveDefenceType`. 아래 방어 타입 규칙(2026-11-08, 10-30)은 이전 기획.
- (2026-11-08, 2026-11-10 간격 4→3) **일반 적 방어 타입 = wave 번호가 3의 배수(`DEFENCE_WAVE_INTERVAL`)면 Ethereal → Armored 순서로 번갈아(3=정령, 6=갑옷, 9=정령…), 나머지는 Normal** (`GameManager.GetWaveDefenceType`). WaveData의 DefenceTypes/DefenceWaveTypes는 사용 안 함(아래는 이전 기획).
- (2026-10-30) 방어 타입 설정은 **WaveData/{난이도}.csv의 구간 단위**: 행마다 `StartWave`(waveIdx), `DefenceTypes`(등장 목록), `DefenceWaveTypes`(구간 시작부터 순서대로, `(wave - StartWave) % 개수`). wave에는 `StartWave <= wave`인 가장 늦은 행이 적용(예: 0/9/19 → wave 18은 9 행). 마지막 행은 무한히 반복.
- **한 wave 안에서는 모든 일반 적이 같은 방어 타입.** wave가 바뀌면 다음 칸. `Random`은 DefenceTypes 중 하나를 wave 시작 시 한 번 뽑음.
- 적의 방어 타입 기준은 **프리팹의 `Enemy.defenceType`** (2026-10-30, 이전엔 EnemyData.defenceType). 적 프리팹은 `Stage/{order}/Enemy` 폴더(하위 Elite/BonusElite 포함)에 종류×방어 타입별로 두고(엘리트·보스는 Normal만), EnemyData는 (종류, 방어 타입) 데이터가 없으면 같은 종류의 Normal 데이터를 공유(엘리트는 Normal 하나).
- **2026-10-30: 공격 속성은 물리(Physic)·마법(Magic) 두 가지 + 만능.** 진동(Vibrational) 삭제 → 진동형이던 Bounce/Pierce/Boom은 물리로, 진동 약점뿐이던 **중갑형(HeavyArmor) 방어 타입도 삭제**(enum 번호는 비워 둠: DefenceType 3, DefenceWaveType 4).
- 공격 속성(Physic/Magic) × 방어 타입 배율은 `Resources/DefenceInfo` 에셋 한 곳에서 관리 (인스펙터 표). 방어형 클래스마다 계산을 따로 두지 않는다.
  - (2026-10-30) 적 프리팹의 `EnemyDefence` 컴포넌트(방어 타입별 하위 클래스)가 `CalculateDamage`로 최종 피해 배율(표 배율·만능 무시·거인 사냥 포함)을 반환, `Enemy.TakeDamage`는 이것만 호출. 수치는 여전히 표에서 가져오고, 하위 클래스는 방어형별 특수 처리 자리. 프리팹에 컴포넌트가 없으면 `Enemy.Awake`에서 `enemyData.defenceType`에 맞는 것을 붙인다.
  - 기본값(물/마): **갑옷 0.5/1.5 · 정령 1.5/0.5** · 일반 1/1 (2026-11-10 완화, 이전 0.2/2.5는 12.5배 차이라 물리가 거의 안 통함)
  - (2026-11-10) **아군 최소 피해 보장**: 배율·내성으로 1 미만이 되면 1(`AllyUnitDamageData.GetMinGuaranteedDamage`, 원래 피해가 0인 공격은 0). 이전엔 1 미만 = 0이라 공격력 낮은 물리 슬라임이 갑옷 적에게 0 피해.
  - (2026-11-07) **생체형(Organic) 방어 타입 삭제**(enum 번호 비워 둠: DefenceType 4, DefenceWaveType 5). 프리팹·EnemyData 에셋·CSV 행(EnemyData, WaveData 69 구간)도 삭제.
  - 방어 타입 설명(`DefenceInfo.Desc`)은 배율 표에서 자동 생성 → 표만 고치면 UI 설명도 맞춰짐. 수치 대신 `약점 : 마법형`(배율 >1, 잘 통함) / `내성 : 물리형`(배율 <1, 안 통함) 두 줄로 표기(1배는 생략).
  - **2026-10-27 의도: 방어 타입을 크게 체감되는 판단 요소로 만든다.** wave마다 "어떤 공격 속성 슬라임을 좋은 칸/킹 오라/아이템 칸에 둘지" 다시 고민하게 하는 장치. (현재 슬라임 에셋 attackType이 전부 AllRounder라 효과 없음 → 속성 부여 필요)
  - **만능(`AttackType.AllRounder`)은 방어 타입 배율 무시(항상 100%)** (2026-10-25). TrueDamageTile의 `AllyUnitDamageData.ignoreDefence`도 같은 취급 → 판정은 `DamageData.IgnoreDefence` 한 곳.

## 스테이지 (2026-10-30)
- (2026-11-20) **스테이지 order(`StageData.order`, 0부터, 화면 표시 = order + 1) × 난이도 = StageData 하나.** 출시 땐 스테이지 1(order 0)만, 2·3은 데이터만 추가하면 확장(에셋 `{난이도}_{order}.asset` + StageData.csv 행(`order` 열, 비면 0). 기존 `Normal.asset` 같은 이름은 order 0으로 읽음). **key = `{난이도}_{order}`**(`StageData.MakeKey`, 예 `Normal_0` = 스테이지 1 보통), 유저 기록(`UserStage.key`)도 같은 key — 이전 `Normal` 저장 기록은 `Normal_0`으로 옮김. 선택 = `UserDataManager.stageOrder` + `difficultyType`(저장 안 함), 조회 = `StageManager.GetStageData(order, 난이도)`/`GetCurrentStageData`.
  - 로비 `BattleCanvas`: `stageText` = "스테이지 N"(난이도 이름은 `DifficultyDisplayButton`). **`<`(`preStageButton`) = 이전 / `>`(`nextStageButton`) = 다음 스테이지**, 그 방향에 스테이지가 없으면 숨김. **`>`는 다음 스테이지 Normal이 해금됐을 때만(= 지금 스테이지 Normal 클리어) 보임**(`BattleCanvas.CanShowNextStage`). 이동한 스테이지에 지금 난이도가 없으면 Normal. 난이도 선택 창은 지금 스테이지에 있는 난이도만 보임.
  - 해금: 스테이지 1 Normal은 항상 / 같은 스테이지의 Hard·Hell = 그 스테이지 한 단계 낮은 난이도 클리어 / 스테이지 N Normal = 스테이지 N-1 Normal 클리어(임시, 번역 `Stage_Locked`).
  - 랭킹은 아직 스테이지 1(order 0) 기준(`RankingManager.RANKING_STAGE_ORDER`, 서버 경로는 난이도별 그대로).
  - 아래 "난이도마다 스테이지 하나"는 이전 기획.
- **난이도(Normal/Hard/Hell)마다 스테이지 하나.** 스테이지 순서(order) 개념 없음, key = 난이도 이름(`Normal`/`Hard`/`Hell`).
- 에셋은 `AddressableResources/StageData/{난이도}.asset`, 파일명으로 StageData.csv 행(Difficulty)을 찾는다. 적 프리팹은 난이도 공용 `3.Prefabs/Stage/0/Enemy`.
- (2026-11-19) 해금: Normal은 항상, 그 외는 **한 단계 낮은 난이도 클리어(wave 80 보스 처치, `UserStage.isClear`)**. 아래 wave 100 도달은 이전 기획.
- (2026-11-08) 해금: Normal은 항상, 그 외는 **한 단계 낮은 난이도에서 wave 100(표시 번호) 이상 도달**(`StageData.UNLOCK_WAVE`, 저장값 `maxWave`는 waveIdx라 99 이상). Normal 100 → Hard, Hard 100 → Hell. 잠긴 난이도는 시작 버튼을 눌러도 토스트만.
- (2026-11-09) 난이도 선택 창(`SelectDifficultyCanvas`): 버튼마다 이름(`Difficulty.{난이도}`) + 설명(잠김 = 해금 조건 / 열림 = 최고 기록 wave, 기록 없으면 "기록 없음"). 잠긴 난이도도 선택은 가능(시작만 막힘). 닫기 버튼은 코드에서 연결.
- 이전 저장 데이터의 `{난이도}_0` 기록은 새 key로 옮기고, `_1~_4` 기록은 버린다(`UserStageManager.MigrateStageKeys`).

## 로비 상점 (2026-11-07)
- **상품 구분(ProductData key)은 패널이 아니라 버튼마다**(`ProductPurchaseButton.key`). 한 패널에 10회·20회·광고 버튼처럼 상품이 여러 개.
- 버튼 클릭 → `ProductManager.TryToBuy`(재화 = 잔액 확인 후 차감 / 인앱 = 결제 완료 / 광고 = 시청 완료, 광고 제거 구매자는 광고 없이) → **구매 기록(버튼 key)** → `onPurchased`(인스펙터에서 패널 함수 등록, 뽑기는 `Purchased(int)`에 횟수) → 상위 `ProductPanel.UpdatePanel`.
- 하루 구매 제한은 버튼 `dailyLimit`(0 = 없음, 오늘 구매 기록 수로 판단). 광고 뽑기 = 광고 상품 + dailyLimit (이전 Normal…ProductPanel의 광고 전용 코드는 삭제).
- ~~특별 뽑기 카운트(N회마다 A등급 이상)는 **Unique 뽑기 패널 하위 버튼의 구매 횟수 합만** (`DrawProductPanel.PurchaseCount`, specialDrawInterval > 0인 패널만 사용).~~ (2026-11-20 삭제: 미니언 등급·뽑기 없음)
- `ProductContainer.productPanels`를 비워 두면 하위 패널 전부.
- ~~뽑기(`DrawSO.Draw`): 확률표 등급 중 **아이템이 없는 등급은 빼고 남은 확률로** 뽑는다(지금 SS 슬라임 없음). 슬라임 등급 목록은 `MinionManager.gradeGroupGrowth1MinionDic`(로드 시 growth 1 슬라임으로 채움).~~ (2026-11-20 삭제: 미니언 등급·뽑기 없음)
- ~~뽑기 결과 창(DrawMinionResultCanvas / DrawAuthorityResultCanvas, 공용 `DrawResultCanvas`): 카드(`DrawResultCard`)를 **5열 그리드**(장수가 많으면 스크롤)에 깔고 순서대로 뒤집어 공개(간격 `revealInterval` 0.12초, 뒤집기 0.3초, DOTween·실시간). **S 등급 이상(`revealShakeGrade`)은 뒤집기 전에 0.5초 흔들림**, 흔들리는 동안 다음 카드는 기다림. **(2026-11-12) 전부 공개되면 `touchToCloseText`가 보이고 화면 아무 곳이나 누르면 닫힘**(닫기 버튼 없음, 공개 중 터치는 무시). 결과 창에는 뽑은 상자 종류에 맞춰 `normalChest`/`uniqueChest` 중 하나만 보임(Unique = `UniqueDrawMinionProductPanel`). 카드는 `cardTemplate`(뒷면 + 앞면 = 슬라임 칸 / 권능 썸네일)을 복사해서 씀.~~ (2026-11-20 삭제: 미니언 등급·뽑기 없음)
- 결과 창이 씬에 없으면 지급 후 토스트만. 패널 `LastDrawKeys` = 마지막 결과.
- ~~테스트 `DrawShopPlayTest`: 실제 뽑기 버튼 6개(재화 차감·지급 개수·기록), 광고 뽑기 하루 3회, 고급 슬라임 상자 N회차 S 이상, 슬라임 관리 화면(모든 슬라임 1칸씩·보유 먼저·미보유 표시).~~ (2026-11-20 삭제: 미니언 등급·뽑기 없음)
- 출시 전이라 이전 패널 key 구매 기록은 옮기지 않음.
- 재화 상품(`CurrencyProductPanel` → `DiaProductPanel`/`GoldProductPanel`): 패널 `amount`만큼 지급, 제목은 `{재화} {amount}` 자동. 상품 가격은 CSV에서.
- (2026-11-12) **패키지 상품(`PackageProductPanel`)**: 하나를 사면 여러 아이템 지급, **한 번만 구매 → 구매하면 패널 숨김**(하위 버튼 key의 구매 기록으로 판단, 구매 복원도 포함). 지급 목록은 **패널 인스펙터 `rewards`(재화 종류 + 개수)**, 광고 제거·배속 같은 효과는 상품 key 구매 기록으로 적용. 구매 완료 토스트 = `Purchase_Package`, 광고 제거(`RemoveAdPackagePanel`) = `Purchase_RemoveAd`, 게임 속도 증가(`GameSpeedUpPackagePanel`) = `Purchase_GameSpeedUp`. 패키지 상품 key = `RemoveAdPackage`/`GameSpeedUpPackage`(단품 `RemoveAd`/`GameSpeedUp`와 별개 인앱 상품). **광고 제거·배속 효과는 단품·패키지 중 하나라도 샀으면 적용**(`InAppProductInfo.REMOVE_AD_KEYS`/`GAME_SPEED_UP_KEYS`).
- (2026-11-12) **상품 설명 = 번역 시트 `Product.{key}.Desc`**(`ProductData.GetDesc`, 패널 `descText`). **뽑기 확률 버튼(`DrawInfoButton`)** = 상위 뽑기 패널(`DrawMinionProductPanel` 일반 / `UniqueDrawMinionProductPanel` 고급)의 `drawMinion`을 `DrawInfoCanvas`에 넘김.
- (2026-11-07) 상품 데이터 = `Json/ProductData.csv`(key, productId, currencyType, price) → DataTableImporter(`Tools/Data/Import ProductData`, Play 시 자동) → `Resources/ProductData.asset`(`ProductDataTable`) → `ProductManager.Awake`에서 불러옴(씬 인스펙터 `productDatas`는 무시). price는 float: 재화 상품 = 개수(정수), 인앱 = 스토어 가격을 못 받을 때 표시용 달러. **인앱 상품은 productId가 있어야 결제**(없으면 재화 차감으로 처리돼 구매 불가, `ProductDataTableTest`가 검사).
  - (2026-11-10) **인앱 상품 정보(key·스토어 productId·소모/비소모)는 `InAppProductInfo` 한 곳.** 추가 = 거기 한 줄 + CSV에 같은 key의 InApp 행(가격 표시용). productId는 임포트할 때 클래스에서 key로 채움(CSV productId 열은 무시). IAP 매니저의 상품 목록·구매 복원도 이 목록 사용. 코드용 key 상수(`REMOVE_AD_KEY`, `GAME_SPEED_UP_KEY`)도 GameSetting에서 이동.

## 에너지 (2026-11-12)
- **게임 시작(로비 전투 버튼)마다 에너지 5 소모**(`GameSetting.PLAY_COST_ENERGE`, `EnergeManager.TryUsePlayCost`). 모자라면 시작하지 않고 토스트(`Energe_NotEnough`)만.
- **5분마다 1 충전**(2026-11-19, 이전 10분. `EnergeManager.CHARGE_INTERVAL_MINUTES`), 자동 충전은 **최대 30**(`MAX_ENERGE`)까지만. **구매·보상으로 받은 에너지는 30을 넘겨 쌓인다.**
  - (2026-11-19) **자리 비운 시간도 충전**: 기준 = 마지막 충전 시각(PlayerPrefs `Energe_LastChargeTime`, 실제 기기 시간). 앱을 껐다 켜거나 **백그라운드(포커스 잃음) → 복귀** 시 흐른 시간만큼 한 번에 충전하고 **토스트 "자리를 비운 동안 에너지 N 충전!"**(`Energe_OfflineCharged`, 실제로 오른 경우만). 켜져 있는 동안은 1초마다 실제 시간으로 확인(timeScale·배속 무관, 토스트 없음).
  - 최대 이상이면 타이머는 계속 "지금"으로 → 에너지를 쓴 시점부터 5분. 기기 시간을 되돌리면 지금부터 다시 셈. 남은 시간 = `EnergeManager.RemainTimeToNextCharge`(표시 UI는 아직 없음).
- **`BuildSetting.ignoreEnerge`(테스트용)**: 체크하면 에너지를 항상 최대로 취급 — 차감 안 함, 표시도 최대(`EnergeManager.Current`), **저장값은 그대로**라 끄면 원래 값. 자동 테스트는 설정과 상관없이 실제 차감(`EnergeManager.DisableIgnoreForTest`).
- 테스트 `EnergeRuleTest`, `GameCyclePlayTest`(부족 시 시작 안 함·시작 시 차감).

## 업적 (2026-11-12)
- **일일(`Resources/Json/DailyAchievement.csv`: Type/Goal/RewardType/RewardValue) · 누적(`CumulativeAchievement.csv`: Type/InitGoal/IncreaseGoal/RewardType/RewardValue).** 한 행 = 업적 하나, **업적 창 패널 수·순서 = CSV 행**. Type = `AchievementType` 이름(없는 이름이면 에러 로그 후 그 행 건너뜀, `AchievementRuleTest`가 검사). enum은 저장 데이터에 번호로 들어가므로 **뒤에만 추가**.
- **누적 목표 = InitGoal + IncreaseGoal × 클리어 횟수.** 보상을 받으면 목표만큼 빼고 **초과분은 이월**, 한 번에 한 단계씩 받는다. IncreaseGoal 0 = 목표 고정(Watch_Ad: 2회마다 다이아 3).
- **일일은 목표 달성 → 보상 1번, 기기 시간 자정에 진행도·수령 여부 초기화.** 날짜 확인은 집계 직전·업적 창 열 때·앱으로 돌아올 때(켜 둔 채 자정이 지나도 초기화). 기록 날짜는 저장 데이터(`UserDailyAchievementData.date`).
- **"하루에 한 번" 판단은 `DailyCheck` 한 곳**(Today / IsToday / PlayerPrefs용 IsTodayPrefs·MarkTodayPrefs, 테스트용 `NowForTest`). 일일 업적·Login·일일 부활 횟수·상품 하루 구매 제한이 사용. 새 일일 기능도 여기로.
- **집계는 행동이 일어나는 곳에서 `AchievementManager.Report(type, 횟수)` 직접 호출**(씬 이벤트 구독 방식은 폐기). 일일·누적에 같이 쌓이고, 저장은 프레임 끝에 한 번.
  - KillEnemy = 처치만(경로 끝 도착 제외, 보너스 엘리트 포함) · KillBonusElite · TryStage = 결과 화면(부활해도 한 판) · LevelUpMinion = 로비 강화 · **DrawMinion = 뽑은 장수**(10회 = 10) · **DrawItem = 시도마다**(확률 실패 포함) · UpgradeReinforce = 인게임 강화 · **SpawnMinion = 소환 버튼만**(무료 소환·합성 결과 제외) · MergeMinion = 합성(자리 교체 제외) · **Login = 접속한 날 수**(날짜가 바뀐 뒤 첫 확인 때 1) · **Watch_Ad = 광고 보상을 받을 때마다, 광고 제거 구매자 포함**. ClearStage는 사용 안 함.
  - **광고 제거 여부 확인은 `AdManager.Show` 안에서만**(제거 구매자 = 광고 없이 성공). 호출하는 쪽에서 `isAdRemoved`를 따로 보지 않는다 → Watch_Ad 집계도 Show 한 곳.
- UI(`AchievementCanvas`, 일일/누적 탭): **패널은 씬의 첫 패널을 모자란 만큼 코드가 복사**(`AchievementContainer`), 남는 패널은 숨김. 진행도 = Slider + `값/목표`(일일은 목표까지만 표시). 상태 오브젝트는 하나만: 일일 = 진행 중 `processingObj` / 받을 수 있음 `claimObj` / 받음 `gottenRewardObj`, 누적 = 진행 중 / 받을 수 있음 둘뿐. 받을 수 있으면 패널·탭·로비 업적 버튼 redDot. **(2026-11-13) 표시 순서 = 받을 수 있음 → 진행 중 → 완료(일일 보상 받음)**, 같은 상태끼리는 CSV 순서. 갱신할 때마다 다시 정렬(`AchievementContainer.SortPanels`, 패널 `State`). 이름 = Translate `Achievement.{Type}`.
- 테스트 `AchievementRuleTest`(CSV 전부 읽힘·목표 계산·자정 판단·이름 key), `AchievementPlayTest`(로비: 창 구성·탭·집계 → 표시 → 보상·누적 이월·광고·날짜 변경 초기화·저장 파일 / 인게임: 소환 버튼·합성·강화·아이템 뽑기·처치·보너스 엘리트).

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
- RankingButton: 로그인 안 돼 있으면 SettingCanvas(설정 창) → 로그인 성공 시 RankingCanvas.
- (2026-11-12) UI: 로비 전투 화면 `RankingButton`(업적 버튼 위) → `RankingCanvas`. **난이도는 드롭다운(`difficultDropdown`)**: 옵션은 코드가 DifficultyType 순서 + 번역 이름으로 채우고 **잠긴 난이도 랭킹도 볼 수 있다**, 열 때는 현재 선택 난이도(이전 난이도 탭 버튼은 삭제). 순위 줄(`RankingPanel` 프리팹) = 순위·닉네임·**wave 숫자만**(머리글에 WAVE), 내 줄 강조. 아래 내 순위 줄: 목록 안 = 그 순위 / 밖 = `100+` + 내 로컬 최고 wave / 기록 없음 = `-`.
- 테스트 `RankingRuleTest`(순위 매기기·100+·닉네임·wave 글씨), `RankingPlayTest`(버튼 → 창·드롭다운·줄 표시·내 순위 3가지·빈 랭킹·닫기). **에디터엔 Firebase가 없어 서버 대신 가짜 랭킹을 넣는다**(`RankingManager.FetchForTest`/`UidForTest`, 기본 null) → 서버 업로드·캐시·로그인 흐름은 자동 테스트 대상이 아님(기기에서 확인).
- 갱신: 열 때마다 내 기록을 먼저 올리고(서버보다 높을 때만 씀) 목록은 **난이도별 10분 캐시**(캔버스를 껐다 켜도 유지). 단 내 최고 wave가 올라 업로드한 경우는 바로 다시 불러온다.

## 설정 창 (2026-11-12)
- 로비 `SettingButton` → `SettingCanvas`(씬에서는 꺼 둠). **로그인 창(FirebaseLoginCanvas)은 삭제하고 설정 창에 통합**(스크립트 guid는 이전 FirebaseLoginCanvas 것).
- **BGM / SFX 스위치 = 음소거**(`SettingSwitch`, 켜짐 = 소리 남). 볼륨 값(`BGMVolume`/`SFXVolume`)은 그대로 두고 음소거 여부만 따로 저장(`AudioVolumeUtil.BGMMuteKey`/`SFXMuteKey`) → 믹서에 넣을 때 0(`GetAppliedVolume`, `SoundManager.ApplyVolumes`). 볼륨 슬라이더는 없음.
- **진동 스위치** = `Vibration.SetVibrationEnabled`. **언어 버튼** = 누를 때마다 다음 언어(`SettingCanvas.LANGUAGES` = 번역 시트에 있는 kr·en만), 글씨 = 현재 언어 이름. 씬에 박힌 글씨는 `TranslateText`로 바뀜.
- **로그인 안 됨 = `LoginPanel`**(게스트·Google·Apple, Google/Apple은 지원 플랫폼만) / **로그인됨(게스트 포함) = `LoginStatePanel`**(Discord·서버 저장·불러오기·Player ID). 로그인에 성공해도 창은 닫지 않고 패널만 바뀜(랭킹 버튼으로 열었을 때만 닫고 랭킹으로).
  - 지금 화면엔 **로그아웃·게스트 → Google/Apple 계정 연결 버튼이 없다**(코드 `FirebaseLogoutLoginButton`은 남아 있음). 필요하면 LoginStatePanel에 추가.
- **Player ID = Firebase uid 전체**, 누르면 클립보드 복사(문의용). **Discord 주소는 `SettingCanvas.DISCORD_URL`**(비어 있으면 "준비 중" 토스트). 버전 글씨 = `Application.version`. Privacy / Terms 글씨는 아직 동작 없음.
- 테스트 `SettingPlayTest`.

## 튜토리얼 (2026-11-13, 구조만)
- **튜토리얼은 여러 종류로 나눈다**(`TutorialType`, 종류마다 `Tutorial` 하나). `Tutorial` = 튜토리얼 기준, `TutorialStep` = 그 안에서 진행할 단계(자식 오브젝트 순서 = 진행 순서, 단계별 동작은 상속해서 `OnEnter`/`OnExit`, 조건 만족 시 `Complete()` → 다음 단계). 위치 `Common/Tutorial/`.
- `TutorialManager`(프리팹 `Resources/Init/TutorialManager`, StartSetting이 생성): **튜토리얼은 씬에 두지 않는다**(2026-11-13). 매니저의 `tutorialPrefabs`(종류별 Tutorial 프리팹)에서 `TryBegin(종류)` 때 지금 씬에 생성 → 끝나면 Tutorial이 자기를 삭제, 씬이 바뀌면 씬과 같이 사라짐(완료 기록 없음). 게임 코드는 `TutorialManager.Instance.TryBegin(종류)`만 호출. **한 번에 하나만 진행.** **완료 여부는 PlayerPrefs에만 저장**(`Tutorial_Completed_{종류}`, 유저 데이터·서버 저장 아님 → UserTutorialManager 없음). 끝까지 진행해야 완료, 진행 중에 씬이 닫히면 다음에 처음부터.
- **튜토리얼 전용 Canvas·안내 오브젝트는 그 `Tutorial` 오브젝트의 자식으로 둔다** → 튜토리얼이 끝나면 `Tutorial`이 자기 오브젝트를 Destroy해서 통째로 지워짐. 이미 완료한 튜토리얼은 씬이 열릴 때(Awake) 바로 삭제. 기존 게임 Canvas는 참조만 하고 지우지 않는다.
- **전용 화면 `TutorialCanvas`**(Tutorial의 자식): 화면 블록(전체 화면 투명 Raycast 이미지 → UI와 필드 입력 모두 막음) + **화면 중앙 대화창**(터치하면 다음 대사). **튜토리얼 동안 화면은 기본으로 막혀 있고, 직접 조작하는 블록(킹 이동·소환)만 그 블록 동안 푼다.**
- **불필요한 UI 숨김** = `Tutorial.hideObjectNames`(**씬 오브젝트 이름 또는 경로**, 프리팹은 씬 오브젝트를 참조할 수 없어서. `TutorialSceneFinder`가 꺼진 오브젝트까지 찾음, 없거나 이름이 겹치면 경고). 시작할 때 끄고 끝나면 다시 켬. 중간에 보이게 하려면 `SetActiveTutorialStep`(`targetNames`).
- **블록(TutorialStep) = `Common/Tutorial/Steps/`**, 인스펙터에서 자식 순서로 조립(대화를 사이사이 끼워 넣는 용도): `Dialogue`(화자 + 대사 목록, 번역 key면 번역문) · `Wait`(초) · `WaitFadeEnd` · **`StartWave`(튜토리얼 도중 wave 0 시작 → 이후 블록은 적이 나오는 중에 진행, `GameManager.StartGame`. 없으면 튜토리얼이 끝난 뒤 시작)** · `SetActive` · `ShowSpawnButton`(숨긴 소환 버튼 켜기, 씬 연결 없이) · `SetKingTile`(킹을 칸으로 바로 옮김) · `MoveKing`(플레이어가 킹을 지정 칸으로 드래그) · `SpawnMinion`(**1기 = 블록 하나**, 여러 기는 블록을 이어 둠. 소환 버튼을 누르면 **지정 key가 지정 칸에**: `MinionSpawnButton.forcedMinionKey`/`forcedTile`, 장착과 무관. 코인이 모자라면 모자란 만큼 채워 주고 소환은 평소처럼 코인을 냄) · `MergeMinion`(from 칸 미니언을 to 칸 미니언 위로 드래그해 합성).
  - (2026-11-13) **포커스(`TutorialCanvas.ShowFocus(대상)`/`HideFocus`)**: 대상 UI만 밝게 두고 나머지 화면을 어둡게 + **대상 밖 터치는 막고 대상 위 터치만 통과**. 스텐실 반전 마스크: `Focus/Hole`(Image + Mask, 그림 안 보임, 대상 Image 스프라이트로 모양을 맞춤) 자식 `Dim`(`TutorialFocusDim`, 스텐실 비교를 NotEqual로 바꾼 머티리얼 → 구멍 밖에만 그려짐). 터치 막기는 Mask 밖의 `Blocker`(`TutorialFocusBlocker`)가 담당(Mask 자식은 Mask 레이캐스트 필터 때문에 구멍 밖을 못 막음). 대상을 매 프레임 따라가고 살짝 커졌다 작아짐, 대상이 꺼져 있으면 어둡게 안 함. 소환 블록은 `useFocus`(기본 켬)로 소환 버튼에 포커스. 프리팹엔 메뉴 `Tools/Tutorial/Add Focus To Tutorial Prefabs`로 구워 둠(모양·색은 프리팹에서), 없으면 실행 중에 기본 모양 생성.
  - 드래그 블록(`DragTutorialStep`: MoveKing·MergeMinion): 지정 유닛만 드래그·지정 칸에만 드롭(`MinionFieldController.dragFilter`/`dropFilter`, 다른 칸이면 원위치). **손 드래그 연출(유닛 칸 → 목표 칸)이 완료할 때까지 반복**(손 = 프리팹의 `TutorialHand` 이미지, `TutorialCanvas.finger`, 손끝 = pivot), 플레이어가 드래그하는 동안만 숨김. 소환 블록은 버튼 위 손가락 터치 연출(`fingerTarget`).
  - **화자**: `TutorialCanvas.speakers`(key·이름·초상화)에 한 번 등록, 대화 블록은 `speakerKey`로 고름 → 화자가 바뀌면 이름·초상화도 바뀜. 킹슬라임 화자 이름 = **"마왕을 꿈꾸는 슬라임"**(key `KingSlime`). 초상화가 없으면 숨김(이미지는 준비 중). 이름은 아직 번역 시트에 없음(글씨 그대로).
  - 칸 지정 = `Tile.index`(6*6, x 왼→오 0~5, y 위→아래 0~5). 블록이 쓰는 칸이 막힌 칸이면 시작할 때 풀고 다른 빈 칸을 대신 막는다(`TileManager.EnsureUsable`, 막힌 칸 수 유지).
- **FirstPlay(첫 플레이) 튜토리얼**: 처음 실행한 유저는 **로비 대신 바로 InGame**(`LobbyManager.Start` → `TutorialManager.TryRedirectToFirstPlay`, 에너지 소모 없음, 앱을 켠 동안 1번만). `GameManager.Start`가 페이드 전에 시작. 흐름(2026-11-13): 킹을 오른쪽 아래 그룹에 둠 → 대화 → **wave 시작 → 3초 뒤** 킹을 왼쪽 위 그룹의 왼쪽 아래 칸(0,2)으로 드래그 → 블록 → 대화 → 다시 킹 이동(칸은 인스펙터 입력) → 소환 버튼 보이기 → **소환 3기(1번째·3번째를 같은 종으로 지정) → 3번째를 1번째 위로 합성**. 이후 단계는 미정.
  - **프리팹 = `3.Prefabs/Tutorial/FirstPlayTutorial`**(전용 Canvas + 블록, 매니저 `tutorialPrefabs`에 등록). 대사·칸·모양·숨길 UI 이름 모두 프리팹에서 고침. 메뉴 `Tools/Tutorial/Create Tutorial Prefabs`(없으면 생성·등록) · `Reset Tutorial Progress`(완료 기록 삭제).
- 테스트 `TutorialPlayTest`: 튜토리얼을 끝까지 진행(대화 터치·드롭·소환 버튼) → wave 시작 블록 전 wave 미시작, 킹 칸·소환 종/칸·합성 결과, 끝나면 완료 기록·오브젝트 삭제·wave 진행.
- 끄기: `BuildSetting.skipTutorial` / 자동 테스트는 플레이 진입 전에 `SessionState`(`TutorialManager.DISABLE_FOR_TEST_KEY`)로 끔(`InGamePlayTestBase.BeforeEnterPlayMode`, `TutorialPlayTest`만 켬) — 테스트가 정한 값이 `skipTutorial`보다 우선(skipTutorial을 켜 둬도 `TutorialPlayTest`는 돌아감).

## 기능 해금 버튼 (2026-11-13)
- **`UnlockGate`**(잠글 버튼 오브젝트에 붙임) + **`UnlockCondition`**(종류·value·count, `ConditionData`와 같은 모양이지만 별개 — ConditionData는 인게임 능력·탄 CSV용이라 건드리지 않음). 조건은 **모두 만족해야 해금**.
  - 조건 종류(`UnlockConditionType`, 뒤에만 추가): UserLevel(유저 레벨) · BestWave(최고 도달 wave, **표시 번호** = maxWave + 1, value = 난이도 / 비우면 전체 중 최고) · CurrentWave(인게임 진행 중 wave).
  - 잠김 표현은 인스펙터 조합: 기본 = 보이지만 동작 안 함 + 누르면 조건 안내 토스트(`Unlock_*`) / `hideWhenLocked` = 아예 숨김 / `lockedObjs`·`unlockedObjs` = 다른 오브젝트로 대체 / 그 밖은 `onChanged(bool)` 또는 상속(`Apply`).
  - 갱신: 켜질 때 + 유저 경험치 변경(`ChangedUserExpEvent`) + wave 시작(`WaveStartEvent`). 다른 조건을 추가하면 값이 바뀌는 곳에서 `Refresh()`.
  - 튜토리얼(`Common/Tutorial`)과는 독립. 자동 테스트 없음(사용자 요청).

## 튜토리얼 (2026-11-13, 작업 중)
- **킹 이동 안내(`MoveKingTutorialStep`) 동안 게임 슬로우**: 게임 속도 0.3 → 5초(실제 시간)에 걸쳐 0까지 줄임, 단계가 끝나면 이전 속도로 복구. 드래그·손가락 연출·목표 칸 표시·킹 애니메이션은 실제 시간이라 원래 속도로 보인다.

## 사운드 (2026-11-16, 준비만)
- 사운드 파일은 **프리팹 `Resources/Init/SoundManager`의 `sfxDatas`/`bgmDatas` 칸에 드래그**. 칸은 `SFXType`/`BGMType` enum마다 하나씩 enum 순서로 자동 생성(`SoundManager.OnValidate`, 칸 이름 = 종류). 클립이 빈 종류는 재생해도 무음.
- enum은 번호로 직렬화되므로 **새 종류는 맨 뒤에만 추가**. 자주 나는 소리는 `minInterval`(겹침 방지)·`pitchRandom`으로 조절. BGM은 BGM 믹서 그룹의 AudioSource 하나(`PlayBGM`/`StopBGM`), 루프 SFX는 `StopSound`. 이전 `BGMPlayer` 자식 방식은 삭제.
- **미니언 공격 소리 = 종마다 `MinionData.attackSfx`(SFXData, 에셋 인스펙터)**, AudioClip이 비면 공통 `SFXType.MinionAttack`(`SoundManager.PlaySound(SFXData, fallback)`). 재생 시점 = 공격 모션과 같음(`Minion.Update`에서 공격 성공 + `UseFindTarget`일 때 `PlayAttackSound`) → 회전체·경로 순환(Orbit·Mud·Shield)은 공격 소리 없음(적중 소리는 따로).
- (2026-11-16) SFX 칸 전부 = **Universal Sound FX에서 고른 임시 소리**(복사본 `Assets/7.Sounds/SFX/`, 새 guid). 귀여운 톤 위주(RETRO·CARTOON·PUZZLE), 자주 나는 소리(적중·코인·처치·공격)는 볼륨 낮게 + minInterval + pitchRandom.
- (2026-11-16) **SFX 종류는 클립이 있는 것만 둔다**: 안 쓰기로 한 PopupClose·DragStart·DrawItemFail·ItemSelectAppear·ItemSelect는 enum에서 삭제(87종). 빈 칸으로 남겨 두지 않음.
- (2026-11-17) 클립이 비어 있던 31종(Hit*·Status*·EliteDead·King*·IceHit·HailSpawn·Bounce·ShieldBlock·BerserkerStack·Joker 카드 4종·DarknessOpen·BuffReceive·WaveStart·EliteAppear·CardShake/RevealHighGrade·ExpBarFill·TutorialNext) 삭제 → **56종**. 프리팹 칸은 이름 기준으로 다시 맞춤. `7.Sounds`에서 어디에도 참조되지 않는 소리 파일 93개 삭제(지금 쓰는 파일 = `7.Sounds` 루트·`SFX`·`SFX2`). 서드파티 팩(Universal Sound FX, FREE CASUAL PACK)은 그대로.
- (2026-11-19) **사운드 팩 원본은 git에 올리지 않는다**(`.gitignore`: `Assets/Universal Sound FX/`, `Assets/SoundPackage/`). 쓸 소리는 `Assets/7.Sounds`로 **복사해서** 연결(팩 파일을 직접 참조하면 다른 PC에서 소리가 빠짐).
- (2026-11-17) **게임 코드에서 재생 = `SoundManager.Play(SFXType.X)`**(static, SoundManager 없으면 무시). **루프 소리(지옥불·레이저) = `SoundManager.SetLoop(type, owner, on)`**: owner가 여럿이어도 첫 owner가 켜면 재생, 마지막 owner가 끄면 정지 → owner는 꺼질 때(OnDisable) 반드시 off.
- (2026-11-16) **BGM은 로비·인게임 2종뿐**(`BGMType` Lobby/InGame). 엘리트·보스 wave·튜토리얼·실패·결과 BGM은 없음(만들지 않음).
- (2026-11-17) **56종 모두 게임 코드에 연결.** 창 열기 소리는 `CanvasUI.OpenCanvas`(꺼져 있다 켜질 때만) → `PlayOpenSound()` 기본 = PopupOpen. **팝업이 아니거나 자기 소리가 있는 창은 재정의**(빈 재정의: WaveWarning·WayPointer·Boss·Fail·Result·SelectItem·BlockCanvas / 다른 소리: DrawResultCanvas = ChestOpen, UserLevelUpCanvas = UserLevelUp). 새 창을 만들 때도 확인. 버서커는 공통 공격 소리 대신 `PlayAttackSound` 재정의로 BerserkerHit. 탭 버튼 = TabChange. BGM: 로비 `LobbyManager.Start`, 인게임 `GameManager`(페이드 콜백에서 한 번 더, SoundManager.Start가 늦게 Lobby를 틀 수 있어서). 처치 코인 외 코인(시작·wave 종료·판매·조커·시체수집)과 성장·변신으로 교체된 미니언 등장은 소리 없음.

## 테스트 (2026-11-07)
- (2026-11-15) **수동 테스트 창 `Tools/Test/Minion Test`**(`Test/Editor/MinionTestWindow`, 에디터 전용, InGame Play 중): 미니언 key·머지 레벨·진화·칸 지정 소환, 선택한 미니언 스탯·버프·스킬 진행도 + **쿨타임 채우기**(`Minion.FillSkillForTest`, 스킬 미니언마다 재정의: 시간형 = 다 참, 횟수형 = 1회 남김), 킹 무적·코인 무한·속도, wave 이동·적 종류/방어 타입 소환·적 정지(허수아비), 선택 칸 타일 아이템 설치·레벨·제거. 값은 EditorPrefs. 기기용 `TestCanvas`(배속·골드·다음 wave)는 그대로. **새 스킬 미니언을 만들면 `FillSkillForTest`도 재정의.**
- (2026-11-20) **미니언 피해 측정**(`Minion Test` 창 맨 위 "피해 측정", `Test/Editor/MinionDamageBenchmark`): 미니언 1마리를 **칸 (2,2)**(왼쪽 위 TileGroup의 오른쪽 아래)에 두고 **wave N(기본 10)을 처음부터 끝까지**(적 전부 처리 또는 wave 시간 초과) 돌려 준 피해(`DamageMeterSystem`)를 **머지 레벨 0~4**로 잼. 버튼 = 선택 Key만 / 전체(옵션: 게임에서 쓰는 종만). 측정 중 킹 무적·킹 공격 끔·**권능 끔(다시 안 켜짐)**·속도 조절, 측정마다 랜덤 시드 고정·필드 미니언/적/지뢰 정리. 결과는 `Library/MinionDamageTest.json`에 누적(같은 난이도·wave·key·레벨은 덮어씀) → **`Assets/MinionDamageTest/MinionDamage_{난이도}_Wave{N}.xlsx`**(요약 = 종 × 머지 레벨 피해·4/0 배율, 상세 = 공격력·공속·피해·DPS·처치·경로 끝·시각). 엑셀은 외부 라이브러리 없이 `SimpleXlsxWriter`로 씀(엑셀에서 열려 있으면 저장 실패 경고 → 다음 측정 때 다시 저장).
- (2026-11-17) **타일 아이템 수동 테스트 창 `Tools/Test/TileItem Test`**(`Test/Editor/TileItemTestWindow`): 6*6 칸 판에서 칸을 직접 고름(빈 칸·막힌 칸도 가능, 필드 클릭 선택과 무관) → 아이템을 **원하는 레벨로 설치**(설치 후 확정 강화)·교체·레벨 +1·제거·막힘 풀기, 그 칸에 미니언 올려 스탯·탄 수·버프 확인. 개척(막힌 칸 선택)·슬라임 수 증가(칸 없이 획득)는 방식에 맞는 버튼. 보유 목록·빈 칸 전부 설치·전부 제거, 뽑기 확률/후보 수·선택 창/뽑기 창 열기, 무적·코인 무한·속도·적 소환(`MinionTestWindow.SpawnEnemies` 공용)·적 정지.
- **테스트는 메인 에디터가 아니라 테스트 전용 복사본(`~/KingSlime-Test`, git worktree)에서 돌린다** (2026-11-08). `Tools/run-tests.sh [필터]` = 메인의 커밋 + 작업 중 변경을 복사본에 동기화(`Tools/sync-test-project.sh`) → 복사본 에디터가 열려 있으면 그 에디터에서, 아니면 배치모드로 실행. 백그라운드로 돌리고 끝나면 결과를 알려준다. 메인 씬·저장 파일을 건드리지 않으므로 작업 중에도 가능.
- EditMode 테스트는 `Assets/Tests/Editor/`(asmdef 없음 → `Assembly-CSharp-Editor`라서 게임 코드를 바로 참조). 게임 스크립트에 asmdef가 없으므로 테스트에도 asmdef를 만들지 않는다.
- 기획 규칙은 **씬·싱글톤 없이 계산할 수 있는 static 함수**로 빼서 테스트한다(예: `Minion.CanMerge`, `SacrificeMinion.CanSacrificeMerge`, `DrawItemButton.GetDrawChance(int)`). 기획이 바뀌면 `GameRuleTest.cs`의 기대값도 같이 고친다.
- 플레이 테스트(`MinionPlayTest`): EditMode 테스트에서 InGame 씬을 열고 `EnterPlayMode`로 들어가 실제 게임을 돌린다(PlayMode asmdef 없이). 로비 없이 InGame 씬만으로 시작 가능. 로드된 **모든 MinionData key × 머지 레벨 0/4/4+진화**를 하나씩 소환해 배속(×4)으로 싸우게 하고 에러·예외가 없는지 본다. 새 슬라임은 MinionData 에셋만 있으면 자동으로 대상이 된다.
  - 에디터용 Firebase 네이티브 라이브러리가 없어 플레이마다 `DllNotFoundException: FirebaseCppApp`이 난다 → 테스트에서는 무시 목록(`IGNORE_ERRORS`)으로 거른다.
  - 사망 시 `EndGame`에서 유저 데이터가 저장되므로 테스트 중엔 킹 체력을 계속 채워 죽지 않게 한다.
  - 콘솔 Error Pause가 켜져 있으면 에러에서 멈추므로 테스트가 일시정지를 풀면서 진행한다.
  - **테스트 중엔 광고·인앱 결제를 띄우지 않고 바로 성공**(`AdManager.SkipAdForTest`, `ProductManager.SkipInAppPurchaseForTest`). 테스트 기본 클래스가 플레이 진입 후(`WaitGameStart`/`WaitLobbyStart`) 켜고 TearDown에서 끔. 기본값 false라 실제 게임엔 영향 없음.
  - (2026-11-12) **테스트 중엔 소리 음소거**(`InGamePlayTestBase.MuteAudio` = 에디터 `EditorUtility.audioMasterMute`). 플레이 진입 전(`BeforeEnterPlayMode`)에 끄고 TearDown에서 테스트 전 값으로 되돌림.
  - 공용 준비는 `InGamePlayTestBase`(씬 열기, 에러 수집, 저장 파일 `Assets/User*.json` 백업·복원 — 플레이 중 업적 처치 수 등이 저장되기 때문).
  - `TileItemPlayTest`: 타일 index 6*6 중복 없음, 모든 타일 아이템 설치→전투→판매, 스탯 타일 효과, 버프 범위 확장(대각선 버프). `DrawItemButtonPlayTest`: 뽑기 버튼 성공/실패/코인 부족 흐름(실패는 `Random.InitState` 시드로 고정).
  - `DefenceTypePlayTest`(2026-11-10): 일반·갑옷·정령 적이 실제 경로(wave → 프리팹 → 풀 → EnemyDefence)로 물리·마법 슬라임 피해를 배율표대로 받는지.
  - `WaveResultPlayTest`: wave를 강제로 넘겨(`waveTimer`/`mainWaveTimer` = 0) 엘리트·보스 주기, 보스 wave는 시간으로 안 넘어감, 보스 처치 → 다음 wave + 종료 코인. 사망 → 실패 화면 → 결과 화면 보상(골드 = waveIdx, 경험치 = waveIdx/2, 뽑기권 = 합성 횟수)·최고 wave 기록·홈 → 로비. 부활 흐름.
  - `EvolutionPlayTest`(2026-11-09): 진화 타일 흐름(최대 레벨만 진화·옮겨도 유지·타일 재사용·슬라임 위 설치 시 진화·희생/성장/조커 제외·새 소환은 미진화) + 각성 효과(상태 값, 발사 탄 스펙, 가시방패·화염 스킬 실제 피해, 포위 진형 버프, 시체수집 엘리트 보너스).
  - `GameCyclePlayTest`(2026-11-09): 로비 KS 업그레이드(버튼) → 전투 버튼(에너지 차감) → 인게임 스탯 반영 → 사망·부활(보상 없음) → 사망·포기 → 결과 보상 → 로비 레벨업 창(레벨마다 하나씩, 2레벨 연속) → 재업그레이드 → 2판 wave 0 사망(보상 0, 최고 기록 유지). 단계마다 UserDataManager 메모리 값과 **저장 파일(SaveManager.LoadData)** 을 같이 확인.
  - **도메인 리로드 후 테스트 이터레이터는 직렬화로 복원되는데 람다가 캡처한 지역 변수(클로저)는 null이 된다** → 플레이 모드 진입 뒤 본문은 별도 이터레이터(`XxxBody()`)로 분리.
- 새 데이터 추가 시 주의 (테스트로 잡힌 실수):
  - ItemData 에셋을 **복사해서 만들면 key·itemPrefab이 원본 그대로** 남는다(`ItemData.LoadData`는 기존 key로 CSV 행을 찾고 프리팹은 안 채움) → key와 itemPrefab을 직접 바꾸고 프리팹도 만들 것.
  - 슬라임 데이터 에셋은 **`{Key}MinionData`(EnhanceMinionData 계열)** 로 만들어야 LoadData가 MinionEnhance CSV를 읽는다. 기본 `MinionData`로 만들면 강화 데이터가 비어 소환 시 NullReference.

## 빌드·테스트 배포 (2026-11-13)
- **자동화는 테스트 배포까지만, 스토어 릴리스는 사용자가 수동.** 지금은 Firebase App Distribution(Android APK / iOS Ad Hoc IPA). 나중에 Play 내부 테스트 트랙·TestFlight 업로드(fastlane)를 붙이면 그 빌드를 콘솔에서 승격해서 릴리스.
- `Tools/build-release.sh android|ios|all [-m 노트]`: **빌드 전용 복사본(`~/KingSlime-Build`, git worktree)** 에 메인을 동기화해 배치 빌드(`ReleaseBuild.BuildAndroid/BuildIOS`) → `firebase appdistribution:distribute`. 메인·테스트 복사본의 플랫폼을 바꾸지 않으려고 따로 둠. 앱 ID `com.BEI.KingSlime`, 빌드 번호 = 날짜 시각(`yyMMddHHm`), Addressables는 빌드 직전에 같이 빌드.
- (2026-11-13) **Analytics 이벤트**: 코드에서는 `FirebaseEvent.Log(이름, ("key", 값)...)` 한 줄, 씬·프리팹에서는 `FirebaseEventTrigger`(버튼 클릭 / 켜질 때 / UnityEvent). 이름은 `FirebaseEventName` 상수로 모음. 초기화 전 호출은 큐에 쌓았다 보냄, **에디터는 콘솔 로그만**(Firebase 네이티브 없음).
- 비밀 값(테스터 그룹 `FIREBASE_GROUPS`, `KS_APPLE_TEAM_ID`, 키스토어 `KS_KEYSTORE_*`)은 `~/.kingslime-release/config.sh`(git 밖). 키스토어가 없으면 디버그 키로 서명.

## 데이터 파이프라인
- (2026-11-16) **`EffectType` 같은 enum은 프리팹·에셋에 번호로 저장되므로 항상 맨 뒤에만 추가**(중간에 넣으면 기존 프리팹 값이 다른 항목을 가리킴. 다크니스 이펙트가 사라졌던 원인).
- 이펙트 글씨 색은 **회색 금지**: 밝은 회색 타일 위에서 안 보임.
- (2026-11-18) **튀어나오는 글씨는 공용 `EffectManager.PlayPopText`**: 자유 글씨·색 = `(pos, text, color)`, 자주 쓰는 문구 = `(pos, PopTextType)` 프리셋(글씨 = 번역 `PopText_{종류}`, 색 = `GetPopTextColor`). 새 프리셋은 enum + 번역 key + 색만 추가.
- (2026-11-08) **번역**: `Json/Translate.csv`(시트 Key / Kr / En) → DataTableImporter(`Tools/Data/Import TranslateData`, Play 시 자동) → `Resources/TranslateData.asset` → `TranslateManager.GetText`가 처음 호출될 때 로드. 한국어 기기 = Kr, 그 외 = En, 칸이 비면 Kr로 대체, key가 없으면 `*key`. 코드에서 보여주는 문구는 한글 리터럴 대신 key로(조합 key: `Difficulty.{난이도}`, `Minion.{Key}`/`Minion.{Key}.Desc`, `{StatType}_title`, `Grade.{등급}`). `TranslateDataTest`가 코드 key가 시트에 다 있는지 검사. (2026-11-12) **로비 씬의 고정 글씨(제목·탭·버튼 이름 등, 코드가 참조하지 않는 TMP_Text)는 `TranslateText`(key, `formatArg` = 번역문 `{0}` 자리)** 로 출력, 언어를 바꾸면 꺼져 있는 창까지 다시 그림. 업적 패널 버튼 글씨는 프리팹(`DailyAchievementPanel`/`CumulativeAchievementPanel`)에 붙임. 새 고정 글씨를 놓으면 TranslateText + 시트 key도 추가(`SettingPlayTest`가 key 존재·언어 변경 반영 검사). BGM·SFX·ON/OFF·DISCORD·BEST·NEW·WAVE·MY·PLAY처럼 두 언어가 같은 글씨와 Layer Lab 샘플 UI(Progression_Pass2, Shop_List, Character_Hero_List)는 제외. InGame 씬은 아직 미적용. 슬라임 설명 중 CSV Desc를 그대로 쓰는 종(Bounce, Orbit 등)과 아이템 이름·설명(ItemData.csv)은 아직 번역 안 됨.
- **MinionData 배열 필드(bounces, shootCounts 등)는 에셋에 직렬화된 값이 코드 초기값보다 우선.** 코드 기본값만 바꾸면 기존 에셋엔 반영 안 됨 → 에셋도 같이 수정.
- 기획 데이터는 `Assets/Json/**/*.csv`(엑셀 → TSV). 에디터에서 `LoadData`(DataTableImporter)를 실행해야 에셋(`AddressableResources`)에 반영됨.
  - WaveData/{난이도}.csv: StartWave, DefenceTypes, DefenceWaveTypes (WaveType은 번호로 결정, 위 웨이브 섹션)
  - (EnemyPatternData/{난이도}.csv는 2026-10-16부터 코드에서 사용 안 함. 일반 스폰은 1초마다 1마리 고정)
  - MinionData.csv: Key, Desc, Grade, AttackType(enum 이름 그대로: Physic/Magic/AllRounder), Growth, **AttackSpeed, AttackRange, CritChance**(2026-11-06 MinionEnhance에서 이동. **강화 레벨·머지 레벨과 무관한 고정 기본 스탯** → `MinionData.baseMinionStats`, CritChance는 %), ReinforceAttackPower, ReinforceAttackSpeed(코드 미반영). 슬라임 치명타 배율은 `GameSetting.INIT_CRIT_POWER`(1.5, 타일 치명타는 2배).
  - MinionEnhance/MinionEnhance_{등급}.csv (2026-11-05, 시트도 등급별 탭, SheetDownloadSetting.entries에 각각 등록): `Key`는 블록 첫 행에만 쓰고 아래 행은 비우면 위 Key를 이어받음. 읽을 때는 등급과 상관없이 `MinionEnhance_*.csv` 전체에서 key 블록을 찾음(같은 key가 두 파일에 있으면 경고). **Key / Level / AttackPower 만** 읽음(시트 범위 A:C, EnhanceAbility 열은 2026-11-09 삭제)(공속·사거리는 MinionData.csv)(`/`로 mergeLv 값).
  - (2026-11-04) MinionStatType은 StatType으로 통합. 슬라임 고유 스탯(`uniqueMinionStats`)은 CSV가 아니라 **각 슬라임 MinionData의 `LoadData` 재정의**에서 `SetUniqueStat`으로 입력.
