using Photon.Pun;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ColorPicker.InGame
{
    public class PlayerClassAssigner
    {

        public void AssignRoles()
        {
            if (!PhotonNetwork.IsMasterClient) return;
            
            List<PrivatePlayerData> playerDataList = GameDataManager.Instance.GetAllPrivatePlayerData();
            GameRuleSettings gameRules = GameDataManager.Instance.GetGameRules(); //게임 세팅 가져오기

            int totalRequired = gameRules.mafiaAmount + gameRules.detectiveAmount;
            
            // 유효성 검사
            if (playerDataList.Count < totalRequired)
            {
                Debug.LogError("[PlayerClassAssigner] 플레이어 수가 역할 수보다 적습니다. 분배 중단.");
                return;
            }

            List<PrivatePlayerData> shuffled = playerDataList.OrderBy(_ => UnityEngine.Random.value).ToList(); // 랜덤 값으로 순서를 변경해 리스트형태로 반환

            int assigned = 0;

            // 직업 종류 데이터 추후 자동화 업데이트 필요 **********
            AssignRoleToCount(shuffled, PlayerClassType.mafia, gameRules.mafiaAmount, ref assigned); // 랜덤으로 섞인 리스트 기준으로 마피아 수 만큼 할당
            AssignRoleToCount(shuffled, PlayerClassType.detective, gameRules.detectiveAmount, ref assigned); // 랜덤으로 섞인 리스트 기준으로 탐정 수 만큼 할당

            // 나머지 플레이어 시민으로 할당
            for (int i = assigned; i < shuffled.Count; i++)
            {
                shuffled[i].classType = (int)PlayerClassType.citizen;
                GameDataManager.Instance.UpdatePrivatePlayerData(shuffled[i]);
            }

            AssignColorsBasedOnClass();

            GameDataManager.Instance.SyncAllPlayerDataToClients();
        }

        /// <summary>
        /// 플레이어 데이터리스트를 받아 count 만큼 할당 하는 함수 (ref index)로 참조해 사용
        /// </summary>
        /// <param name="list"></param>
        /// <param name="role"></param>
        /// <param name="count"></param>
        /// <param name="index"></param>
        private void AssignRoleToCount(List<PrivatePlayerData> list, PlayerClassType role, int count, ref int index)
        {
            for (int i = 0; i < count && index < list.Count; i++, index++)
            {
                list[index].classType = (int)role;
                GameDataManager.Instance.UpdatePrivatePlayerData(list[index]);
            }
        }

        private void AssignColorsBasedOnClass()
        {
            var availableColors = System.Enum.GetValues(typeof(ColorType))
                .Cast<ColorType>()
                .Where(c => c != ColorType.Black && c != ColorType.White)
                .OrderBy(_ => UnityEngine.Random.value)
                .ToList();

            List<PrivatePlayerData> players = GameDataManager.Instance.GetAllPrivatePlayerData();

            int colorIndex = 0;

            foreach (var player in players)
            {
                if (player.classType == (int)PlayerClassType.mafia)
                {
                    player.identityColorId = (int)ColorType.Black;
                }
                else
                {
                    if (colorIndex >= availableColors.Count)
                    {
                        Debug.LogWarning("[PlayerClassAssigner] 사용 가능한 색상이 부족합니다.");
                        player.identityColorId = (int)ColorType.White;
                    }
                    else
                    {
                        player.identityColorId = (int)availableColors[colorIndex++];
                    }
                }

                GameDataManager.Instance.UpdatePrivatePlayerData(player);
            }
        }
    }
}

