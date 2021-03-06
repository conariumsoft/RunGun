﻿using RunGun.Core.Generic;
using RunGun.Core.Networking;
using RunGun.Core.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace RunGun.Client.Networking
{
	public class NetworkClient : IClient {
		public bool IsConnected { get; private set; }
		public bool IsConnecting { get; private set; }

		public int DataTotalOut { get; set; }
		public int DataTotalIn { get; set; }

		public int DataCountOut { get; set; }
		public int DataCountIn { get; set; }

		public float DataAverageIn { get; set; } = 1;
		public float DataAverageOut { get; set; } = 1;

		public CircularArray<int> DataSamplesIn { get; set; }
		public CircularArray<int> DataSamplesOut { get; set; }

		float reset = 0;

		public IPEndPoint EndPoint;

		UdpClient udpClient;
		Queue<UdpReceiveResult> messageQueue;
		bool isListenThreadRunning = true;
		public delegate void Callback<T>(T packet);
		public delegate void ListCallback<T, U>(T header, List<U> slices);
		public delegate void EventListener(byte[] bytedata);
		public event EventListener OnServerMessage;

		public float connectionRetryTimer = 1.0f;
		public int connectionRetries = 5;

		public NetworkClient() {

			DataSamplesIn = new CircularArray<int>(50);
			DataSamplesOut = new CircularArray<int>(50);

			messageQueue = new Queue<UdpReceiveResult>();
			udpClient = new UdpClient();

			AddListener<SPConnectAccept>(Protocol.S_ConnectOK, (packet) => {
				IsConnected = true;
				IsConnecting = false;
			}); 
		}
		~NetworkClient() {}

		public void AddListener<T>(Protocol code, Callback<T> method) where T : new() {
			OnServerMessage += (bytedata) => {
				
				if (bytedata[0] == (byte)code) {
					method(ClassSerializer.Deserialize<T>(bytedata));
				}
			};
		}
		public void AddListener<T, U>(Protocol code, ListCallback<T, U> method) where T : new() where U : new() {
			OnServerMessage += (bytedata) => {
				if (bytedata[0] == (byte)code) {
					int headerSize = ClassSerializer.GetProfile(typeof(T)).BufferLength;
					int sliceSize = ClassSerializer.GetProfile(typeof(U)).BufferLength;
					int numSlices = (bytedata.Length - headerSize) / sliceSize;
					byte[] headerData = new byte[headerSize];
					Array.Copy(bytedata, 0, headerData, 0, headerSize);
					T header = ClassSerializer.Deserialize<T>(headerData);
					List<U> slices = new List<U>();

					for (int i = 0; i < numSlices; i++) {
						byte[] sliceData = new byte[sliceSize];
						Array.Copy(bytedata, headerSize + (sliceSize * i), sliceData, 0, sliceSize);
						slices.Add(ClassSerializer.Deserialize<U>(sliceData));
					}
					method(header, slices);
				}
			};
		}

		private bool TryEstablishUdpConnection(IPEndPoint endpoint) {
			try {
				udpClient.Connect(endpoint);
			} catch (Exception) {
				return false;
			}
			return true;
		}

		public bool Connect(IPEndPoint endpoint, string nickname) {
			for (int i = 0; i < connectionRetries; i++) {
				bool result = TryEstablishUdpConnection(endpoint);

				if (result == true) {
					StartListeningThread();
					IsConnecting = true;
					Send(new CConnectRequest(nickname));
					Console.WriteLine("Lets GO");
					return true;
				}	
			}
			return false;
		}
		public void Disconnect() {
			Send(new CDisconnect());
			isListenThreadRunning = false;
			IsConnecting = false;
			IsConnected = false;
		}
		public void Send<T>(T packet) where T : IPacket {
			if (IsConnecting == false && IsConnected == false)
				return;
			
			byte[] data = ClassSerializer.Serialize(packet);
			
			try {
				if (udpClient != null) {
					DataCountOut += data.Length;
					DataTotalOut += data.Length;
					udpClient.Send(data, data.Length);
					ByteUtil.DumpNum(data);
				}
			} catch (SocketException exception) {
				Console.WriteLine(exception.ToString());
				// Error codes:
				// https://docs.microsoft.com/en-us/windows/win32/winsock/windows-sockets-error-codes-2
			}
		}
		public void Send<T, U>(T packet, U[] slices) where T : IPacketHeader where U : IDataSlice {
			if (IsConnecting == false && IsConnected == false)
				return;

			int headerSize = ClassSerializer.GetProfile(typeof(T)).BufferLength;
			int sliceSize = ClassSerializer.GetProfile(typeof(U)).BufferLength;
			byte[] datagram = new byte[headerSize + (sliceSize * slices.Length)];

			byte[] header = ClassSerializer.Serialize(packet);
			ByteUtil.Put(0, header, ref datagram);

			for (int i = 0; i < slices.Length; i++) {
				byte[] slicedata = ClassSerializer.Serialize(slices[i]);
				ByteUtil.Put(headerSize + (i * sliceSize), slicedata, ref datagram);
			}
			ByteUtil.DumpNum(datagram);

			try {
				DataCountOut += datagram.Length;
				DataTotalOut += datagram.Length;
				udpClient.Send(datagram, datagram.Length);
			} catch (SocketException exception) {
				Console.WriteLine(exception.ToString());
				// Error codes:
				// https://docs.microsoft.com/en-us/windows/win32/winsock/windows-sockets-error-codes-2
			}
		}


		int fart = 0;
		public void Update(float dt) {
			if (IsConnecting) {
				connectionRetryTimer += dt;

				if (connectionRetryTimer > 1) {
					//TryEstablishUdpConnection();
					connectionRetryTimer = 0;

				}
			}
			
			ReadPacketQueue();



			reset += dt;

			if (reset > (1.0f/10.0f)) {
				reset = 0;
				fart++;

				DataAverageOut -= DataSamplesOut.Get(fart);
				DataSamplesOut.Set(fart, DataCountOut);
				DataAverageOut += DataSamplesOut.Get(fart);

				DataCountOut = 0;

				DataAverageIn -= DataSamplesIn.Get(fart);
				DataSamplesIn.Set(fart, DataCountIn);
				DataAverageIn += DataSamplesIn.Get(fart);

				DataCountIn = 0;
			}
		}
		private async void NetworkThread() {
			while (isListenThreadRunning) {
				try {
					var received = await udpClient.ReceiveAsync();

					DataCountIn += received.Buffer.Length;
					DataTotalIn += received.Buffer.Length;
					lock (messageQueue) {
						messageQueue.Enqueue(received);
					}
				} catch (Exception ex) {
					Console.WriteLine("CLIENT NETWORK ERR: " + ex.Message + " " + ex.Source + " " + ex.StackTrace);
					throw ex;
				} finally {

				}
			}
		}
		private void StartListeningThread() {
			Task.Factory.StartNew(NetworkThread);
		}
		private void ReadPacketQueue() {
			for (int i = 0; i < messageQueue.Count; i++) {
				var recv = messageQueue.Dequeue();

				ByteUtil.DumpNum(recv.Buffer);
				OnServerMessage?.Invoke(recv.Buffer);
			}
		}
		private void Send(byte[] packet) {
			if (IsConnected && udpClient != null) {
				udpClient.Send(packet, packet.Length);
			}
		}
	}
}