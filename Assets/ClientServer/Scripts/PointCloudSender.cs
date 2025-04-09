using UnityEngine;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

public class PointCloudSender : MonoBehaviour
{
    [Header("Network settings for sending")]
    [Tooltip("Recipient's IP address")]
    public string remoteIP = "127.0.0.1";
    [Tooltip("Recipient's port")]
    public int remotePort = 9000;
    [Tooltip("Maximum number of points to send (logical limit)")]
    public int maxPoints = 20000;
    [Tooltip("Maximum UDP packet size (bytes). A value of ~1200 is recommended to avoid fragmentation")]
    public int maxPacketSize = 1200; // bytes

    UdpClient udpClient;
    IPEndPoint remoteEndPoint;
    // Each vector occupies 12 bytes (3 floats of 4 bytes each)
    int pointsPerPacket => maxPacketSize / 12; // e.g., 1200 / 12 = 100 points

    // Buffer for a single packet
    byte[] packetBuffer;

    void Awake()
    {
        udpClient = new UdpClient();
        remoteEndPoint = new IPEndPoint(IPAddress.Parse(remoteIP), remotePort);
        packetBuffer = new byte[pointsPerPacket * 12];
    }

    /// <summary>
    /// Sends a list of points, splitting it into UDP packets.
    /// If there are more points than maxPoints, only the first maxPoints are sent.
    /// </summary>
    public void SendPoints(List<Vector3> points)
    {
        //Debug.Log($"Sending {points.Count} points to {remoteIP}:{remotePort}");

        int totalPoints = Mathf.Min(points.Count, maxPoints);
        int totalPackets = Mathf.CeilToInt((float)totalPoints / pointsPerPacket);

        for (int packetIndex = 0; packetIndex < totalPackets; packetIndex++)
        {
            int start = packetIndex * pointsPerPacket;
            int count = Mathf.Min(pointsPerPacket, totalPoints - start);
            SerializePointsChunk(points, start, count);
            udpClient.Send(packetBuffer, count * 12, remoteEndPoint);
        }
    }

    // Serializes a part of the list of points (starting from index 'start', 'count' points) into the packetBuffer.
    unsafe void SerializePointsChunk(List<Vector3> points, int start, int count)
    {
        fixed (byte* bufferPtr = packetBuffer)
        {
            float* floatPtr = (float*)bufferPtr;
            for (int i = 0; i < count; i++)
            {
                Vector3 p = points[start + i];
                floatPtr[i * 3]     = p.x;
                floatPtr[i * 3 + 1] = p.y;
                floatPtr[i * 3 + 2] = p.z;
            }
        }
    }

    private void OnApplicationQuit()
    {
        udpClient.Close();
    }
}
