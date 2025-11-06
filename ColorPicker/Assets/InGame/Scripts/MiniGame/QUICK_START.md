# 🚀 MiniGame System - Quick Start

어몽어스 스타일 미니게임 14종 빠른 시작 가이드

---

## 📦 구현 완료된 미니게임 (7종)

| 미니게임 | 타입 | 폴더 | 완성도 |
|---------|------|------|--------|
| Clean | Drag | `Clean/` | ✅ 100% + 초기화 |
| ToySort | Drag | `ToySort/` | ✅ 100% |
| CardSwipe | Drag | `CardSwipe/` | ✅ 100% |
| WireConnect | Drag | `WireConnect/` | ✅ 100% |
| ButtonSequence | Click | `ButtonSequence/` | ✅ 100% |
| Download | Click+Time | `Download/` | ✅ 100% |
| ShootAsteroids | Click | `ShootAsteroids/` | ✅ 100% |

---

## 🎯 주요 파일

### 필수 파일
```
MiniGameBase.cs          ← 모든 미니게임의 베이스 클래스
MiniGameType.cs          ← 14종 게임 타입 enum
MiniGameTag.cs           ← 게임 타입 태그
DraggableObject.cs       ← 드래그 오브젝트
ClickableObject.cs       ← 클릭 오브젝트
DragTriggerSensor.cs     ← 드래그 트리거 감지
IMiniGameInteractable.cs ← IDraggable, IClickable
```

### 미니게임 폴더
```
Clean/                   ✅ 청소 (더러운 부분 드래그)
ToySort/                 ✅ 장난감 정리 (상자에 넣기)
CardSwipe/               ✅ 카드 긁기 (오른쪽 드래그)
WireConnect/             ✅ 전선 연결 (색상 매칭)
ButtonSequence/          ✅ 버튼 순서 (기억력)
Download/                ✅ 다운로드 (진행 바)
ShootAsteroids/          ✅ 운석 슈팅 (클릭)

FuelEngine/              📝 연료 주입 (템플릿)
AlignEngine/             📝 엔진 정렬 (템플릿)
UnlockManifolds/         📝 자물쇠 (템플릿)
EmptyGarbage/            📝 쓰레기 버리기 (템플릿)
SortSamples/             📝 샘플 분류 (템플릿)
NumberPad/               📝 숫자 입력 (템플릿)
FixWiring/               📝 배선 수리 (템플릿)
ScanBody/                📝 몸 스캔 (템플릿)
```

---

## ⚡ 빠른 사용법

### 1. 미니게임 타입 확인
```csharp
public enum MiniGameType
{
    Clean = 1,
    ToySort = 2,
    CardSwipe = 3,
    WireConnect = 4,
    // ... (총 15종)
}
```

### 2. 미니게임 클래스 구조
```csharp
[RequireComponent(typeof(MiniGameTag))]
public class ExampleMiniGame : MiniGameBase
{
    public override void Initialize() { }
    public override void StartGame() { }
    public void ResetMission() { } // ⭐ 라운드 초기화
}
```

### 3. 미션 완료 보고
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

## 🔄 미션 초기화 (중요!)

### 모든 미니게임은 ResetMission() 구현 필수!

```csharp
public void ResetMission()
{
    // 1. 진행 상황 리셋
    progress = 0;

    // 2. 오브젝트 위치 리셋
    foreach (var obj in objects)
    {
        obj.ResetPosition();
        obj.SetActive(true);
    }

    // 3. UI 리셋
    progressBar.value = 0f;

    // 4. 상태 플래그 리셋
    isCompleted = false;
}
```

### 라운드 시작 시 자동 호출
RoundManager가 모든 미니게임의 `ResetMission()`을 호출합니다.

---

## 🛠️ Unity 설정 (간단 버전)

### DraggableObject 사용
1. UI 오브젝트에 **DraggableObject** 컴포넌트 추가
2. Canvas 연결 (Inspector)
3. 끝!

### ClickableObject 사용
1. UI 오브젝트에 **ClickableObject** 컴포넌트 추가
2. `OnClickEvent` 구독
3. 끝!

### DragTriggerSensor 사용
1. Collider2D 추가 (IsTrigger = true)
2. **DragTriggerSensor** 컴포넌트 추가
3. `OnTriggerEntered` 구독
4. 끝!

---

## 📚 상세 가이드

더 자세한 내용은 **[MINIGAME_GUIDE.md](MINIGAME_GUIDE.md)**를 참고하세요:

- 📖 각 미니게임 상세 설명
- 🎨 Unity 설정 가이드
- 🔧 커스터마이징 방법
- 🚀 새 미니게임 추가 방법
- ✅ 체크리스트

---

## 🎮 미니게임 별 핵심 포인트

### Clean (청소)
- 더러운 부분을 드래그 → 알파값 3단계 감소
- `CleanableObject.Initialized()` 로 초기화

### ToySort (장난감 정리)
- 장난감을 "ToyBox" 태그 상자로 드래그
- 랜덤 위치 스폰

### CardSwipe (카드 긁기)
- 카드를 오른쪽으로 200px 이상 드래그
- Update()에서 거리 체크

### WireConnect (전선 연결)
- 전선을 같은 색상 단자에 드래그
- ColorMatch() 로 색상 검사

### ButtonSequence (버튼 순서)
- 랜덤 순서 생성 및 하이라이트
- 틀리면 재시작

### Download (다운로드)
- 버튼 클릭 후 진행 바 대기
- Slider로 진행 상황 표시

### ShootAsteroids (운석 슈팅)
- 위에서 떨어지는 운석 클릭
- 20개 파괴 시 완료

---

## 🎯 핵심 3단계

### 1단계: 파일 확인
```bash
MiniGame/
├── MiniGameBase.cs
├── MiniGameType.cs
├── [각 미니게임 폴더]
```

### 2단계: Unity 설정
- Canvas 생성
- 미니게임 UI 배치
- 컴포넌트 추가
- 참조 연결

### 3단계: 테스트
- StartGame() 호출
- 플레이어 상호작용
- 완료 보고 확인
- ResetMission() 테스트

---

## 💡 팁

### Drag 미니게임
- DragTriggerSensor로 충돌 감지
- IsTrigger = true 필수
- Tag로 대상 구분

### Click 미니게임
- ClickableObject로 간편하게
- OnClickEvent 구독

### Time 미니게임
- Update()에서 타이머 관리
- Slider로 진행 표시

---

## ✅ 체크리스트

- [ ] MiniGameType.cs 확인
- [ ] 7종 구현 완료 미니게임 확인
- [ ] 각 미니게임 폴더 구조 확인
- [ ] MINIGAME_GUIDE.md 읽기
- [ ] Unity에서 프리팹 생성
- [ ] ResetMission() 테스트
- [ ] RoundManager 연동 확인

---

## 🚨 주의사항

1. **ResetMission() 필수**: 라운드 시작 시 초기화를 위해 반드시 구현
2. **Tag 설정**: ToyBox, Terminal 등 필요한 Tag 추가
3. **Collider2D**: DragTriggerSensor 사용 시 IsTrigger = true
4. **Canvas 참조**: DraggableObject는 Canvas 참조 필요

---

## 🎉 시작하기

1. **MINIGAME_GUIDE.md** 읽기 📖
2. Unity에서 예시 미니게임 열기 🎮
3. 원하는 미니게임 선택 및 커스터마이징 🎨
4. 테스트 및 배포 🚀

**Happy Gaming!** 🎉
