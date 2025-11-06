using System.Collections.Generic;
using UnityEngine;

namespace ColorPicker.InGame
{
    /// <summary>
    /// 환자 기록 정리 미니게임 - 서류를 알파벳 순서대로 정리
    /// </summary>
    [RequireComponent(typeof(MiniGameTag))]
    public class PatientFileSortMiniGame : MiniGameBase
    {
        [SerializeField] private Transform filesParent;
        private List<PatientFile> files = new();
        private int sortedCount = 0;
        private int playerId;
        private MiniGameTag miniGameTag;

        private void Awake() { Initialize(); }

        public override void Initialize()
        {
            miniGameTag = GetComponent<MiniGameTag>();
            playerId = Photon.Pun.PhotonNetwork.LocalPlayer.ActorNumber;
            files.Clear();

            foreach (Transform child in filesParent)
            {
                if (child.TryGetComponent(out PatientFile file))
                {
                    file.OnFileSorted += () => { sortedCount++; CheckCompletion(); };
                    files.Add(file);
                }
            }
        }

        private void CheckCompletion()
        {
            if (sortedCount >= files.Count)
            {
                onComplete?.Invoke(new MiniGameReport()
                {
                    playerId = playerId,
                    miniGameType = miniGameTag.miniGameType,
                    success = true
                });
            }
        }

        public override void StartGame()
        {
            foreach (var file in files) { file.ResetPosition(); file.gameObject.SetActive(true); }
        }

        public void ResetMission()
        {
            sortedCount = 0;
            foreach (var file in files) file.ResetPosition();
        }
    }

    /// <summary>
    /// 환자 파일
    /// </summary>
    [RequireComponent(typeof(DraggableObject))]
    public class PatientFile : MonoBehaviour
    {
        [SerializeField] private string patientName;
        [SerializeField] private string targetSlotTag = "FileSlot";
        public System.Action OnFileSorted;
        private RectTransform rectTransform;
        private Vector2 startPos;
        private bool isSorted = false;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            startPos = rectTransform.localPosition;
            var sensor = GetComponent<DragTriggerSensor>();
            if (sensor != null) sensor.OnTriggerEntered += OnTrigger;
        }

        private void OnTrigger(Collider2D col)
        {
            if (!isSorted && col.CompareTag(targetSlotTag))
            {
                isSorted = true;
                OnFileSorted?.Invoke();
                gameObject.SetActive(false);
            }
        }

        public void ResetPosition()
        {
            isSorted = false;
            rectTransform.localPosition = startPos;
        }
    }
}
