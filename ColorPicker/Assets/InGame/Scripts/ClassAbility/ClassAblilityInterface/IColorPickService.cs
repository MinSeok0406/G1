using Photon.Pun;
using UnityEngine;

namespace ColorPicker.InGame
{
    public interface IColorPickService
    {
        ColorPickResult ValidateAndJudge(int targetActorId, int killerActorId, int deductionColor, ICooldownService cooldowns, float wrongCooldown, PhotonView sender);
    }

    public sealed class ColorPickService : IColorPickService
    {
        public ColorPickResult ValidateAndJudge(int targetActorId, int killerActorId, int deductionColor, ICooldownService cooldowns, float wrongCooldown, PhotonView sender)
        {
            if (!PhotonNetwork.IsMasterClient) return ColorPickResult.Error;

            if (!GameDataManager.Instance.TryGetPrivatePlayerDataByActorId(targetActorId, out var target) ||
                !GameDataManager.Instance.TryGetPrivatePlayerDataByActorId(killerActorId, out var killer))
                return ColorPickResult.Error;

            if (target.classType == (int)PlayerClassType.mafia) return ColorPickResult.Error;
            if (target.classType == (int)PlayerClassType.ghost) return ColorPickResult.Error;

            GameDataManager.Instance.TryGetViewIDByUID(killer.googleUID, out int killerViewId);
            if (cooldowns.IsOnCooldown(killerViewId)) return ColorPickResult.Error;

            if (target.identityColorId == deductionColor)
                return ColorPickResult.Success;

            if (killerViewId > 0)
                cooldowns.StartCooldownHostOnly(killerViewId, wrongCooldown, sender);

            return ColorPickResult.Fail;
        }
    }
}
