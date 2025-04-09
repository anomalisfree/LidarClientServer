/*!
# SPDX-FileCopyrightText: © 2023 Dolphin Co.,Ltd. <http://lidar.jp>
#
# SPDX-License-Identifier: MIT
*/

using System;
using UnityEngine;

using DllPointCloudDataSaveReadonly;

public class PointCloudDataSaveStatic
{
    public static string GetDataPath()
    {
#if UNITY_IOS && !UNITY_EDITOR
        string dataPath = Application.persistentDataPath;
#elif UNITY_ANDROID && !UNITY_EDITOR
    // /storage/emulated/0/Android/data/com.Dolphin.DolphinViewer/files/pointcloud07/log_data/yyyy.mm.dd/filename
    // /storage/emulated/0/../../../../Documents/pointcloud07/log_data/yyyy.mm.dd/filename
        string dataPath = Application.persistentDataPath + "/" + Application.productName;
#elif UNITY_STANDALONE_OSX && !UNITY_EDITOR
    //string dataPath = Application.persistentDataPath;
        string dataPath = Application.dataPath + "/../.." + "/" + Application.productName;
#else
        string dataPath = Application.dataPath;
#endif
        return dataPath;
    }

    public static string GetSaveFolder(DateTime now)
    {
        return PointCloudDataSaveReadonly.logFolderName + "/" + now.ToString("yyyy.MM.dd");
    }

    public static string GetCommonPrefixName(DateTime now)
    {
        return PointCloudDataSaveReadonly.commonPrefixName + now.ToString(PointCloudDataSaveReadonly.dateFormatAsFileName);
    }
}
