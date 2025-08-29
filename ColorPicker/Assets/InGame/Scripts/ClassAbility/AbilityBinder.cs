using Photon.Pun;
using UnityEngine;

namespace ColorPicker.InGame
{
    /// <summary>
    /// 로컬 유저의 직업에 맞춰 능력 스크립트/버튼을 활성화/비활성화.
    /// </summary>
    public class AbilityBinder : MonoBehaviourPun
    {
        [SerializeField] private MafiaAbility mafiaAbility;
        [SerializeField] private DetectiveAbility detectiveAbility;
        [SerializeField] private CitizenAbility citizenAbility;

        /// <summary>모든 능력 비활성화</summary>
        public void DisableAllAbilities()
        {
            if (mafiaAbility) mafiaAbility.DisableAbility();
            if (detectiveAbility) detectiveAbility.DisableAbility();
            if (citizenAbility) citizenAbility.DisableAbility();
        }

        public void BroadCastApplyLocalAbilities()
        {
            photonView.RPC(nameof(RPC_RefreshLocalAbilities), RpcTarget.All);
        }

        [PunRPC]
        public void RPC_RefreshLocalAbilities()
        {
            int actorId = PhotonNetwork.LocalPlayer?.ActorNumber ?? -1;
            if (actorId < 0) return;
            if (!GameDataManager.Instance.TryGetPrivatePlayerDataByActorId(actorId, out var pdata)) { return; }

            var cls = (PlayerClassType)pdata.classType;

            ApplyClass(cls);
        }

        public void TryAutoWire(Transform root)
        {
            if (!mafiaAbility) mafiaAbility = root.GetComponentInChildren<MafiaAbility>(true);
            if (!detectiveAbility) detectiveAbility = root.GetComponentInChildren<DetectiveAbility>(true);
            if (!citizenAbility) citizenAbility = root.GetComponentInChildren<CitizenAbility>(true);
        }

        private void ApplyClass(PlayerClassType cls)
        {
            DisableAllAbilities(); // 항상 전체 OFF 후 시작

            switch (cls)
            {
                case PlayerClassType.mafia:
                    citizenAbility?.EnableAbility();
                    mafiaAbility?.EnableAbility();
                    mafiaAbility?.SendMessage("RefreshKillButtonForLocalClass", SendMessageOptions.DontRequireReceiver);
                    break;

                case PlayerClassType.detective:
                    citizenAbility?.EnableAbility();
                    detectiveAbility?.EnableAbility();
                    break;

                default:
                    citizenAbility?.EnableAbility();
                    break;
            }
        }
    }
}
