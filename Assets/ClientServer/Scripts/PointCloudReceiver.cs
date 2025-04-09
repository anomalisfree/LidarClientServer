using UnityEngine;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System;
using System.Collections.Concurrent;

public class PointCloudReceiver : MonoBehaviour
{
    [Header("Network settings for receiving")]
    [Tooltip("Port to listen for incoming traffic")]
    public int listenPort = 9000;

    [SerializeField] private bool visualizePoints;
    [SerializeField] private PointsCloudVisualizationController pointsCloudVisualizationController;

    private readonly List<Vector3> _framePoints = new();
    private bool _framePointsReady;

    UdpClient udpClient;
    IPEndPoint remoteEndPoint;

    // Event for processing received points (each packet contains part of the point cloud)
    public Action<List<Vector3>> OnPointsReceived;

    // Thread-safe queue for transferring data to the main thread
    private ConcurrentQueue<List<Vector3>> pointsQueue = new ConcurrentQueue<List<Vector3>>();

    void Start()
    {
        udpClient = new UdpClient(listenPort);
        remoteEndPoint = new IPEndPoint(IPAddress.Any, listenPort);
        BeginReceive();
    }

    void Update()
    {
        if (_framePointsReady)
        {
            if (visualizePoints)
            {
                pointsCloudVisualizationController.VisualizePoints(_framePoints);
            }

            _framePoints.Clear();

            _framePointsReady = false;
        }
    }

    void BeginReceive()
    {
        udpClient.BeginReceive(ReceiveCallback, null);
    }


    void ReceiveCallback(IAsyncResult ar)
    {
        try
        {
            byte[] data = udpClient.EndReceive(ar, ref remoteEndPoint);
            List<Vector3> points = DeserializePoints(data);

            _framePoints.AddRange(points);

            if (points.Count != 100)
            {
                _framePointsReady = true;
            }

            // if (OnPointsReceived != null)
            //     OnPointsReceived(points);
            // else
            //     Debug.Log("Received " + points.Count + " points in the packet.");
        }
        catch (Exception e)
        {
            Debug.LogError("Error while receiving: " + e);
        }
        finally
        {
            BeginReceive();
        }
    }

    // Deserializes a byte array into a list of Vector3.
    // Each vector occupies 12 bytes (3 floats of 4 bytes each).
    unsafe List<Vector3> DeserializePoints(byte[] data)
    {
        int count = data.Length / 12;
        List<Vector3> points = new List<Vector3>(count);
        fixed (byte* bufferPtr = data)
        {
            float* floatPtr = (float*)bufferPtr;
            for (int i = 0; i < count; i++)
            {
                float x = floatPtr[i * 3];
                float y = floatPtr[i * 3 + 1];
                float z = floatPtr[i * 3 + 2];
                points.Add(new Vector3(x, y, z));
            }
        }
        return points;
    }

    private void OnApplicationQuit()
    {
        if (udpClient != null)
            udpClient.Close();
    }
}
