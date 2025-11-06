using System.Collections.Generic;
using UnityEngine;

namespace ColorPicker.InGame
{
    /// <summary>
    /// 정신 상태 검사 미니게임
    /// 간단한 산수 문제를 풀어야 하는 미니게임 (ClickableObject 사용)
    /// </summary>
    [RequireComponent(typeof(MiniGameTag))]
    public class MentalStateExamMiniGame : MiniGameBase
    {
        [Header("UI Components")]
        [SerializeField] private TMPro.TMP_Text questionText;
        [SerializeField] private List<AnswerButton> answerButtons = new(); // AnswerButton 리스트 (4개)

        [Header("Game Settings")]
        [SerializeField] private int questionsToComplete = 3; // 성공하려면 맞춰야 할 문제 수
        [SerializeField] private int minNumber = 1; // 최소 숫자
        [SerializeField] private int maxNumber = 20; // 최대 숫자

        private int correctAnswersCount = 0;
        private int currentCorrectAnswer = 0;
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

            // AnswerButton 이벤트 연결
            for (int i = 0; i < answerButtons.Count; i++)
            {
                var button = answerButtons[i];
                if (button != null)
                {
                    button.OnAnswerClicked += OnAnswerClicked;
                }
                else
                {
                    Debug.LogWarning($"[MentalStateExam] answerButton[{i}]이 null입니다!", this);
                }
            }
        }

        public override void StartGame()
        {
            correctAnswersCount = 0;
            GenerateAndShowQuestion();
        }

        /// <summary>
        /// 새로운 산수 문제 생성 및 표시
        /// </summary>
        private void GenerateAndShowQuestion()
        {
            // 랜덤 산수 문제 생성
            int num1 = Random.Range(minNumber, maxNumber + 1);
            int num2 = Random.Range(minNumber, maxNumber + 1);
            int operationType = Random.Range(0, 3); // 0: +, 1: -, 2: *

            string operatorSymbol = "";
            int correctAnswer = 0;

            switch (operationType)
            {
                case 0: // 덧셈
                    operatorSymbol = "+";
                    correctAnswer = num1 + num2;
                    break;
                case 1: // 뺄셈
                    // 음수 방지: 큰 수에서 작은 수 빼기
                    if (num1 < num2)
                    {
                        int temp = num1;
                        num1 = num2;
                        num2 = temp;
                    }
                    operatorSymbol = "-";
                    correctAnswer = num1 - num2;
                    break;
                case 2: // 곱셈 (작은 숫자로 제한)
                    num1 = Random.Range(1, 11);
                    num2 = Random.Range(1, 11);
                    operatorSymbol = "×";
                    correctAnswer = num1 * num2;
                    break;
            }

            currentCorrectAnswer = correctAnswer;

            // 문제 텍스트 설정
            questionText.text = $"{num1} {operatorSymbol} {num2} = ?";

            // 오답 생성 (정답 근처의 값들)
            List<int> answers = new List<int> { correctAnswer };
            while (answers.Count < 4)
            {
                int wrongAnswer = correctAnswer + Random.Range(-5, 6);
                if (wrongAnswer != correctAnswer && !answers.Contains(wrongAnswer) && wrongAnswer >= 0)
                {
                    answers.Add(wrongAnswer);
                }
            }

            // 답안 섞기
            for (int i = 0; i < answers.Count; i++)
            {
                int randomIndex = Random.Range(0, answers.Count);
                int temp = answers[i];
                answers[i] = answers[randomIndex];
                answers[randomIndex] = temp;
            }

            // 답안 버튼에 표시
            for (int i = 0; i < answerButtons.Count && i < answers.Count; i++)
            {
                var button = answerButtons[i];
                if (button != null)
                {
                    button.SetAnswer(answers[i]);
                    button.SetInteractable(true);
                    button.ResetColor();
                    button.gameObject.SetActive(true);
                }
            }

            Debug.Log($"[MentalStateExam] Question generated: {questionText.text}, Correct: {correctAnswer}");
        }

        private void OnAnswerClicked(int selectedAnswer)
        {
            if (selectedAnswer == currentCorrectAnswer)
            {
                // 정답!
                correctAnswersCount++;
                Debug.Log($"[MentalStateExam] Correct! ({correctAnswersCount}/{questionsToComplete})");

                // 정답 피드백 표시
                foreach (var button in answerButtons)
                {
                    if (button != null && button.GetAnswer() == currentCorrectAnswer)
                    {
                        button.PlayCorrectFeedback();
                        break;
                    }
                }

                // 잠시 대기 후 다음 문제 또는 완료
                StartCoroutine(WaitAndProceed(true));
            }
            else
            {
                // 오답! 리셋하고 새 문제
                correctAnswersCount = 0;
                Debug.Log("[MentalStateExam] Wrong answer! Reset.");

                // 오답 피드백 표시
                foreach (var button in answerButtons)
                {
                    if (button != null && button.GetAnswer() == selectedAnswer)
                    {
                        button.PlayWrongFeedback();
                        break;
                    }
                }

                // 잠시 대기 후 새 문제
                StartCoroutine(WaitAndProceed(false));
            }
        }

        private System.Collections.IEnumerator WaitAndProceed(bool wasCorrect)
        {
            // 모든 버튼 비활성화 (중복 클릭 방지)
            foreach (var button in answerButtons)
            {
                if (button != null)
                {
                    button.SetInteractable(false);
                }
            }

            // 피드백 애니메이션 대기
            yield return new WaitForSeconds(0.5f);

            if (wasCorrect && correctAnswersCount >= questionsToComplete)
            {
                CompleteExam();
            }
            else
            {
                GenerateAndShowQuestion();
            }
        }

        private void CompleteExam()
        {
            MiniGameReport repo = new MiniGameReport()
            {
                playerId = playerId,
                miniGameType = miniGameTag.miniGameType,
                success = true
            };

            onComplete?.Invoke(repo);
        }

        public void ResetMission()
        {
            correctAnswersCount = 0;
            GenerateAndShowQuestion();
        }

        private void OnDestroy()
        {
            // 이벤트 정리
            foreach (var button in answerButtons)
            {
                if (button != null)
                {
                    button.OnAnswerClicked -= OnAnswerClicked;
                }
            }
        }
    }
}
