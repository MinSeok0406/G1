using System.Collections.Generic;
using UnityEngine;

namespace ColorPicker.InGame
{
    /// <summary>
    /// 구속복 묶기 미니게임
    /// 구속복의 끈을 드래그하여 올바른 버클에 연결
    /// </summary>
    [RequireComponent(typeof(MiniGameTag))]
    public class StraitjacketTieMiniGame : MiniGameBase
    {
        [SerializeField] private List<JacketStrap> straps = new();

        private int connectedCount = 0;
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

            connectedCount = 0;

            foreach (var strap in straps)
            {
                strap.OnStrapConnected += OnStrapConnected;
            }
        }

        public override void StartGame()
        {
            foreach (var strap in straps)
            {
                strap.gameObject.SetActive(true);
                strap.ResetStrap();
            }
        }

        private void OnStrapConnected()
        {
            connectedCount++;

            if (connectedCount >= straps.Count)
            {
                MiniGameReport repo = new MiniGameReport()
                {
                    playerId = playerId,
                    miniGameType = miniGameTag.miniGameType,
                    success = true
                };

                onComplete?.Invoke(repo);
            }
        }

        public void ResetMission()
        {
            connectedCount = 0;

            foreach (var strap in straps)
            {
                strap.ResetStrap();
            }
        }

        private void OnDestroy()
        {
            foreach (var strap in straps)
            {
                strap.OnStrapConnected -= OnStrapConnected;
            }
        }
    }

    /// <summary>
    /// 구속복 끈 오브젝트
    /// </summary>
    [RequireComponent(typeof(DraggableObject))]
    public class JacketStrap : MonoBehaviour
    {
        [SerializeField] private int strapID; // 끈의 고유 ID
        [SerializeField] private string targetBuckleTag = "Buckle"; // 버클 태그

        public System.Action OnStrapConnected;

        private RectTransform rectTransform;
        private Vector2 startPosition;
        private bool isConnected = false;
        private DragTriggerSensor dragTriggerSensor;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            dragTriggerSensor = GetComponent<DragTriggerSensor>();
            startPosition = rectTransform.localPosition;
        }

        private void OnEnable()
        {
            if (dragTriggerSensor != null)
            {
                dragTriggerSensor.OnTriggerEntered += DragTriggerSensor_OnTriggerEntered;
            }
        }

        private void OnDisable()
        {
            if (dragTriggerSensor != null)
            {
                dragTriggerSensor.OnTriggerEntered -= DragTriggerSensor_OnTriggerEntered;
            }
        }

        private void DragTriggerSensor_OnTriggerEntered(Collider2D collider)
        {
            if (isConnected) return;

            // 버클인지 확인
            if (collider.CompareTag(targetBuckleTag))
            {
                // 같은 ID인지 확인
                if (collider.TryGetComponent(out JacketBuckle buckle))
                {
                    if (buckle.GetBuckleID() == strapID)
                    {
                        ConnectStrap(buckle.transform.position);
                    }
                }
            }
        }

        private void ConnectStrap(Vector3 bucklePosition)
        {
            isConnected = true;
            rectTransform.position = bucklePosition;
            OnStrapConnected?.Invoke();
            Debug.Log($"[JacketStrap] Strap {strapID} connected!");
        }

        public void ResetStrap()
        {
            isConnected = false;
            rectTransform.localPosition = startPosition;
        }
    }

    /// <summary>
    /// 구속복 버클 컴포넌트
    /// </summary>
    public class JacketBuckle : MonoBehaviour
    {
        [SerializeField] private int buckleID; // 버클의 고유 ID

        public int GetBuckleID() => buckleID;
    }
}
