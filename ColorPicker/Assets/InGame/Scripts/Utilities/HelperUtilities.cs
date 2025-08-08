using ColorPicker.InGame;
using Photon.Pun;
using System;
using System.Collections.Generic;
using UnityEngine;

public  static class HelperUtilities 
{
    /// <summary>
    /// 스크린 좌표를 로컬 좌표(RectTransform 기준)로 변환해주는 함수
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
    /// 커스터마이징 색상(Enum)을 Unity의 Color 타입으로 변환해주는 함수  
    /// 플레이어 색상 적용 시 사용
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
    /// 현재 GameState 객체를 GameStateType enum 값으로 매핑하는 함수  
    /// </summary>
    public static GameStateType ToPhase(GameState state)
    {
        switch(state)
        {
            case GameStartedState:
                return GameStateType.GameStarted;
            case PlayingGameState:
                return GameStateType.Playing;
            case MeetingState :
                return GameStateType.Meeting;
            case VotingState:
                return GameStateType.Voting;

            default:
                Debug.Log($"Unknown GameState: {state.GetType().Name}");
                return GameStateType.None;
        };
    }

    /// <summary>
    /// GameStateType enum 값을 실제 GameState 객체로 변환하는 함수  
    /// </summary>
    public static GameState ToState(GameStateType phase)
    {
        switch(phase)
        {
            case GameStateType.GameStarted : 
                return GameManager.Instance.GameStartedState;
            case GameStateType.Playing :
                return GameManager.Instance.PlayingGameState;
            case GameStateType.Meeting:
                return GameManager.Instance.MeetingState;
            case GameStateType.Voting :
                return GameManager.Instance.VotingState;

            default:
                Debug.Log($"Not find state : {phase}");
                return null;
        }
    }

    /// <summary>
    /// 리스트의 요소들을 무작위로 섞는 함수  
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
    public static bool IsRoomView(this PhotonView view) // this 확장 매서드로 마치 view 객체의 원래 메서드처럼 사용 가능
    {
        return view.GetComponent<RoomObjectMarker>() != null ||
               view.gameObject.name.StartsWith("Room_");
    }

    
}
