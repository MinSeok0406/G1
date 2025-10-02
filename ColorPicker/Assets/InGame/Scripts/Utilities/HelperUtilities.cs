using ColorPicker.InGame;
using ExitGames.Client.Photon;
using Photon.Pun;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ColorPicker.InGame
{
    public static class HelperUtilities
    {
        /// <summary>
        /// ��ũ�� ��ǥ�� ���� ��ǥ(RectTransform ����)�� ��ȯ���ִ� �Լ�
        /// </summary>
        public static Vector2 ScreenToLocalPointInRect(Canvas canvas, RectTransform targetRect, Vector2 screenPosition)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                targetRect,
                screenPosition,
                canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
                out Vector2 localPoint
            );

            return localPoint;
        }

        /// <summary>
        /// Ŀ���͸���¡ ����(Enum)�� Unity�� Color Ÿ������ ��ȯ���ִ� �Լ�  
        /// �÷��̾� ���� ���� �� ���
        /// </summary>
        public static Color GetUnityColor(CustomizationColor color)
        {
            switch (color)
            {
                case CustomizationColor.Red: return Color.red;
                case CustomizationColor.Green: return Color.green;
                case CustomizationColor.Blue: return Color.blue;
                case CustomizationColor.Yellow: return Color.yellow;
                case CustomizationColor.Cyan: return Color.cyan;
                case CustomizationColor.Orange: return new Color(1.0f, 0.647f, 0.0f);
                case CustomizationColor.Purple: return new Color(0.5f, 0.0f, 0.5f);
                case CustomizationColor.Pink: return new Color(1.0f, 192 / 255f, 203 / 255f);
                case CustomizationColor.Brown: return new Color(0.65f, 0.16f, 0.16f);
                case CustomizationColor.White: return Color.white;

                default: return Color.white;
            }
        }

        /// <summary>
        /// ColorType에 대응하는 UnityEngine.Color를 반환
        /// </summary>
        public static Color ToUnityColor(ColorType type)
        {
            switch (type)
            {
                case ColorType.White: return Color.white;
                case ColorType.Red: return Color.red;
                case ColorType.Green: return Color.green;
                case ColorType.Blue: return Color.blue;
                case ColorType.Yellow: return Color.yellow;
                case ColorType.Orange: return new Color(1f, 0.5f, 0f);         // 오렌지 (255,128,0)
                case ColorType.Purple: return new Color(0.5f, 0f, 0.5f);       // 퍼플 (128,0,128)
                case ColorType.Cyan: return Color.cyan;
                case ColorType.Pink: return new Color(1f, 0.4f, 0.7f);       // 핑크 (255,102,178)
                case ColorType.Brown: return new Color(0.6f, 0.3f, 0.1f);     // 브라운 (153,76,25)
                case ColorType.Black: return Color.black;
                default: return Color.magenta; // 매핑 안된 경우 디버그 색
            }
        }
        

        /// <summary>
        /// ���� GameState ��ü�� GameStateType enum ������ �����ϴ� �Լ�  
        /// </summary>
        public static GameStateType ToPhase(GameState state)
        {
            switch (state)
            {
                case GameStartedState:
                    return GameStateType.GameStarted;
                case PlayingGameState:
                    return GameStateType.Playing;
                case MeetingState:
                    return GameStateType.Meeting;

                default:
                    Debug.Log($"Unknown GameState: {state.GetType().Name}");
                    return GameStateType.None;
            }
            ;
        }

        /// <summary>
        /// GameStateType enum ���� ���� GameState ��ü�� ��ȯ�ϴ� �Լ�  
        /// </summary>
        public static GameState ToState(GameStateType phase)
        {
            switch (phase)
            {
                case GameStateType.GameStarted:
                    return GameManager.Instance.GameStartedState;
                case GameStateType.Playing:
                    return GameManager.Instance.PlayingGameState;
                case GameStateType.Meeting:
                    return GameManager.Instance.MeetingState;

                default:
                    Debug.Log($"Not find state : {phase}");
                    return null;
            }
        }

        /// <summary>
        /// ����Ʈ�� ��ҵ��� �������� ���� �Լ�  
        /// </summary>
        public static void Shuffle<T>(this List<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = UnityEngine.Random.Range(0, n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
        public static bool IsRoomView(this PhotonView view) // this Ȯ�� �ż���� ��ġ view ��ü�� ���� �޼���ó�� ��� ����
        {
            return view.GetComponent<RoomObjectMarker>() != null ||
                   view.gameObject.name.StartsWith("Room_");
        }

        /// <summary>
        /// ���� ���� CurrentScene Ŀ���� ������Ƽ�� �����մϴ�.
        /// </summary>
        /// <param name="sceneName">������ �� �̸� (��: "LobbyScene", "InGameScene")</param>
        public static void SetCurrentScene(string sceneName)
        {
            if (PhotonNetwork.InRoom && PhotonNetwork.CurrentRoom != null && PhotonNetwork.IsMasterClient)
            {
                Hashtable props = new Hashtable
                {
                    { Settings.currentSceneKey, sceneName }
                };

                PhotonNetwork.CurrentRoom.SetCustomProperties(props);
                Debug.Log($"[RoomSceneUtils] CurrentScene set to: {sceneName}");
            }
            else
            {
                Debug.LogWarning("[RoomSceneUtils] Failed to set CurrentScene. Not in room or not master.");
            }
        }

    }

}