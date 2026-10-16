# DigDig 기획 노트

Claude가 기획 의도를 유지하기 위한 문서. 기획이 바뀌거나 사용자가 새 의도를 설명하면 이 파일을 갱신한다.
코드에서 바로 알 수 있는 구현 세부는 적지 않고, **왜 그렇게 하는지(의도)** 위주로 적는다.

## 핵심 루프
- 적 처치 → 코인 획득 → 슬라임 소환(뽑기) → 칸이 차면 합성 → 빈 칸에 다시 소환
- 돈이 계속 쌓이는 만큼 계속 강해질 수 있어야 한다.

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

### 의도
- **소환 가격은 올리지 않는다.** 싸게 계속 뽑는 뽑기 재미가 핵심.
- **합성은 스탯 합계로 보면 약간 손해**여야 한다. 대신 칸이 비어서 다시 소환할 수 있다는 게 합성의 이득.
  - 칸이 남아 있으면 소환이 이득, 칸이 차면 손해를 감수하고 합성 → 이 순환이 의도.
- 강화 레벨(`userSlime.enhanceLevel`, 로비에서 영구 성장)과 머지 레벨(`mergeLevel`, 인게임 합성)은 별개.
  - 슬라임 에셋의 `level: N` 블록 = 강화 레벨, 그 안의 `values` 배열 = mergeLv별 값.
  - `values` 2개 = `기본/증가량`(선형), 그 외 = mergeLv별 값(모자라면 마지막 값).
- Ice / Flame / Wacky / Bounce: mergeLv가 오를수록 한 번에 발사하는 탄 수 증가(`shootCounts`), 탄마다 **서로 다른 타겟**을 노림.
- 랜덤 데미지(Wacky 등): 데이터에는 **중간값만** 입력, SlimeData의 `attackPowerRandomRatio`로 `중간값 × (1 ± 비율)` 범위 결정. `3~15` 같은 범위 직접 입력은 쓰지 않는다.

## 웨이브 / 페이즈
- phase 0~19 (총 20), 19 = 보스 (`GameSetting.BOSS_PHASE`). 각 phase의 WaveType은 WaveData에 **하나로 고정**, 선택 없이 바로 진행. (웨이브 선택 UI `SelectWaveCanvas`는 2026-10-16 삭제)
- WaveType: Normal, Hard, Shop, Elite, Boss. 각 타입별 `Wave` 컴포넌트가 진행/종료 담당. (Break, Break_Snake, MidBoss 웨이브는 2026-10-16 삭제)
- 전투 웨이브(BattleWave) 흐름 (2026-10-16 변경): **웨이브 60초**(앞 45초 스폰 + 15초 진행) → **휴식 20초** → 다음 phase.
  - 웨이브는 시간으로 끝난다. 끝날 때 필드에 남은 적은 제거하지 않고 계속 이동 (휴식 중에도).
  - **웨이브 스킵**: 30초(`CAN_START_NEXT_PHASE_TIME`)부터 가능. 바로 누르면 10골드, 점점 줄어 종료 5초 전(55초)에 0골드 + 스킵 불가. 스킵해도 남은 적은 그대로 → 빨리 넘기는 만큼 위험을 감수하는 구조.
  - **휴식 스킵**: 언제든 가능, 보상 없음.
- **부활(Resume)** (2026-10-16): 필드 적 전부 제거(처치 보상 없음) → 체력 가득 → **죽은 phase를 처음부터 다시 시작**. 휴식 중에 죽었으면 그 phase는 이미 끝났으므로 다음 phase부터.
- Normal vs Hard: 스포너 설정(인스펙터 값)과 배율만 다르게, 코드는 공유하는 방향.

### 난이도 의도 (2026-10-16 변경)
- **소환량은 고정: 1초마다 1마리** (`PhaseEnemySpawnPattern`). 소환 수를 늘리는 방식(PeakCountMul 곡선)은 폐기.
- **난이도는 적 체력으로만 올린다.** (`Enemy.GetHp`: phase별 상승 + 웨이브 진행에 따른 상승)
- 전투 웨이브의 특수 스폰(BundleWave 등 랜덤 스포너 호출)은 2026-10-16 제거. 일반 스폰만 사용.

### 적 방어 타입 (2026-10-16)
- StageData.csv `DefenceTypes`: 그 스테이지에 나오는 방어 타입 목록. `DefenceWaveTypes`: phase마다 순서대로 적용(`phase % 개수`).
- **한 phase 안에서는 모든 일반 적이 같은 방어 타입.** phase가 바뀌면 다음 칸. `Random`은 DefenceTypes 중 하나를 phase 시작 시 한 번 뽑음.
- 적의 방어 타입 기준은 프리팹이 참조하는 **EnemyData.defenceType**.
- 공격 속성(Physic/Magic/Vibrational) × 방어 타입 배율은 `Resources/DefenceTable` 에셋 한 곳에서 관리 (인스펙터 표). 방어형 클래스마다 계산을 따로 두지 않는다.
  - 기본값: 갑옷 물0.5/마1.5/진1 · 정령 1.5/0.5/1 · 중갑 0.7/0.7/1.6 · 생체 1.3/0.7/1 · 일반 1/1/1

## 데이터 파이프라인
- **SlimeData 배열 필드(bounces, shootCounts 등)는 에셋에 직렬화된 값이 코드 초기값보다 우선.** 코드 기본값만 바꾸면 기존 에셋엔 반영 안 됨 → 에셋도 같이 수정.
- 기획 데이터는 `Assets/Json/**/*.csv`(엑셀 → TSV). 에디터에서 `LoadData`(DataTableImporter)를 실행해야 에셋(`AddressableResources`)에 반영됨.
  - WaveData/{난이도}.csv: Phase, WaveType(phase당 1개)
  - (EnemyPatternData/{난이도}.csv는 2026-10-16부터 코드에서 사용 안 함. 일반 스폰은 1초마다 1마리 고정)
  - SlimeEnhance/{key}.csv: 강화 레벨별 AttackPower/AttackSpeed/AttackRange(`/`로 mergeLv 값) + 고유 스탯 컬럼
