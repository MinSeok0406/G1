#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using Photon.Pun;
using UnityEngine;

namespace ColorPicker.InGame
{
    /// <summary>
    /// 디버그 및 테스트용 매니저
    /// 에디터 환경에서만 작동하는 디버그 기능 제공
    /// </summary>
    public sealed class DebugManager : SingletonNetworkBehaviour<DebugManager>
    {
        #region Serialized Fields
        [Header("Debug Settings")]
        [SerializeField] private bool enableDebugMode = false;
        [SerializeField] private KeyCode debugMenuKey = KeyCode.F1;
        [SerializeField] private KeyCode forceWinCitizenKey = KeyCode.F2;
        [SerializeField] private KeyCode forceWinMafiaKey = KeyCode.F3;
        [SerializeField] private KeyCode startRoundKey = KeyCode.F4;
        [SerializeField] private KeyCode endRoundKey = KeyCode.F5;
        [SerializeField] private KeyCode startBodyReportMeetingKey = KeyCode.F6;
        [SerializeField] private KeyCode startEmergencyMeetingKey = KeyCode.F7;
        #endregion

        #region Private Fields
        private bool _showDebugMenu = false;
        private Rect _debugMenuRect = new Rect(10, 10, 300, 500);
        #endregion

        #region Unity Lifecycle
        protected override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(gameObject);
        }

        private void Update()
        {
#if UNITY_EDITOR
            if (!enableDebugMode) return;

            // 디버그 메뉴 토글
            if (IsKeyDown(debugMenuKey))
            {
                _showDebugMenu = !_showDebugMenu;
            }

            // 단축키 체크
            if (IsKeyDown(forceWinCitizenKey))
            {
                Debug_ForceWin(0);
            }

            if (IsKeyDown(forceWinMafiaKey))
            {
                Debug_ForceWin(1);
            }

            if (IsKeyDown(startRoundKey))
            {
                Debug_StartRound();
            }

            if (IsKeyDown(endRoundKey))
            {
                Debug_EndRound();
            }

            if (IsKeyDown(startBodyReportMeetingKey))
            {
                Debug_StartBodyReportMeeting();
            }

            if (IsKeyDown(startEmergencyMeetingKey))
            {
                Debug_StartEmergencyMeeting();
            }
#endif
        }

        /// <summary>
        /// 키 입력 체크 (새 Input System과 레거시 모두 지원)
        /// </summary>
        private bool IsKeyDown(KeyCode keyCode)
        {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
            var kb = Keyboard.current;
            if (kb == null) return false;

            // KeyCode를 Key로 변환
            var key = ConvertKeyCodeToKey(keyCode);
            if (key == Key.None) return false;

            return kb[key].wasPressedThisFrame;
#else
            return Input.GetKeyDown(keyCode);
#endif
        }

#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        /// <summary>
        /// KeyCode를 새 Input System의 Key로 변환
        /// </summary>
        private Key ConvertKeyCodeToKey(KeyCode keyCode)
        {
            return keyCode switch
            {
                KeyCode.F1 => Key.F1,
                KeyCode.F2 => Key.F2,
                KeyCode.F3 => Key.F3,
                KeyCode.F4 => Key.F4,
                KeyCode.F5 => Key.F5,
                KeyCode.F6 => Key.F6,
                KeyCode.F7 => Key.F7,
                KeyCode.F8 => Key.F8,
                KeyCode.F9 => Key.F9,
                KeyCode.F10 => Key.F10,
                KeyCode.F11 => Key.F11,
                KeyCode.F12 => Key.F12,
                _ => Key.None
            };
        }
#endif

        private void OnGUI()
        {
#if UNITY_EDITOR
            if (!enableDebugMode || !_showDebugMenu) return;

            _debugMenuRect = GUI.Window(0, _debugMenuRect, DrawDebugMenu, "Debug Menu");
#endif
        }
        #endregion

        #region Debug Menu GUI
        private void DrawDebugMenu(int windowID)
        {
            GUILayout.Label($"Host: {PhotonNetwork.IsMasterClient}");
            GUILayout.Label($"Actor ID: {PhotonNetwork.LocalPlayer?.ActorNumber ?? -1}");
            GUILayout.Space(10);

            // === Victory ===
            GUILayout.Label("=== Victory ===");
            if (GUILayout.Button("Force Citizen Win (F2)"))
            {
                Debug_ForceWin(0);
            }
            if (GUILayout.Button("Force Mafia Win (F3)"))
            {
                Debug_ForceWin(1);
            }
            GUILayout.Space(10);

            // === Round ===
            GUILayout.Label("=== Round ===");
            if (GUILayout.Button("Start Round (F4)"))
            {
                Debug_StartRound();
            }
            if (GUILayout.Button("End Round (F5)"))
            {
                Debug_EndRound();
            }
            GUILayout.Space(10);

            // === Meeting ===
            GUILayout.Label("=== Meeting ===");
            if (GUILayout.Button("Body Report Meeting (F6)"))
            {
                Debug_StartBodyReportMeeting();
            }
            if (GUILayout.Button("Emergency Meeting (F7)"))
            {
                Debug_StartEmergencyMeeting();
            }
            GUILayout.Space(10);

            // === Mission ===
            GUILayout.Label("=== Mission ===");
            if (MissionManager.Instance != null)
            {
                float progress = MissionManager.Instance.CalculateMissionProgress();
                GUILayout.Label($"Mission Progress: {(progress * 100):F1}%");
            }

            if (GUILayout.Button("Print Mission Status"))
            {
                MissionManager.Instance?.DebugPrintMissions();
            }
            GUILayout.Space(10);

            // === Game State ===
            GUILayout.Label("=== Game State ===");
            if (GameManager.Instance != null)
            {
                var currentState = GameManager.Instance.StateMachine?.CurrentState;
                GUILayout.Label($"Current State: {currentState?.GetType().Name ?? "None"}");
            }

            GUI.DragWindow();
        }
        #endregion

        #region Debug Functions
        /// <summary>
        /// [Debug] 강제 승리
        /// </summary>
        private void Debug_ForceWin(int team)
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                Debug.LogWarning("[DebugManager] Debug command is host-only.");
                return;
            }

            if (VictoryConditionManager.Instance != null)
            {
                VictoryConditionManager.Instance.DebugForceWin(team);
            }
        }

        /// <summary>
        /// [Debug] 라운드 시작
        /// </summary>
        private void Debug_StartRound()
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                Debug.LogWarning("[DebugManager] Debug command is host-only.");
                return;
            }

            if (RoundManager.Instance != null)
            {
                RoundManager.Instance.DebugStartRound();
            }
        }

        /// <summary>
        /// [Debug] 라운드 종료
        /// </summary>
        private void Debug_EndRound()
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                Debug.LogWarning("[DebugManager] Debug command is host-only.");
                return;
            }

            if (RoundManager.Instance != null)
            {
                RoundManager.Instance.DebugEndRound();
            }
        }

        /// <summary>
        /// [Debug] 시체 발견 미팅 시작
        /// </summary>
        private void Debug_StartBodyReportMeeting()
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                Debug.LogWarning("[DebugManager] Debug command is host-only.");
                return;
            }

            if (MeetingManager.Instance != null)
            {
                MeetingManager.Instance.DebugStartBodyReportMeeting();
            }
        }

        /// <summary>
        /// [Debug] 긴급 소집 미팅 시작
        /// </summary>
        private void Debug_StartEmergencyMeeting()
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                Debug.LogWarning("[DebugManager] Debug command is host-only.");
                return;
            }

            if (MeetingManager.Instance != null)
            {
                MeetingManager.Instance.DebugStartEmergencyMeeting();
            }
        }
        #endregion

        #region Public API
        /// <summary>
        /// 디버그 모드 활성화/비활성화
        /// </summary>
        public void SetDebugMode(bool enabled)
        {
            enableDebugMode = enabled;
            Debug.Log($"[DebugManager] Debug mode: {(enabled ? "Enabled" : "Disabled")}");
        }

        /// <summary>
        /// 디버그 모드 상태 반환
        /// </summary>
        public bool IsDebugModeEnabled() => enableDebugMode;
        #endregion
    }
}
