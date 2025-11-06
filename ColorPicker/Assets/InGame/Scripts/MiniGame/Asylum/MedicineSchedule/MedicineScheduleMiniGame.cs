using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ColorPicker.InGame
{
    /// <summary>
    /// 투약 시간 체크 미니게임
    /// 정해진 시간에 약물 투여 버튼을 클릭하여 일정 완성
    /// </summary>
    [RequireComponent(typeof(MiniGameTag))]
    public class MedicineScheduleMiniGame : MiniGameBase
    {
        [SerializeField] private List<MedicineSlot> medicineSlots = new();
        [SerializeField] private float gameTime = 30f; // 게임 진행 시간 (초)
        [SerializeField] private TMPro.TMP_Text timerText;

        private int completedCount = 0;
        private float currentTime = 0f;
        private bool isGameActive = false;
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

            foreach (var slot in medicineSlots)
            {
                slot.OnMedicineGiven += OnMedicineGiven;
            }
        }

        public override void StartGame()
        {
            completedCount = 0;
            currentTime = 0f;
            isGameActive = true;

            foreach (var slot in medicineSlots)
            {
                slot.ResetSlot();
            }
        }

        private void Update()
        {
            if (!isGameActive) return;

            currentTime += Time.deltaTime;

            // 타이머 UI 업데이트
            if (timerText != null)
            {
                float remainingTime = gameTime - currentTime;
                timerText.text = $"Time: {Mathf.CeilToInt(remainingTime)}s";
            }

            // 각 슬롯에 현재 시간 전달
            foreach (var slot in medicineSlots)
            {
                slot.UpdateTime(currentTime);
            }

            // 시간 초과 체크
            if (currentTime >= gameTime)
            {
                FailGame();
            }
        }

        private void OnMedicineGiven()
        {
            completedCount++;

            if (completedCount >= medicineSlots.Count)
            {
                CompleteGame();
            }
        }

        private void CompleteGame()
        {
            isGameActive = false;

            MiniGameReport repo = new MiniGameReport()
            {
                playerId = playerId,
                miniGameType = miniGameTag.miniGameType,
                success = true
            };

            onComplete?.Invoke(repo);
        }

        private void FailGame()
        {
            isGameActive = false;

            MiniGameReport repo = new MiniGameReport()
            {
                playerId = playerId,
                miniGameType = miniGameTag.miniGameType,
                success = false
            };

            onComplete?.Invoke(repo);
        }

        public void ResetMission()
        {
            completedCount = 0;
            currentTime = 0f;
            isGameActive = false;

            foreach (var slot in medicineSlots)
            {
                slot.ResetSlot();
            }
        }

        private void OnDestroy()
        {
            foreach (var slot in medicineSlots)
            {
                slot.OnMedicineGiven -= OnMedicineGiven;
            }
        }
    }

    /// <summary>
    /// 투약 슬롯 (특정 시간에 클릭해야 함)
    /// </summary>
    public class MedicineSlot : MonoBehaviour
    {
        [SerializeField] private float targetTime; // 투여해야 하는 시간 (초)
        [SerializeField] private float timeWindow = 2f; // 허용 시간 범위 (초)
        [SerializeField] private Button giveButton;
        [SerializeField] private Image timerIndicator; // 시간 진행 표시
        [SerializeField] private Color activeColor = Color.yellow;
        [SerializeField] private Color completeColor = Color.green;

        public System.Action OnMedicineGiven;

        private bool isCompleted = false;
        private bool isActive = false;

        private void Awake()
        {
            if (giveButton != null)
            {
                giveButton.onClick.AddListener(OnButtonClicked);
            }
        }

        public void UpdateTime(float currentTime)
        {
            if (isCompleted) return;

            // 투여 가능 시간인지 체크
            float timeDiff = Mathf.Abs(currentTime - targetTime);
            isActive = timeDiff <= timeWindow;

            // 버튼 활성화
            if (giveButton != null)
            {
                giveButton.interactable = isActive;
            }

            // 시간 표시 업데이트
            if (timerIndicator != null)
            {
                timerIndicator.fillAmount = currentTime / (targetTime + timeWindow);
                timerIndicator.color = isActive ? activeColor : Color.gray;
            }
        }

        private void OnButtonClicked()
        {
            if (isActive && !isCompleted)
            {
                isCompleted = true;

                if (giveButton != null)
                {
                    giveButton.interactable = false;
                }

                if (timerIndicator != null)
                {
                    timerIndicator.color = completeColor;
                }

                OnMedicineGiven?.Invoke();
                Debug.Log($"[MedicineSlot] Medicine given at time {targetTime}");
            }
        }

        public void ResetSlot()
        {
            isCompleted = false;
            isActive = false;

            if (giveButton != null)
            {
                giveButton.interactable = false;
            }

            if (timerIndicator != null)
            {
                timerIndicator.fillAmount = 0f;
                timerIndicator.color = Color.gray;
            }
        }

        private void OnDestroy()
        {
            if (giveButton != null)
            {
                giveButton.onClick.RemoveListener(OnButtonClicked);
            }
        }
    }
}
