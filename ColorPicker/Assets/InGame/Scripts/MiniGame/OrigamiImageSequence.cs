using UnityEngine;
using UnityEngine.UI;

namespace ColorPicker.InGame
{
    [RequireComponent(typeof(ClickableObject))]
    public class OrigamiImageSequence : MonoBehaviour
    {
        [Header("Image Sequence")]
        [SerializeField] private Sprite[] foldingSteps;
        [SerializeField] private Image displayImage;
        
        private int currentStep = 0;
        private ClickableObject clickableObject;
        
        public System.Action OnSequenceComplete;
        public int CurrentStep => currentStep;
        public int TotalSteps => foldingSteps.Length;
        public bool IsComplete => currentStep >= foldingSteps.Length;
        
        private void Start()
        {
            if (displayImage == null)
            {
                displayImage = GetComponent<Image>();
            }
            
            clickableObject = GetComponent<ClickableObject>();
            if (clickableObject != null)
            {
                clickableObject.OnClickEvent += HandleClick;
            }
            
            ShowCurrentStep();
        }
        
        private void HandleClick()
        {
            if (!IsComplete)
            {
                NextStep();
                Debug.Log($"종이 클릭됨! 다음 단계로 진행");
            }
            else
            {
                Debug.Log("이미 완성된 종이접기입니다!");
            }
        }
        
        public void NextStep()
        {
            if (IsComplete) return;
            
            currentStep++;
            ShowCurrentStep();
            
            Debug.Log($"종이접기 단계: {currentStep}/{foldingSteps.Length}");
            
            if (IsComplete)
            {
                Debug.Log("종이접기 완성!");
                OnSequenceComplete?.Invoke();
            }
        }
        
        public void Reset()
        {
            currentStep = 0;
            ShowCurrentStep();
        }
        
        private void ShowCurrentStep()
        {
            if (displayImage != null && foldingSteps != null && currentStep < foldingSteps.Length)
            {
                displayImage.sprite = foldingSteps[currentStep];
            }
        }
        
        private void OnDestroy()
        {
            if (clickableObject != null)
            {
                clickableObject.OnClickEvent -= HandleClick;
            }
        }
    }
}
