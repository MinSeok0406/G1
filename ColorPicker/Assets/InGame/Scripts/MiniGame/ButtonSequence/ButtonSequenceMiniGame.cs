using System.Collections.Generic;
using UnityEngine;

namespace ColorPicker.InGame
{
    /// <summary>
    /// 버튼 순서 누르기 미니게임
    /// 화면에 표시되는 순서대로 버튼을 눌러야 함
    /// </summary>
    [RequireComponent(typeof(MiniGameTag))]
    public class ButtonSequenceMiniGame : MiniGameBase
    {
        [SerializeField] private List<SequenceButton> buttons = new();
        [SerializeField] private int sequenceLength = 5;

        private List<int> targetSequence = new();
        private int currentStep = 0;
        private int playerId;
        private MiniGameTag miniGameTag;

        private void Awake()
        {
            Initialize();
        }

        public override void Initialize()
        {
            miniGameTag = GetComponent<MiniGameTag>();
            playerId = Photon.Pun.PhotonNetwork.LocalPlayer.ActorNumber;

            // 버튼 클릭 이벤트 연결
            for (int i = 0; i < buttons.Count; i++)
            {
                int index = i; // 클로저 문제 방지
                buttons[i].OnButtonClicked += () => OnButtonClicked(index);
            }
        }

        public override void StartGame()
        {
            GenerateSequence();
            ShowSequence();
        }

        /// <summary>
        /// 랜덤 순서 생성
        /// </summary>
        private void GenerateSequence()
        {
            targetSequence.Clear();
            currentStep = 0;

            for (int i = 0; i < sequenceLength; i++)
            {
                targetSequence.Add(Random.Range(0, buttons.Count));
            }

            Debug.Log($"[ButtonSequence] Generated sequence: {string.Join(", ", targetSequence)}");
        }

        /// <summary>
        /// 순서 표시 (애니메이션)
        /// </summary>
        private void ShowSequence()
        {
            StartCoroutine(ShowSequenceCoroutine());
        }

        private System.Collections.IEnumerator ShowSequenceCoroutine()
        {
            yield return new WaitForSeconds(0.5f);

            foreach (int buttonIndex in targetSequence)
            {
                buttons[buttonIndex].Highlight();
                yield return new WaitForSeconds(0.5f);
                buttons[buttonIndex].Unhighlight();
                yield return new WaitForSeconds(0.3f);
            }

            // 플레이어 입력 활성화
            EnableButtons(true);
        }

        private void OnButtonClicked(int buttonIndex)
        {
            if (targetSequence[currentStep] == buttonIndex)
            {
                // 올바른 버튼
                currentStep++;
                buttons[buttonIndex].PlayCorrectFeedback();

                if (currentStep >= targetSequence.Count)
                {
                    CompleteGame();
                }
            }
            else
            {
                // 틀린 버튼 - 재시작
                buttons[buttonIndex].PlayWrongFeedback();
                RestartSequence();
            }
        }

        private void RestartSequence()
        {
            currentStep = 0;
            EnableButtons(false);
            ShowSequence();
        }

        private void CompleteGame()
        {
            EnableButtons(false);

            MiniGameReport repo = new MiniGameReport()
            {
                playerId = playerId,
                miniGameType = miniGameTag.miniGameType,
                success = true
            };

            onComplete?.Invoke(repo);
        }

        private void EnableButtons(bool enabled)
        {
            foreach (var button in buttons)
            {
                button.SetInteractable(enabled);
            }
        }

        /// <summary>
        /// 미션 초기화
        /// </summary>
        public void ResetMission()
        {
            currentStep = 0;
            targetSequence.Clear();
            EnableButtons(false);
        }

        private void OnDestroy()
        {
            foreach (var button in buttons)
            {
                button.OnButtonClicked = null;
            }
        }
    }
}
