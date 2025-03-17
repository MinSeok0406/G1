using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_ChatScene : UI_Base
{
    public GameObject _content;
    GameObject _contentText;

    enum GameObjects
    {
        Write
    }

    enum Texts
    {
        LogText
    }

    enum Images
    {
        Chat_Button
    }

    public override void Init()
    {
        _contentText = _content.transform.GetChild(0).gameObject;

        Bind<GameObject>(typeof(GameObjects));
        Bind<TMP_Text>(typeof(Texts));
        Bind<Image>(typeof(Images));

        GetImage((int)Images.Chat_Button).gameObject.BindEvent(SendChat);
    }

    public void SendChat(PointerEventData evt)
    {
        string text = Get<GameObject>((int)GameObjects.Write).GetComponent<TMP_InputField>().text;

        Debug.Log(text);

        C_Chat chat = new C_Chat();
        chat.Msg = text;
        chat.Type = MessageType.Public;
        Managers.Network.Send(chat);

        Get<GameObject>((int)GameObjects.Write).GetComponent<TMP_InputField>().text = "";
    }

    public void ReadChat(S_Chat chatPacket)
    {
        //Get<TMP_Text>((int)Texts.LogText).text = $"{chatPacket.SenderId}: {chatPacket.Msg}";
        GameObject goText = Instantiate(_contentText, _content.transform);
        goText.GetComponent<TMP_Text>().text = $"{chatPacket.SenderId}: {chatPacket.Msg}";
        _content.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;
    }
}
