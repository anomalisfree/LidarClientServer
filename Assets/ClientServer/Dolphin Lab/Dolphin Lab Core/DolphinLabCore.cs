/*!
# SPDX-FileCopyrightText: © 2023 Dolphin Co.,Ltd. <http://lidar.jp>
#
# SPDX-License-Identifier: MIT
*/

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;

using DllDolphinLabCoreReadonly;

/// <summary>
/// Unity package core classes
/// </summary>
public class DolphinLabCore : MonoBehaviour
{
    /// <summary>
    /// Enqueue when New Point Cloud Data is ready
    /// </summary>
    private ConcurrentQueue<DolphinLabCoreStatic.PointCloudOutputData> Queue { get { return pointCloudData.Queue; } set { pointCloudData.Queue = value; } }
    public bool DataOutputEnable { get { return pointCloudData.DataOutputEnable; } set { pointCloudData.DataOutputEnable = value; } }

    private PointCloudUtil pointCloudUtil;
    private OscUtil oscUtil;

    public OscController oscController { get { return oscUtil.oscController; } }
    public OcsConverter ocsConverter { get { return oscUtil.ocsConverter; } }
    public PointCloudData pointCloudData { get { return pointCloudUtil.pointCloudData; } }
    public PointCloudDataSave pointCloudDataSave { get { return pointCloudUtil.pointCloudDataSave; } }
    public PointCloudGenerator pointCloudGenerator { get { return pointCloudUtil.pointCloudGenerator; } }

    public bool DrawPointCloud { get { return pointCloudData.DrawPointCloud; } set { pointCloudData.DrawPointCloud = value; } }
    public bool State { get { return oscUtil.State; } }
    public bool UdpConnection { get { return oscUtil.UdpConnection; } }
    public int MaxLineNumber { get { return oscUtil.MaxLineNumber; } }
    public int FrameNumber { get { return oscUtil.FrameNumber; } }
    public int WidthValue { get { return oscUtil.WidthValue; } }
    public bool BootDone { get { return oscUtil.BootDone; } }

    private List<float> DrawlapTimesMs { get { return pointCloudData.drawlapTimesMs; } }
    private List<float> OscConvertlapTimesMs { get { return ocsConverter.oscConvertlapTimesMs; } }

    private int StateMode { set { oscController.SetStartStop(value); } }
    private int LineMode { set { oscController.LineMode = value; } }

    private bool Selfie { set { oscController.Selfie = value; } }
    public bool DebugSelfie { get { return oscController.DebugSelfie; } }
    private bool DebugSelfieOsc { set { ocsConverter.Selfie = value; } }

    private bool UpsideDown { set { oscController.UpsideDown = value; } }
    public bool DebugUpsideDown { get { return oscController.DebugUpsideDown; } }
    private bool DebugUpsideDownOsc { set { ocsConverter.UpsideDown = value; } }

    public bool SaveXyZ { get { return saveXyz; } set { saveXyz = value; } }
    public bool SavePts { get { return savePts; } set { savePts = value; } }
    public bool SavePly { get { return savePly; } set { savePly = value; } }
    public bool SaveStl { get { return saveStl; } set { saveStl = value; } }
    public bool SaveCsv { get { return saveCsv; } set { saveCsv = value; } }
    public DateTime NowDateTime { get { return ocsConverter.nowDateTime; } }

    /*
     * view in Unity Editor
     */
    public bool speedingUp = false;
    public List<float> drawlapTimesMs;
    public List<float> oscConvertlapTimesMs;

    private bool dataOutput = false;
    private int frameNumber;
    private int maxLineNumber;
    private int pointsPerLine;
    private bool drawPointCloud = true;

    public int stateMode = 0; // 1: Start/Stop
    public int lineMode = 0; // 1:up, -1:down

    public bool selfie = false;
    public bool upsideDown = false;

    public bool saveXyz = false;
    public bool savePts = false;
    public bool savePly = false;
    public bool saveStl = false;
    public bool saveCsv = false;

    private void Awake()
    {
        DolphinLabCoreStatic.gameObject = gameObject;
        DolphinLabCoreStatic.gameObject.name = DolphinLabCoreReadonly.gameObjectName;

        IpConfigSetting ipConfigSetting = new();

#if UNITY_SERVER && !UNITY_EDITOR
        string path = Application.dataPath + "/..";
#elif UNITY_ANDROID && !UNITY_EDITOR
        //string path = Application.persistentDataPath + "/" + Application.productName;
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

        pointCloudUtil = new PointCloudUtil();
        oscUtil = new OscUtil(
            OscStatic.debugOutOsc,
            DolphinLabCoreIpConfigStatic.OscUserConfig,
            pointCloudUtil.GetRxCallBack(),
            pointCloudUtil.GetLocalCallBack(),
            DolphinLabCoreIpConfigStatic.ControlUserConfig
            );
    }

    // Use this for initialization
    void Start()
    {
        Debug.Log("[PID]" + System.Diagnostics.Process.GetCurrentProcess().Id);
    }

    /// <summary>
    /// Enable/disable Point Cloud data output
    /// </summary>
    /// <param name="enable">Enable / Disable</param>
    public void SetPointCloudDataOut(bool enable)
    {
        Queue.Clear();
        frameNumber = 0;
        maxLineNumber = 0;
        pointsPerLine = 0;
        DataOutputEnable = enable;
    }

    /// <summary>
    /// Return Point Cloud Data via out parameter modifier
    /// </summary>
    /// <param name="rcdData">out parameter modifier for PointCloudOutputData structure</param>
    /// <returns>true: valid New Point Cloud Data, false: invalid Point Cloud Data</returns>
    public bool IsPointCloudDataValid(out DolphinLabCoreStatic.PointCloudOutputData rcdData)
    {
        if (Queue.TryDequeue(out rcdData))
        {
            /*
             * view in Unity Editor
             */
            frameNumber = rcdData.frameNumber;
            maxLineNumber = rcdData.maxLineNumber;
            pointsPerLine = rcdData.pointsPerLine;
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Set enable/disable of accelerated processing
    /// </summary>
    /// <param name="enable">Enable / Disable</param>
    public void SetSpeedingUp(bool enable)
    {
        speedingUp = enable;
    }

    /// <summary>
    /// Enabling/disabling point cloud drawing
    /// </summary>
    /// <param name="enable">Enable / Disable</param>
    public void SetDrawPointCloud(bool enable)
    {
        drawPointCloud = enable;
    }

    /// <summary>
    /// Toggle LiDAR start/stop
    /// </summary>
    public void ToggleLiDARStateMode()
    {
        stateMode = 1;
    }

    /// <summary>
    /// Increase or decrease LiDAR line mode
    /// </summary>
    /// <param name="control">increase / decrease</param>
    public void SetLiDarLineMode(bool control)
    {
        if(control)
        {
            lineMode = 1;
        }
        else
        {
            lineMode = -1;
        }
    }

    /// <summary>
    /// Setting selfie enable/disable
    /// </summary>
    /// <param name="enable">Enable / Disable</param>
    public void SetSelfie(bool enable)
    {
        selfie = enable;
    }

    /// <summary>
    /// Setting upside down enable/disable
    /// </summary>
    /// <param name="enable">Enable / Disable</param>
    public void SetUpsideDown(bool enable)
    {
        upsideDown = enable;
    }

    // Update is called once per frame
    void Update()
    {
        if (0 != stateMode)
        {
            StateMode = stateMode;
            stateMode = 0;
        }
        else if (0 != lineMode)
        {
            LineMode = lineMode;
            lineMode = 0;
        }

        UpdateParameters();
        UpdateDataSave(!State);
    }

    private void UpdateParameters()
    {
        DataOutputEnable = dataOutput;
        drawlapTimesMs = DrawlapTimesMs;
        oscConvertlapTimesMs = OscConvertlapTimesMs;

        DebugStatic.SpeedingUp = speedingUp;
        DrawPointCloud = drawPointCloud;

        Selfie = selfie;
        DebugSelfieOsc = DebugSelfie;

        UpsideDown = upsideDown;
        DebugUpsideDownOsc = DebugUpsideDown;
    }

    public void UpdateDataSave(bool dataSave)
    {
        if (dataSave)
        {
            if (saveXyz)
            {
                saveXyz = false;
                pointCloudDataSave.SaveXyz(ocsConverter.nowDateTime);
            }
            if (savePts)
            {
                savePts = false;
                pointCloudDataSave.SavePts(ocsConverter.nowDateTime);
            }
            else if (savePly)
            {
                savePly = false;
                pointCloudDataSave.SavePly(false, ocsConverter.nowDateTime, pointCloudData.CubeSizeRatioX, pointCloudData.CubeSizeRatioY);
                pointCloudDataSave.SavePly(true, ocsConverter.nowDateTime, pointCloudData.CubeSizeRatioX, pointCloudData.CubeSizeRatioY);
            }
            else if (saveStl)
            {
                saveStl = false;
                pointCloudDataSave.SaveStl(ocsConverter.nowDateTime, pointCloudData.CubeSizeRatioX, pointCloudData.CubeSizeRatioY);
            }
            else if (saveCsv)
            {
                saveCsv = false;
                pointCloudDataSave.SaveCsv(ocsConverter.nowDateTime, false);
            }
        }
        else
        {
            saveXyz = false;
            savePts = false;
            savePly = false;
            saveStl = false;
            saveCsv = false;
        }
    }

    public void DebugSetSelfie(bool mode)
    {
        selfie = mode;
    }

    public void DebugSetUpsideDown(bool mode)
    {
        upsideDown = mode;
    }

    public void DebugToggleSetSelfie()
    {
        selfie = !selfie;
    }

    public void DebugToggleSetUpsideDown()
    {
        upsideDown = !upsideDown;
    }
}
