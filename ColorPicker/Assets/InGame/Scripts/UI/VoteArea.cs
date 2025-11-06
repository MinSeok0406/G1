using UnityEngine;

namespace ColorPicker.InGame
{
    /// <summary>
    /// 투표 결과를 시각적으로 표시하는 컴포넌트
    /// 득표수만큼 아이콘을 활성화하여 표시
    /// </summary>
    public class VoteArea : MonoBehaviour
    {
        [SerializeField] private GameObject[] voteIcons;

        /// <summary>
        /// 득표수를 설정하여 시각적으로 표시
        /// </summary>
        /// <param name="voteCount">받은 투표 수</param>
        public void SetVoteCount(int voteCount)
        {
            if (voteIcons == null || voteIcons.Length == 0)
            {
                Debug.LogWarning("[VoteArea] Vote icons array is null or empty.");
                return;
            }

            // 모든 아이콘을 먼저 비활성화
            for (int i = 0; i < voteIcons.Length; i++)
            {
                if (voteIcons[i] != null)
                {
                    voteIcons[i].SetActive(false);
                }
            }

            // 득표수만큼 아이콘 활성화
            int displayCount = Mathf.Min(voteCount, voteIcons.Length);
            for (int i = 0; i < displayCount; i++)
            {
                if (voteIcons[i] != null)
                {
                    voteIcons[i].SetActive(true);
                }
            }

            // 득표수가 배열 크기보다 크면 경고
            if (voteCount > voteIcons.Length)
            {
                Debug.LogWarning($"[VoteArea] Vote count ({voteCount}) exceeds icon array size ({voteIcons.Length}).");
            }
        }

        /// <summary>
        /// 투표 영역 초기화 (모든 아이콘 비활성화)
        /// </summary>
        public void Clear()
        {
            SetVoteCount(0);
        }

        /// <summary>
        /// 최대 표시 가능한 투표 수
        /// </summary>
        public int MaxVoteCount => voteIcons?.Length ?? 0;

#if UNITY_EDITOR
        /// <summary>
        /// 에디터에서 테스트용
        /// </summary>
        [ContextMenu("Test: Show 3 Votes")]
        private void TestShowThreeVotes()
        {
            SetVoteCount(3);
        }

        [ContextMenu("Test: Clear Votes")]
        private void TestClearVotes()
        {
            Clear();
        }
#endif
    }
}
