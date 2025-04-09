/*!
# SPDX-FileCopyrightText: © 2023 Dolphin Co.,Ltd. <http://lidar.jp>
#
# SPDX-License-Identifier: MIT
*/

using System;
using UnityEngine;

using DllPointCloudReadonly;

public class PointCloudStatic
{
    public static GameObject gameObject = GameObject.Find(PointCloudReadonly.gameObjectName);

    public static GameObject gameObjectMesh = GameObject.Find(PointCloudReadonly.gameObjectNameMesh);

    public static GameObject gameObjectPrefab = GameObject.Find(PointCloudReadonly.gameObjectNamePrefab);

    public static float ConvertTof2Meter(float tofNsec)
    {
        float lightDistance = 0.3f;
        float val;
        val = tofNsec * lightDistance;
        return val;
    }

    public static float GetTofNsec2Meter(UInt16 tofOrigin)
    {
        int roundTrip = 2;
        return ConvertTof2Meter((float)tofOrigin / (float)roundTrip);
    }
}

