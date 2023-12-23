using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using UnityEngine.SceneManagement;

public class NetworkManager : MonoBehaviour
{
    public static readonly string ServerAddress = "lucadion.mcv.kr"; 
    public static readonly int ServerPort = 7777;
    public static NetworkManager Instance { get; private set; }

    private Socket _socket;
    private readonly byte[] _buffer = new byte[1024];
    private readonly StringBuilder _packetBuilder = new();

    public IClient Client;
    public PingData PingData = new();
    public RoomInfo[] RoomInfos = new RoomInfo[0];
    public bool IsPingDataSetted = false;

    public bool IsInRoom { get => PingData.RoomID?.Length > 0; }

    public async UniTask Connect()
    {
        _socket = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
        {
            NoDelay = true
        };
        var serverIp = Dns.GetHostAddresses(ServerAddress)[0];
        var endPoint = new IPEndPoint(serverIp, ServerPort);
        await _socket.ConnectAsync(endPoint);
        BeginReceive().Forget();
        GetRoomListTask().Forget();
    }

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        Connect().Forget();
    }

    private async UniTask GetRoomListTask()
    {
        while(true)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(1));
            SendPacket("server", "get-room-list", "");
        }
    }

    public void SendPacket(string data)
    {
        try
        {
            _socket.SendAsync(Encoding.UTF8.GetBytes(data + '\0'), SocketFlags.None);
        }
        catch (ObjectDisposedException) { }
    }

    public void SendPacket(string target, string eventName, string message)
    {
        SendPacket(target + ":" + eventName + ":" + message);
    }

    public void JoinRoom(string roomId)
    {
        SendPacket("server", "join-room", roomId);
    }

    public void KickPlayer(string uid)
    {
        SendPacket("server", "kick-player", uid);
    }

    public void ChangeNickname(string name)
    {
        SendPacket("server", "change-nickname", name);
    }

    public void LeaveRoom()
    {
        SendPacket("server", "leave-room", "");
    }

    public void CreateRoom()
    {
        SendPacket("server", "create-room", "");
    }

    private async UniTask BeginReceive()
    {
        while(_socket is not null && _socket.Connected)
        {
            int received = 0;
            try
            {
                received = await _socket.ReceiveAsync(_buffer, SocketFlags.None);
            }
            catch (ObjectDisposedException) { }

            if (received > 0)
            {
                var response = Encoding.UTF8.GetString(_buffer, 0, received);
                foreach (var ch in response)
                {
                    if(ch == '\0')
                    {
                        try
                        {
                            ProcessPacket(_packetBuilder.ToString());
                        }
                        catch(Exception e)
                        {
                            Debug.LogError(_packetBuilder.ToString());
                            Debug.LogError(e);
                        }
                        _packetBuilder.Clear();
                    }
                    else _packetBuilder.Append(ch);
                }
            }
        }
    }

    private void ProcessPacket(string packet)
    {
        var splitResult = packet.Split(':', 3);

        if (splitResult.Length == 3)
        {
            var target = splitResult[0];
            var eventName = splitResult[1];
            var message = splitResult[2];

            OnEvent(target, eventName, message);
        }
    }

    public void SetRoomState(string key, string value)
    {
        if (IsPingDataSetted && PingData.IsMasterClient)
        {
            SendPacket("server", "set-room-state", key + ":" + value);
            PingData.RoomState[key] = value;
        }
    }

    public void RemoveRoomState(string key)
    {
        if (IsPingDataSetted && PingData.IsMasterClient)
        {
            SendPacket("server", "remove-room-state", key);
            PingData.RoomState.Remove(key);
        }
    }

    private void OnEvent(string from, string eventName, string message)
    {
        if (from.Equals("server"))
        {
            switch (eventName)
            {
                case "room-list":
                    JArray roomList = JArray.Parse(message);
                    RoomInfos = new RoomInfo[roomList.Count];
                    for(var i = 0; i < roomList.Count; i++)
                    {
                        if (roomList[i].Type != JTokenType.Object) continue;
                        JObject roomInfoJson = (JObject)roomList[i];
                        RoomInfos[i] = new()
                        {
                            UID = roomInfoJson["uid"] + "",
                            ClientCount = roomInfoJson["client_count"]?.Type == JTokenType.Integer ? (int)roomInfoJson["client_count"] : 0,
                            MaxClientCount = roomInfoJson["max_client_count"]?.Type == JTokenType.Integer ? (int)roomInfoJson["max_client_count"] : 0,
                            MasterClientNickname = roomInfoJson["master_client_nickname"] + ""
                        };
                    }
                    break;
                case "ping":
                    SendPacket("server", "pong", "");
                    JObject pingData = JObject.Parse(message);
                    PingData = new()
                    {
                        Ping = pingData["ping"]?.Type == JTokenType.Integer ? (int)pingData["ping"] : 0,
                        UID = pingData["uid"] + "",
                        RoomID = pingData["room_id"] + "",
                        Nickname = pingData["nickname"] + "",
                        MaxClientCount = pingData["max_client_count"]?.Type == JTokenType.Integer ? (int)pingData["max_client_count"] : 0,
                        IsMasterClient = pingData["is_master_client"]?.Type == JTokenType.Boolean && (bool)pingData["is_master_client"],
                        RoomState = new()
                    };
                    if (pingData["room_state"]?.Type == JTokenType.Object)
                    {
                        foreach(var entry in (JObject)pingData["room_state"])
                        {
                            PingData.RoomState.Add(entry.Key, entry.Value.ToString());
                        }
                    }
                    if (pingData["clients"]?.Type == JTokenType.Array)
                    {
                        var arr = pingData["clients"].Values<JObject>().ToArray();
                        PingData.Clients = new ClientInfo[arr.Length];

                        for(int i = 0; i < arr.Length; i++)
                        {
                            PingData.Clients[i] = new()
                            {
                                Ping = arr[i]["ping"]?.Type == JTokenType.Integer ? (int)arr[i]["ping"] : 0,
                                UID = arr[i]["uid"] + "",
                                Nickname = arr[i]["nickname"] + ""
                            };
                        }
                    }
                    IsPingDataSetted = true;
                    break;
                case "join-client":
                    Client.OnJoinClient(message);
                    break;
                case "join-room-failed":
                    Client.OnJoinFailed(message);
                    break;
                case "leave-client":
                    Client.OnLeaveClient(message);
                    break;
            }
        }
        else
        {
            Client.OnEvent(from, eventName, message);
        }
    }

    private void OnDestroy()
    {
        _socket?.Close();
    }
}

public struct RoomInfo
{
    public string UID;
    public int ClientCount, MaxClientCount;
    public string MasterClientNickname;
}

public struct ClientInfo
{
    public int Ping;
    public string UID, Nickname;
}

public struct PingData
{
    public int Ping;
    public string UID;
    public string Nickname;
    public string RoomID;
    public int MaxClientCount;
    public ClientInfo[] Clients;
    public bool IsMasterClient;
    public Dictionary<string, string> RoomState;
}