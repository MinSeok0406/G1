# Asylum Minigame System - Complete Guide

## Overview

이 문서는 정신병원(Asylum) 테마의 10가지 미니게임 구현에 대한 완전한 가이드입니다.
모든 미니게임은 어두운 분위기와 긴장감을 제공하며, Among Us 스타일의 간단하면서도 몰입감 있는 게임플레이를 제공합니다.

---

## 목차

1. [공통 구조](#공통-구조)
2. [미니게임 목록](#미니게임-목록)
   - [1. PillSort (약물 분류)](#1-pillsort-약물-분류)
   - [2. HeartRateMonitor (심박수 모니터링)](#2-heartratemonitor-심박수-모니터링)
   - [3. SedativeInjection (진정제 주사)](#3-sedativeinjection-진정제-주사)
   - [4. MentalStateExam (정신 상태 검사)](#4-mentalstateexam-정신-상태-검사)
   - [5. SecurityCamera (감시 카메라)](#5-securitycamera-감시-카메라)
   - [6. PatientFileSort (환자 기록 정리)](#6-patientfilesort-환자-기록-정리)
   - [7. BrainWavePattern (뇌파 패턴 맞추기)](#7-brainwavepattern-뇌파-패턴-맞추기)
   - [8. StraitjacketTie (구속복 묶기)](#8-straitjackettie-구속복-묶기)
   - [9. DiagnosisForm (진단서 작성)](#9-diagnosisform-진단서-작성)
   - [10. MedicineSchedule (투약 시간 체크)](#10-medicineschedule-투약-시간-체크)
3. [Unity 설정 가이드](#unity-설정-가이드)
4. [커스터마이징](#커스터마이징)

---

## 공통 구조

모든 Asylum 미니게임은 다음 구조를 따릅니다:

### 필수 컴포넌트
- `MiniGameBase` 상속
- `MiniGameTag` 컴포넌트 필수 (RequireComponent)
- Photon PUN2 연동 (playerId 사용)

### 필수 메서드
```csharp
public override void Initialize()     // 초기화
public override void StartGame()      // 게임 시작
public void ResetMission()           // 라운드 초기화
```

### 완료 보고
```csharp
MiniGameReport repo = new MiniGameReport()
{
    playerId = playerId,
    miniGameType = miniGameTag.miniGameType,
    success = true
};
onComplete?.Invoke(repo);
```

---

## 미니게임 목록

## 1. PillSort (약물 분류)

**파일**: `Assets/InGame/Scripts/MiniGame/Asylum/PillSort/PillSortMiniGame.cs`

### 게임 설명
여러 색상/종류의 약물을 올바른 컨테이너에 드래그하여 분류하는 게임입니다.

### 클래스 구조
- **PillSortMiniGame**: 메인 게임 로직
- **PillObject**: 드래그 가능한 약물 오브젝트
- **PillContainer**: 약물을 받는 컨테이너

### 핵심 기능
```csharp
// 약물 타입 정의
public enum PillType
{
    Red,      // 빨강 약물
    Blue,     // 파랑 약물
    Yellow,   // 노랑 약물
    Green     // 초록 약물
}

// 약물 정렬 확인
private void OnPillSorted()
{
    sortedCount++;
    if (sortedCount >= pills.Count)
        CompleteGame();
}
```

### Unity 설정
1. Canvas 하위에 빈 GameObject 생성 → `PillSortMiniGame` 컴포넌트 추가
2. `PillsParent`: 약물들의 부모 Transform
3. 각 약물 오브젝트:
   - `PillObject` 컴포넌트
   - `DraggableObject` 컴포넌트
   - `DragTriggerSensor` 컴포넌트
   - Collider2D (IsTrigger = true)
4. 각 컨테이너:
   - `PillContainer` 컴포넌트
   - Tag: "PillContainer"
   - Collider2D

---

## 2. HeartRateMonitor (심박수 모니터링)

**파일**: `Assets/InGame/Scripts/MiniGame/Asylum/HeartRateMonitor/HeartRateMonitorMiniGame.cs`

### 게임 설명
여러 심박수 모니터 화면에서 비정상 심박수를 발견하고 클릭하는 게임입니다.

### 클래스 구조
- **HeartRateMonitorMiniGame**: 메인 게임 로직
- **HeartRateDisplay**: 개별 모니터 화면

### 핵심 기능
```csharp
// 비정상 심박수 생성
private void SpawnAbnormalHeartbeat()
{
    var normalDisplays = displays.FindAll(d => !d.IsAbnormal());
    if (normalDisplays.Count > 0)
    {
        int randomIndex = Random.Range(0, normalDisplays.Count);
        normalDisplays[randomIndex].ShowAbnormal();
    }
}

// 일정 간격으로 생성
[SerializeField] private float spawnInterval = 2f;
```

### Unity 설정
1. Canvas 하위에 빈 GameObject 생성 → `HeartRateMonitorMiniGame` 컴포넌트
2. Displays: 여러 개의 HeartRateDisplay 추가
3. 각 Display:
   - `HeartRateDisplay` 컴포넌트
   - `ClickableObject` 컴포넌트
   - `AbnormalIndicator`: 비정상 표시 GameObject
   - `NormalSprite`, `AbnormalSprite`: 심박수 그래프 스프라이트

---

## 3. SedativeInjection (진정제 주사)

**파일**: `Assets/InGame/Scripts/MiniGame/Asylum/SedativeInjection/SedativeInjectionMiniGame.cs`

### 게임 설명
진정제 주사기를 드래그하여 환자에게 주사하는 게임입니다.

### 클래스 구조
- **SedativeInjectionMiniGame**: 메인 게임 로직
- **SyringeObject**: 드래그 가능한 주사기
- **PatientTarget**: 환자 타겟

### 핵심 기능
```csharp
// 주사기-환자 충돌 감지
private void DragTriggerSensor_OnTriggerEntered(Collider2D collider)
{
    if (collider.CompareTag(patientTag))
    {
        InjectPatient(collider.transform.position);
    }
}

// 주사 완료
private void InjectPatient(Vector3 patientPosition)
{
    isInjected = true;
    rectTransform.position = patientPosition;
    OnSyringeInjected?.Invoke();
}
```

### Unity 설정
1. Canvas 하위에 빈 GameObject 생성 → `SedativeInjectionMiniGame` 컴포넌트
2. 주사기 오브젝트:
   - `SyringeObject` 컴포넌트
   - `DraggableObject` 컴포넌트
   - `DragTriggerSensor` 컴포넌트
   - Collider2D (IsTrigger = true)
3. 환자 오브젝트:
   - `PatientTarget` 컴포넌트
   - Tag: "Patient"
   - Collider2D

---

## 4. MentalStateExam (정신 상태 검사)

**파일**: `Assets/InGame/Scripts/MiniGame/Asylum/MentalStateExam/MentalStateExamMiniGame.cs`

### 게임 설명
정신 상태 진단 질문에 올바른 답변을 클릭하는 게임입니다.

### 클래스 구조
- **MentalStateExamMiniGame**: 메인 게임 로직
- **ExamQuestion**: 개별 질문

### 핵심 기능
```csharp
// 질문 데이터
[SerializeField] private string questionText;
[SerializeField] private List<string> answerOptions;
[SerializeField] private int correctAnswerIndex;

// 다음 질문으로 진행
private void ShowNextQuestion()
{
    currentQuestionIndex++;

    if (currentQuestionIndex >= questions.Count)
    {
        CompleteGame();
    }
    else
    {
        DisplayQuestion(questions[currentQuestionIndex]);
    }
}
```

### Unity 설정
1. Canvas 하위에 빈 GameObject 생성 → `MentalStateExamMiniGame` 컴포넌트
2. UI 요소:
   - `QuestionText`: TMP_Text (질문 텍스트)
   - `AnswerButtonsParent`: 답변 버튼들의 부모 Transform
3. 각 답변 버튼:
   - Button 컴포넌트
   - Text/TMP_Text 자식 오브젝트

---

## 5. SecurityCamera (감시 카메라)

**파일**: `Assets/InGame/Scripts/MiniGame/Asylum/SecurityCamera/SecurityCameraMiniGame.cs`

### 게임 설명
여러 카메라 화면에서 이상 행동을 발견하여 클릭하는 게임입니다.

### 클래스 구조
- **SecurityCameraMiniGame**: 메인 게임 로직
- **CameraFeed**: 개별 카메라 화면

### 핵심 기능
```csharp
// 카메라에 이상 현상 생성
private void SpawnAnomaly()
{
    var normalFeeds = cameraFeeds.FindAll(f => !f.HasAnomaly());
    if (normalFeeds.Count > 0)
    {
        int randomIndex = Random.Range(0, normalFeeds.Count);
        normalFeeds[randomIndex].ShowAnomaly();
    }
}

// 일정 간격으로 생성
[SerializeField] private float spawnInterval = 3f;
```

### Unity 설정
1. Canvas 하위에 빈 GameObject 생성 → `SecurityCameraMiniGame` 컴포넌트
2. CameraFeeds: 여러 개의 CameraFeed 추가
3. 각 카메라:
   - `CameraFeed` 컴포넌트
   - `ClickableObject` 컴포넌트
   - `AnomalyIndicator`: 이상 현상 표시 GameObject

---

## 6. PatientFileSort (환자 기록 정리)

**파일**: `Assets/InGame/Scripts/MiniGame/Asylum/PatientFileSort/PatientFileSortMiniGame.cs`

### 게임 설명
환자 파일을 드래그하여 파일 슬롯에 정리하는 게임입니다.

### 클래스 구조
- **PatientFileSortMiniGame**: 메인 게임 로직
- **PatientFile**: 드래그 가능한 환자 파일

### 핵심 기능
```csharp
// 파일 정렬 확인
private void OnTrigger(Collider2D col)
{
    if (!isSorted && col.CompareTag(targetSlotTag))
    {
        isSorted = true;
        OnFileSorted?.Invoke();
        gameObject.SetActive(false);
    }
}

// 초기 위치로 리셋
public void ResetPosition()
{
    isSorted = false;
    rectTransform.localPosition = startPos;
}
```

### Unity 설정
1. Canvas 하위에 빈 GameObject 생성 → `PatientFileSortMiniGame` 컴포넌트
2. FilesParent: 파일들의 부모 Transform
3. 각 파일:
   - `PatientFile` 컴포넌트
   - `DraggableObject` 컴포넌트
   - `DragTriggerSensor` 컴포넌트
   - Collider2D (IsTrigger = true)
4. 파일 슬롯:
   - Tag: "FileSlot"
   - Collider2D

---

## 7. BrainWavePattern (뇌파 패턴 맞추기)

**파일**: `Assets/InGame/Scripts/MiniGame/Asylum/BrainWavePattern/BrainWavePatternMiniGame.cs`

### 게임 설명
여러 슬라이더를 조절하여 목표 뇌파 패턴과 일치시키는 게임입니다.

### 클래스 구조
- **BrainWavePatternMiniGame**: 메인 게임 로직
- **BrainWaveSlider**: 개별 슬라이더

### 핵심 기능
```csharp
// 슬라이더 값 확인
public bool IsMatched(float threshold)
{
    return Mathf.Abs(slider.value - targetValue) <= threshold;
}

// 모든 슬라이더 확인
private void CheckAllSliders()
{
    bool allMatched = true;
    foreach (var slider in sliders)
    {
        if (!slider.IsMatched(matchThreshold))
        {
            allMatched = false;
            break;
        }
    }

    if (allMatched)
        CompleteGame();
}
```

### Unity 설정
1. Canvas 하위에 빈 GameObject 생성 → `BrainWavePatternMiniGame` 컴포넌트
2. Sliders: 여러 개의 BrainWaveSlider 추가
3. 각 슬라이더:
   - `BrainWaveSlider` 컴포넌트
   - UI Slider 컴포넌트
   - `TargetValue`: 목표 값 (0~1)
   - `FillImage`: 슬라이더 Fill 이미지 (색상 변경용)

---

## 8. StraitjacketTie (구속복 묶기)

**파일**: `Assets/InGame/Scripts/MiniGame/Asylum/StraitjacketTie/StraitjacketTieMiniGame.cs`

### 게임 설명
구속복의 끈을 드래그하여 올바른 버클에 연결하는 게임입니다.

### 클래스 구조
- **StraitjacketTieMiniGame**: 메인 게임 로직
- **JacketStrap**: 드래그 가능한 끈
- **JacketBuckle**: 버클

### 핵심 기능
```csharp
// ID 매칭 시스템
[SerializeField] private int strapID;  // 끈의 ID

// 버클과 ID 확인
if (buckle.GetBuckleID() == strapID)
{
    ConnectStrap(buckle.transform.position);
}
```

### Unity 설정
1. Canvas 하위에 빈 GameObject 생성 → `StraitjacketTieMiniGame` 컴포넌트
2. Straps: 여러 개의 JacketStrap 추가
3. 각 끈:
   - `JacketStrap` 컴포넌트
   - `DraggableObject` 컴포넌트
   - `DragTriggerSensor` 컴포넌트
   - `StrapID`: 고유 ID (0, 1, 2...)
   - Collider2D (IsTrigger = true)
4. 각 버클:
   - `JacketBuckle` 컴포넌트
   - `BuckleID`: 끈과 같은 ID
   - Tag: "Buckle"
   - Collider2D

---

## 9. DiagnosisForm (진단서 작성)

**파일**: `Assets/InGame/Scripts/MiniGame/Asylum/DiagnosisForm/DiagnosisFormMiniGame.cs`

### 게임 설명
진단서의 체크박스 항목들을 올바르게 선택하여 진단서를 완성하는 게임입니다.

### 클래스 구조
- **DiagnosisFormMiniGame**: 메인 게임 로직
- **DiagnosisCheckbox**: 개별 체크박스

### 핵심 기능
```csharp
// 체크박스 정답 확인
[SerializeField] private bool shouldBeChecked;  // 이 항목이 체크되어야 하는가?

public bool IsCorrect()
{
    return toggle.isOn == shouldBeChecked;
}

// 모든 체크박스 확인
private void CheckCompletion()
{
    bool allCorrect = true;
    foreach (var checkbox in checkboxes)
    {
        if (!checkbox.IsCorrect())
        {
            allCorrect = false;
            break;
        }
    }

    if (allCorrect)
        CompleteGame();
}
```

### Unity 설정
1. Canvas 하위에 빈 GameObject 생성 → `DiagnosisFormMiniGame` 컴포넌트
2. Checkboxes: 여러 개의 DiagnosisCheckbox 추가
3. 각 체크박스:
   - `DiagnosisCheckbox` 컴포넌트
   - UI Toggle 컴포넌트
   - `ShouldBeChecked`: true/false 설정
   - `CheckmarkImage`: 체크마크 이미지

---

## 10. MedicineSchedule (투약 시간 체크)

**파일**: `Assets/InGame/Scripts/MiniGame/Asylum/MedicineSchedule/MedicineScheduleMiniGame.cs`

### 게임 설명
정해진 시간에 약물 투여 버튼을 클릭하여 투약 일정을 완성하는 게임입니다.

### 클래스 구조
- **MedicineScheduleMiniGame**: 메인 게임 로직
- **MedicineSlot**: 개별 투약 슬롯

### 핵심 기능
```csharp
// 시간 기반 활성화
[SerializeField] private float targetTime;    // 투여해야 하는 시간
[SerializeField] private float timeWindow = 2f;  // 허용 시간 범위

public void UpdateTime(float currentTime)
{
    float timeDiff = Mathf.Abs(currentTime - targetTime);
    isActive = timeDiff <= timeWindow;

    giveButton.interactable = isActive;
}

// 게임 진행 시간
[SerializeField] private float gameTime = 30f;
```

### Unity 설정
1. Canvas 하위에 빈 GameObject 생성 → `MedicineScheduleMiniGame` 컴포넌트
2. MedicineSlots: 여러 개의 MedicineSlot 추가
3. TimerText: TMP_Text (남은 시간 표시)
4. 각 슬롯:
   - `MedicineSlot` 컴포넌트
   - `GiveButton`: 투약 버튼
   - `TargetTime`: 투여 시간 (5, 10, 15초 등)
   - `TimeWindow`: 허용 범위 (2초 등)
   - `TimerIndicator`: Fill 이미지 (진행 표시)

---

## Unity 설정 가이드

### 1. MiniGameTag 설정

모든 미니게임 오브젝트에 필수:

```
1. GameObject에 MiniGameTag 컴포넌트 추가
2. MiniGameType을 해당 게임 타입으로 설정
   - PillSort: MiniGameType.PillSort
   - HeartRateMonitor: MiniGameType.HeartRateMonitor
   - 등등...
```

### 2. 공통 UI 설정

Asylum 미니게임은 Screen Space - Overlay Canvas 사용:

```
1. Canvas 생성
   - Render Mode: Screen Space - Overlay
   - Canvas Scaler 추가 (UI Scale Mode: Scale With Screen Size)
   - Reference Resolution: 1920x1080

2. 미니게임 오브젝트를 Canvas 하위에 배치
```

### 3. Draggable 오브젝트 설정

드래그가 필요한 오브젝트 (약물, 주사기, 끈 등):

```
필수 컴포넌트:
- DraggableObject
- DragTriggerSensor
- Collider2D (IsTrigger = true)
- RectTransform

설정:
1. Collider2D 크기를 오브젝트에 맞게 조정
2. DragTriggerSensor가 충돌을 감지할 수 있도록 Layer 확인
```

### 4. Clickable 오브젝트 설정

클릭이 필요한 오브젝트 (모니터, 카메라 등):

```
필수 컴포넌트:
- ClickableObject

설정:
1. Raycast Target 활성화된 UI 이미지 필요
2. Canvas의 Graphic Raycaster 컴포넌트 확인
```

### 5. Collider 설정

Asylum 미니게임은 UI 기반이므로:

```
사용 가능한 Collider:
- BoxCollider2D (권장)
- CircleCollider2D

설정:
- IsTrigger: true로 설정
- Layer: UI 레이어 사용
```

---

## 커스터마이징

### 난이도 조정

각 미니게임의 난이도를 조절할 수 있습니다:

#### PillSort
```csharp
[SerializeField] private int pillCount = 10;  // 약물 개수 증가
```

#### HeartRateMonitor
```csharp
[SerializeField] private int heartbeatsToFix = 10;  // 목표 개수
[SerializeField] private float spawnInterval = 2f;  // 생성 간격 감소
```

#### SecurityCamera
```csharp
[SerializeField] private int anomaliesToFind = 5;   // 목표 개수
[SerializeField] private float spawnInterval = 3f;  // 생성 간격
```

#### BrainWavePattern
```csharp
[SerializeField] private float matchThreshold = 0.1f;  // 허용 오차 감소
```

#### MedicineSchedule
```csharp
[SerializeField] private float gameTime = 30f;     // 제한 시간 감소
[SerializeField] private float timeWindow = 2f;    // 허용 범위 감소
```

### 비주얼 커스터마이징

#### 색상 테마

Asylum 미니게임의 기본 색상:

```csharp
// 일반 상태
Color normalColor = new Color(0.3f, 0.3f, 0.3f);  // 어두운 회색

// 경고 상태
Color warningColor = new Color(0.8f, 0.2f, 0.2f); // 어두운 빨강

// 완료 상태
Color completeColor = new Color(0.2f, 0.6f, 0.2f); // 어두운 초록
```

#### 애니메이션

각 미니게임에 애니메이션 추가:

```csharp
// 페이드 인/아웃
public IEnumerator FadeIn(CanvasGroup group, float duration)
{
    float elapsed = 0f;
    while (elapsed < duration)
    {
        elapsed += Time.deltaTime;
        group.alpha = elapsed / duration;
        yield return null;
    }
    group.alpha = 1f;
}

// 흔들림 효과
public IEnumerator Shake(Transform target, float duration)
{
    Vector3 originalPos = target.localPosition;
    float elapsed = 0f;

    while (elapsed < duration)
    {
        float x = Random.Range(-10f, 10f);
        float y = Random.Range(-10f, 10f);
        target.localPosition = originalPos + new Vector3(x, y, 0f);
        elapsed += Time.deltaTime;
        yield return null;
    }

    target.localPosition = originalPos;
}
```

---

## 통합 가이드

### MiniGameManager와 통합

모든 Asylum 미니게임은 MiniGameManager와 자동으로 통합됩니다:

```csharp
// MiniGameType.cs에 이미 추가됨
public enum MiniGameType
{
    // Asylum Theme (20-29)
    PillSort = 20,
    PatientFileSort = 21,
    SedativeInjection = 22,
    StraitjacketTie = 23,
    BrainWavePattern = 24,
    HeartRateMonitor = 25,
    MentalStateExam = 26,
    SecurityCamera = 27,
    DiagnosisForm = 28,
    MedicineSchedule = 29,
}
```

### 라운드 시스템과 연동

`ResetMission()` 메서드로 라운드 초기화:

```csharp
// RoundManager에서 호출
public void ResetAllMissions()
{
    foreach (var miniGame in allMiniGames)
    {
        miniGame.ResetMission();
    }
}
```

---

## 트러블슈팅

### 드래그가 작동하지 않을 때

1. DraggableObject 컴포넌트 확인
2. Canvas의 Graphic Raycaster 확인
3. Collider2D의 IsTrigger = true 확인
4. Layer 충돌 매트릭스 확인

### 클릭이 작동하지 않을 때

1. ClickableObject 컴포넌트 확인
2. UI 이미지의 Raycast Target 활성화
3. Canvas의 Graphic Raycaster 확인

### 완료 보고가 전송되지 않을 때

1. MiniGameTag의 MiniGameType 확인
2. onComplete 이벤트 등록 확인
3. MiniGameManager 연동 확인

---

## 결론

이 10가지 Asylum 미니게임은 정신병원이라는 어두운 테마를 바탕으로 긴장감 있는 게임플레이를 제공합니다.
각 미니게임은 Among Us 스타일의 간단하면서도 몰입감 있는 디자인을 따르며,
DraggableObject와 ClickableObject를 활용하여 직관적인 조작을 제공합니다.

모든 미니게임은 MiniGameManager와 완벽하게 통합되며,
라운드 시스템과 함께 작동하여 반복적인 게임플레이를 지원합니다.

**Happy Coding!** 🏥💉🧠
