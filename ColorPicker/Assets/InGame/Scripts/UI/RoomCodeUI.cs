using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoomCodeUI : MonoBehaviour
{
    [SerializeField] private TMP_Text roomCodeText;
    [SerializeField] private Button hideButton;
    [SerializeField] private Button showButton;

    [SerializeField] private bool startHidden = true; // 시작 시 숨김 여부
    [SerializeField] private char maskChar = '*';     // 마스킹 문자
    [SerializeField] private int minMaskLen = 4;      // 방 코드가 없을 때 최소 마스크 길이

    private bool isHidden;

    private void Awake()
    {
        isHidden = startHidden;
    }

    private void OnEnable()
    {
        Refresh();
    }

    public void OnClick_ShowRoomCode()
    {
        isHidden = false;
        Debug.Log("a");
        Refresh();
    }

    public void OnClick_HideRoomCode()
    {
        isHidden = true;
        Refresh();
    }

    private void Refresh()
    {
        string code = GetRoomCode();

        if (isHidden)
        {
            int len = Mathf.Max(string.IsNullOrEmpty(code) ? 0 : code.Length, minMaskLen);
            roomCodeText.text = $"Code : {new string(maskChar, len)}";
        }
        else
        {
            roomCodeText.text = $"Code : {code}";
        }

        // 버튼 토글(숨김 상태면 Show만 보이게)
        if (showButton) showButton.gameObject.SetActive(!isHidden);
        if (hideButton) hideButton.gameObject.SetActive(isHidden);
    }

    private static string GetRoomCode()
    {
        var room = PhotonNetwork.CurrentRoom;
        return room != null ? room.Name : string.Empty;
    }
}