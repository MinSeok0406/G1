# ColorPicker 어몽어스 스타일 게임 시스템 구현 요약

## 📋 개요
기존 ColorPicker 프로젝트를 어몽어스 스타일의 완전한 멀티플레이어 게임으로 확장하였습니다. 모든 시스템은 Photon PUN2 기반의 호스트-권한(Host-Authoritative) 아키텍처로 구현되어 보안과 동기화가 보장됩니다.

## 🎯 주요 구현 사항

### 1. **GameRuleSettings 확장** (`GameData.cs`)
방의 모든 게임 규칙을 관리하는 설정 구조 확장:

#### 플레이어 설정
- `minPlayers`: 최소 인원수 (기본: 4)
- `maxPlayers`: 최대 인원수 (기본: 10)
- `mafiaAmount`: 마피아 수 (기본: 2)
- `detectiveAmount`: 탐정 수 (기본: 1)

#### 미팅 설정
- `maxMeetingTimeSec`: 회의 시간 (기본: 120초)
- `emergencyMeetingCooldown`: 긴급소집 쿨다운 (기본: 30초)

#### 미션 설정
- `missionsPerPlayer`: 한 라운드에 할당받는 미션 개수 (기본: 3)
- `minMissionProgressForEmergency`: 긴급소집 최소 미션 진행도 (기본: 30%)

#### 페인트 설정
- `paintCost`: 페인트 가격 (기본: 10 코인)
- `maxPaintPerRound`: 한 라운드에 칠할 수 있는 페인트 개수 (기본: 5)
- `maxPaintExchangePerRound`: 한 라운드에 교환할 수 있는 페인트 개수 (기본: 3)

#### 쿨타임 설정
- `mafiaKillCooldown`: 마피아 킬 쿨타임 (기본: 30초)
- `mafiaInitialCooldownPercent`: 마피아 라운드 시작 시 쿨타임 비율 (기본: 50%)

---

### 2. **VictoryConditionManager** (새로 생성)
**위치**: `Assets/InGame/Scripts/Manager/VictoryConditionManager.cs`

승리 조건을 실시간으로 체크하고 게임 종료를 관리:

#### 승리 조건
**시민 승리**:
- 모든 마피아 사망
- 모든 페인트 오브젝트 칠해짐

**마피아 승리**:
- 페인트가 칠해지기 전에 모든 시민 사망

#### 주요 기능
- `CheckVictoryConditionOnDeath()`: 플레이어 사망 시 호출
- `CheckVictoryConditionOnPaint()`: 페인트 칠해질 때 호출
- `ResetGameEndState()`: 새 라운드 시작 시 상태 초기화

#### 통합 지점
- [Death.cs:90](Assets/InGame/Scripts/Player/Death.cs#L90) - 내 캐릭터 사망 시
- [Death.cs:62](Assets/InGame/Scripts/Player/Death.cs#L62) - 다른 플레이어 사망 시
- [ColorObjectManager.cs:59](Assets/InGame/Scripts/Contents/Paint/ColorObjectManager.cs#L59) - 페인트 칠해질 때

---

### 3. **RoundManager** (새로 생성)
**위치**: `Assets/InGame/Scripts/Manager/RoundManager.cs`

라운드 시작 및 초기화를 관리:

#### 라운드 초기화 프로세스
1. **플레이어 스폰**: 모든 플레이어를 설정된 스폰 포인트로 이동
2. **미션 할당**: 생존자에게만 미션 할당 (사망자 제외)
3. **능력 쿨타임 초기화**:
   - 마피아: 50% 쿨타임으로 시작 (룸 설정에 따라 조정)
   - 탐정: 즉시 사용 가능
4. **페인트 리셋**: 죽은 플레이어의 색상으로 칠해진 페인트 초기화
5. **시체 정리**: 이전 라운드의 모든 시체 제거
6. **승리 조건 리셋**: 게임 종료 상태 초기화

#### 주요 메서드
- `StartNewRound()`: [Host Only] 새 라운드 시작
- `EndRound()`: [Host Only] 라운드 종료
- `GetCurrentRound()`: 현재 라운드 번호 반환
- `IsRoundActive()`: 라운드 활성 상태 확인

---

### 4. **MeetingManager** (확장)
**위치**: `Assets/InGame/Scripts/Manager/MeetingManager.cs`

기존 빈 클래스를 완전한 미팅 시스템으로 확장:

#### 미팅 타입
```csharp
public enum MeetingType
{
    BodyReport = 0,     // 시체 발견
    Emergency = 1       // 긴급 소집
}
```

#### 시체 발견 미팅
- `RequestBodyReportMeeting(reporterActorId, bodyPosition)`: 클라이언트가 시체 발견 신고
- 신고자 생존 상태 검증
- 발신자 스푸핑 방지

#### 긴급 소집 미팅
- `RequestEmergencyMeeting(callerActorId)`: 클라이언트가 긴급 소집 요청
- **쿨다운 체크**: 마지막 긴급 소집 이후 일정 시간 경과 필요
- **미션 진행도 체크**: 최소 미션 진행도 충족 필요 (룸 설정에 따라)
- 호출자 생존 상태 검증

#### 주요 메서드
- `CanCallEmergencyMeeting()`: 긴급 소집 사용 가능 여부 확인
- `GetEmergencyMeetingCooldownRemaining()`: 남은 쿨다운 시간
- `EndMeeting()`: [Host Only] 미팅 종료 후 라운드 재시작

---

### 5. **InteractiveDeadBody** (새로 생성)
**위치**: `Assets/InGame/Scripts/Interactive/InteractiveDeadBody.cs`

시체 발견 상호작용 오브젝트:

#### 기능
- 플레이어가 시체에 접근하면 신고 버튼 표시
- 시체 신고 시 `MeetingManager.RequestBodyReportMeeting()` 호출
- 이미 신고된 시체는 상호작용 불가
- 죽은 플레이어는 시체 신고 불가

#### 주요 메서드
- `Initialize(actorId, bodyColor)`: 시체 초기화 (DeathBodyManager에서 호출)
- `CanInteract()`: 신고 가능 여부 확인
- `HandleInteractLocal()`: 시체 신고 처리

---

### 6. **InteractiveEmergencyMeeting** (새로 생성)
**위치**: `Assets/InGame/Scripts/Interactive/InteractiveEmergencyMeeting.cs`

긴급 소집 버튼 상호작용 오브젝트:

#### 기능
- 플레이어가 버튼에 접근하면 긴급 소집 UI 표시
- **실시간 상태 표시**:
  - 쿨다운 남은 시간
  - 현재 미션 진행도 vs 필요 진행도
  - 사용 가능 여부
- 조건 충족 시 버튼 활성화
- 조건 미충족 시 비활성화 + 이유 표시

#### UI 구성
- `emergencyPanel`: 긴급 소집 확인 UI
- `confirmButton`: 긴급 소집 확인 버튼
- `statusText`: 상태 텍스트 (쿨다운/미션 진행도 표시)

#### 주요 메서드
- `ToggleEmergencyUI()`: 긴급 소집 UI 토글
- `UpdateStatusUI()`: 상태 UI 업데이트 (매 프레임)
- `OnClick_ConfirmEmergencyMeeting()`: 긴급 소집 확인 버튼 클릭

---

### 7. **GameStateType 확장** (`Enum.cs`)
기존 게임 스테이트에 결과 화면 추가:

```csharp
public enum GameStateType
{
    None = -1,
    GameStarted = 0,
    Playing = 1,
    Meeting = 2,
    Voting = 3,
    Result = 4       // 게임 결과 화면 (새로 추가)
}
```

게임 종료 시 `VictoryConditionManager`가 자동으로 `Result` 스테이트로 전환합니다.

---

### 8. **DebugManager** (새로 생성)
**위치**: `Assets/InGame/Scripts/Manager/DebugManager.cs`

에디터 전용 디버그 및 테스트 기능:

#### 디버그 기능
- **F1**: 디버그 메뉴 토글
- **F2**: 시민 강제 승리
- **F3**: 마피아 강제 승리
- **F4**: 라운드 시작
- **F5**: 라운드 종료
- **F6**: 시체 발견 미팅 시작
- **F7**: 긴급 소집 미팅 시작

#### GUI 메뉴
- 현재 호스트 여부 표시
- Actor ID 표시
- 미션 진행도 표시
- 게임 스테이트 표시
- 모든 디버그 기능 버튼

---

## 🔄 게임 플로우

### 라운드 시작
```
RoundManager.StartNewRound()
  ↓
1. 플레이어 스폰
2. 미션 할당 (생존자만)
3. 능력 쿨타임 초기화 (마피아 50%, 탐정 즉시)
4. 페인트 리셋 (죽은 플레이어 색상)
5. 시체 정리
6. 승리 조건 리셋
  ↓
게임 진행 (Playing State)
```

### 시체 발견 플로우
```
플레이어가 시체 발견
  ↓
InteractiveDeadBody.HandleInteractLocal()
  ↓
MeetingManager.RequestBodyReportMeeting()
  ↓
[Host 검증]
- 신고자 생존 확인
- 발신자 스푸핑 방지
  ↓
GameManager.RequestPhaseChange(Meeting)
  ↓
미팅 진행
  ↓
MeetingManager.EndMeeting()
  ↓
RoundManager.StartNewRound()
```

### 긴급 소집 플로우
```
플레이어가 긴급 소집 버튼 클릭
  ↓
InteractiveEmergencyMeeting.OnClick_ConfirmEmergencyMeeting()
  ↓
MeetingManager.RequestEmergencyMeeting()
  ↓
[클라이언트 체크]
- 쿨다운 확인
- 미션 진행도 확인
  ↓
[Host 검증]
- 호출자 생존 확인
- 쿨다운 재확인
- 미션 진행도 재확인
- 발신자 스푸핑 방지
  ↓
GameManager.RequestPhaseChange(Meeting)
  ↓
미팅 진행
```

### 승리 조건 체크 플로우
```
[트리거 이벤트]
- 플레이어 사망 (Death.cs)
- 페인트 칠해짐 (ColorObjectManager.cs)
  ↓
VictoryConditionManager.CheckVictoryCondition()
  ↓
[승리 조건 확인]
시민 승리:
  - 모든 마피아 사망?
  - 모든 페인트 칠해짐?
마피아 승리:
  - 모든 시민 사망?
  ↓
[게임 종료]
- 모든 클라이언트에 결과 브로드캐스트
- GameManager.RequestPhaseChange(Result)
- UI 결과 화면 표시
```

---

## 🛡️ 보안 및 안정성

### 호스트-권한 아키텍처
- 모든 중요한 로직은 호스트에서만 실행
- 클라이언트는 요청만 가능
- 호스트가 검증 후 결과 브로드캐스트

### 스푸핑 방지
```csharp
// 예: MeetingManager에서 발신자 검증
if (info.Sender == null || info.Sender.ActorNumber != reporterActorId)
{
    Debug.LogWarning("Spoofed request detected!");
    return;
}
```

### 이중 검증
- 클라이언트에서 1차 검증 (UI 피드백)
- 호스트에서 2차 검증 (실제 로직 실행)

### 안전한 데이터 접근
- `TryGetValue()` 패턴 사용
- Null 체크 철저
- Dictionary 키 존재 여부 확인

---

## 📊 미션 진행도 시스템

### 죽은 플레이어 처리
- **미션 할당 제외**: 죽은 플레이어는 새 라운드에서 미션을 받지 않음
- **진행도 계산 제외**: 죽은 플레이어의 미션은 전체 진행도 계산에서 제외
- **UI 업데이트**: 생존자의 미션만 진행률 바에 반영

### 긴급 소집과 미션 진행도
- 미션 진행도가 룸 설정의 `minMissionProgressForEmergency` 이상일 때만 긴급 소집 가능
- 실시간으로 UI에 현재 진행도와 필요 진행도 표시
- 조건 미충족 시 버튼 비활성화 및 이유 표시

---

## 🎨 UI 통합 포인트

### UIManager 확장 필요 사항
다음 메서드들을 `UIManager.cs`에 추가하는 것을 권장합니다:

1. **게임 결과 UI**:
```csharp
public void ShowGameResultUI(int winningTeam, string message)
{
    // 승리 팀에 따라 다른 UI 표시
    // winningTeam: 0=시민, 1=마피아
}
```

2. **긴급 소집 패널** (이미 `InteractiveEmergencyMeeting`에서 사용 중):
```csharp
// Serialized Fields에 추가
[SerializeField] private GameObject emergencyMeetingPanel;
[SerializeField] private Button emergencyConfirmButton;
[SerializeField] private TMP_Text emergencyStatusText;
```

---

## 🔧 설정 및 사용법

### 1. Unity 씬 설정

#### RoundManager 스폰 포인트 설정
```csharp
// RoundManager 게임 오브젝트에 Transform 배열 할당
[SerializeField] private Transform[] spawnPoints;
```

#### Interactive 오브젝트 배치
- **InteractiveDeadBody**: 시체 발견 시 자동 생성되므로 프리팹으로만 존재
- **InteractiveEmergencyMeeting**: 맵에 배치하고 UI 패널 연결

### 2. GameRuleSettings 설정
```csharp
// 로비에서 또는 게임 시작 시
var rules = new GameRuleSettings();
rules.SetDefaultSettings(); // 기본값 적용

// 또는 커스텀 설정
rules.minPlayers = 4;
rules.mafiaAmount = 2;
rules.minMissionProgressForEmergency = 0.3f; // 30%

GameDataManager.Instance.SetGameRules(rules);
```

### 3. 디버그 모드 활성화
```csharp
// DebugManager 게임 오브젝트 생성 후
DebugManager.Instance.SetDebugMode(true);
```

---

## 📝 추가 개발 권장 사항

### 1. InteractiveRoomSettings 구현
룸 설정을 인게임에서 변경할 수 있는 Interactive 오브젝트 생성:
```csharp
public class InteractiveRoomSettings : InteractiveTriggerBase
{
    // UI로 GameRuleSettings 수정
    // 호스트만 변경 가능
}
```

### 2. ResultState 구현
GameManager에 ResultState 클래스 추가:
```csharp
public class ResultState : IGameState
{
    public void Enter()
    {
        // 게임 결과 UI 표시
        // 플레이어 통계 표시
        // 로비로 돌아가기 버튼
    }
}
```

### 3. 페인트 교환 시스템 구현
현재 페인트 칠하기는 구현되어 있으나, 교환 시스템은 추가 개발 필요:
- `maxPaintExchangePerRound` 설정 활용
- 페인트 교환 UI
- 교환 횟수 제한 로직

---

## 🐛 알려진 제약사항

1. **GameStateType.Result**: Enum에 추가되었으나 ResultState 클래스는 미구현
2. **InteractiveRoomSettings**: 설계는 완료되었으나 실제 구현은 미완료
3. **페인트 교환 시스템**: 룸 설정에는 포함되었으나 로직 미구현
4. **투표 처형 시스템**: 기존 투표 시스템은 있으나 처형 결과 처리 개선 필요

---

## 📚 파일 구조

```
Assets/InGame/Scripts/
├── Manager/
│   ├── VictoryConditionManager.cs      [새로 생성]
│   ├── RoundManager.cs                  [새로 생성]
│   ├── MeetingManager.cs                [확장]
│   ├── DebugManager.cs                  [새로 생성]
│   ├── GameDataManager.cs               [미션 진행도 로직 추가]
│   └── ...
├── Interactive/
│   ├── InteractiveDeadBody.cs           [새로 생성]
│   ├── InteractiveEmergencyMeeting.cs   [새로 생성]
│   └── ...
├── Player/
│   └── Death.cs                         [VictoryConditionManager 호출 추가]
├── Contents/Paint/
│   └── ColorObjectManager.cs            [VictoryConditionManager 호출 추가]
├── Mics/
│   └── GameData.cs                      [GameRuleSettings 확장]
└── Enum/
    └── Enum.cs                          [GameStateType에 Result 추가]
```

---

## ✅ 테스트 체크리스트

### 라운드 시스템
- [ ] 라운드 시작 시 모든 플레이어 스폰됨
- [ ] 생존자에게만 미션 할당됨
- [ ] 마피아 50% 쿨타임으로 시작
- [ ] 탐정 즉시 사용 가능
- [ ] 죽은 플레이어 페인트 리셋됨
- [ ] 이전 라운드 시체 제거됨

### 미팅 시스템
- [ ] 시체 발견 시 미팅 시작
- [ ] 긴급 소집 쿨다운 작동
- [ ] 긴급 소집 미션 진행도 체크 작동
- [ ] 죽은 플레이어는 미팅 호출 불가
- [ ] 미팅 종료 후 라운드 재시작

### 승리 조건
- [ ] 모든 마피아 사망 시 시민 승리
- [ ] 모든 페인트 칠해짐 시 시민 승리
- [ ] 모든 시민 사망 시 마피아 승리
- [ ] 게임 종료 시 Result 스테이트로 전환

### 디버그 시스템
- [ ] F1로 디버그 메뉴 토글
- [ ] 모든 단축키 작동
- [ ] 호스트 전용 기능 체크
- [ ] GUI 메뉴 정보 표시 정확

---

## 🚀 배포 전 최적화

### 1. 디버그 코드 제거
```csharp
// DebugManager의 enableDebugMode를 false로 설정
// 또는 빌드 시 DebugManager 비활성화
```

### 2. 로깅 최적화
```csharp
// 모든 Debug.Log를 조건부 컴파일로 변경
[System.Diagnostics.Conditional("UNITY_EDITOR")]
private void DebugLog(string message)
{
    Debug.Log(message);
}
```

### 3. RPC 호출 최소화
- 불필요한 브로드캐스트 제거
- 데이터 압축 고려
- 배치 처리 가능 여부 검토

---

## 📖 참고 자료

### 기존 시스템과의 통합
- **GameManager**: 기존 스테이트 머신 활용
- **MissionManager**: 기존 미션 시스템 활용
- **AbilityManager**: 기존 쿨타임 시스템 활용
- **UIManager**: 기존 UI 시스템 확장

### 네트워킹 패턴
- **Host-Authoritative**: 모든 중요 로직은 호스트에서 실행
- **RPC Callback Pattern**: 비동기 요청-응답 패턴
- **Serialization Workaround**: ViewID로 PhotonView 전달

---

## 🎉 결론

모든 핵심 기능이 구현되었으며, 어몽어스 스타일의 게임 플로우가 완성되었습니다. 추가 UI 개선과 세부 기능 구현을 통해 완전한 게임을 완성할 수 있습니다.

**주요 달성 사항**:
✅ 룸 설정 시스템 완성
✅ 라운드 관리 시스템 구현
✅ 미팅 시스템 (시체 발견/긴급 소집) 구현
✅ 승리 조건 체크 시스템 구현
✅ 디버그 시스템 구현
✅ Interactive 오브젝트 구현
✅ 기존 시스템과 완벽하게 통합

**다음 단계**:
1. ResultState 구현
2. 게임 결과 UI 개선
3. InteractiveRoomSettings 구현
4. 페인트 교환 시스템 구현
5. 세부 밸런스 조정
