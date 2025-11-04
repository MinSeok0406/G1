using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.Events;

namespace ColorPicker.InGame
{
    [DefaultExecutionOrder(0)]
    public class InteractionDetector : MonoBehaviour
    {
        private readonly List<IInteractive> _nearby = new();
        private PlayerControlBase _playerControl;

        private IInteractive _current;              // 현재 타겟
        private UnityAction _cachedClickAction;     // 버튼 콜백 캐시

        private void Awake()
        {
            // PlayerControl 또는 LobbyPlayerControl 찾기
            _playerControl = GetComponentInParent<PlayerControlBase>();
            if (!_playerControl)
            {
                Debug.LogError("[InteractionDetector] PlayerControlBase not found.", this);
                enabled = false;
                return;
            }
            _cachedClickAction = OnInteractionClicked; // GC 절감
        }

        private void OnEnable()
        {
            if (_playerControl?.photonView == null || !_playerControl.photonView.IsMine)
            {
                enabled = false; // 로컬 소유 아닐 때는 완전 비활성
                return;
            }
        }

        private void OnDisable()
        {
            // 하이라이트/버튼 정리
            SetHighlighted(_current, false);
            _current = null;

            // PlayerControl 또는 LobbyPlayerControl의 CurrentInteractive 초기화
            if (_playerControl is PlayerControl pc)
                pc.CurrentInteractive = null;
            else if (_playerControl is LobbyPlayerControl lpc)
                lpc.CurrentInteractive = null;

            HideButton();

            _nearby.Clear();
        }

        public void AddInteractable(IInteractive target)
        {
            if (!IsValid(target)) return;
            if (!_nearby.Contains(target)) _nearby.Add(target);
            UpdateClosest();
        }

        public void RemoveInteractable(IInteractive target)
        {
            if (target == null) return;

            if (_nearby.Remove(target))
                SetHighlighted(target, false);

            UpdateClosest();
        }

        private void UpdateClosest()
        {
            // 1) 정리: 파괴/널 제거
            for (int i = _nearby.Count - 1; i >= 0; i--)
            {
                var it = _nearby[i];
                if (!IsValid(it)) _nearby.RemoveAt(i);
            }

            // 2) 탐색: 가장 가까운 대상
            IInteractive closest = null;
            float minDistSq = float.MaxValue;
            Vector3 selfPos = transform.position;

            for (int i = 0; i < _nearby.Count; i++)
            {
                var obj = _nearby[i];
                if (!IsValid(obj)) continue;

                float d2 = (obj.GetPosition() - selfPos).sqrMagnitude;
                if (d2 < minDistSq)
                {
                    minDistSq = d2;
                    closest = obj;
                }
            }

            // 3) 변경 사항에만 반응
            if (!ReferenceEquals(_current, closest))
            {
                SetHighlighted(_current, false);
                SetHighlighted(closest, true);

                _current = closest;

                // PlayerControl 또는 LobbyPlayerControl에 CurrentInteractive 설정
                if (_playerControl is PlayerControl pc)
                    pc.CurrentInteractive = _current;
                else if (_playerControl is LobbyPlayerControl lpc)
                    lpc.CurrentInteractive = _current;

                if (_current != null)
                {
                    ShowButton();
                }
                else
                {
                    HideButton();
                }
            }
        }

        private void OnInteractionClicked()
        {
            if (_playerControl == null) { HideButton(); return; }

            IInteractive target = null;
            if (_playerControl is PlayerControl pc)
                target = pc.CurrentInteractive;
            else if (_playerControl is LobbyPlayerControl lpc)
                target = lpc.CurrentInteractive;

            if (!IsValid(target)) { HideButton(); return; }

            // TryInteract 호출
            if (_playerControl is PlayerControl playerControl)
                playerControl.TryInteract();
            else if (_playerControl is LobbyPlayerControl lobbyPlayerControl)
                lobbyPlayerControl.TryInteract();
        }

        private static bool IsValid(IInteractive it)
        {
            if (it == null) return false;
            var obj = it as Object;
            return obj != null; // Unity fake-null 방지
        }

        private void ShowButton()
        {
            // 로비 또는 인게임에 따라 적절한 매니저 사용
            if (LobbyUIManager.Instance != null)
                LobbyUIManager.Instance.ShowInteractionButton(true, _cachedClickAction);
            else if (UIManager.Instance != null)
                UIManager.Instance.ShowInteractionButton(true, _cachedClickAction);
        }

        private void HideButton()
        {
            // 로비 또는 인게임에 따라 적절한 매니저 사용
            if (LobbyUIManager.Instance != null)
                LobbyUIManager.Instance.ShowInteractionButton(false);
            else if (UIManager.Instance != null)
                UIManager.Instance.ShowInteractionButton(false);
        }

        /// <summary>
        /// 하이라이트 적용: 1) IInteractive.ToggleHighlight 우선 사용
        ///                2) 없으면 HighlightController를 붙여 사용
        /// </summary>
        private static void SetHighlighted(IInteractive target, bool on)
        {
            if (!IsValid(target)) return;

            // 1) 인터페이스가 자체 하이라이트를 제공하면 그걸 사용 (기존 호환)
            try
            {
                target.ToggleHighlight(on);
                return;
            }
            catch
            {
                // ToggleHighlight를 던지거나 미구현인 경우 → 폴백
            }

            // 2) 폴백: HighlightController 자동 부착/사용
            var obj = target as Object;
            var go = (obj as Component)?.gameObject ?? (obj as GameObject);
            if (!go) return;

            var hi = go.GetComponent<HighlightController>();
            if (!hi) hi = go.AddComponent<HighlightController>();
            hi.SetHighlighted(on);
        }
    }
}
