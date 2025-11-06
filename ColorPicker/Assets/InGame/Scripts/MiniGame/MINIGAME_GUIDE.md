# 🎮 MiniGame System Guide
어몽어스 스타일 미니게임 시스템 (14종)

---

## 📋 목차
1. [개요](#개요)
2. [구현된 미니게임 목록](#구현된-미니게임-목록)
3. [미니게임 구조](#미니게임-구조)
4. [폴더 구조](#폴더-구조)
5. [미니게임 상세 설명](#미니게임-상세-설명)
6. [미션 초기화 시스템](#미션-초기화-시스템)
7. [Unity 설정 가이드](#unity-설정-가이드)
8. [확장 가이드](#확장-가이드)

---

## 🎯 개요

이 시스템은 **Among Us** 스타일의 간단하고 직관적인 미니게임 14종을 제공합니다.

### 주요 특징
- ✅ **간단하고 직관적**: 30초~1분 내 완료 가능
- ✅ **Drag & Click 기반**: DraggableObject, ClickableObject 활용
- ✅ **미션 초기화**: 라운드 시작 시 자동 리셋
- ✅ **MiniGameBase 상속**: 일관된 구조
- ✅ **보고 시스템**: MiniGameReport로 완료 보고

### 핵심 컴포넌트
```csharp
MiniGameBase          // 모든 미니게임의 베이스 클래스
DraggableObject       // 드래그 가능한 UI 오브젝트
ClickableObject       // 클릭 가능한 UI 오브젝트
DragTriggerSensor     // 드래그 트리거 감지
MiniGameTag           // 미니게임 타입 태그
```

---

## 📚 구현된 미니게임 목록

### Drag 기반 미니게임 (9종)
| 번호 | 이름 | MiniGameType | 설명 | 상태 |
|------|------|--------------|------|------|
| 1 | Clean | `Clean` | 더러운 부분을 드래그하여 청소 | ✅ 완료 + 초기화 |
| 2 | ToySort | `ToySort` | 장난감을 상자에 정리 | ✅ 완료 |
| 3 | CardSwipe | `CardSwipe` | 카드를 오른쪽으로 긁기 | ✅ 완료 |
| 4 | WireConnect | `WireConnect` | 전선을 같은 색 단자에 연결 | ✅ 완료 |
| 5 | FuelEngine | `FuelEngine` | 연료 탱크를 엔진에 주입 | 📝 템플릿 |
| 6 | AlignEngine | `AlignEngine` | 엔진 슬라이더를 정렬 | 📝 템플릿 |
| 7 | UnlockManifolds | `UnlockManifolds` | 숫자 다이얼 맞추기 | 📝 템플릿 |
| 8 | EmptyGarbage | `EmptyGarbage` | 쓰레기를 우주로 버리기 | 📝 템플릿 |
| 9 | SortSamples | `SortSamples` | 샘플을 색상별로 분류 | 📝 템플릿 |

### Click 기반 미니게임 (3종)
| 번호 | 이름 | MiniGameType | 설명 | 상태 |
|------|------|--------------|------|------|
| 10 | ButtonSequence | `ButtonSequence` | 순서대로 버튼 누르기 | ✅ 완료 |
| 11 | NumberPad | `NumberPad` | 숫자 패드로 비밀번호 입력 | 📝 템플릿 |
| 12 | ShootAsteroids | `ShootAsteroids` | 운석을 클릭하여 파괴 (레트로) | ✅ 완료 |
| 13 | FixWiring | `FixWiring` | 스위치를 올바른 순서로 켜기 | 📝 템플릿 |

### Click + Time 기반 미니게임 (2종)
| 번호 | 이름 | MiniGameType | 설명 | 상태 |
|------|------|--------------|------|------|
| 14 | Download | `Download` | 다운로드 완료될 때까지 대기 | ✅ 완료 |
| 15 | ScanBody | `ScanBody` | 몸 스캔 완료될 때까지 대기 | 📝 템플릿 |

---

## 🏗️ 미니게임 구조

### MiniGameBase (베이스 클래스)
```csharp
public abstract class MiniGameBase : MonoBehaviour
{
    protected Action<MiniGameReport> onComplete;

    public void SetCallback(Action<MiniGameReport> callback);
    public abstract void Initialize();
    public abstract void StartGame();

    // [권장] 미션 초기화 메서드
    public void ResetMission();
}
```

### MiniGameReport (보고 데이터)
```csharp
public class MiniGameReport
{
    public int playerId;           // 플레이어 ID
    public MiniGameType miniGameType;  // 미니게임 타입
    public bool success;           // 성공 여부
}
```

### 미니게임 생명주기
```
1. Awake() → Initialize()
2. StartGame() ← MissionManager 호출
3. 플레이어 상호작용
4. onComplete?.Invoke(report) ← 완료 보고
5. ResetMission() ← 라운드 시작 시 초기화
```

---

## 📁 폴더 구조

```
Assets/InGame/Scripts/MiniGame/
├── MiniGameBase.cs              # 베이스 클래스
├── MiniGameType.cs              # 미니게임 타입 enum
├── MiniGameTag.cs               # 타입 태그 컴포넌트
├── DraggableObject.cs           # 드래그 오브젝트
├── ClickableObject.cs           # 클릭 오브젝트
├── DragTriggerSensor.cs         # 드래그 트리거
├── IMiniGameInteractable.cs     # 인터페이스
│
├── Clean/                       # ✅ 청소하기
│   ├── CleanMiniGame.cs
│   └── CleanableObject.cs
│
├── ToySort/                     # ✅ 장난감 정리
│   ├── ToySortMiniGame.cs
│   └── ToyObject.cs
│
├── CardSwipe/                   # ✅ 카드 긁기
│   ├── CardSwipeMiniGame.cs
│   └── CardObject.cs
│
├── WireConnect/                 # ✅ 전선 연결
│   ├── WireConnectMiniGame.cs
│   └── WireObject.cs (+ Terminal)
│
├── ButtonSequence/              # ✅ 버튼 순서
│   ├── ButtonSequenceMiniGame.cs
│   └── SequenceButton.cs
│
├── Download/                    # ✅ 다운로드
│   └── DownloadMiniGame.cs
│
├── ShootAsteroids/              # ✅ 운석 슈팅
│   ├── ShootAsteroidsMiniGame.cs
│   └── AsteroidObject.cs
│
└── [나머지 미니게임 폴더...]
```

---

## 🎯 미니게임 상세 설명

### 1. Clean (청소하기) ✅
**타입**: Drag
**목표**: 더러운 부분을 드래그하여 투명하게 만들기

**구성요소**:
- `CleanMiniGame.cs`: 메인 로직
- `CleanableObject.cs`: 더러운 오브젝트 (3단계 알파 감소)

**동작**:
1. Cleaner Tool을 더러운 부분에 드래그
2. 알파값이 3단계로 감소 (1.0 → 0.66 → 0.33 → 0.0)
3. 모든 더러운 부분이 사라지면 완료

**초기화**:
```csharp
public void ResetMission()
{
    cleanedCount = 0;
    foreach (CleanableObject cleanable in cleanables)
    {
        cleanable.Initialized(); // 알파값 복원
    }
}
```

---

### 2. ToySort (장난감 정리) ✅
**타입**: Drag
**목표**: 흩어진 장난감을 상자에 정리

**구성요소**:
- `ToySortMiniGame.cs`: 메인 로직
- `ToyObject.cs`: 장난감 오브젝트

**동작**:
1. 장난감이 랜덤 위치에 스폰
2. 장난감을 "ToyBox" 태그의 상자로 드래그
3. DragTriggerSensor로 충돌 감지
4. 모든 장난감이 정리되면 완료

**Unity 설정**:
- ToyBox에 `Tag: "ToyBox"` 설정
- Collider2D 필수 (IsTrigger = true)

---

### 3. CardSwipe (카드 긁기) ✅
**타입**: Drag
**목표**: 카드를 오른쪽으로 드래그하여 인식

**구성요소**:
- `CardSwipeMiniGame.cs`: 메인 로직
- `CardObject.cs`: 카드 오브젝트

**동작**:
1. 카드를 시작 위치에서 오른쪽으로 드래그
2. 지정된 거리(기본 200px) 이상 이동하면 완료
3. Update()에서 거리 체크

**커스터마이징**:
```csharp
card.SetSwipeDistance(300f); // 거리 조절
```

---

### 4. WireConnect (전선 연결) ✅
**타입**: Drag
**목표**: 전선을 같은 색상의 단자에 연결

**구성요소**:
- `WireConnectMiniGame.cs`: 메인 로직
- `WireObject.cs`: 전선 (왼쪽)
- `Terminal.cs`: 단자 (오른쪽)

**동작**:
1. 왼쪽 전선을 드래그
2. 오른쪽 단자에 가져다 대면 DragTriggerSensor 감지
3. 색상이 일치하면 연결 (ColorMatch 검사)
4. 모든 전선이 연결되면 완료

**Unity 설정**:
- Wire: DraggableObject + DragTriggerSensor + Collider2D
- Terminal: `Tag: "Terminal"` + Terminal 컴포넌트 + Collider2D (IsTrigger)
- 색상 설정: Wire와 Terminal의 Color 일치

---

### 5. ButtonSequence (버튼 순서) ✅
**타입**: Click
**목표**: 표시된 순서대로 버튼 누르기

**구성요소**:
- `ButtonSequenceMiniGame.cs`: 메인 로직
- `SequenceButton.cs`: 버튼

**동작**:
1. 랜덤 순서 생성 (예: 2, 0, 1, 3, 2)
2. 버튼들이 순서대로 하이라이트 (노란색)
3. 플레이어가 같은 순서로 클릭
4. 틀리면 재시작, 맞으면 진행
5. 모든 순서를 맞추면 완료

**피드백**:
- 정답: 초록색 깜빡임
- 오답: 빨간색 깜빡임

---

### 6. Download (다운로드) ✅
**타입**: Click + Time
**목표**: 다운로드 버튼을 누르고 완료될 때까지 대기

**구성요소**:
- `DownloadMiniGame.cs`: 메인 로직
- Slider (프로그레스 바)
- Button (시작 버튼)

**동작**:
1. "다운로드" 버튼 클릭
2. 프로그레스 바가 0% → 100%로 채워짐
3. 완료되면 자동으로 미션 완료

**커스터마이징**:
```csharp
downloadDuration = 5f; // 다운로드 시간 (초)
```

---

### 7. ShootAsteroids (운석 슈팅) ✅
**타입**: Click
**목표**: 화면에 나타나는 운석을 클릭하여 파괴

**구성요소**:
- `ShootAsteroidsMiniGame.cs`: 메인 로직
- `AsteroidObject.cs`: 운석

**동작**:
1. 운석이 위에서 아래로 떨어짐 (0.5초 간격)
2. 플레이어가 클릭하면 파괴
3. 화면 밖으로 나가면 재스폰 (풀링)
4. 20개 파괴하면 완료

**레트로 스타일**:
- 간단한 클릭 게임
- 빠른 반응 요구

---

## 🔄 미션 초기화 시스템

### ResetMission() 메서드
모든 미니게임은 `ResetMission()` 메서드를 구현해야 합니다.
**호출 시점**: 라운드 시작 시 (RoundManager에서 호출)

### 초기화 구현 예시

```csharp
public class ExampleMiniGame : MiniGameBase
{
    private int progress = 0;
    private List<GameObject> objects = new();

    public void ResetMission()
    {
        // 1. 진행 상황 리셋
        progress = 0;

        // 2. 오브젝트 위치 리셋
        foreach (var obj in objects)
        {
            obj.transform.localPosition = Vector3.zero;
            obj.SetActive(true);
        }

        // 3. UI 리셋
        progressBar.value = 0f;

        // 4. 상태 플래그 리셋
        isCompleted = false;

        Debug.Log("[ExampleMiniGame] Mission reset");
    }
}
```

### RoundManager 통합
```csharp
// RoundManager.cs에서 호출
private void InitializeMissions()
{
    var allMinigames = FindObjectsOfType<MiniGameBase>();
    foreach (var minigame in allMinigames)
    {
        // ResetMission() 호출
        minigame.ResetMission();
    }
}
```

---

## 🛠️ Unity 설정 가이드

### 기본 세팅

#### 1. Canvas 설정
```
Canvas (Screen Space - Overlay)
└── MiniGamePanel
    ├── BackgroundImage
    ├── [미니게임 UI 요소들]
    └── CloseButton
```

#### 2. DraggableObject 설정
1. UI 오브젝트에 **DraggableObject** 컴포넌트 추가
2. Canvas 참조 연결 (Inspector)
3. RectTransform 자동 연결됨

#### 3. ClickableObject 설정
1. UI 오브젝트에 **ClickableObject** 컴포넌트 추가
2. Button 컴포넌트와 함께 사용 가능

#### 4. DragTriggerSensor 설정
1. Collider2D 추가 (IsTrigger = true)
2. **DragTriggerSensor** 컴포넌트 추가
3. Rigidbody2D 추가 (IsKinematic = true)

#### 5. MiniGameTag 설정
1. 미니게임 루트 GameObject에 **MiniGameTag** 추가
2. MiniGameType 설정 (예: Clean, ToySort)

---

### 예시: ToySort 설정

```
Canvas
└── ToySortPanel
    ├── ToyBox (Tag: "ToyBox", Collider2D + IsTrigger)
    ├── ToysParent
    │   ├── Toy1 (DraggableObject + DragTriggerSensor + ToyObject)
    │   ├── Toy2 (DraggableObject + DragTriggerSensor + ToyObject)
    │   └── Toy3 (DraggableObject + DragTriggerSensor + ToyObject)
    └── ToySortMiniGame (Script + MiniGameTag)
```

**Inspector 설정**:
- ToySortMiniGame:
  - `Toys Parent`: ToysParent Transform
  - `Toy Box Transform`: ToyBox Transform

---

## 📝 나머지 미니게임 템플릿

나머지 미니게임들은 유사한 구조로 구현할 수 있습니다:

### FuelEngine (연료 주입)
```csharp
// FuelCanister를 드래그하여 Engine에 주입
// 연료 게이지가 100%가 되면 완료
```

### AlignEngine (엔진 정렬)
```csharp
// 슬라이더를 드래그하여 녹색 영역에 맞추기
// 모든 슬라이더를 정렬하면 완료
```

### UnlockManifolds (자물쇠)
```csharp
// 숫자 다이얼을 드래그하여 올바른 숫자에 맞추기
// 3개 다이얼을 모두 맞추면 완료
```

### EmptyGarbage (쓰레기)
```csharp
// 쓰레기 레버를 아래로 드래그
// 쓰레기가 우주로 배출되면 완료
```

### SortSamples (샘플 분류)
```csharp
// 샘플을 색상별로 정렬
// 빨강/파랑/초록 샘플을 각 트레이에 배치
```

### NumberPad (숫자 입력)
```csharp
// 버튼을 클릭하여 비밀번호 입력
// 올바른 번호 입력 시 완료
```

### FixWiring (배선 수리)
```csharp
// 스위치를 올바른 순서로 켜기
// 모든 스위치가 켜지면 완료
```

### ScanBody (몸 스캔)
```csharp
// 스캔 버튼 클릭 후 10초 대기
// 타이머가 완료되면 완료
```

---

## 🚀 확장 가이드

### 새 미니게임 추가 방법

#### 1. MiniGameType에 추가
```csharp
public enum MiniGameType
{
    // 기존...
    NewMiniGame = 16,
}
```

#### 2. 폴더 및 클래스 생성
```
MiniGame/
└── NewMiniGame/
    ├── NewMiniGame.cs (MiniGameBase 상속)
    └── NewMiniGameObject.cs
```

#### 3. MiniGameBase 구현
```csharp
[RequireComponent(typeof(MiniGameTag))]
public class NewMiniGame : MiniGameBase
{
    private MiniGameTag miniGameTag;
    private int playerId;

    private void Awake()
    {
        Initialize();
    }

    public override void Initialize()
    {
        miniGameTag = GetComponent<MiniGameTag>();
        playerId = Photon.Pun.PhotonNetwork.LocalPlayer.ActorNumber;

        // 초기화 로직
    }

    public override void StartGame()
    {
        // 게임 시작 로직
    }

    public void ResetMission()
    {
        // 미션 초기화 로직
    }

    private void CompleteGame()
    {
        MiniGameReport repo = new MiniGameReport()
        {
            playerId = playerId,
            miniGameType = miniGameTag.miniGameType,
            success = true
        };

        onComplete?.Invoke(repo);
    }
}
```

#### 4. Unity에서 설정
1. GameObject에 NewMiniGame 스크립트 추가
2. MiniGameTag 추가 및 타입 설정
3. 필요한 UI/오브젝트 연결

---

## ✅ 체크리스트

### 미니게임 구현 시
- [ ] MiniGameBase 상속
- [ ] MiniGameTag 컴포넌트 추가
- [ ] Initialize() 구현
- [ ] StartGame() 구현
- [ ] ResetMission() 구현 ⭐
- [ ] onComplete 호출
- [ ] MiniGameType enum에 등록
- [ ] 폴더 구조 정리
- [ ] Unity 프리팹 생성

### Unity 설정 시
- [ ] Canvas 설정
- [ ] DraggableObject / ClickableObject 추가
- [ ] Collider2D 설정 (IsTrigger)
- [ ] Tag 설정 (필요 시)
- [ ] Inspector 참조 연결

---

## 🎉 완료!

7종의 미니게임이 완전히 구현되었고, 나머지 7종은 템플릿으로 제공됩니다.
모든 미니게임은 **ResetMission()** 메서드를 통해 라운드 시작 시 초기화됩니다.

**구현 완료**: Clean, ToySort, CardSwipe, WireConnect, ButtonSequence, Download, ShootAsteroids
**템플릿 제공**: FuelEngine, AlignEngine, UnlockManifolds, EmptyGarbage, SortSamples, NumberPad, FixWiring, ScanBody

필요에 따라 템플릿을 참고하여 나머지 미니게임을 쉽게 구현할 수 있습니다!

---

**문의사항이나 버그가 있다면 이슈를 남겨주세요!** 🚀
