using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;



public class RoomBroadcaster : MonoBehaviour
{
    public string roomName = "MyRoom";
    public int stage = 10;
    public int currentPlayers = 1;
    private int maxPlayers = 2;
    private int discoveryPort = 47778;

    private UdpClient server;
    private bool isRunning;
    public string localIP { get; private set; }

    private void Start()
    {
        localIP = GetLocalIPAddress();
    }
    public void StartBroadcast()
    {
        if (isRunning) return;
        server = new UdpClient(discoveryPort);
        server.Client.ReceiveTimeout = 1000;
        isRunning = true;
        StartCoroutine(HandleRequests());
    }
    public void StopBroadcast()
    {
        isRunning = false;
        server?.Close();
        StopCoroutine(HandleRequests());

        //Debug.Log("[RoomBroadcast] Stopped.");
    }
    private IEnumerator HandleRequests()
    {
        IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
        while (isRunning)
        {
            if (server.Available > 0)
            {
                byte[] data = server.Receive(ref remoteEP);
                string req = Encoding.UTF8.GetString(data);
                if (req == "DISCOVER_ROOM")
                {
                    string response = $"{roomName}|{currentPlayers}|{maxPlayers}|{localIP}";
                    byte[] respBytes = Encoding.UTF8.GetBytes(response);
                    server.Send(respBytes, respBytes.Length, remoteEP);
                }
            }
            yield return null;
        }
        isRunning = false;
    }
    private string GetLocalIPAddress()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
                try
        {
            using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                var wifiManager = activity.Call<AndroidJavaObject>("getSystemService", "wifi");
                int ipInt = wifiManager.Call<AndroidJavaObject>("getConnectionInfo").Call<int>("getIpAddress");
                string ip = string.Format("{0}.{1}.{2}.{3}",
                    ipInt & 0xff,
                    (ipInt >> 8) & 0xff,
                    (ipInt >> 16) & 0xff,
                    (ipInt >> 24) & 0xff);
                return ip;
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning("[RoomBroadcaster] Failed to get local IP: " + e.Message);
            return "127.0.0.1";
        }
#else
        foreach (var ni in System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces())
        {
            if ((ni.NetworkInterfaceType == System.Net.NetworkInformation.NetworkInterfaceType.Wireless80211 ||
                 ni.NetworkInterfaceType == System.Net.NetworkInformation.NetworkInterfaceType.Ethernet) &&
                ni.OperationalStatus == System.Net.NetworkInformation.OperationalStatus.Up)
            {
                foreach (var ua in ni.GetIPProperties().UnicastAddresses)
                {
                    if (ua.Address.AddressFamily == AddressFamily.InterNetwork && !ua.Address.ToString().StartsWith("169.254"))
                        return ua.Address.ToString();
                }
            }
        }
        return "127.0.0.1";
#endif
    }
    private void OnDestroy() => StopBroadcast();
    private void OnApplicationQuit()
    {
        StopBroadcast();
    }
}