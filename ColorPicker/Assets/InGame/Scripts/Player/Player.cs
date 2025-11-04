using FunkyCode;
using Photon.Pun;
using TMPro;
using UnityEngine;

namespace ColorPicker.InGame
{
    /// <summary>
    /// 인게임 씬에서 사용되는 플레이어
    /// 라이트, 사망 처리 등 게임 로직 포함
    /// </summary>
    public sealed class Player : PlayerBase
    {
        [SerializeField] private Light2D playerLight;
        [SerializeField] private TMP_Text nicknameText;

        private DeathEvent _deathEvent;
        private PlayerCameraSetup _cameraSetup;
        private PlayerLightController _lightController;

        public DeathEvent DeathEvent => _deathEvent ??= GetComponent<DeathEvent>();
        public int OwnerActNum { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(gameObject);

            _cameraSetup = new PlayerCameraSetup(transform);
            _lightController = new PlayerLightController(playerLight);
        }

        protected override void PerformInitialization()
        {
            SetupCamera();
            EnableLight();
            CacheOwnerInfo();
            RegisterToManager();
            SetupNickname();
            LoadCustomizeColor();
        }

        private void SetupCamera()
        {
            _cameraSetup?.Setup();
        }

        private void EnableLight()
        {
            _lightController?.Enable();
        }

        private void CacheOwnerInfo()
        {
            OwnerActNum = PhotonNetwork.LocalPlayer?.ActorNumber ?? -1;
        }

        private void RegisterToManager()
        {
            if (PlayerManager.Instance == null)
            {
                Debug.LogError("[Player] PlayerManager 인스턴스를 찾을 수 없습니다.", this);
                return;
            }

            PlayerManager.Instance.SetMyPlayer(this);
        }

        private void SetupNickname()
        {
            if (nicknameText == null) return;

            // 플레이어 닉네임 설정
            int actorNum = photonView.Owner.ActorNumber;
            if (GameDataManager.Instance.TryGetPublicPlayerDataByActorId(actorNum, out var publicData))
            {
                nicknameText.text = publicData.nickname;
            }

            // 로컬 플레이어 관점에서 마피아 표시
            UpdateNicknameColor();
        }

        /// <summary>
        /// 로컬 플레이어가 마피아면 다른 마피아 플레이어들을 빨간색으로 표시
        /// </summary>
        public void UpdateNicknameColor()
        {
            if (nicknameText == null) return;

            // 로컬 플레이어의 직업 확인
            var myPlayer = PlayerManager.Instance?.GetMyPlayer();
            if (myPlayer == null) return;

            int myActorNum = myPlayer.OwnerActNum;
            if (!GameDataManager.Instance.TryGetPrivatePlayerDataByActorId(myActorNum, out var myPrivateData))
            {
                nicknameText.color = Color.white;
                return;
            }

            // 로컬 플레이어가 마피아가 아니면 모두 흰색
            if (myPrivateData.classType != (int)PlayerClassType.mafia)
            {
                nicknameText.color = Color.white;
                return;
            }

            // 로컬 플레이어가 마피아면, 이 플레이어도 마피아인지 확인
            int targetActorNum = photonView.Owner.ActorNumber;
            if (GameDataManager.Instance.TryGetPrivatePlayerDataByActorId(targetActorNum, out var targetPrivateData))
            {
                if (targetPrivateData.classType == (int)PlayerClassType.mafia)
                {
                    nicknameText.color = Color.red; // 마피아끼리는 빨간색
                }
                else
                {
                    nicknameText.color = Color.white;
                }
            }
            else
            {
                nicknameText.color = Color.white;
            }
        }

        private void LoadCustomizeColor()
        {
            if (CustomizeManager.Instance == null)
            {
                Debug.LogError("[Player] CustomizeManager instance not found. Cannot load color.", this);
                return;
            }

            if (photonView == null || photonView.Owner == null)
            {
                Debug.LogError("[Player] photonView or Owner is null. Cannot load color.", this);
                return;
            }

            int actorNumber = photonView.Owner.ActorNumber;
            if (actorNumber < 0)
            {
                Debug.LogError($"[Player] Invalid actor number: {actorNumber}", this);
                return;
            }

            Debug.Log($"[Player] Loading customize color for actor {actorNumber}...");

            // GameDataManager 기반으로 색상 로드
            int colorIndex = CustomizeManager.Instance.GetPlayerColorIndex(actorNumber);
            if (colorIndex >= 0)
            {
                Color color = CustomizeManager.Instance.GetColor(colorIndex);
                ApplyCustomizeColor(color);
                Debug.Log($"[Player] Successfully applied color index {colorIndex} (RGB: {color.r:F2}, {color.g:F2}, {color.b:F2}) for actor {actorNumber}");
            }
            else
            {
                Debug.LogWarning($"[Player] No color assigned for actor {actorNumber}. Color will not be applied.", this);

                // GameDataManager 상태 확인
                if (GameDataManager.Instance != null &&
                    GameDataManager.Instance.TryGetPublicPlayerDataByActorId(actorNumber, out var playerData))
                {
                    Debug.LogWarning($"[Player] PlayerData exists but customization data is missing for {playerData.nickname}");
                }
            }
        }

        /// <summary>
        /// 플레이어 사망 처리
        /// </summary>
        public void Die()
        {
            DeathEvent?.CallDeathEvent();

            // 호스트에게 가시성 업데이트 요청
            if (photonView.IsMine)
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    // 호스트면 직접 처리
                    UpdateAllPlayersVisibilityFromHost();
                }
                else
                {
                    // 클라이언트면 호스트에게 요청
                    photonView.RPC(nameof(RPC_RequestVisibilityUpdate), RpcTarget.MasterClient);
                }
            }
        }

        /// <summary>
        /// 생존 여부 확인
        /// </summary>
        public bool IsAlive()
        {
            if (!GameDataManager.Instance.TryGetInGameDataByActorId(OwnerActNum, out var data))
            {
                return true;
            }

            return data.isAlive;
        }

        /// <summary>
        /// [Host Only] 투표로 인한 사망 처리 (RPC로 전파)
        /// </summary>
        public void TriggerVoteDeath()
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                Debug.LogWarning("[Player] TriggerVoteDeath is host-only.");
                return;
            }

            photonView.RPC(nameof(RPC_TriggerVoteDeath), RpcTarget.All);
        }

        [PunRPC]
        private void RPC_TriggerVoteDeath()
        {
            Die();
        }

        /// <summary>
        /// [RPC] 클라이언트가 호스트에게 가시성 업데이트 요청
        /// </summary>
        [PunRPC]
        private void RPC_RequestVisibilityUpdate()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                UpdateAllPlayersVisibilityFromHost();
            }
        }

        /// <summary>
        /// [Host Only] GameDataManager에서 모든 플레이어 상태를 조회하여 가시성 업데이트
        /// </summary>
        private void UpdateAllPlayersVisibilityFromHost()
        {
            if (!PhotonNetwork.IsMasterClient) return;
            if (GameDataManager.Instance == null) return;

            // 모든 플레이어 상태 조회
            var allPlayerData = GameDataManager.Instance.GetAllPublicPlayerData();
            if (allPlayerData == null || allPlayerData.Count == 0) return;

            // 각 플레이어의 생존 상태를 ActorNumber로 매핑
            var playerStates = new System.Collections.Generic.Dictionary<int, bool>(); // ActorNumber -> isAlive

            foreach (var playerData in allPlayerData)
            {
                if (playerData == null) continue;

                var inGameData = GameDataManager.Instance.GetInGameData(playerData.googleUID);
                if (inGameData != null)
                {
                    playerStates[playerData.currentActorId] = inGameData.isAlive;
                }
            }

            // 각 플레이어에게 개별적으로 가시성 정보 전송
            foreach (var kvp in playerStates)
            {
                int viewerActorNum = kvp.Key;
                bool isViewerAlive = kvp.Value;

                // 해당 플레이어가 볼 수 있는 ActorNumber 목록 생성
                var visibleActors = new System.Collections.Generic.List<int>();

                foreach (var otherKvp in playerStates)
                {
                    int targetActorNum = otherKvp.Key;
                    bool isTargetAlive = otherKvp.Value;

                    // 자기 자신은 제외
                    if (targetActorNum == viewerActorNum) continue;

                    // 가시성 규칙:
                    // 1. 내가 죽었으면: 모든 플레이어 보임
                    // 2. 내가 살았으면: 살아있는 플레이어만 보임
                    if (!isViewerAlive)
                    {
                        // 죽었으면 모두 보임
                        visibleActors.Add(targetActorNum);
                    }
                    else if (isTargetAlive)
                    {
                        // 살았고 상대도 살았으면 보임
                        visibleActors.Add(targetActorNum);
                    }
                    // 살았고 상대가 죽었으면 안보임 (추가하지 않음)
                }

                // 해당 플레이어에게 가시성 정보 전송
                var targetPlayer = PhotonNetwork.CurrentRoom.GetPlayer(viewerActorNum);
                if (targetPlayer != null)
                {
                    photonView.RPC(nameof(RPC_UpdatePlayerVisibility), targetPlayer, visibleActors.ToArray());
                }
            }
        }

        /// <summary>
        /// [RPC] 클라이언트가 자신이 볼 수 있는 플레이어 목록을 받아 가시성 설정
        /// </summary>
        [PunRPC]
        private void RPC_UpdatePlayerVisibility(int[] visibleActorNumbers)
        {
            var allPlayers = FindObjectsOfType<Player>();
            var visibleSet = new System.Collections.Generic.HashSet<int>(visibleActorNumbers);

            foreach (var player in allPlayers)
            {
                if (player.photonView.IsMine) continue; // 자기 자신은 항상 보임

                int actorNum = player.photonView.Owner.ActorNumber;

                // visibleSet에 있으면 보이게, 없으면 안보이게
                player.gameObject.SetActive(visibleSet.Contains(actorNum));
            }
        }
    }
}