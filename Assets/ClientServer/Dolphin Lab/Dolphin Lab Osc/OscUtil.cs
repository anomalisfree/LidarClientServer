/*!
# SPDX-FileCopyrightText: © 2023 Dolphin Co.,Ltd. <http://lidar.jp>
#
# SPDX-License-Identifier: MIT
*/

using System;
using System.Reflection;
using UnityEngine;

using DllBinaryParser;
using DllOscReadonly;

public class OscUtil
{
    readonly DebugOut debugOut = new(true, typeof(OscUtil).Name);

    public OcsConverter ocsConverter;
    public OscController oscController;

    public bool State { get { return ocsConverter.State; } }
    public bool UdpConnection { get { return ocsConverter.UdpConnection; } }
    public int MaxLineNumber { get { return ocsConverter.MaxLineNumber; } }
    public int FrameNumber { get { return ocsConverter.FrameNumber; } }
    public int WidthValue { get { return ocsConverter.WidthValue; } }
    
    public bool BootDone { get { return ocsConverter.InfoGetDone; } }

    public OscUtil(bool debugOutOsc, IpConfig oscIpConfig, Action<byte[]> rxCallBack, Action<BinaryDataInformation, Vector3[]> localCallBack, IpConfig controlIpConfig)
    {
        debugOut.Print("[" + MethodBase.GetCurrentMethod().Name + "]");

        OscStatic.gameObject = new(OscReadonly.gameObjectName);

        ocsConverter = OscStatic.gameObject.AddComponent<OcsConverter>();
        ocsConverter.StartConverter(debugOutOsc, oscIpConfig, rxCallBack, localCallBack);

        oscController = OscStatic.gameObject.AddComponent<OscController>();
        oscController.StartController(controlIpConfig);
    }

}
