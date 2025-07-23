using UnityEngine;
using UnityEngine.UI;

namespace ColorPicker.InGame
{
    [RequireComponent(typeof(MiniGameTag))]
    public class OrigamiGame : MiniGameBase
    {
        [Header("Game Settings")]
        [SerializeField] private OrigamiImageSequence origamiSequence;
        
        [Header("UI Settings")]
        [SerializeField] private Text instructionText;
        [SerializeField] private Text progressText;
        
        private int playerId;
        private MiniGameTag miniGameTag;
        
        private void Awake()
        {
            miniGameTag = GetComponent<MiniGameTag>();
        }
        
        public override void Initialize()
        {
            if (origamiSequence != null)
            {
                origamiSequence.Reset();
                origamiSequence.OnSequenceComplete += OnOrigamiComplete;
            }
            
            UpdateUI();
        }
        
        public override void StartGame()
        {
            Debug.Log("간단한 종이접기 미니게임 시작!");
            UpdateUI();
        }
        
        private void OnOrigamiComplete()
        {
            CompleteGame(true);
        }
        
        private void UpdateUI()
        {
            if (origamiSequence != null)
            {
                if (instructionText != null)
                {
                    if (origamiSequence.IsComplete)
                    {
                        instructionText.text = "종이접기 완성! 축하합니다!";
                    }
                    else
                    {
                        instructionText.text = "종이를 클릭해서 접어보세요!";
                    }
                }
                
                if (progressText != null)
                {
                    progressText.text = $"진행: {origamiSequence.CurrentStep} / {origamiSequence.TotalSteps}";
                }
            }
        }
        
        private void CompleteGame(bool success)
        {
            Debug.Log($"종이접기 게임 완료: {(success ? "성공" : "실패")}");
            
            UpdateUI();
            
            var report = new MiniGameReport
            {
                playerId = playerId,
                miniGameType = MiniGameType.OrigamiFolding,
                success = success
            };
            
            onComplete?.Invoke(report);
        }
        
        public void SetPlayerId(int id)
        {
            playerId = id;
        }
        
        private void OnDestroy()
        {
            if (origamiSequence != null)
            {
                origamiSequence.OnSequenceComplete -= OnOrigamiComplete;
            }
        }
        
        private void Update()
        {
            if (origamiSequence != null && !origamiSequence.IsComplete)
            {
                UpdateUI();
            }
        }
    }
}
