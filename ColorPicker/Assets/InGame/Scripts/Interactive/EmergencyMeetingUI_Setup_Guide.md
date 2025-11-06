# 긴급 소집 UI 설정 가이드 (어몽어스 스타일)

## 📋 개요
InteractiveEmergencyMeeting 클래스는 어몽어스와 같은 팝업 UI를 제공합니다.
- 화면 중앙 팝업
- 페이드 인/아웃 애니메이션
- 어두운 배경 (Dimmed Background)
- ESC 키로 닫기
- 플레이어 입력 차단

---

## 🎨 UI 구조

### Canvas 계층 구조
```
Canvas (Screen Space - Overlay)
├── EmergencyMeetingPopup (GameObject)
│   ├── CanvasGroup (Component)
│   ├── DimmedBackground (Image - 반투명 검정색)
│   └── PopupPanel (Image - 흰색 배경)
│       ├── TitleText (TextMeshPro)
│       ├── DescriptionText (TextMeshPro)
│       ├── StatusText (TextMeshPro)
│       ├── ConfirmButton (Button)
│       │   └── ButtonText (TextMeshPro: "긴급 소집")
│       └── CloseButton (Button)
│           └── ButtonText (TextMeshPro: "X")
```

---

## 🔧 Unity 에디터 설정 단계

### 1. Canvas 생성
1. **Hierarchy** → 우클릭 → **UI** → **Canvas**
2. Canvas 설정:
   - Render Mode: **Screen Space - Overlay**
   - Pixel Perfect: **체크 해제**
   - Canvas Scaler:
     - UI Scale Mode: **Scale With Screen Size**
     - Reference Resolution: **1920 x 1080**
     - Match: **0.5** (Width/Height 중간값)

### 2. 팝업 UI 구조 생성

#### A. EmergencyMeetingPopup (루트)
1. Canvas 하위에 **GameObject** 생성 → 이름: `EmergencyMeetingPopup`
2. **CanvasGroup 컴포넌트 추가** (Add Component → Canvas Group)
   - Interactable: ✅
   - Block Raycasts: ✅
   - Ignore Parent Groups: ❌
3. RectTransform 설정:
   - Anchor: **Stretch-Stretch** (전체 화면)
   - Left: 0, Right: 0, Top: 0, Bottom: 0

#### B. DimmedBackground (어두운 배경)
1. EmergencyMeetingPopup 하위에 **Image** 생성 → 이름: `DimmedBackground`
2. Image 설정:
   - Source Image: **None** (또는 투명 Sprite)
   - Color: **검정색 (0, 0, 0, 180)** ← 알파값 180으로 반투명
3. RectTransform:
   - Anchor: **Stretch-Stretch**
   - Left: 0, Right: 0, Top: 0, Bottom: 0

#### C. PopupPanel (팝업 창)
1. EmergencyMeetingPopup 하위에 **Image** 생성 → 이름: `PopupPanel`
2. Image 설정:
   - Source Image: **UI-Sprite (또는 둥근 모서리 Sprite)**
   - Color: **흰색 (255, 255, 255, 255)**
3. RectTransform:
   - Anchor: **Middle-Center**
   - Width: **600**
   - Height: **400**
   - Pos X: 0, Pos Y: 0

#### D. TitleText (타이틀)
1. PopupPanel 하위에 **TextMeshPro** 생성 → 이름: `TitleText`
2. 설정:
   - Text: `긴급 소집`
   - Font Size: **48**
   - Alignment: **Center (수평/수직)**
   - Color: **검정색**
3. RectTransform:
   - Anchor: **Top-Center**
   - Width: 500, Height: 80
   - Pos X: 0, Pos Y: **-50**

#### E. DescriptionText (설명)
1. PopupPanel 하위에 **TextMeshPro** 생성 → 이름: `DescriptionText`
2. 설정:
   - Text: `모든 플레이어를 소집합니다.`
   - Font Size: **24**
   - Alignment: **Center**
   - Color: **회색 (128, 128, 128)**
3. RectTransform:
   - Anchor: **Top-Center**
   - Width: 500, Height: 60
   - Pos X: 0, Pos Y: **-130**

#### F. StatusText (상태 표시)
1. PopupPanel 하위에 **TextMeshPro** 생성 → 이름: `StatusText`
2. 설정:
   - Text: `쿨다운: 30초 남음`
   - Font Size: **28**
   - Alignment: **Center**
   - Color: **빨강/초록 (동적 변경됨)**
3. RectTransform:
   - Anchor: **Middle-Center**
   - Width: 500, Height: 80
   - Pos X: 0, Pos Y: **0**

#### G. ConfirmButton (확인 버튼)
1. PopupPanel 하위에 **Button** 생성 → 이름: `ConfirmButton`
2. Button 설정:
   - Transition: **Color Tint**
   - Normal Color: **초록색 (0, 200, 0)**
   - Highlighted: **밝은 초록**
   - Pressed: **어두운 초록**
   - Disabled: **회색 (128, 128, 128)**
3. RectTransform:
   - Anchor: **Bottom-Center**
   - Width: 250, Height: 60
   - Pos X: **-140**, Pos Y: **40**
4. 하위 Text:
   - Text: `긴급 소집`
   - Font Size: **32**
   - Color: **흰색**

#### H. CloseButton (닫기 버튼)
1. PopupPanel 하위에 **Button** 생성 → 이름: `CloseButton`
2. Button 설정:
   - Transition: **Color Tint**
   - Normal Color: **빨강색 (200, 0, 0)**
   - Highlighted: **밝은 빨강**
   - Pressed: **어두운 빨강**
3. RectTransform:
   - Anchor: **Bottom-Center**
   - Width: 250, Height: 60
   - Pos X: **140**, Pos Y: **40**
4. 하위 Text:
   - Text: `닫기`
   - Font Size: **32**
   - Color: **흰색**

---

## 🔗 InteractiveEmergencyMeeting 컴포넌트 설정

### Inspector 설정

1. 인게임 씬에서 긴급소집 버튼 GameObject 선택
2. **InteractiveEmergencyMeeting** 컴포넌트 설정:

#### Emergency Meeting Button
- **Button Sprite**: 월드 상의 긴급소집 버튼 SpriteRenderer

#### UI References (Screen Space - Overlay)
- **Emergency Popup UI**: `EmergencyMeetingPopup` GameObject
- **Popup Canvas Group**: EmergencyMeetingPopup의 `CanvasGroup` 컴포넌트
- **Dimmed Background**: `DimmedBackground` GameObject

#### UI Components
- **Confirm Button**: `ConfirmButton` Button 컴포넌트
- **Close Button**: `CloseButton` Button 컴포넌트
- **Title Text**: `TitleText` TextMeshPro 컴포넌트
- **Status Text**: `StatusText` TextMeshPro 컴포넌트
- **Description Text**: `DescriptionText` TextMeshPro 컴포넌트

#### Animation Settings
- **Fade In Duration**: `0.2` (초)
- **Fade Out Duration**: `0.15` (초)

---

## ✨ 주요 기능

### 1. 페이드 인/아웃 애니메이션
- UI가 열릴 때: 부드럽게 페이드 인
- UI가 닫힐 때: 빠르게 페이드 아웃

### 2. 상태 실시간 표시
- ✅ **사용 가능**: `"긴급 소집 사용 가능"` (초록색)
- ⏱️ **쿨다운**: `"쿨다운: XX초 남음"` (빨강색)
- 📊 **미션 부족**: `"미션 진행도 부족\n현재: XX% / 필요: XX%"` (빨강색)

### 3. 입력 처리
- **확인 버튼**: 긴급 소집 시작
- **닫기 버튼**: UI 닫기
- **ESC 키**: UI 닫기
- **플레이어 입력 차단**: UI가 열려있을 때 이동 불가

### 4. 어두운 배경
- 배경을 어둡게 처리하여 팝업에 집중
- 클릭 불가 (레이캐스트 차단)

---

## 🎮 사용 흐름

```
1. 플레이어가 긴급소집 버튼에 접근
   ↓
2. E 키 (또는 상호작용 키) 입력
   ↓
3. 팝업 UI 페이드 인 + 배경 어둡게
   ↓
4. 플레이어 입력 차단
   ↓
5. 상태 확인 (쿨다운/미션 진행도)
   ↓
6-A. 확인 버튼 클릭 → 긴급 소집 시작 → UI 닫기
6-B. 닫기 버튼/ESC 클릭 → UI 닫기
   ↓
7. 페이드 아웃 + 플레이어 입력 복구
```

---

## 🐛 문제 해결

### UI가 보이지 않음
1. Canvas의 Render Mode가 **Screen Space - Overlay**인지 확인
2. EmergencyMeetingPopup이 Canvas 하위에 있는지 확인
3. CanvasGroup의 Alpha가 0이 아닌지 확인

### 버튼 클릭이 안됨
1. CanvasGroup의 **Block Raycasts**가 활성화되어 있는지 확인
2. EventSystem이 씬에 있는지 확인
3. Button의 Interactable이 체크되어 있는지 확인

### 애니메이션이 작동하지 않음
1. CanvasGroup 컴포넌트가 EmergencyMeetingPopup에 있는지 확인
2. Fade Duration이 0이 아닌지 확인
3. 코루틴이 정상적으로 시작되는지 로그 확인

### 플레이어 입력이 차단되지 않음
1. PlayerControl 컴포넌트가 있는지 확인
2. SetControlEnabled() 메서드가 구현되어 있는지 확인

---

## 📝 추가 커스터마이징

### 색상 변경
- **PopupPanel 배경색**: Image의 Color 변경
- **버튼 색상**: Button의 Normal/Highlighted/Pressed 색상 변경
- **텍스트 색상**: TMP_Text의 Color 변경

### 크기 조정
- **PopupPanel**: Width/Height 조절
- **버튼**: Width/Height 조절
- **텍스트**: Font Size 조절

### 애니메이션 속도
- **fadeInDuration**: 페이드 인 속도 (기본 0.2초)
- **fadeOutDuration**: 페이드 아웃 속도 (기본 0.15초)

---

## 🎨 어몽어스 스타일 참고

### 권장 색상
- **배경**: 흰색 (#FFFFFF)
- **타이틀**: 검정색 (#000000)
- **확인 버튼**: 빨강색 (#D93636) - 어몽어스 테마
- **닫기 버튼**: 회색 (#808080)
- **Dimmed Background**: 검정 반투명 (0, 0, 0, 180)

### 권장 크기
- **팝업 너비**: 600px
- **팝업 높이**: 400px
- **버튼 너비**: 250px
- **버튼 높이**: 60px

---

## ✅ 체크리스트

- [ ] Canvas 생성 (Screen Space - Overlay)
- [ ] EmergencyMeetingPopup + CanvasGroup 추가
- [ ] DimmedBackground (반투명 검정)
- [ ] PopupPanel (흰색 배경)
- [ ] TitleText, DescriptionText, StatusText 추가
- [ ] ConfirmButton, CloseButton 추가
- [ ] InteractiveEmergencyMeeting에 모든 참조 연결
- [ ] 애니메이션 Duration 설정
- [ ] 테스트: UI 열기/닫기
- [ ] 테스트: 버튼 클릭
- [ ] 테스트: ESC 키로 닫기
- [ ] 테스트: 상태 텍스트 업데이트

---

완료! 이제 어몽어스 스타일의 긴급 소집 UI를 사용할 수 있습니다! 🎉
