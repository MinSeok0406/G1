using UnityEngine;
using UnityEngine.UI;

namespace ColorPicker.InGame
{
    /// <summary>
    /// 다운로드 대기 미니게임
    /// ClickableObject를 클릭하여 다운로드를 시작하고 완료될 때까지 대기
    /// </summary>
    [RequireComponent(typeof(MiniGameTag))]
    public class DownloadMiniGame : MiniGameBase
    {
        [Header("UI Components")]
        [SerializeField] private GameObject startButtonObject; // ClickableObject가 포함된 GameObject
        [SerializeField] private Slider progressBar;

        [Header("Settings")]
        [SerializeField] private float downloadDuration = 5f; // 다운로드 시간 (초)
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color disabledColor = new(0.5f, 0.5f, 0.5f, 0.5f);

        private int playerId;
        private MiniGameTag miniGameTag;
        private bool isDownloading = false;
        private float downloadProgress = 0f;

        private ClickableObject clickable;
        private Image buttonImage;
        private bool isButtonInteractable = true;

        private void Awake()
        {
            Initialize();
        }

        public override void Initialize()
        {
            miniGameTag = GetComponent<MiniGameTag>();
            playerId = Photon.Pun.PhotonNetwork.LocalPlayer.ActorNumber;

            if (startButtonObject != null)
            {
                clickable = startButtonObject.GetComponent<ClickableObject>();
                buttonImage = startButtonObject.GetComponent<Image>();

                if (clickable != null)
                {
                    clickable.OnClickEvent += OnStartButtonClicked;
                }
                else
                {
                    Debug.LogWarning("[DownloadMiniGame] startButtonObject에 ClickableObject 컴포넌트가 없습니다!", this);
                }

                if (buttonImage != null)
                {
                    buttonImage.color = normalColor;
                }
            }

            if (progressBar != null)
            {
                progressBar.value = 0f;
            }
        }

        public override void StartGame()
        {
            ResetDownload();
        }

        private void Update()
        {
            if (isDownloading)
            {
                downloadProgress += Time.deltaTime / downloadDuration;

                if (progressBar != null)
                {
                    progressBar.value = Mathf.Clamp01(downloadProgress);
                }

                if (downloadProgress >= 1f)
                {
                    CompleteDownload();
                }
            }
        }

        private void OnStartButtonClicked(Vector2 localPosition)
        {
            if (!isDownloading && isButtonInteractable)
            {
                StartDownload();
            }
        }

        private void StartDownload()
        {
            isDownloading = true;
            downloadProgress = 0f;

            SetButtonInteractable(false);

            Debug.Log("[Download] Download started!");
        }

        /// <summary>
        /// 버튼 활성화/비활성화
        /// </summary>
        private void SetButtonInteractable(bool interactable)
        {
            isButtonInteractable = interactable;

            if (buttonImage != null)
            {
                buttonImage.color = interactable ? normalColor : disabledColor;
            }
        }

        private void CompleteDownload()
        {
            isDownloading = false;

            MiniGameReport repo = new MiniGameReport()
            {
                playerId = playerId,
                miniGameType = miniGameTag.miniGameType,
                success = true
            };

            onComplete?.Invoke(repo);
            Debug.Log("[Download] Download complete!");
        }

        /// <summary>
        /// 다운로드 초기화
        /// </summary>
        private void ResetDownload()
        {
            isDownloading = false;
            downloadProgress = 0f;

            if (progressBar != null)
            {
                progressBar.value = 0f;
            }

            SetButtonInteractable(true);
        }

        /// <summary>
        /// 미션 초기화
        /// </summary>
        public void ResetMission()
        {
            ResetDownload();
        }

        private void OnDestroy()
        {
            if (clickable != null)
            {
                clickable.OnClickEvent -= OnStartButtonClicked;
            }
        }
    }
}
