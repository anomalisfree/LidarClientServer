/*!
# SPDX-FileCopyrightText: © 2023 Dolphin Co.,Ltd. <http://lidar.jp>
#
# SPDX-License-Identifier: MIT
*/

using System;
using System.Reflection;
using UnityEngine;

using DllBinaryParser;
using DllPointCloudGeneratorAccelerated;
using DllPointCloudReadonly;

public class PointCloudUtil
{
    private readonly DebugOut debugOut = new(false, typeof(PointCloudUtil).Name);

    public PointCloudData pointCloudData;
    public PointCloudGenerator pointCloudGenerator;
    public PointCloudDataSave pointCloudDataSave;

    public PointCloudUtil()
    {
        debugOut.Print("[" + MethodBase.GetCurrentMethod().Name + "]");

        PointCloudStatic.gameObject = new(PointCloudReadonly.gameObjectName);
        pointCloudData = PointCloudStatic.gameObject.AddComponent<PointCloudData>();
        pointCloudGenerator = PointCloudStatic.gameObject.AddComponent<PointCloudGenerator>();
        pointCloudDataSave = PointCloudStatic.gameObject.AddComponent<PointCloudDataSave>();
        PointCloudStatic.gameObject.AddComponent<PointCloudGeneratorAccelerated>();
    }

    public Action<byte[]> GetRxCallBack()
    {
        return pointCloudData.OscRecevieData;
    }

    public Action<BinaryDataInformation, Vector3[]> GetLocalCallBack()
    {
        return pointCloudData.XyzRecevieData;
    }
}
