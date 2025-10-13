# **Chat System Module Documentation**

## **1. 개요**

기존의 단일 `ChatManager` 클래스를 **SOLID 원칙에 기반한 계층형 아키텍처(Domain, Application, Infrastructure, Presentation)로 리팩토링**한 채팅 시스템 모듈입니다.

본 모듈은 기능적 요구사항(글로벌/유령 채널, 히스토리 동기화 등)을 모두 유지하면서, **테스트 용이성, 확장성, 유지보수성 및 성능을 극대화**하는 것을 목표로 합니다.

---

## **2. 주요 기능**

- **채널 분리**: 생존/사망 상태에 따른 자동 채널 분리 (Global/Ghost)  
- **스팸 방지**: 클라이언트/서버 이중 Rate Limiting 적용  
- **히스토리 동기화**: 중도 입장 플레이어를 위한 최근 대화 내용 자동 동기화  
- **성능 최적화**: `ObjectPool`을 사용한 채팅 버블 UI 재사용으로 GC 최소화  
- **설정 관리**: `ScriptableObject`를 통한 유연한 채팅 정책 설정  

---

## **3. 아키텍처**

### **3.1. 디렉토리 구조**

```
Assets/InGame/Chat/
 ├─ Domain/         // 순수 데이터 모델, 핵심 인터페이스 (엔진/플랫폼 비의존적)
 ├─ Application/    // 핵심 비즈니스 로직 (채팅 서비스, 필터링, 히스토리 등)
 ├─ Infrastructure/ // 외부 시스템 연동 (Photon 네트워크, 게임 데이터 등)
 └─ Presentation/   // UI, 입력 처리, 의존성 조립 (Unity 종속적)
```

### **3.2. 핵심 클래스**

| 계층 | 클래스명 | 역할 |
|------|-----------|------|
| Application | `ChatService` | 채팅 정책(정화, 쿨다운, 채널 판정, 히스토리 관리)을 총괄하는 핵심 서비스 |
| Infrastructure | `PhotonChatTransport` | Photon Pun2의 RPC 송수신을 처리하는 네트워크 어댑터 |
| Presentation | `ChatUIController` | 채팅 UI(버블 생성, 풀링, 스크롤, 입력)를 제어하는 뷰 컨트롤러 |
| Presentation | `ChatInstaller` | 각 계층의 의존성을 조립하고 주입하는 Composition Root |

---

## **4. 설치 및 설정 가이드**

> **사전 조건**  
> 프로젝트에 Photon PUN2가 임포트되어 있으며, 룸 입/퇴장 로직이 구현된 상태여야 합니다.

### **Step 1: 설정 에셋 생성**

1. Unity Project 창에서  
   `마우스 우클릭 > Create > Game > Chat > Config` 선택 → `ChatConfig` 에셋 생성  
2. 생성된 에셋을 선택하고 아래 값으로 조정:
   - **Max Messages**: 150  
   - **Max Chars Per Message**: 200  
   - **Min Send Interval Sec**: 0.5  
   - **Sync Batch Count**: 50  
   - **Pool Default Capacity**: 32  
   - **Pool Max Size**: 256  

---

### **Step 2: 씬 오브젝트 배치**

#### **① ChatTransport (빈 오브젝트)**

- **추가 컴포넌트**: `PhotonView`, `PhotonChatTransport`  
- `PhotonView`의 Observed Components는 **비워둡니다.**

#### **② ChatUI (Canvas 하위)**

- **추가 컴포넌트**: `PhotonView`, `ChatUIController`  
- **Inspector 연결**
  - `Chat Content Root`: Scroll View의 `Content` Transform  
  - `Input`: `TMP_InputField` (⚠️ **Rich Text 비활성화**)  
  - `Scroll Rect`: `ScrollRect`  
  - `Config`: `ChatConfig` 에셋  
  - `Prefabs`: 각 상황별 채팅 버블 프리팹 (유령용 미지정 시 일반 프리팹 폴백)

#### **③ ChatInstaller (빈 오브젝트)**

- **추가 컴포넌트**: `ChatInstaller`  
- **Inspector 연결**
  - `Config`: `ChatConfig` 에셋  
  - `Transport`: `ChatTransport` 오브젝트  
  - `UI`: `ChatUI` 오브젝트  

---

### **Step 3: 버블 프리팹 설정**

1. 각 채팅 버블 프리팹에 `ChatBubbleView` 추가  
2. `ChatBubbleView`의 닉네임/본문 `TMP_Text` 연결  
3. `Content` 오브젝트에는 `Vertical Layout Group` + `Content Size Fitter (Vertical Fit: Preferred Size)` 권장  

---

### **Step 4: UI 이벤트 연결**

- **전송 버튼**: `Button.OnClick()` → `ChatUIController.OnClick_Send`  
- **엔터 키 전송**: `TMP_InputField.OnSubmit()` → `ChatUIController.OnClick_Send`  

---

## **5. 기존 ChatManager 마이그레이션**

- 기존 `ChatManager`는 반드시 **비활성화 또는 제거**해야 함  
- 중복 RPC 호출, 콜백 충돌 등 **심각한 오작동 위험**  
- 코드상 `ChatManager.Instance` 참조는 브릿지(Bridge) 패턴으로 점진적 제거 권장  

---

## **6. FAQ 및 문제 해결**

**Q. 메시지가 두 번씩 표시됩니다.**  
→ 씬 내 구버전 `ChatManager` 존재 가능성 99%. 완전히 제거하십시오.

**Q. `GameDataAliveQuery`에서 컴파일 오류 발생**  
→ `GameDataManager.Instance.TryGetInGameDataByActorId()` 및 `data.isAlive`를  
프로젝트 구조에 맞게 수정 필요 (이 부분은 **커스터마이징 지점**입니다)

**Q. 유령 채팅이 생존자에게 보입니다.**  
→ `GameDataAliveQuery`의 상태 판정이 현재 게임 상태를 정확히 반환하는지 확인

**Q. `<b>`, `<color>` 태그가 그대로 노출됩니다.**  
→ `TMP_InputField`의 **Rich Text 옵션 비활성화** 필요  

---

## **7. 호환성 및 주의사항 ⚠️**

- **Unity**: `2022.3.62 LTS`  
- **Photon**: `PUN 2.x`  
- **Dependencies**: `UnityEngine.Pool` (Unity 2021 이상)  

본 문서는 상기 버전을 기준으로 작성되었습니다.  
다른 버전의 Unity/Photon을 사용할 경우,  
`Infrastructure` 계층 (`PhotonChatTransport`, `GameDataAliveQuery`) 수정이 필요할 수 있습니다.
