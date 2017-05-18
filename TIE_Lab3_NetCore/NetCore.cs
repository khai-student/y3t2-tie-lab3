using System;
using System.Text;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace TIE_Lab3_NetCore
{
	/// <summary>
	/// Stores connection and stream to endpoint.
	/// </summary>
	public class Connection : IDisposable
	{
		private TcpClient _tcpConnection;
		public NetworkStream Stream { get; private set; }

		private Connection () { } 

		public Connection(string ip, UInt16 port = _inPort)
		{
			try
			{
				_tcpConnection = new TcpClient ();
				_tcpConnection.Connect(ip, port);
				Stream = _tcpConnection.GetStream();
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		public Connection(TcpClient client)
		{
			_tcpConnection = client;
		}

		~Connection()
		{
			Dispose ();
		}

		public void Dispose()
		{
			_tcpConnection.Close ();
		}

		/// <summary>
		/// Sends string to endpoint.
		/// </summary>
		/// <param name="text">Text to send.</param>
		public void Send(string text)
		{
			if (_tcpConnection != default(TcpClient))
			{
				byte[] data = System.Text.Encoding.UTF8.GetBytes(text);
				Stream.Write(data, 0, data.Length);
			}
		}

		/// <summary>
		/// Receive string.
		/// </summary>
		public string Receive()
		{
			if (_tcpConnection != default(TcpClient))
			{
				string result = "";

				while (!_tcpConnection.Available)
					;

				while (_tcpConnection.Available)
				{
					byte[] data = new byte[65536];
					int bytes = Stream.Read(data, 0, data.Length);
					result += Encoding.UTF8.GetString (data, 0, bytes);
				}

				return result;
			}
		}
	}

	public class Worker : IDisposable
	{
		/// <summary>
		/// Port to listen incoming.
		/// </summary>
		UInt16 _port = 23001;

		private IPAddress _ipAddress;
		private TcpListener _tcpListener;
		private Connection _connection;

		public Worker (UInt16 port = 23001)
		{
			try 
			{
				_ipAddress = IPAddress.Parse("127.0.0.1");
				_tcpListener = new TcpListener(_ipAddress, _port);
				_tcpListener.Start();
			} 
			catch (Exception ex) 
			{
				throw new Exception (ex.Message);
			}
		}

		~Worker()
		{
			Dispose ();
		}

		public void Dispose()
		{
			try
			{
				_tcpListener.Stop ();
			}
			catch {
				;
			}
		}

		public Connection ConnectTo(string ip, UInt16 port = _port)
		{
			try
			{
				_connection = new Connection (ip, port);
				return _connection;
			}
			catch (Exception ex) {
				throw ex;
			}
		}

		public void Disconnect(Stream stream)
		{
			_connection.Dispose ();
		}

		/// <summary>
		/// Listen for a new connection.
		/// </summary>
		public Connection Listen()
		{
			try
			{
				_connection = new Connection (_tcpListener.AcceptTcpClient());
				return _connection;
			}
			catch (Exception ex) {
				throw ex;
			}

			return null;
		}
	}
}

