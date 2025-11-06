using UnityEngine;

namespace ColorPicker.InGame
{
    /// <summary>
    /// 진정제 주사 미니게임 (정신병원 테마)
    /// 주사기를 드래그하여 환자에게 진정제 주입
    /// </summary>
    [RequireComponent(typeof(MiniGameTag))]
    public class SedativeInjectionMiniGame : MiniGameBase
    {
        [SerializeField] private SyringeObject syringe;
        [SerializeField] private Transform patientTarget;
        [SerializeField] private float injectionDistance = 50f;

        private int playerId;
        private MiniGameTag miniGameTag;
        private bool isCompleted = false;

        private void Awake()
        {
            Initialize();
        }

        public override void Initialize()
        {
            miniGameTag = GetComponent<MiniGameTag>();
            playerId = Photon.Pun.PhotonNetwork.LocalPlayer.ActorNumber;
            isCompleted = false;

            if (syringe != null)
            {
                syringe.OnInjectionComplete += OnInjectionComplete;
                syringe.SetInjectionDistance(injectionDistance);
                syringe.SetTargetPosition(patientTarget.position);
            }
        }

        private void OnInjectionComplete()
        {
            if (isCompleted) return;

            isCompleted = true;

            MiniGameReport repo = new MiniGameReport()
            {
                playerId = playerId,
                miniGameType = miniGameTag.miniGameType,
                success = true
            };

            onComplete?.Invoke(repo);
            Debug.Log("[SedativeInjection] Injection complete!");
        }

        public override void StartGame()
        {
            if (syringe != null)
            {
                syringe.gameObject.SetActive(true);
                syringe.ResetSyringe();
            }
        }

        public void ResetMission()
        {
            isCompleted = false;
            if (syringe != null)
            {
                syringe.ResetSyringe();
            }
        }

        private void OnDestroy()
        {
            if (syringe != null)
            {
                syringe.OnInjectionComplete -= OnInjectionComplete;
            }
        }
    }
}
