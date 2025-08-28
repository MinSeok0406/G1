using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.Events; // 로컬 판정용

namespace ColorPicker.InGame
{
    public class InteractionDetector : MonoBehaviour
    {
        private readonly List<IInteractive> nearby = new();
        private PlayerControl player;

        private IInteractive currentClosest;
        private UnityAction cachedClickAction; // GC 줄이기용 고정 콜백

        private void Awake()
        {
            // 자식에 붙어있을 수 있으므로 부모에서 찾는다
            player = GetComponentInParent<PlayerControl>();
            if (!player)
            {
                Debug.LogError("[InteractionDetector] PlayerControl not found in parents.");
                enabled = false;
                return;
            }

            // 클릭 콜백 고정(리스너 중복 방지)
            cachedClickAction = OnInteractionClicked;
        }

        private void OnEnable()
        {
            // 내 소유 플레이어만 UI/탐지 동작
            if (!player.photonView || !player.photonView.IsMine)
            {
                enabled = false;
                return;
            }
        }

        private void OnDisable()
        {
            // 비활성화 때 UI 정리
            if (UIManager.Instance != null)
                UIManager.Instance.ShowInteractionButton(false);

            currentClosest = null;
            if (player != null) player.currentInteractive = null;
            nearby.Clear();
        }

        public void AddInteractable(IInteractive target)
        {
            if (target == null) return;
            if (!nearby.Contains(target))
                nearby.Add(target);

            UpdateClosest();
        }

        public void RemoveInteractable(IInteractive target)
        {
            if (target == null) return;

            if (nearby.Remove(target))
            {
                // 제거 대상 하이라이트 해제
                target.ToggleHighlight(false);
            }

            UpdateClosest();
        }

        private void UpdateClosest()
        {
            // 파괴/널 정리
            for (int i = nearby.Count - 1; i >= 0; i--)
            {
                // UnityEngine.Object의 fake null 대비
                if (nearby[i] == null || (nearby[i] as Object) == null)
                    nearby.RemoveAt(i);
            }

            // 가장 가까운 대상 탐색 (sqrMagnitude로 비용 절약)
            IInteractive closest = null;
            float minDistSq = float.MaxValue;
            Vector3 selfPos = transform.position;

            for (int i = 0; i < nearby.Count; i++)
            {
                var obj = nearby[i];
                if (obj == null || (obj as Object) == null) continue;

                Vector3 p = obj.GetPosition();
                float d2 = (p - selfPos).sqrMagnitude;
                if (d2 < minDistSq)
                {
                    minDistSq = d2;
                    closest = obj;
                }
            }

            // 변경된 경우에만 하이라이트/버튼 갱신
            if (!ReferenceEquals(currentClosest, closest))
            {
                currentClosest?.ToggleHighlight(false);
                closest?.ToggleHighlight(true);

                currentClosest = closest;
                player.currentInteractive = closest;

                if (closest != null)
                {
                    // UIManager.ShowInteractionButton는 내부적으로 리스너를 교체(=중복추가 X)해야 안전
                    UIManager.Instance.ShowInteractionButton(true, cachedClickAction);
                }
                else
                {
                    UIManager.Instance.ShowInteractionButton(false);
                }
            }
        }

        private void OnInteractionClicked()
        {
            // 버튼 콜백: 대상이 여전히 유효한지 재검증
            if (player == null) return;

            var target = player.currentInteractive;
            if (target == null || (target as Object) == null)
            {
                UIManager.Instance.ShowInteractionButton(false);
                return;
            }

            player.TryInteract();
        }
    }
}
