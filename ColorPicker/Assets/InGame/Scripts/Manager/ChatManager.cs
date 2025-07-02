using Photon.Pun;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ColorPicker.InGame
{
    public class ChatManager : SingletonNetworkBehaviour<ChatManager>
    {
        private List<ChatMessage> chatMessages;

        [SerializeField] GameObject chatContentContainer;
        [SerializeField] TMP_InputField messageInputField;

        protected override void Awake()
        {
            base.Awake();
        }

        private void Start()
        {
            S_InitializedChatList();

            C_InitializedChat();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Z))
            {
                C_InitializedChat();
            }
        }

        private void S_InitializedChatList()
        {
            if (!PhotonNetwork.IsMasterClient) return;

            if(chatMessages == null)
            {
                chatMessages = new List<ChatMessage>();
            }

            chatMessages.Clear();
        }

        [PunRPC]
        private void C_InitializedChat()
        {
            foreach (Transform content in chatContentContainer.transform)
            {
                Destroy(content.gameObject);
            }
        }

        public void C_SendMessage()
        {
            string message = messageInputField.text;

            if (!string.IsNullOrEmpty(message))
            {
                photonView.RPC("S_ReciveMessage", RpcTarget.MasterClient, PhotonNetwork.LocalPlayer.ActorNumber, message);
                messageInputField.text = "";

                messageInputField.ActivateInputField();
                messageInputField.Select();
            }
        }

        [PunRPC]
        private void S_ReciveMessage(int playerId, string message)
        {
            if (!PhotonNetwork.IsMasterClient) return;

            if(chatMessages == null)
            {
                chatMessages = new List<ChatMessage>();
            }

            chatMessages.Add(new ChatMessage(playerId, message));

            photonView.RPC("C_ReciveMessages", RpcTarget.All, chatMessages.Last().playerId, chatMessages.Last().message);
        }

        [PunRPC]
        private void C_ReciveMessages(int playerId, string message)
        {

            GameObject chatBubble = Instantiate(playerId == PhotonNetwork.LocalPlayer.ActorNumber ?
                GameResources.Instance.myChatContentPrefab : GameResources.Instance.chatContentPrefab,
                chatContentContainer.transform);

            TMP_Text[] messageText = chatBubble.GetComponentsInChildren<TMP_Text>();
            messageText[0].text = $"{playerId}";
            messageText[1].text = $"{message}";

            chatBubble.SetActive(chatContentContainer.activeSelf);

            ScrollRect scrollRect = chatBubble.GetComponentInParent<ScrollRect>();

            if (scrollRect != null)
            {
                scrollRect.verticalNormalizedPosition = 0;
            }
        }

        public void C_DisablePlayerControl(bool disableControl)
        {
            PlayerManager.Instance.GetMyPlayer().GetComponent<PlayerControl>().DisablePlayerControl(disableControl);
        }
    }

    [System.Serializable]
    public class ChatMessage
    {
        public int playerId;
        public string message; 

        public ChatMessage(int playerId, string message)
        {
            this.playerId = playerId;
            this.message = message;
        }
    }
}
