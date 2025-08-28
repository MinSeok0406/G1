using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

namespace ColorPicker.InGame
{
    public class MiniGameManager : SingletonMonobehaviour<MiniGameManager>
    {
        [Header("UI Roots")]
        public Transform uiRoot; // 미니게임 UI를 붙일 부모(선택)
        public GameObject mingameFrame; // 미니게임 프레임
        public Camera crtCamera; // 미니게임 전용 카메라
        public RectTransform targetRect; // 맵핑 영역
        public RectTransform rawImageRect; // 드래그 영역

        [Header("State (Runtime)")]
        public GameObject currentUI;             // 현재 실행 중인 미니게임 UI
        private readonly Dictionary<MiniGameType, GameObject> minigameDictionary = new();

        [Header("Options")]
        [SerializeField] private bool deactivateAllAtBoot = true;   // 부팅 시 전부 비활성화
        [SerializeField] private bool autoEnableInputOnStart = false; // 개발 테스트용

        // 실행 상태 가드
        private bool isRunning;

        // 외부에서 구독 가능한 이벤트(선택)
        public event Action<MiniGameType> OnMinigameStarted;
        public event Action<MiniGameReport> OnMinigameCompleted;

        protected override void Awake()
        {
            base.Awake();
            BuildCache(includeInactive: true);

            if (deactivateAllAtBoot)
            {
                foreach (var go in minigameDictionary.Values)
                    if (go) go.SetActive(false);
            }

            MiniGameInputHandler.rtCamera = crtCamera;
            MiniGameInputHandler.rawImageRect = rawImageRect;
            MiniGameInputHandler.targetRect = targetRect;
        }

        private void Start()
        {
            if (autoEnableInputOnStart)
                MiniGameInputContext.EnableInput();
            else
                MiniGameInputContext.DisableInput();

            // 에디터에서 currentUI가 이미 활성이라면 상태 맞춤
            if (currentUI && currentUI.activeSelf)
            {
                isRunning = true;
                MiniGameInputContext.EnableInput();
            }
        }

        private void OnDestroy()
        {
            // 안전하게 입력 원복
            MiniGameInputContext.DisableInput();
            isRunning = false;
            currentUI = null;
        }

        private void Update()
        {
            if (!isRunning || currentUI == null || !currentUI.activeSelf)
                return;

            // 프레임별 입력 처리(미니게임 공용 핸들러)
            MiniGameInputHandler.ProcessInput();
        }

        /// <summary>
        /// 시작 시 한 번, 또는 런타임에 태그를 재수집합니다.
        /// </summary>
        public void BuildCache(bool includeInactive)
        {
            minigameDictionary.Clear();

            // 기준 Transform: uiRoot가 있으면 그 하위, 없으면 이 매니저 하위
            var root = uiRoot ? uiRoot : transform;

            // 비활성 포함 수집
            var tags = root.GetComponentsInChildren<MiniGameTag>(includeInactive);
            for (int i = 0; i < tags.Length; i++)
            {
                var tag = tags[i];
                if (!tag) continue;

                // 중복 타입이 있으면 첫 번째만 사용
                if (minigameDictionary.ContainsKey(tag.miniGameType))
                {
                    Debug.LogWarning($"[MinigameManager] Duplicate MiniGameType={tag.miniGameType} on {tag.gameObject.name}. Ignored.");
                    continue;
                }
                minigameDictionary.Add(tag.miniGameType, tag.gameObject);
                Debug.Log($"[MinigameManager] Registered MiniGameType={tag.miniGameType} on {tag.gameObject.name}");
            }
        }

        /// <summary>
        /// 외부에서 수동 등록이 필요할 때 사용(동적 생성 등)
        /// </summary>
        public bool Register(MiniGameTag tag)
        {
            if (!tag || !tag.gameObject) return false;
            if (minigameDictionary.ContainsKey(tag.miniGameType)) return false;
            minigameDictionary.Add(tag.miniGameType, tag.gameObject);
            if (deactivateAllAtBoot) tag.gameObject.SetActive(false);
            return true;
        }

        /// <summary>
        /// 외부에서 수동 제거가 필요할 때 사용
        /// </summary>
        public bool Unregister(MiniGameTag tag)
        {
            if (!tag) return false;
            return minigameDictionary.Remove(tag.miniGameType);
        }

        /// <summary>
        /// 기존 시그니처 유지. 중복 실행, 누락, 예외 처리 가드 추가.
        /// </summary>
        public void StartMiniGame(MiniGameType miniGameType)
        {
            // 이미 실행 중이면 무시(호환성을 위해 void 유지, 필요하면 아래 TryStart를 사용)
            if (isRunning)
            {
                Debug.LogWarning("[MinigameManager] A minigame is already running.");
                return;
            }

            if (!minigameDictionary.TryGetValue(miniGameType, out var go) || !go)
            {
                Debug.LogWarning($"[MinigameManager] MiniGame GameObject not found for type: {miniGameType}");
                return;
            }

            currentUI = go;

            // uiRoot가 지정되어 있고 다른 부모라면 붙여준다(옵션)
            if (uiRoot && currentUI.transform.parent != uiRoot)
                currentUI.transform.SetParent(uiRoot, worldPositionStays: false);

            if (!currentUI.TryGetComponent(out MiniGameBase game))
            {
                Debug.LogError($"[MinigameManager] MiniGameBase missing on {currentUI.name}");
                currentUI = null;
                return;
            }

            // 드래그 영역 제공(있을 때만; 인터페이스 지원)
            if (rawImageRect && game is IDragAreaConsumer consumer)
                consumer.SetDragArea(rawImageRect);

            try
            {
                // 실행 상태 설정
                isRunning = true;
                MiniGameInputContext.EnableInput();

                // 콜백 바인딩 → 게임 시작
                game.SetCallback(OnComplete);
                currentUI.SetActive(true);
                OnMinigameStarted?.Invoke(miniGameType);
                game.StartGame();

                mingameFrame.SetActive(true);
                crtCamera.gameObject.SetActive(true);
            }
            catch (Exception e)
            {
                Debug.LogError($"[MinigameManager] StartMiniGame exception: {e}");
                // 비정상 시작 시 안전 종료
                SafeAbortCurrent();
            }
        }

        /// <summary>
        /// bool 반환이 필요한 곳에서 사용할 수 있는 안전 시작 API (추가)
        /// </summary>
        public bool TryStartMiniGame(MiniGameType type)
        {
            if (isRunning) return false;
            StartMiniGame(type);
            return isRunning;
        }

        /// <summary>
        /// 기존 시그니처 유지. 미니게임에서 완료 시 호출.
        /// </summary>
        private void OnComplete(MiniGameReport report)
        {
            // UI/입력 원복
            if (currentUI) currentUI.SetActive(false);
            currentUI = null;
            isRunning = false;
            MiniGameInputContext.DisableInput();

            OnMinigameCompleted?.Invoke(report);

            mingameFrame.SetActive(false);
            crtCamera.gameObject.SetActive(false);

            MissionManager.Instance.RequestMissionComplete(PhotonNetwork.LocalPlayer.ActorNumber, (int)report.miniGameType);
        }

        /// <summary>
        /// 강제 종료(중간 중단)용 API. MiniGameBase가 Abort를 제공하면 호출.
        /// </summary>
        public void AbortCurrentMinigame()
        {
            if (!isRunning || currentUI == null) return;

            if (currentUI.TryGetComponent(out MiniGameBase game))
            {
                // 선택적 인터페이스 우선
                if (game is IAbortable abortable)
                {
                    abortable.Abort();
                }
                else
                {
                    // 존재할 수 있는 Abort 메서드를 느슨하게 호출
                    currentUI.SendMessage("Abort", SendMessageOptions.DontRequireReceiver);
                }
            }

            SafeAbortCurrent();
        }

        private void SafeAbortCurrent()
        {
            if (currentUI) currentUI.SetActive(false);
            currentUI = null;
            isRunning = false;
            MiniGameInputContext.DisableInput();
        }
    }

    /// <summary>
    /// (선택) 드래그 영역을 제공받고 싶은 미니게임이 구현
    /// </summary>
    public interface IDragAreaConsumer
    {
        void SetDragArea(RectTransform area);
    }

    /// <summary>
    /// (선택) 강제 종료를 지원하는 미니게임이 구현
    /// </summary>
    public interface IAbortable
    {
        void Abort();
    }
}
