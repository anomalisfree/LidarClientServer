using System;
using System.Collections.Generic;
using UnityEngine;

public class LidarBaseController : MonoBehaviour
{
    [SerializeField] private PointsCloudVisualizationController pointsCloudVisualizationController;
    [SerializeField] private bool visualizePoints;
    [SerializeField] private bool generatePointsInBox;
    [SerializeField] private Transform boxPointGenerator;
    [SerializeField] private PointCloudSender pointCloudSender;

    private PointCloudUtil _pointCloudUtil;
    private Action _lidarCallBack;

    private void Start()
    {
        Init();
    }

    private void Init()
    {
        Debug.Log("DataSender Init");

        IpConfigSetting ipConfigSetting = new();

#if UNITY_SERVER && !UNITY_EDITOR
        string path = Application.dataPath + "/..";
#elif UNITY_ANDROID && !UNITY_EDITOR
        string path = Application.persistentDataPath;
#elif UNITY_IOS && !UNITY_EDITOR
        string path = Application.persistentDataPath;
#else
        string path = Application.dataPath;
#endif
        if (DolphinLabCoreStatic.converterMode)
        {
            ipConfigSetting.InitIpConfigConverter(path);
        }
        else if (DolphinLabCoreStatic.standaloneMode)
        {
            ipConfigSetting.InitIpConfigStandalone(path);
        }
        else
        {
            ipConfigSetting.InitIpConfigViaOsc(path);
        }

        _pointCloudUtil = new PointCloudUtil();

        var oscUtil = new OscUtil(
                        OscStatic.debugOutOsc,
                        DolphinLabCoreIpConfigStatic.OscUserConfig,
                        _pointCloudUtil.GetRxCallBack(),
                        _pointCloudUtil.GetLocalCallBack(),
                        DolphinLabCoreIpConfigStatic.ControlUserConfig
                        );

        _lidarCallBack += LidarCallBack;
        _pointCloudUtil.pointCloudData.OnUICallBacks.Add(_lidarCallBack);
    }

    private void LidarCallBack()
    {
        var newPoints = GetNewPoints();
        pointCloudSender.SendPoints(newPoints);

        if (visualizePoints)
        {
            pointsCloudVisualizationController.VisualizePoints(newPoints);
        }
    }


    private void Update()
    {
        if (generatePointsInBox)
        {
            var newPoints = GenarateNewPoints();
            pointCloudSender.SendPoints(newPoints);

            if (visualizePoints)
            {
                pointsCloudVisualizationController.VisualizePoints(newPoints);
            }
        }
    }
    private List<Vector3> GetNewPoints()
    {
        var dataBuffer = _pointCloudUtil.pointCloudData.DataBuffer[_pointCloudUtil.pointCloudData.DrawFrameBufferNum];
        var length1 = _pointCloudUtil.pointCloudData.OutputNumScanLine;
        var length2 = _pointCloudUtil.pointCloudData.OutputNumPointsPerLine;

        var newPointsVectors = new List<Vector3>(length1 * length2);

        for (var index1 = 0; index1 < length1; ++index1)
        {
            for (var index2 = 0; index2 < length2; ++index2)
            {
                var point = dataBuffer[index1][index2];

                if (point != Vector3.zero)
                {
                    newPointsVectors.Add(point);
                }
            }
        }

        return newPointsVectors;
    }

    private List<Vector3> GenarateNewPoints()
    {
        var newPointsVectors = new List<Vector3>();

        var box = boxPointGenerator.GetComponent<BoxCollider>();

        var size = box.size;
        var center = box.center + boxPointGenerator.position;

        var halfSize = size / 2;

        for (var index1 = 0; index1 < 20; ++index1)
        {
            var randomPoint = new Vector3(
                UnityEngine.Random.Range(center.x - halfSize.x, center.x + halfSize.x),
                UnityEngine.Random.Range(center.y - halfSize.y, center.y + halfSize.y),
                UnityEngine.Random.Range(center.z - halfSize.z, center.z + halfSize.z)
                );

            newPointsVectors.Add(randomPoint);
        }

        return newPointsVectors;
    }

}
