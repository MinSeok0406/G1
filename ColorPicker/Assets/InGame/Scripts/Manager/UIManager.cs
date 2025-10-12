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
        [SerializeField] private GameObject timeControlPopup;
        [SerializeField] private Button addTimeButton;
        [SerializeField] private Button reduceTimeButton;
        [SerializeField] private TMP_Text timeControlMessage;

        [Header("Meeting UI")]
        [SerializeField] private GameObject meetingOBJ;
        [SerializeField] private TMP_Text meetingTimerText;
        [SerializeField] private GameObject pickerUI;
        [SerializeField] private List<ColorPickButton> colorPickButtons;

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
            CloseTimeControlPopup();
        }
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

            // 기존 프로필 제거
            meetingUI?.ClearAllProfilesSafe();
            deductionUI?.ClearAllProfilesSafe();

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
            meetingUI?.CreatePlayerCard(actorNum, nickname, isAlive);
            deductionUI?.CreatePlayerCard(actorNum, nickname, isAlive);
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
            // 이미 투표했는지 확인
            if (_hasVoted)
            {
                ShowToastToScreen(MSG_ALREADY_VOTED);
                return;
            }

            // 시간 조정을 사용했는지 확인
            if (GameManager.Instance != null && !GameManager.Instance.CanModifyTime())
            {
                ShowToastToScreen("시간 조정 후에는 투표할 수 없습니다.");
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
        #endregion

        #region Time Control UI
        /// <summary>
        /// 시간 조정 팝업 표시
        /// </summary>
        public void ShowTimeControlPopup()
        {
            if (GameManager.Instance == null) return;

            // 이미 투표했다면 시간 조정 불가
            if (_hasVoted)
            {
                ShowToastToScreen(MSG_VOTE_AFTER_TIME_CONTROL);
                return;
            }

            // 이미 시간 조정을 사용했다면 불가
            if (!GameManager.Instance.CanModifyTime())
            {
                ShowToastToScreen(MSG_TIME_ALREADY_MODIFIED);
                return;
            }

            if (timeControlPopup != null)
            {
                timeControlPopup.SetActive(true);
            }

            UpdateTimeControlButtons();
        }

        /// <summary>
        /// 시간 조정 팝업 닫기
        /// </summary>
        public void CloseTimeControlPopup()
        {
            if (timeControlPopup != null)
            {
                timeControlPopup.SetActive(false);
            }
        }

        /// <summary>
        /// 시간 조정 버튼 상태 업데이트
        /// </summary>
        private void UpdateTimeControlButtons()
        {
            if (GameManager.Instance == null) return;

            // 시간 추가 버튼 (항상 가능)
            if (addTimeButton != null)
            {
                addTimeButton.interactable = GameManager.Instance.CanModifyTime();
            }

            // 시간 감소 버튼 (10초 초과일 때만 가능)
            if (reduceTimeButton != null)
            {
                reduceTimeButton.interactable = GameManager.Instance.CanReduceTime();
            }

            // 안내 메시지
            if (timeControlMessage != null)
            {
                if (!GameManager.Instance.CanModifyTime())
                {
                    timeControlMessage.text = MSG_TIME_ALREADY_MODIFIED;
                }
                else if (!GameManager.Instance.CanReduceTime())
                {
                    timeControlMessage.text = "시간 증가만 가능합니다.\n(감소는 10초 초과 시 가능)";
                }
                else
                {
                    timeControlMessage.text = "시간을 증가 또는 감소하시겠습니까?";
                }
            }
        }

        /// <summary>
        /// 시간 추가 버튼 클릭
        /// </summary>
        public void OnClick_AddTime()
        {
            if (GameManager.Instance == null) return;

            if (!GameManager.Instance.CanModifyTime())
            {
                ShowToastToScreen(MSG_TIME_ALREADY_MODIFIED);
                CloseTimeControlPopup();
                return;
            }

            GameManager.Instance.RequestAddMeetingTime();
            CloseTimeControlPopup();
            ShowToastToScreen("시간이 추가되었습니다.");
        }

        /// <summary>
        /// 시간 감소 버튼 클릭
        /// </summary>
        public void OnClick_ReduceTime()
        {
            if (GameManager.Instance == null) return;

            if (!GameManager.Instance.CanModifyTime())
            {
                ShowToastToScreen(MSG_TIME_ALREADY_MODIFIED);
                CloseTimeControlPopup();
                return;
            }

            if (!GameManager.Instance.CanReduceTime())
            {
                ShowToastToScreen(MSG_CANNOT_REDUCE_TIME);
                CloseTimeControlPopup();
                return;
            }

            GameManager.Instance.RequestReduceMeetingTime();
            CloseTimeControlPopup();
            ShowToastToScreen("시간이 감소되었습니다.");
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
    }
}