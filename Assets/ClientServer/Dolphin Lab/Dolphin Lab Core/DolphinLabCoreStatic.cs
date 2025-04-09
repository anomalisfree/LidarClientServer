/*!
# SPDX-FileCopyrightText: © 2023 Dolphin Co.,Ltd. <http://lidar.jp>
#
# SPDX-License-Identifier: MIT
*/

using System;
using System.Collections.Concurrent;
using UnityEngine;

using DllDolphinLabCoreReadonly;

public class DolphinLabCoreStatic
{
    public static GameObject gameObject = GameObject.Find(DolphinLabCoreReadonly.gameObjectName);

    /*
     standaloneMode : true : Receive data in binary
                    : false: Receive data in OSC format
     */
#if UNITY_IOS || UNITY_ANDROID
    public static readonly bool standaloneMode = true;
#else
    public static bool standaloneMode = true;
#endif
    
    /*
     converterMode  : true : Operating mode is OSC converter
                    : false: Operating mode is viewer
     */
    public static bool converterMode = false;

    /// <summary>
    /// Point Cloud data output structure
    /// </summary>
    public struct PointCloudOutputData
    {
        public int frameNumber;
        public int maxLineNumber;
        public int pointsPerLine;
        public Vector3[][] dataBuffer;

        public PointCloudOutputData(int frame, int line, int points, Vector3[][] data)
        {
            frameNumber = frame;
            maxLineNumber = line;
            pointsPerLine = points;
            dataBuffer = data;
        }
    }
}
