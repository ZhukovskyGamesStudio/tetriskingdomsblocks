using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

public class NetworkTimeAPI : MonoBehaviour
{
    private const string NtpServer = "pool.ntp.org";
    private const int NtpPort = 123;
    private const int NtpDataLength = 48;
    private const byte NtpModeClient = 3;
    private const byte NtpVersionNumber = 3;
    private const double UnixEpoch = 2208988800;

    public async void GetNetworkTime(Action<DateTime> onTimeReceived, Action<string> onError = null)
    {
        try
        {
            DateTime networkTime = await RequestNetworkTimeAsync();
            onTimeReceived?.Invoke(networkTime);
        }
        catch (Exception ex)
        {
            onError?.Invoke(ex.Message);
            Debug.LogError($"NTP Error: {ex.Message}");
        }
    }

    private async Task<DateTime> RequestNetworkTimeAsync()
    {
        IPAddress[] addresses = Dns.GetHostEntry(NtpServer).AddressList;
        IPEndPoint ipEndPoint = new IPEndPoint(addresses[0], NtpPort);
        
        byte[] ntpData = new byte[NtpDataLength];
        ntpData[0] = (NtpModeClient << 3) | NtpVersionNumber;

        using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
        {
            socket.Connect(ipEndPoint);
            socket.ReceiveTimeout = 3000;
            
            await socket.SendAsync(new ArraySegment<byte>(ntpData), SocketFlags.None);
            await socket.ReceiveAsync(new ArraySegment<byte>(ntpData), SocketFlags.None);
        }

        ulong intPart = (ulong)ntpData[40] << 24 | (ulong)ntpData[41] << 16 | (ulong)ntpData[42] << 8 | ntpData[43];
        ulong fractPart = (ulong)ntpData[44] << 24 | (ulong)ntpData[45] << 16 | (ulong)ntpData[46] << 8 | ntpData[47];

        ulong milliseconds = (intPart * 1000) + ((fractPart * 1000) / 0x100000000L);
        DateTime networkDateTime = new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            .AddMilliseconds((long)milliseconds);

        return networkDateTime;
    }
}
