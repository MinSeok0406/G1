# In-Game Interaction & Ability System (Unity 2022.3.62 LTS, PUN2)

# 1) 목표

- **SOLID** 원칙에 따라 책임을 분리하고 테스트 용이성/유지보수성을 강화
- **보안성 강화**: RPC 발신자/소유권 검증, 서버 권한 판정 일원화
- **성능 최적화**: NonAlloc 탐색, delegate 캐시, 최소 틱 갱신, Outline 공유/복구
- **가독성/확장성**: 서비스 계층화, 인터페이스 기반 의존성 역전(DIP), 하드코딩 제거
- **호환성 보장**: 기존 API를 유지하고 브릿지/래퍼 제공

---

## 2) 폴더/디렉토리 구조 (추천)

```
Assets/InGame/
 ├─ Interaction/
 │   ├─ InteractiveTriggerBase.cs
 │   ├─ HighlightController.cs
 │   └─ OutlineMarker.cs
 ├─ Abilities/
 │   ├─ Base/
 │   │   └─ TargetingAbilityBase.cs
 │   ├─ MafiaAbility.cs
 │   ├─ DetectiveAbility.cs
 │   ├─ AbilityManager.cs     // 파사드 + RPC 엔드포인트
 │   └─ Services/
 │       ├─ CooldownService.cs (ICooldownService)
 │       ├─ KillService.cs     (IKillService)
 │       ├─ DetectiveService.cs(IDetectiveService)
 │       └─ ColorPickService.cs(IColorPickService)
 ├─ UI/
 │   └─ (UIManager, PlayerCardUI, ColorPickKillButton ...)
 └─ Common/
     ├─ GameResources.cs
     ├─ Settings.cs
     └─ Enums_Ability.cs      // ColorPickResult, 메시지 상수 등
```

---

## 3) 핵심 컴포넌트/서비스

### 3.1 Interaction
- **InteractiveTriggerBase**
  - 2D 트리거 진입/이탈 처리, 로컬 플레이어(IsMine) 판정
  - `InteractionDetector` Add/Remove 연동
  - UI 버튼 표시/숨김 + 콜백 캐시
  - 하이라이트 on/off 일원화(`HighlightController` 사용)
  - 파생 클래스는 `HandleInteractLocal()`/`CanInteract()`만 구현

- **HighlightController**
  - SpriteRenderer: `sharedMaterial` 스왑
  - Mesh/SkinnedMesh: `OutlineMarker` 경유(전 슬롯 교체/복구)
  - 프로젝트 공통 머티리얼(`GameResources.Instance.outlineMaterial`) 폴백

- **OutlineMarker**
  - 원본 `sharedMaterials` 캐시/복원
  - 중복 적용/런타임 제거 안전
  - `GetOrAdd(GameObject)` 제공

### 3.2 Abilities
- **TargetingAbilityBase**
  - 타깃팅/하이라이트/틱(에임, UI) 공통 제공
  - `ITargetingStrategy` + `IHighlighter` 주입
  - 파생: 버튼 로직/쿨다운 UI만 구현

- **MafiaAbility**
  - 반경 내 최근접 타깃 자동 선정, 킬 요청, 쿨다운 UI

- **DetectiveAbility**
  - 최근접 대상 색상 정보 조회(라운드당 1회), UI 결과 표시

### 3.3 Services (SOLID)
- **ICooldownService / CooldownService**
  - 서버 쿨다운 관리, **클라 endTime 로컬 캐시**(RPC 수신 시 `SetEndTimeFromServer`)
  - 플레이어 퇴장 시 연관 뷰 정리

- **IKillService / KillService**
  - 역할/상태/거리 검증 및 킬 적용(유령 전환, 브로드캐스트, 쿨다운 시작)

- **IDetectiveService / DetectiveService**
  - 탐문 판정(반경 + 보정치)

- **IColorPickService / ColorPickService**
  - 컬러 추리 검증 및 판정(성공/실패/에러, 실패 시 쿨다운 부여)

- **AbilityManager (Facade + RPC)**
  - RPC 엔드포인트 유지, 내부 서비스로 위임
  - **RPC 보안**: 발신자/소유권 검증(killerViewID ↔ info.Sender)
  - **현지화/가독성**: 결과코드 enum/메시지 상수화
  - **설정 외부화**: 탐정 보정치(SerializeField), 쿨다운(설정값)

---

## 4) 적용/설치 가이드

### 4.1 의존성
- Unity **2022.3.62 LTS**
- Photon **PUN2.x**
- TextMeshPro
- `UnityEngine.Pool`(2021+)

### 4.2 씬 배치
- **Player**(로컬): `PlayerControl` + `InteractionDetector`
- **UI**: `UIManager`, `ColorPickKillButton`, `PlayerCardUI`
- **AbilityManager**: 싱글톤 오브젝트에 부착(동일 씬 상주 권장)

### 4.3 인스펙터 설정
- `HighlightController`: outlineMaterial 비우면 `GameResources` 폴백
- `MafiaAbility`/`DetectiveAbility`:
  - `outlineMaterial`, `aimMask2D`, `aimRadius`, `aimTickInterval`, `uiTickInterval`
- `AbilityManager`:
  - `killCooldownSeconds`
  - `colorPickWrongCooldownSeconds`
  - `detectiveInspectTolerance` (기본 1.2)

---

## 5) 마이그레이션 노트

- 기존 `IInteractive.ToggleHighlight(bool)`/`OnInteract()`/`GetPosition()` **그대로 유지**
- `InteractiveObjectEffect`는 **Deprecated** → `HighlightController`로 이관(래퍼 유지 가능)
- 기존 상호작용 트리거: `InteractiveTriggerBase` 상속으로 교체
- 타깃팅/하이라이트 로직을 각 클래스에서 제거(베이스/서비스로 일원화)

---

## 6) 보안/안정성 체크리스트

- `RPC_RequestKill`에서 **killerViewID 소유권** 검증(`info.Sender`와 일치 확인)
- 모든 권한/검증/판정은 **서버(마스터)** 에서 최종 결정
- 쿨다운 **클라 로컬 상태 저장**(RPC 수신 시 `SetEndTimeFromServer`) → UI/로직 일관성
- Fake-null/널 가드(싱글톤/뷰 탐색 실패 시 안전 종료/토스트)
- Transform/레이어 **재귀 유틸**(중복 적용/핑퐁 방지)

---

## 7) 성능 최적화 포인트

- OverlapCircle**NonAlloc** + 뷰ID 중복 제거(딕셔너리)
- 하이라이트 변경은 **타깃 변경시에만** 적용/복구
- 버튼 콜백 **delegate 캐시**(GC 감축)
- 틱 분리(에임/UX 0.1s) + 쿨다운 중 타깃팅 **일시 정지**
- 리스트/딕셔너리 간단 풀(ListPool/DictionaryPool)

---

## 8) 주요 코드 스니펫

### 8.1 Cooldown RPC 동기화 (클라 캐시 반영)
```csharp
[PunRPC]
private void RPC_SyncCooldown(int viewID, double endTime)
{
    cooldowns.SetEndTimeFromServer(viewID, endTime);
    UIManager.Instance?.UpdateAbilityCooldownUI(viewID, cooldowns.GetRemaining(viewID));
}
```

### 8.2 Kill 요청의 소유권 검증
```csharp
[PunRPC]
private void RPC_RequestKill(int targetViewID, int killerViewID, float killRadius, PhotonMessageInfo info)
{
    if (!PhotonNetwork.IsMasterClient) return;

    var killerView = PhotonView.Find(killerViewID);
    if (!killerView || killerView.OwnerActorNr != info.Sender?.ActorNumber)
    {
        Debug.LogWarning("[Ability] KillerView ownership mismatch.");
        return;
    }
    // ... 검증 → 적용
}
```

### 8.3 레이어 재귀 적용 유틸
```csharp
private static void SetLayerRecursively(GameObject root, int layer)
{
    if (!root) return;
    if (root.layer != layer) root.layer = layer;
    foreach (Transform child in root.transform)
        SetLayerRecursively(child.gameObject, layer);
}
```

---

## 9) FAQ / 트러블슈팅

- **Q. 하이라이트가 핑크로 출력됩니다.**  
  A. 프로젝트의 outline 셰이더가 대상 렌더러(Sprite/Mesh/SkinnedMesh)에 호환되는지 확인하고, `HighlightController`의 override 머티리얼을 지정하세요.

- **Q. 킬 버튼이 비활성화됩니다.**  
  A. 쿨다운 중이거나 타깃이 없을 때 비활성화됩니다. `AbilityManager.IsOnCooldown()`/`GetRemainingCooldown()`로 확인하세요.

- **Q. 탐정 판정 반경을 바꾸고 싶어요.**  
  A. `AbilityManager.detectiveInspectTolerance`와 각 Ability의 `aimRadius`를 인스펙터에서 조절하세요.

---

## 10) 버전/호환성

- Unity **2022.3.62 LTS**
- Photon **PUN 2.x**
- `UnityEngine.Pool` (2021+)

> 상이한 Unity/Photon 버전, 프로젝트별 데이터 구조(`GameDataManager`)에서 **Infrastructure 계층 수정**이 필요할 수 있습니다.
