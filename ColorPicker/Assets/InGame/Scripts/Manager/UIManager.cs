using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Photon.Pun;

namespace ColorPicker.InGame
{
    public class UIManager : SingletonNetworkBehaviour<UIManager>
    {
        #region Serialized Fields
        [Header("Interaction")]
        [SerializeField] private Button interactButton;

        [Header("Info UI")]
        [SerializeField] private TMP_Text coinInfoText;
        [SerializeField] private TMP_Text paintInfoText;
        [SerializeField] private Image colorIcon;
        [SerializeField] private Slider missionStatusBar;

        [Header("Toast Messages")]
        [SerializeField] private TMP_Text messageText;
        [SerializeField] private TMP_Text screenMessageText;
        [SerializeField] private Image screenMessagePanel;

        [Header("Vote UI")]
        [SerializeField] private GameObject votePopup;
        [SerializeField] private TMP_Text voteMessage;

        [Header("Time Control UI")]
        [SerializeField] private GameObject meetingTimerPopup;

        [Header("Meeting UI")]
        [SerializeField] private GameObject meetingOBJ;
        [SerializeField] private TMP_Text meetingTimerText;
        [SerializeField] private GameObject pickerUI;
        [SerializeField] private List<ColorPickButton> colorPickButtons;

        [Header("Picker Player Display")]
        [SerializeField] private Image[] pickerPlayerImages; // 피커 UI에서 플레이어 색상 표시용
        [SerializeField] private TMP_Text pickerPlayerNameText; // 피커 UI에서 플레이어 이름 표시용

        [Header("UI Scripts")]
        [SerializeField] private MeetingUI meetingUI;
        [SerializeField] private DeductionUI deductionUI;
        [SerializeField] private LoadingScreenUI loadingScreenUI;

        [Header("Animation Settings")]
        [SerializeField] private float fadeDuration = 0.5f;
        [SerializeField] private float showDuration = 2.0f;
        #endregion

        #region Constants
        private const string MSG_ALREADY_VOTED = "이미 투표를 완료했습니다.";
        private const string MSG_TIME_ALREADY_MODIFIED = "시간 조정은 한 번만 가능합니다.";
        private const string MSG_CANNOT_REDUCE_TIME = "남은 시간이 10초 이하일 때는 시간을 줄일 수 없습니다.";
        private const string MSG_VOTE_AFTER_TIME_CONTROL = "시간 조정 후에는 투표할 수 없습니다.";
        #endregion

        #region Private Fields
        private UnityAction _defaultInteractionClick;
        private Coroutine _currentToastRoutine;
        private Coroutine _currentScreenToastRoutine;
        private Coroutine _meetingTimerCoroutine;

        private int _currentVoteActorNum = -1;
        private PlayerCardUI _currentPlayerCard;
        private bool _hasVoted = false;

        private MafiaAbility MafiaAbility => AbilityManager.Instance?.GetComponentInChildren<MafiaAbility>();
        #endregion

        #region Unity Lifecycle
        protected override void Awake()
        {
            base.Awake();

            ValidateReferences();
            InitializeComponents();
        }
        #endregion

        #region Initialization
        private void ValidateReferences()
        {
            if (interactButton == null)
            {
                Debug.LogError("[UIManager] interactButton이 할당되지 않았습니다.");
            }

            if (meetingTimerText == null)
            {
                Debug.LogWarning("[UIManager] meetingTimerText가 할당되지 않았습니다.");
            }
        }

        private void InitializeComponents()
        {
            // 상호작용 버튼 초기화
            if (interactButton != null)
            {
                _defaultInteractionClick = OnClick_Interaction;
                WireDefaultClick();
                interactButton.interactable = false;
            }

            // UI 초기화
            UpdateMissionStatusBarUI(0f);
            UpdateCoinInfo(0);
            UpdatePaintInfo(0);

            if (messageText != null)
            {
                messageText.alpha = 0f;
            }

            // 투표 상태 초기화
            ResetVoteState();
        }

        /// <summary>
        /// 투표 상태 초기화
        /// </summary>
        public void ResetVoteState()
        {
            _hasVoted = false;
            _currentVoteActorNum = -1;
            CloseVotePopup();

            // 모든 투표 표시 초기화
            meetingUI?.ClearAllVotes();
        }

        public void BroadCastResetVoteState()
        {
            photonView.RPC(nameof(RPC_ResetVoteState), RpcTarget.All);
        }
        
        [PunRPC]
        public void RPC_ResetVoteState() => ResetVoteState();
        #endregion

        #region Interaction Button
        /// <summary>
        /// 상호작용 버튼 표시/숨김
        /// </summary>
        public void ShowInteractionButton(bool show, UnityAction onClick = null)
        {
            if (interactButton == null) return;

            if (show)
            {
                interactButton.onClick.RemoveAllListeners();
                interactButton.onClick.AddListener(onClick ?? _defaultInteractionClick);
                interactButton.interactable = true;
            }
            else
            {
                interactButton.interactable = false;
                WireDefaultClick();
            }
        }

        private void WireDefaultClick()
        {
            if (interactButton == null) return;

            interactButton.onClick.RemoveAllListeners();
            interactButton.onClick.AddListener(_defaultInteractionClick);
        }

        private void OnClick_Interaction()
        {
            var myPlayer = PlayerManager.Instance?.GetMyPlayer();
            if (myPlayer == null)
            {
                Debug.LogWarning("[UIManager] 로컬 플레이어를 찾을 수 없습니다.");
                return;
            }

            var pc = myPlayer.GetComponent<PlayerControl>();
            if (pc == null)
            {
                Debug.LogWarning("[UIManager] PlayerControl 컴포넌트가 없습니다.");
                return;
            }

            pc.TryInteract();
        }
        #endregion

        #region Info UI Updates
        public void UpdateMissionStatusBarUI(float percent)
        {
            if (missionStatusBar != null)
            {
                missionStatusBar.value = Mathf.Clamp01(percent);
            }
        }

        public void UpdateCoinInfo(int coin)
        {
            if (coinInfoText != null)
            {
                coinInfoText.text = $"x {coin}";
            }
        }

        public void UpdatePaintInfo(int paint)
        {
            if (paintInfoText != null)
            {
                paintInfoText.text = $"x {paint}";
            }
        }

        public void BroadcastUpdateColorIcon()
        {
            photonView.RPC(nameof(RPC_UpdateColorIcon), RpcTarget.All);
        }

        [PunRPC]
        private void RPC_UpdateColorIcon()
        {
            if (!GameDataManager.Instance.TryGetPrivatePlayerDataByActorId(
                PhotonNetwork.LocalPlayer.ActorNumber, out var data))
            {
                return;
            }

            if (colorIcon != null)
            {
                colorIcon.color = HelperUtilities.ToUnityColor((ColorType)data.identityColorId);
            }
        }
        #endregion

        #region Ability UI
        public void UpdateAbilityCooldownUI(int viewID, float remain)
        {
            PhotonView targetView = PhotonView.Find(viewID);
            if (targetView == null || targetView.Owner == null)
            {
                Debug.LogWarning($"[Ability] PhotonView {viewID}를 찾을 수 없거나 소유자가 없습니다.");
                return;
            }

            var targetPlayer = PhotonNetwork.CurrentRoom.GetPlayer(targetView.OwnerActorNr);
            photonView.RPC(nameof(RPC_UpdateAbilityCooldownUI), targetPlayer, remain);
        }

        [PunRPC]
        private void RPC_UpdateAbilityCooldownUI(float remain)
        {
            MafiaAbility?.StartCooldownUI(remain);
        }
        #endregion

        #region Toast Messages
        /// <summary>
        /// 월드 좌표에 토스트 메시지 표시
        /// </summary>
        public void ShowToast(string msg, Vector3 targetPos)
        {
            if (string.IsNullOrEmpty(msg)) return;

            if (_currentToastRoutine != null)
            {
                StopCoroutine(_currentToastRoutine);
            }

            _currentToastRoutine = StartCoroutine(Co_ShowToast(msg, targetPos));
        }

        /// <summary>
        /// 화면 중앙에 토스트 메시지 표시
        /// </summary>
        public void ShowToastToScreen(string msg)
        {
            if (string.IsNullOrEmpty(msg)) return;

            if (_currentScreenToastRoutine != null)
            {
                StopCoroutine(_currentScreenToastRoutine);
            }

            _currentScreenToastRoutine = StartCoroutine(Co_ScreenShowToast(msg));
        }

        private IEnumerator Co_ShowToast(string msg, Vector3 targetPos)
        {
            if (messageText == null) yield break;

            messageText.text = msg;
            messageText.transform.position = targetPos;

            yield return FadeText(messageText, 0f, 1f, fadeDuration);
            yield return new WaitForSeconds(showDuration);
            yield return FadeText(messageText, 1f, 0f, fadeDuration);

            _currentToastRoutine = null;
        }

        private IEnumerator Co_ScreenShowToast(string msg)
        {
            if (screenMessageText == null) yield break;

            screenMessageText.text = msg;

            yield return FadeScreenMessage(0f, 1f, fadeDuration);
            yield return new WaitForSeconds(showDuration);
            yield return FadeScreenMessage(1f, 0f, fadeDuration);

            _currentScreenToastRoutine = null;
        }

        private IEnumerator FadeText(TMP_Text text, float from, float to, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(from, to, elapsed / duration);
                
                Color c = text.color;
                c.a = alpha;
                text.color = c;

                yield return null;
            }

            Color finalColor = text.color;
            finalColor.a = to;
            text.color = finalColor;
        }

        private IEnumerator FadeScreenMessage(float from, float to, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(from, to, elapsed / duration);
                SetScreenMessageAlpha(alpha);
                yield return null;
            }
            SetScreenMessageAlpha(to);
        }

        private void SetScreenMessageAlpha(float alpha)
        {
            if (screenMessageText != null)
            {
                Color textColor = screenMessageText.color;
                textColor.a = alpha;
                screenMessageText.color = textColor;
            }

            if (screenMessagePanel != null)
            {
                Color panelColor = screenMessagePanel.color;
                panelColor.a = alpha;
                screenMessagePanel.color = panelColor;
            }
        }
        #endregion

        #region Player Profile
        /// <summary>
        /// [마스터 전용] 플레이어 프로필 초기화
        /// </summary>
        public void InitializePlayerProfile()
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                Debug.LogWarning("[UIManager] 플레이어 프로필 초기화는 마스터 클라이언트만 가능합니다.");
                return;
            }

            // 모든 클라이언트에서 기존 프로필 제거
            photonView.RPC(nameof(RPC_ClearAllProfiles), RpcTarget.All);

            // 모든 플레이어 데이터 가져오기
            var playerDataList = GameDataManager.Instance.GetAllPublicPlayerData();
            
            foreach (var data in playerDataList)
            {
                if (data == null) continue;

                if (!GameDataManager.Instance.TryGetInGameDataByActorId(data.currentActorId, out var inGameData))
                {
                    Debug.LogWarning($"[UIManager] Actor {data.currentActorId}의 InGameData를 찾을 수 없습니다.");
                    continue;
                }

                string nickname = string.IsNullOrWhiteSpace(data.nickname) ? "Player" : data.nickname.Trim();

                photonView.RPC(nameof(RPC_CreatePlayerCard), RpcTarget.All, 
                    data.currentActorId, nickname, inGameData.isAlive);
            }
        }

        [PunRPC]
        private void RPC_CreatePlayerCard(int actorNum, string nickname, bool isAlive)
        {
            // 기존 카드가 있으면 제거 후 재생성
            meetingUI?.CreatePlayerCard(actorNum, nickname, isAlive);
            deductionUI?.CreatePlayerCard(actorNum, nickname, isAlive);
        }

        /// <summary>
        /// 클라이언트에서 기존 프로필 제거 (RPC로 호출됨)
        /// </summary>
        [PunRPC]
        private void RPC_ClearAllProfiles()
        {
            meetingUI?.ClearAllProfilesSafe();
            deductionUI?.ClearAllProfilesSafe();
        }

        /// <summary>
        /// 특정 클라이언트에게 프로필 동기화 (마피아 킬 후 즉시 업데이트)
        /// </summary>
        [PunRPC]
        private void RPC_SyncProfileToClient()
        {
            // 기존 프로필 제거
            meetingUI?.ClearAllProfilesSafe();
            deductionUI?.ClearAllProfilesSafe();

            // 최신 데이터로 프로필 재생성
            var playerDataList = GameDataManager.Instance?.GetAllPublicPlayerData();
            if (playerDataList == null) return;

            foreach (var data in playerDataList)
            {
                if (data == null) continue;

                if (!GameDataManager.Instance.TryGetInGameDataByActorId(data.currentActorId, out var inGameData))
                {
                    continue;
                }

                string nickname = string.IsNullOrWhiteSpace(data.nickname) ? "Player" : data.nickname.Trim();

                meetingUI?.CreatePlayerCard(data.currentActorId, nickname, inGameData.isAlive);
                deductionUI?.CreatePlayerCard(data.currentActorId, nickname, inGameData.isAlive);
            }
        }
        #endregion

        #region Deduction UI
        public PlayerCardUI GetPlayerCard()
        {
            return _currentPlayerCard;
        }

        public void ShowDeductionPickerUI(PlayerCardUI playerCard)
        {
            _currentPlayerCard = playerCard;

            if (pickerUI != null)
            {
                pickerUI.SetActive(true);
            }

            // 플레이어 커스터마이즈 정보를 피커 UI에 적용
            if (playerCard != null)
            {
                int actorNum = playerCard.GetActorNum();
                ApplyPlayerCustomizationToUI(actorNum, pickerPlayerImages, pickerPlayerNameText);
            }
        }

        public void InitializeButtonIcon()
        {
            foreach (var button in colorPickButtons)
            {
                button?.DisableIcon();
            }
        }

        public void SetDeductionColor(ColorType color)
        {
            _currentPlayerCard?.SetDeductionColor(color);
        }

        public void DisableDeductionPickerUI()
        {
            _currentPlayerCard = null;
            
            if (pickerUI != null)
            {
                pickerUI.SetActive(false);
            }
        }
        #endregion

        #region Vote UI
        /// <summary>
        /// 투표 팝업 표시
        /// </summary>
        public void ShowVotePopup(int actorNum, string nickname)
        {
            // 죽은 플레이어는 투표할 수 없음
            int myActorNum = PhotonNetwork.LocalPlayer?.ActorNumber ?? -1;
            if (myActorNum > 0 && GameDataManager.Instance.TryGetInGameDataByActorId(myActorNum, out var myData))
            {
                if (!myData.isAlive)
                {
                    ShowToastToScreen("사망한 플레이어는 투표할 수 없습니다.");
                    return;
                }
            }

            // 이미 투표했는지 확인
            if (_hasVoted)
            {
                ShowToastToScreen(MSG_ALREADY_VOTED);
                return;
            }

            if (votePopup != null)
            {
                votePopup.SetActive(true);
            }

            if (voteMessage != null)
            {
                voteMessage.text = $"{nickname}님을 투표하시겠습니까?";
            }

            _currentVoteActorNum = actorNum;
        }

        /// <summary>
        /// 투표 팝업 닫기
        /// </summary>
        public void CloseVotePopup()
        {
            if (votePopup != null)
            {
                votePopup.SetActive(false);
            }

            _currentVoteActorNum = -1;
        }

        /// <summary>
        /// 투표 실행 (버튼 클릭)
        /// </summary>
        public void OnClick_Vote()
        {
            if (_currentVoteActorNum == -1)
            {
                Debug.LogWarning("[UIManager] 투표 대상이 선택되지 않았습니다.");
                return;
            }

            if (_hasVoted)
            {
                ShowToastToScreen(MSG_ALREADY_VOTED);
                CloseVotePopup();
                return;
            }

            // 투표 요청
            GameManager.Instance?.RequestVote(_currentVoteActorNum);
            
            // 투표 완료 상태로 변경
            _hasVoted = true;
            
            CloseVotePopup();
            ShowToastToScreen("투표가 완료되었습니다.");
        }

        /// <summary>
        /// 투표 여부 확인
        /// </summary>
        public bool HasVoted()
        {
            return _hasVoted;
        }

        public void OnClick_OpenMeetingTimerPopup()
        {
            // 1) 이미 투표 완료 → 팝업 열지 않고 메시지
            if (HasVoted())
            {
                ShowToastToScreen(MSG_ALREADY_VOTED);
                return;
            }

            // 2) 이미 시간 조정 1회 사용됨 → 팝업 열지 않고 메시지
            if (!GameManager.Instance._meetingTimer.CanModifyTime())
            {
                ShowToastToScreen(MSG_TIME_ALREADY_MODIFIED);
                return;
            }

            // 3) 조건 통과 → 팝업 오픈
            meetingTimerPopup?.SetActive(true);
        }
        #endregion



        #region Meeting UI
        /// <summary>
        /// [마스터 전용] 역할 배정 화면 표시
        /// </summary>
        public void BroadcastInitializeAssignSceneUI()
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                Debug.LogWarning("[UIManager] 역할 배정은 마스터 클라이언트만 가능합니다.");
                return;
            }

            photonView.RPC(nameof(RPC_InitializeAssignSceneUI), RpcTarget.All);
        }

        [PunRPC]
        private void RPC_InitializeAssignSceneUI()
        {
            int actorId = PhotonNetwork.LocalPlayer?.ActorNumber ?? -1;
            if (actorId < 0) return;

            if (!GameDataManager.Instance.TryGetPrivatePlayerDataByActorId(actorId, out var pdata))
            {
                Debug.LogWarning($"[UIManager] Actor {actorId}의 PrivateData를 찾을 수 없습니다.");
                return;
            }

            loadingScreenUI?.ShowAssignScene(pdata.classType);
        }

        /// <summary>
        /// 미팅 타이머 UI 시작
        /// </summary>
        public void StartMeetingTimerUI(float duration)
        {
            if (float.IsNaN(duration) || float.IsInfinity(duration) || duration <= 0f)
            {
                SetMeetingTimerText(0f);
                return;
            }

            if (_meetingTimerCoroutine != null)
            {
                StopCoroutine(_meetingTimerCoroutine);
            }

            _meetingTimerCoroutine = StartCoroutine(Co_MeetingTimer(duration));
        }

        /// <summary>
        /// 미팅 타이머 UI 정지
        /// </summary>
        public void StopMeetingTimerUI()
        {
            if (_meetingTimerCoroutine != null)
            {
                StopCoroutine(_meetingTimerCoroutine);
                _meetingTimerCoroutine = null;
            }

            SetMeetingTimerText(0f);
        }

        private IEnumerator Co_MeetingTimer(float duration)
        {
            float remaining = duration;

            while (remaining > 0f)
            {
                remaining -= Time.deltaTime;
                SetMeetingTimerText(remaining);
                yield return null;
            }

            SetMeetingTimerText(0f);
            _meetingTimerCoroutine = null;
        }

        private void SetMeetingTimerText(float time)
        {
            if (meetingTimerText == null) return;

            time = Mathf.Max(0f, time);
            int minutes = (int)(time / 60f);
            int seconds = (int)(time % 60f);

            meetingTimerText.text = $"{minutes:00}:{seconds:00}";

            // 10초 이하일 때 빨간색으로 표시
            meetingTimerText.color = (minutes == 0 && seconds <= 10) ? Color.red : Color.white;
        }

        /// <summary>
        /// 미팅 UI 표시/숨김
        /// </summary>
        public void ShowMeetingUI(bool isActive)
        {
            if (meetingOBJ != null)
            {
                meetingOBJ.SetActive(isActive);
            }

            // 미팅 시작 시 투표 상태 초기화
            if (isActive)
            {
                ResetVoteState();
            }
        }

        /// <summary>
        /// 특정 플레이어의 득표수 업데이트 (RPC로 호출됨)
        /// </summary>
        public void UpdateVoteDisplay(int actorNumber, int voteCount)
        {
            meetingUI?.UpdatePlayerVoteCount(actorNumber, voteCount);
        }
        #endregion

        #region Public Utility
        /// <summary>
        /// 일반 메시지 표시 (외부에서 호출 가능)
        /// </summary>
        public void ShowMessage(string message)
        {
            ShowToastToScreen(message);
        }
        #endregion

        #region Player Customization Display
        /// <summary>
        /// 플레이어의 커스터마이즈 색상과 이름을 UI에 적용
        /// </summary>
        /// <param name="actorNumber">플레이어 액터 번호</param>
        /// <param name="images">색상을 적용할 UI 이미지 배열</param>
        /// <param name="nameText">이름을 표시할 TMP_Text</param>
        public void ApplyPlayerCustomizationToUI(int actorNumber, Image[] images, TMP_Text nameText)
        {
            if (actorNumber < 0)
            {
                Debug.LogWarning($"[UIManager] Invalid actor number: {actorNumber}");
                return;
            }

            // 1. 플레이어 닉네임 가져오기
            string nickname = "Unknown";
            if (GameDataManager.Instance != null &&
                GameDataManager.Instance.TryGetPublicPlayerDataByActorId(actorNumber, out var playerData))
            {
                nickname = string.IsNullOrWhiteSpace(playerData.nickname)
                    ? $"Player_{actorNumber}"
                    : playerData.nickname;
            }

            // 2. 이름 텍스트에 적용
            if (nameText != null)
            {
                nameText.text = nickname;
                Debug.Log($"[UIManager] Set player name: {nickname} for actor {actorNumber}");
            }

            // 3. 커스터마이즈 색상 가져오기
            if (CustomizeManager.Instance == null)
            {
                Debug.LogWarning("[UIManager] CustomizeManager instance not found. Cannot apply color.");
                return;
            }

            int colorIndex = CustomizeManager.Instance.GetPlayerColorIndex(actorNumber);
            if (colorIndex < 0)
            {
                Debug.LogWarning($"[UIManager] No color assigned for actor {actorNumber}");
                return;
            }

            Color color = CustomizeManager.Instance.GetColor(colorIndex);

            // 4. UI 이미지 배열에 색상 적용
            if (images == null || images.Length == 0)
            {
                Debug.LogWarning("[UIManager] Image array is null or empty. Cannot apply color.");
                return;
            }

            int appliedCount = 0;
            foreach (var image in images)
            {
                if (image != null)
                {
                    image.color = color;
                    appliedCount++;
                }
            }

            Debug.Log($"[UIManager] Applied color index {colorIndex} (RGB: {color.r:F2}, {color.g:F2}, {color.b:F2}) " +
                     $"to {appliedCount} UI image(s) for player {nickname} (actor {actorNumber})");
        }
        #endregion
    }
}