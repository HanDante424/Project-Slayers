# Project Slayers - Current Status

상태: 현재 활성 구현 현황  
기준: GitHub `main` 커밋 `f9d2f30` (2026-11-06, Town 구조 정리 병합 후), 소스·씬 정적 검토  
용도: 무엇이 구현되어 있고 무엇이 비어 있는지에 대한 단일 기준. `05_DevelopmentRestartPlan.md`를 대체한다.

이 문서는 구현 현황만 기록한다. 게임 디자인·수치의 기준은 `01`~`07` 문서다.
이 문서의 내용은 소스 정적 검토 결과이며, Unity 컴파일과 Play Mode 결과는 각 시스템 문서에 기록된 실제 실행 기록을 따른다.

---

## 1. 환경

- Unity 6000.3.22f1
- 씬: `MainMenu`, `Town`, `Dev_Combat`, `Search_Combat`
- 원격 안정 브랜치: `main`

---

## 2. 구현됨

### Town

- `TownMenuController` 기반 패널 전환: 전투 / 캐릭터 / 상점 / 연구 / 창고 / 옵션
- 캐릭터 패널: 개요, 세부 스탯, 장비, 스킬 정보
- 스테이지 선택: `StageDatabase`, `TownStageSelectController`, 해금 여부 검사
- 전투 진입 캐릭터 선택
- 개발용 상점 (`06` 문서)
- 옵션: 세션 데이터 초기화 (`06` 문서)

### Town 씬 구조 (2026-11-06 정리, PR #5~#12)

```
[System]
├ Managers   StageManager, SceneLoader, InputBindingManager
├ Data       ItemEquipmentRegistry (+ InventoryCharacterDataSource, InventoryCraftingDataSource), Icon, Character(CharacterStats)
├ TownUI
│ ├ TownMenuController   Panel Controllers 목록 (6개)
│ ├ Settings             TownSettingsController
│ ├ Research             TownResearchNavigationController
│ ├ Storage              TownStorageNavigationController, TownStorageListView
│ ├ Shop                 TownShopNavigationController, TownBlacksmithCraftController, TownDevShopController
│ ├ Battle               TownBattleNavigationController, TownStageSelectController, TownStageDetailsView, TownCombatCharacterSelectController
│ └ Character            TownCharacterNavigationController, Stats, Overview, Equipment, Skill
└ Debug      TownTestMenu
```

규칙:

- 패널 컨트롤러는 `ITownPanel`(`Panel`, `OnPanelShown`, `OnPanelHidden`)을 구현하고 `TownMenuController.panelControllers`에 등록한다. `[System]`은 항상 켜져 있으므로 패널 열림/닫힘을 `OnEnable`/`OnDisable`에 의존하지 않는다.
- 패널 안의 하위 화면에 묶인 뷰(대장간, 개발 상점, 스테이지 선택/상세, 스킬 등)는 `Open()`/`Close()`와 `isOpen`을 가지며, 소유 패널 컨트롤러가 하위 화면이 보일 때만 연다.
- 자기 자식 UI를 검색하던 뷰는 `uiRoot` 참조로 원래 화면을 가리킨다.
- 씬보다 오래 사는 이벤트(`InventorySession`, `TownCharacterSelection` 등) 구독은 `OnDestroy`에서도 해제한다.
- `Canvas`에는 UI 위젯 성격의 스크립트만 남긴다: `TownTitleController`(Canvas/Title), `CharacterStatPanelView` ×2, `RotatedFitToParent`.
- 새 Town 패널을 추가하거나 옮길 때는 `Assets/Editor/TownSystemRestructure.cs`의 단계 패턴(Report/Apply)을 사용할 수 있다.

### 캐릭터 스탯

- 35종 독립 스탯 (`StatType`), 레벨 성장 (`StatGrowth`)
- 정의 데이터(`CharacterStatsData`)와 런타임(`CharacterStats`) 분리
- Modifier 계층과 최종 스탯, 장비 Modifier 네임스페이스 교체
- 현재 HP / MP 런타임 자원 (`PlayerHealth`, `PlayerMana`)

### 인벤토리 · 장비 (`06` 문서)

- `SharedInventory`: 아이템 스택, 독립 장비 인스턴스, 골드, 기억의 파편
- 대장간 제작 (원자적 검증·소비·생성)
- 장착 / 해제, 세트 효과 집계
- 씬 전환 간 세션 유지

### 전투

- 이동 / 점프 (`CharacterMotor2D`), 이동 권한 `Locked` / `Reduced` / `Free`
- 메이헴 액션: 블러드 소드, 블러드 슬래시, 블러드 스피어, 블러드 액스, 블러드 램페이지, 블러드 콤비네이션
- 액션별 입력 버퍼 (`ActionInputBuffer`)
- Ern 테스트 스킬 트리플 슬래시 (레거시 대체 경로 포함)
- 히트박스와 기본 피해 계산 (`02` 문서 구현 현황 참고)
- 적 피격 · 경직 · 넉백 · 사망 (고블린)
- 고블린 이동 / 추적 / 공격, `GoblinPool`
- HP / MP / 스킬 쿨다운 HUD, 적 체력바
- 전투 결과 화면 (보상은 "미지급"으로 표시)

### 저장

- `SaveManager`: `GameData`를 JSON으로 저장 / 로드

### Editor 검증 도구

`Tools > Slayers` 메뉴의 Play Mode 자동 검증:

- Verify Character Entry
- Verify Combat Result
- Verify Equipment Inventory
- Verify Search Combat
- Verify Camera Bounds
- Town Cleanup / Town Restructure (Report·Apply): Town 씬 구조 정리용 일회성 도구

알려진 기존 실패:

- `Verify Character Entry`의 "Selected character matches reference viewport framing" 검사는 2026-10-20 카메라 연출 변경(캐릭터 위치 30%/70%) 이후 기대값 19%가 갱신되지 않아 실패한다. 이 검사에서 멈추므로 그 뒤의 전투 쪽 검사는 실행되지 않는다. 사용자 결정으로 현재는 그대로 둔다(2026-11-06).

---

## 3. 미구현 · 부분 구현

| 영역 | 현재 상태 |
|---|---|
| 공통 ActionController | 없음. 캐릭터별 Mechanism이 액션을 직접 관리 |
| 액션 단계 · 캔슬 | `Startup`~`Finished`, `CancelPower` / `InterruptArmor` 없음 |
| 스킬 입력 구조 | 스킬마다 `PlayerInputReader` 소비 메서드와 인터페이스를 개별 추가하는 구조 |
| 피해식 | 기본 피해만. 크리티컬 · 속성 · 방어/저항 배율, 마법 기반 없음 |
| 범용 풀 | 없음. `GoblinPool`만 존재 |
| 히트스톱 · 무력화 | 무력화 수치는 `HitData`에 전달되나 무력화 상태 처리는 없음 |
| 적 AI | 고블린 1종의 기초 추적 / 공격 |
| 스테이지 진행 | 적 배치 · 웨이브 · 클리어 조건 없음. 적은 개발용 Spawn 버튼으로 소환 |
| 보상 지급 | `MapData`에 보상 값은 있으나 지급 경로 없음 |
| 인벤토리 저장 | `InventorySession.Capture` 값 기록은 있으나 디스크 저장 / 복원 없음 |
| 장비 강화 확률 · 옵션 리롤 · 분해 · 천장 | 없음 (`04` 문서 수치 미확정) |
| 메이헴 2~3단계 스킬, 혈류 조작 패시브, 출혈 / 과다출혈 | 없음 |

---

## 4. 사용자 결정 기록 및 남은 결정

### 2026-10-31 확정

1. 블러드 소드는 3타 (`03` 5절)
2. 블러드 슬래시는 메이헴의 정식 스킬 (`03` 5-1절)
3. 최종 피해 소수점은 버림 (`02` 13-1절)
4. 트리플 슬래시 마나 비용 "최대 마나의 10%"는 Ern 테스트 스킬 전용 임시값으로 유지한다. 정식 경제·스킬 수치로 승격하지 않는다.

### 코드 반영이 필요한 항목

- ~~`AttackHitbox`의 `Mathf.RoundToInt`를 확정 규칙(버림)에 맞게 수정~~ → 완료 (PR #3, `Mathf.FloorToInt`)
- `MayhemStats.asset`의 체력·마나·공격력·마력·방어력·저항력·상태이상 저항력 값이 기준 시트(`02` 12절)와 다르다.
  - 체력 성장 110.78 → 73.8, 마나 81 / 12.45 → 121 / 20.2, 공격력 9 / 8.43 → 8.2 / 9, 마력 2 / 1.86 → 3.1 / 3.3,
    방어력 3 / 0.43 → 5.2 / 0.6, 저항력 2.8 / 0.47 → 4.3 / 0.7, 상태이상 저항력 4 / 0.37 → 6 / 0.4
  - 나머지 스탯과 속성 저항은 시트와 일치한다.
  - PR #4에서 반영 대기 중.

### [CHECK] 남은 결정

- 블러드 소드 3타의 동작 설명
- 블러드 슬래시의 스킬 단계, 선행 조건, 동작 설명, 강화 효과, 수치
- 중간 피해 계산 단계의 버림 적용 여부

---

## 5. 갱신 규칙

- 시스템을 새로 구현하거나 상태가 바뀌면 이 문서의 2~3절을 함께 갱신한다.
- 기준 커밋과 날짜를 상단에 갱신한다.
- 4절의 남은 결정이 확정되면 해당 시스템 문서를 고치고 확정 기록으로 옮긴다.
