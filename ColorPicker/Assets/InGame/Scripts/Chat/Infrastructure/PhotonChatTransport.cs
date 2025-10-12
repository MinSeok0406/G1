using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace ColorPicker.Chat
{

    public sealed class PhotonChatTransport : MonoBehaviourPun
    {
        private IAliveQuery _aliveQuery;
        public void Init(IAliveQuery aliveQuery) => _aliveQuery = aliveQuery;

        // 어댑터 구현체를 노출
        public IChatTransport AsAdapter() => new Adapter(this);

        private sealed class Adapter : IChatTransport
        {
            private readonly PhotonChatTransport _self;
            public Adapter(PhotonChatTransport self) { _self = self; }

            public bool IsServer => PhotonNetwork.IsMasterClient;
            public double NowSeconds => Time.realtimeSinceStartupAsDouble;

            public bool ValidateSenderContext(object ctx, int declaredActorNumber)
            {
                // PunRPC의 PhotonMessageInfo를 ctx로 전달받는 전제
                if (ctx is PhotonMessageInfo info)
                    return info.Sender != null && info.Sender.ActorNumber == declaredActorNumber;
                return false;
            }

            public void BroadcastToAll(ChatMessage msg, bool isGhost)
            {
                _self.photonView.RPC(nameof(_self.C_ReceiveMessage),
                RpcTarget.AllViaServer, msg.Seq, msg.PlayerId, msg.Text, msg.ServerTimestamp, isGhost);
            }

            public void BroadcastToGhostOnly(ChatMessage msg)
            {
                foreach (var p in PhotonNetwork.PlayerList)
                {
                    if (_self._aliveQuery != null &&
                        _self._aliveQuery.TryGetAliveState(p.ActorNumber, out bool has, out bool alive) &&
                        has && !alive)
                    {
                        _self.photonView.RPC(nameof(_self.C_ReceiveMessage), p,
                            msg.Seq, msg.PlayerId, msg.Text, msg.ServerTimestamp, true);
                    }
                }
            }

            public void SendBulkTo(int targetActorNumber, List<ChatMessage> batch)
            {
                var target = FindPlayer(targetActorNumber);
                if (target == null) return;

                int len = batch.Count;
                var seqs = new int[len];
                var pids = new int[len];
                var texts = new string[len];
                var tss = new int[len];
                var ghosts = new bool[len];

                for (int i = 0; i < len; i++)
                {
                    var m = batch[i];
                    seqs[i] = m.Seq; pids[i] = m.PlayerId; texts[i] = m.Text; tss[i] = m.ServerTimestamp;
                    ghosts[i] = m.Channel == ChatChannel.Ghost;
                }

                _self.photonView.RPC(nameof(_self.C_BulkReceive), target, seqs, pids, texts, tss, ghosts);
            }

            private static Player FindPlayer(int actorNumber)
            {
                var arr = PhotonNetwork.PlayerList;
                for (int i = 0; i < arr.Length; i++)
                    if (arr[i].ActorNumber == actorNumber) return arr[i];
                return null;
            }
        }

        // === 클라 수신 RPC (UI 계층에서 구독) ===
        [PunRPC]
        public void C_ReceiveMessage(int seq, int playerId, string text, int ts, bool isGhost)
            => OnClientReceive?.Invoke(new ChatMessage(seq, playerId, text, ts, isGhost ? ChatChannel.Ghost : ChatChannel.Global));

        [PunRPC]
        public void C_BulkReceive(int[] seqs, int[] pids, string[] texts, int[] tss, bool[] ghosts)
            => OnClientBulkReceive?.Invoke(seqs, pids, texts, tss, ghosts);

        public System.Action<ChatMessage> OnClientReceive;
        public System.Action<int[], int[], string[], int[], bool[]> OnClientBulkReceive;
    }
}