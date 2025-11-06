using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ColorPicker.InGame
{
    /// <summary>
    /// 진단서 작성 미니게임
    /// 체크리스트 항목들을 올바르게 클릭하여 진단서 완성
    /// </summary>
    [RequireComponent(typeof(MiniGameTag))]
    public class DiagnosisFormMiniGame : MiniGameBase
    {
        [SerializeField] private List<DiagnosisCheckbox> checkboxes = new();

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

            foreach (var checkbox in checkboxes)
            {
                checkbox.OnCheckboxClicked += CheckCompletion;
            }
        }

        public override void StartGame()
        {
            foreach (var checkbox in checkboxes)
            {
                checkbox.ResetCheckbox();
            }
        }

        private void CheckCompletion()
        {
            bool allCorrect = true;

            foreach (var checkbox in checkboxes)
            {
                if (!checkbox.IsCorrect())
                {
                    allCorrect = false;
                    break;
                }
            }

            if (allCorrect)
            {
                CompleteGame();
            }
        }

        private void CompleteGame()
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
            foreach (var checkbox in checkboxes)
            {
                checkbox.ResetCheckbox();
            }
        }

        private void OnDestroy()
        {
            foreach (var checkbox in checkboxes)
            {
                checkbox.OnCheckboxClicked -= CheckCompletion;
            }
        }
    }

    /// <summary>
    /// 진단서 체크박스 항목
    /// </summary>
    public class DiagnosisCheckbox : MonoBehaviour
    {
        [SerializeField] private bool shouldBeChecked; // 이 항목이 체크되어야 하는가?
        [SerializeField] private Image checkmarkImage; // 체크마크 이미지
        [SerializeField] private Toggle toggle; // UI Toggle

        public System.Action OnCheckboxClicked;

        private void Awake()
        {
            if (toggle == null)
                toggle = GetComponent<Toggle>();

            if (toggle != null)
            {
                toggle.onValueChanged.AddListener(OnToggleValueChanged);
            }

            if (checkmarkImage != null)
            {
                checkmarkImage.gameObject.SetActive(false);
            }
        }

        private void OnToggleValueChanged(bool isOn)
        {
            if (checkmarkImage != null)
            {
                checkmarkImage.gameObject.SetActive(isOn);
            }

            OnCheckboxClicked?.Invoke();
        }

        public bool IsCorrect()
        {
            if (toggle != null)
            {
                return toggle.isOn == shouldBeChecked;
            }
            return false;
        }

        public void ResetCheckbox()
        {
            if (toggle != null)
            {
                toggle.isOn = false;
            }

            if (checkmarkImage != null)
            {
                checkmarkImage.gameObject.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            if (toggle != null)
            {
                toggle.onValueChanged.RemoveListener(OnToggleValueChanged);
            }
        }
    }
}
