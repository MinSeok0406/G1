using Google.Protobuf;
using Google.Protobuf.Protocol;
using ServerCore;
using UnityEngine;

class PacketHandler
{
    public static void S_PingHandler(PacketSession session, IMessage packet)
    {
        C_Pong pongPacket = new C_Pong();
		Debug.Log("[Server] PingCheck");
		Managers.Network.Send(pongPacket);
    }

    public static void S_MoveHandler(PacketSession session, IMessage packet)
    {
        S_Move move = packet as S_Move;
    }
}