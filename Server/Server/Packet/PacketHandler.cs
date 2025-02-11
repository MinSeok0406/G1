using Google.Protobuf;
using Google.Protobuf.Protocol;
using Server;
using Server.Game;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

class PacketHandler
{
	public static void C_PongHandler(PacketSession session, IMessage packet)
	{
		ClientSession clientSession = (ClientSession)session;
		clientSession.HandlePong();
	}
}
