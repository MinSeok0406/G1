using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Minseok
{
    public class UI_ChatScene : UI_Base
    {
        public GameObject _content;
        public ScrollRect _scroll;
        GameObject _contentText;

        enum GameObjects
        {
            Write,
        }

        enum Images
        {
            Chat_Button,
            Cancle,
        }

        public override void Init()
        {
            _contentText = _content.transform.GetChild(0).gameObject;

            Bind<GameObject>(typeof(GameObjects));
            Bind<Image>(typeof(Images));

            GetImage((int)Images.Chat_Button).gameObject.BindEvent(SendChat);
            GetImage((int)Images.Cancle).gameObject.BindEvent(Cancle);
        }

        public void Cancle(PointerEventData evt)
        {
            UI_BackGround backSceneUI = Managers.UI.SceneUI as UI_BackGround;
            UI_LobbyScene lobbyUI = backSceneUI.mLobbyUI;

            if (!lobbyUI.gameObject.activeSelf)
            {
                lobbyUI.gameObject.SetActive(true);
                Managers.UI.ShowPopupUI<UI_Joystick>("UI_Joystick");
                gameObject.SetActive(false);
            }
        }

        public void SendChat(PointerEventData evt)
        {
            string text = Get<GameObject>((int)GameObjects.Write).GetComponent<TMP_InputField>().text;

            Debug.Log(text);

            C_Chat chat = new C_Chat();
            chat.Msg = text;
            chat.Type = MessageType.Public;

            Get<GameObject>((int)GameObjects.Write).GetComponent<TMP_InputField>().text = "";

            if (chat.Msg == "")
            {
                return;
            }

            Managers.Network.Send(chat);
        }

        public void ReadChat(S_Chat chatPacket)
        {
            //Get<TMP_Text>((int)Texts.LogText).text = $"{chatPacket.SenderId}: {chatPacket.Msg}";

            /*GameObject goText = Instantiate(_contentText, _content.transform);
            goText.GetComponent<TMP_Text>().text = $"{chatPacket.SenderId}: {chatPacket.Msg}";
            _content.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;*/

            GameObject goText = Instantiate(_contentText, _content.transform);
            TMP_Text textComponent = goText.GetComponent<TMP_Text>();
            textComponent.text = $"{chatPacket.SenderId}: {chatPacket.Msg}";

            if (Managers.Object.MyPlayer.Id == chatPacket.SenderId)
            {
                textComponent.alignment = TextAlignmentOptions.Right;
                textComponent.color = Color.red;
            }
            else
            {
                textComponent.alignment = TextAlignmentOptions.Left;
                textComponent.color = Color.blue;
            }

            RectTransform contentRect = _content.GetComponent<RectTransform>();
            float contentHeight = contentRect.rect.height;

            float newYPosition = contentHeight;  // Move the content down by the new content height
            contentRect.anchoredPosition = new Vector2(contentRect.anchoredPosition.x, newYPosition);

            _scroll = _content.GetComponentInParent<ScrollRect>();
            if (_scroll != null)
            {
                Canvas.ForceUpdateCanvases(); // Ensure the layout is updated immediately
                _scroll.verticalNormalizedPosition = 0f; // Scroll to the bottom
            }
        }
    }
}