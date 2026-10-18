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
- 게임 시작 시 타일 중 **랜덤 5칸은 사용 불가**(`BLOCKED_TILE_COUNT`, 2026-10-18). 소환/드래그/캐릭터 배치 모두 막힌 칸에는 못 놓음.

### 의도
- **소환 가격은 올리지 않는다.** 싸게 계속 뽑는 뽑기 재미가 핵심.
- **합성은 스탯 합계로 보면 약간 손해**여야 한다. 대신 칸이 비어서 다시 소환할 수 있다는 게 합성의 이득.
  - 칸이 남아 있으면 소환이 이득, 칸이 차면 손해를 감수하고 합성 → 이 순환이 의도.
- 강화 레벨(`userSlime.enhanceLevel`, 로비에서 영구 성장)과 머지 레벨(`mergeLevel`, 인게임 합성)은 별개.
  - 슬라임 에셋의 `level: N` 블록 = 강화 레벨, 그 안의 `values` 배열 = mergeLv별 값.
  - `values` 2개 = `기본/증가량`(선형), 그 외 = mergeLv별 값(모자라면 마지막 값).
- Ice / Flame / Wacky / Bounce: mergeLv가 오를수록 한 번에 발사하는 탄 수 증가(`shootCounts`), 탄마다 **서로 다른 타겟**을 노림.
- 랜덤 데미지(Wacky 등): 데이터에는 **중간값만** 입력, SlimeData의 `attackPowerRandomRatio`로 `중간값 × (1 ± 비율)` 범위 결정. `3~15` 같은 범위 직접 입력은 쓰지 않는다.

## 웨이브 (2026-10-17 phase → wave / 2026-10-18 휴식 10초 부활)
- wave 0~19 (총 20), 19 = 보스 (`GameSetting.BOSS_PHASE`). 각 wave의 WaveType은 WaveData에 **하나로 고정**, 선택 없이 바로 진행.
- WaveType: Normal, Hard, Elite, Boss. wave마다 `Wave` 인스턴스를 새로 생성(`GameManager.accWaves`에 누적).
- 전투 웨이브 흐름: **웨이브 30초**(`WAVE_TIME`, 30초 내내 1초마다 1마리 스폰) → **휴식 10초**(`BREAK_TIME`) → 다음 wave.
  - 각 Wave가 자기 타이머를 가지고, 시간이 다 되면 스스로 종료(스폰 중지) → `GameManager.EndWave` → 휴식(`GameManager.isBreak`).
  - 휴식 중에는 스킵 버튼·TimeBar 숨기고 "다음 웨이브까지 N초" 카운트다운 표시.
  - 끝날 때 필드에 남은 적은 제거하지 않고 계속 이동.
- **웨이브 스킵**: 전투 wave 중 **언제든** 가능(휴식 중 불가), 누르면 휴식 없이 바로 다음 wave. 보상은 **고정 골드**(`SKIP_WAVE_GOLD`, 시간에 따라 줄지 않음). 보스 wave에서는 스킵 불가.
  - **스킵해도 이전 wave는 끄지 않는다** → 이전 wave의 남은 스폰이 다음 wave와 겹쳐서 진행(누적). 빨리 넘기는 만큼 위험을 감수하는 구조.
  - 이전 wave가 나중에 끝나도 진행에는 영향 없음(현재 wave가 끝날 때만 다음 wave 시작). 같은 wave의 중복 종료는 무시.
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
- 적의 방어 타입 기준은 프리팹이 참조하는 **EnemyData.defenceType**.
- 공격 속성(Physic/Magic/Vibrational) × 방어 타입 배율은 `Resources/DefenceTable` 에셋 한 곳에서 관리 (인스펙터 표). 방어형 클래스마다 계산을 따로 두지 않는다.
  - 기본값: 갑옷 물0.5/마1.5/진1 · 정령 1.5/0.5/1 · 중갑 0.7/0.7/1.6 · 생체 1.3/0.7/1 · 일반 1/1/1

## 데이터 파이프라인
- **SlimeData 배열 필드(bounces, shootCounts 등)는 에셋에 직렬화된 값이 코드 초기값보다 우선.** 코드 기본값만 바꾸면 기존 에셋엔 반영 안 됨 → 에셋도 같이 수정.
- 기획 데이터는 `Assets/Json/**/*.csv`(엑셀 → TSV). 에디터에서 `LoadData`(DataTableImporter)를 실행해야 에셋(`AddressableResources`)에 반영됨.
  - WaveData/{난이도}.csv: Phase, WaveType(phase당 1개)
  - (EnemyPatternData/{난이도}.csv는 2026-10-16부터 코드에서 사용 안 함. 일반 스폰은 1초마다 1마리 고정)
  - SlimeEnhance/{key}.csv: 강화 레벨별 AttackPower/AttackSpeed/AttackRange(`/`로 mergeLv 값) + 고유 스탯 컬럼
