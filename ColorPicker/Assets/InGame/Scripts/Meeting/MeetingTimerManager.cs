using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

namespace ColorPicker.InGame
{
    /// <summary>
    /// 미팅 타이머 관리 (검증/상태/정책) + GameManager RPC 파사드 호출
    /// </summary>
    public sealed class MeetingTimerManager
    {
        #region Constants
        private const float ALL_VOTED_CAP_SECONDS   = 3f;
        private const float MIN_TIME_FOR_REDUCTION  = 10f;
        private const float MIN_TIME_AFTER_REDUCTION = 10f;
        #endregion

        #region Private Fields
        private readonly GameManager _gameManager;
        private readonly HashSet<int> _addTimeRequesters = new();
        private bool _hasUsedTimeModification;
        private bool _allVotedCapped;
        #endregion

        public MeetingTimerManager(GameManager gameManager)
        {
            _gameManager = gameManager;
        }

        #region Public API
        /// <summary>[Host Only] 미팅 타이머 설정</summary>
        public void SetMeetingTimer(float time)
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                Debug.LogWarning("[MeetingTimer] SetMeetingTimer can only be called by master.");
                return;
            }
            if (!IsValidTime(time))
            {
                Debug.LogWarning($"[MeetingTimer] Invalid meeting time: {time}");
                return;
            }
            _gameManager.meetingTimer = time;
        }

        /// <summary>시간 추가 요청(클라→호스트)</summary>
        public void RequestAddTime(float time)
        {
            if (_hasUsedTimeModification)
            {
                ShowTimeModificationLimitMessage();
                return;
            }
            _gameManager.RpcRequestAddMeetingTime(time); // ← RPC 파사드 사용
        }

        /// <summary>시간 감소 요청(클라→호스트)</summary>
        public void RequestReduceTime(float time)
        {
            if (_hasUsedTimeModification)
            {
                ShowTimeModificationLimitMessage();
                return;
            }
            if (_gameManager.meetingTimer <= MIN_TIME_FOR_REDUCTION)
            {
                ShowCannotReduceMessage();
                return;
            }
            _gameManager.RpcRequestReduceMeetingTime(time); // ← RPC 파사드 사용
        }

        /// <summary>[Host Only] 타이머 브로드캐스트</summary>
        public void BroadcastTimer()
        {
            if (!PhotonNetwork.IsMasterClient) return;
            _gameManager.RpcBroadcastMeetingTimer(); // ← RPC 파사드 사용
        }

        public void ResetRequests()
        {
            _addTimeRequesters.Clear();
            _hasUsedTimeModification = false;
            _allVotedCapped = false;
        }

        public bool CanModifyTime() => !_hasUsedTimeModification;

        public bool CanReduceTime() =>
            !_hasUsedTimeModification && _gameManager.meetingTimer > MIN_TIME_FOR_REDUCTION;

        /// <summary>전원 투표 완료 시 타이머 3초로 제한</summary>
        public void CapTimerOnAllVoted()
        {
            if (_allVotedCapped) return;

            _allVotedCapped = true;
            if (_gameManager.meetingTimer > ALL_VOTED_CAP_SECONDS)
            {
                _gameManager.meetingTimer = ALL_VOTED_CAP_SECONDS;
                BroadcastTimer();
            }
            Debug.Log($"[MeetingTimer] All voted. Timer capped to {ALL_VOTED_CAP_SECONDS}s.");
        }
        #endregion

        #region RPC Handlers (호스트에서 호출됨)
        public void HandleAddTimeRequest(float time, PhotonMessageInfo info)
        {
            if (!PhotonNetwork.IsMasterClient || !IsValidTime(time)) return;

            if (_hasUsedTimeModification)
            {
                Debug.LogWarning("[MeetingTimer] Time modification already used.");
                return;
            }

            int sender = info.Sender?.ActorNumber ?? -1;
            if (sender <= 0 || !_addTimeRequesters.Add(sender))
            {
                Debug.LogWarning($"[MeetingTimer] Duplicate add-time request by {sender}.");
                return;
            }

            _hasUsedTimeModification = true;
            _gameManager.meetingTimer += time;

            Debug.Log($"[MeetingTimer] +{time}s by {sender}. now={_gameManager.meetingTimer}s");

            BroadcastTimer();
            _gameManager.RpcSyncTimeModificationState(true); // ← RPC 파사드 사용
        }

        public void HandleReduceTimeRequest(float time, PhotonMessageInfo info)
        {
            if (!PhotonNetwork.IsMasterClient || !IsValidTime(time)) return;

            if (_hasUsedTimeModification)
            {
                Debug.LogWarning("[MeetingTimer] Time modification already used.");
                return;
            }
            if (_gameManager.meetingTimer <= MIN_TIME_FOR_REDUCTION)
            {
                Debug.LogWarning($"[MeetingTimer] Cannot reduce below {MIN_TIME_FOR_REDUCTION}s.");
                return;
            }

            _hasUsedTimeModification = true;

            float newTime = _gameManager.meetingTimer - time;
            _gameManager.meetingTimer = (newTime < MIN_TIME_AFTER_REDUCTION)
                ? MIN_TIME_AFTER_REDUCTION
                : newTime;

            Debug.Log($"[MeetingTimer] -{time}s by {info.Sender?.ActorNumber}. now={_gameManager.meetingTimer}s");

            BroadcastTimer();
            _gameManager.RpcSyncTimeModificationState(true); // ← RPC 파사드 사용
        }

        public void SyncTimer(float serverRemaining, double sentServerTime)
        {
            float delay = (float)(PhotonNetwork.Time - sentServerTime);
            float adjusted = Mathf.Max(0f, serverRemaining - delay);
            UIManager.Instance?.StartMeetingTimerUI(adjusted);
        }

        public void SyncTimeModificationState(bool hasUsed) =>
            _hasUsedTimeModification = hasUsed;
        #endregion

        #region Helpers
        private bool IsValidTime(float time) =>
            !float.IsNaN(time) && !float.IsInfinity(time) && time > 0f;

        private void ShowTimeModificationLimitMessage()
        {
            Debug.LogWarning("[MeetingTimer] Time modification can only be used once.");
            UIManager.Instance?.ShowToastToScreen("시간 조정은 한 번만 가능합니다.");
        }

        private void ShowCannotReduceMessage()
        {
            Debug.LogWarning($"[MeetingTimer] Cannot reduce time below {MIN_TIME_FOR_REDUCTION}s.");
            UIManager.Instance?.ShowToastToScreen(
                $"남은 시간이 {MIN_TIME_FOR_REDUCTION}초 이하일 때는 시간을 줄일 수 없습니다.");
        }
        #endregion
    }
}
