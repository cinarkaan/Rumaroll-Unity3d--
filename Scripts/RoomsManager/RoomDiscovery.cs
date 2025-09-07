using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Text;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_ANDROID && !UNITY_EDITOR
using UnityEngine.Android;
#endif


public class RoomDiscovery : MonoBehaviour
{
    private int discoveryPort = 47778;
    private float timeoutSeconds = 0.07f;

    private bool isListening = false,isScanning = true;
    private string localIP;

    private UdpClient client;
    private NetworkManager networkManager;
    private RectTransform list;
    private GameObject pref;


    public void StartScanning(ref RectTransform list, ref GameObject prefab, ref NetworkManager manager)
    {
        if (isListening) return;
        isListening = true;
        isScanning = true;
        this.list = list;
        pref = prefab;
        networkManager = manager;
        localIP = GetLocalIPAddress();
        StartCoroutine(ScanCoroutine());
    }
    private IEnumerator ScanCoroutine()
    {
        var (startIP, endIP) = CalculateSubnetRange(localIP);
        client = new UdpClient();
        client.Client.ReceiveTimeout = 1000;
        client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        byte[] request = Encoding.UTF8.GetBytes("DISCOVER_ROOM");
        IPEndPoint remoteEP = null;
        IPAddress current = startIP;
        bool got = false;
        list.GetChild(0).GetChild(0).GetComponent<Text>().text = "Scanning hosts...";
        while (CompareIP(current, endIP) && isScanning)
        {
            client.Send(request, request.Length, current.ToString(), discoveryPort);
            float start = Time.realtimeSinceStartup;
            while (Time.realtimeSinceStartup - start < timeoutSeconds)
            {
                //list.GetChild(0).GetChild(0).GetComponent<Text>().text = current.ToString();
                if (client.Available > 0)
                {
                    byte[] data = client.Receive(ref remoteEP);
                    string resp = Encoding.UTF8.GetString(data);
                    string[] parts = resp.Split('|');
                    if (parts.Length >= 4)
                    {
                        list.GetChild(0).GetChild(0).GetComponent<Text>().text = "";
                        GameObject findedRoom = Instantiate(pref);
                        findedRoom.transform.SetParent(list, false);
                        findedRoom.transform.GetChild(0).GetComponent<Text>().text = parts[0] + " (" + parts[1] + "|" + parts[2] + ")";
                        findedRoom.transform.GetChild(1).GetComponent<Button>().onClick.AddListener(() => ConnectToRoom(parts[3]));
                        got = true;
                        isScanning = false;
                        break;
                    }
                }
                yield return null;
            }
            current = IncrementIP(current);
            yield return null;
        }
        if (!got)
            list.GetChild(0).GetChild(0).GetComponent<TMP_Text>().text = "No available hosts";
        client?.Close();
        isListening = false;
    }
    private (IPAddress start, IPAddress end) CalculateSubnetRange(string ip)
    {
        var parts = ip.Split('.');
        if (parts.Length != 4) throw new Exception("Invalid IP");
        // assume /24
        string prefix = $"{parts[0]}.{parts[1]}.{parts[2]}";
        return (IPAddress.Parse(prefix + ".1"), IPAddress.Parse(prefix + ".254"));
    }
    private bool CompareIP(IPAddress a, IPAddress b)
    {
        var ab = a.GetAddressBytes(); var bb = b.GetAddressBytes();
        for (int i = 0; i < 4; i++) { if (ab[i] < bb[i]) return true; if (ab[i] > bb[i]) return false; }
        return true;
    }
    private IPAddress IncrementIP(IPAddress ip)
    {
        var bytes = ip.GetAddressBytes();
        for (int i = 3; i >= 0; i--) { if (++bytes[i] != 0) break; }
        return new IPAddress(bytes);
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
    private void StopDiscovery()
    {
        isListening = false;
        isScanning = false;
        client?.Close();
        StopCoroutine(ScanCoroutine());
        //Debug.Log("[RoomDiscovery] Stopped.");
    }
    private void OnDestroy() { StopDiscovery(); }
    private void OnApplicationQuit()
    {
        StopDiscovery();
    }
    private void ConnectToRoom(string ip)
    {
        foreach (Transform obj in list)
            if (obj.name != "Information")
                Destroy(obj.gameObject);
        StopDiscovery();
        networkManager.GetComponent<UnityTransport>().SetConnectionData(ip, 7777);
        networkManager.StartClient();

    }
}
