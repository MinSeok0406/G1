using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ServerCore
{
	public class Listener
	{
		Socket _listenSocket;
		Func<Session> _sessionFactory;

		public void Init(IPEndPoint endPoint, Func<Session> sessionFactory, int register = 16, int backlog = 100)
		{
            if (endPoint == null) throw new ArgumentNullException(nameof(endPoint));
            if (sessionFactory == null) throw new ArgumentNullException(nameof(sessionFactory));

            // ✅ 세션 팩토리 대입(중요)
            _sessionFactory = sessionFactory;

            // ✅ 포트만 사용하고, 주소는 안전하게 IPv4 Any(0.0.0.0)로 강제
            int port = endPoint.Port;
            var bindEndPoint = new IPEndPoint(IPAddress.Any, port);

            _listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _listenSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

            /*_listenSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
			_sessionFactory += sessionFactory;*/

            _listenSocket.Bind(bindEndPoint);
            _listenSocket.Listen(backlog);

            for (int i = 0; i < register; i++)
			{
				SocketAsyncEventArgs args = new SocketAsyncEventArgs();
				args.Completed += new EventHandler<SocketAsyncEventArgs>(OnAcceptCompleted);
				RegisterAccept(args);
			}

            Console.WriteLine($"[Listener] Listening on {bindEndPoint.Address}:{bindEndPoint.Port}");
        }

		void RegisterAccept(SocketAsyncEventArgs args)
		{
			args.AcceptSocket = null;

			try
			{
                bool pending = _listenSocket.AcceptAsync(args);
                if (pending == false)
                    OnAcceptCompleted(null, args);
            }
			catch (Exception e)
			{
                Console.WriteLine($"[Listener] Accept register failed: {e}");
            }
		}

		void OnAcceptCompleted(object sender, SocketAsyncEventArgs args)
		{
			try
			{
                if (args.SocketError == SocketError.Success)
                {
                    Session session = _sessionFactory.Invoke();
                    session.Start(args.AcceptSocket);
                    session.OnConnected(args.AcceptSocket.RemoteEndPoint);
                }
                else
                    Console.WriteLine($"[Listener] Accept error: {args.SocketError}");
            }
			catch (Exception e)
			{
                Console.WriteLine($"[Listener] Accept handler exception: {e}");
}

			RegisterAccept(args);
		}
	}
}
