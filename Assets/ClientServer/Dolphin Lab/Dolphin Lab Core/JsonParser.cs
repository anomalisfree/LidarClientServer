/*!
# SPDX-FileCopyrightText: © 2023 Dolphin Co.,Ltd. <http://lidar.jp>
#
# SPDX-License-Identifier: MIT
*/

using System;
using System.Text;
using UnityEngine;

public class JsonParser
{
    public static T JsonDeserialize<T>(byte[] data)
    {
        string jsonText = Encoding.ASCII.GetString(data);
        T jsonObj = JsonUtility.FromJson<T>(jsonText);
        return jsonObj;
    }

    public static T JsonDeserialize<T>(string jsonText)
    {
        T jsonObj = JsonUtility.FromJson<T>(jsonText);
        return jsonObj;
    }

    public static byte[] JsonSerialize(object jsonObj)
    {
        string jsonStr = JsonUtility.ToJson(jsonObj);
        byte[] dgram = Encoding.UTF8.GetBytes(jsonStr);
        return dgram;
    }
}

/*
 * InfoJson
 */
[Serializable]
public class InfoJson
{
    public uint type;
    public InfoJsonSpec spec;
}
[Serializable]
public class InfoJsonSpec
{
    public int state;
    public uint maxFrame;
    public uint maxLine;
    public uint maxPoint;
    public uint seqCnt;
    public string info;
    public string date;
    public string time;
    public uint lmStep;
    public uint adj5; // hmMV
    public int lmArea4Top;
    public int lmArea4Typ;
    public int lmArea4Btm;
    public int selfie;
    public int upsideDown;
}

/*
 * ControlJson
 */
[Serializable]
public class ControlJson
{
    public SettingJson Setting;
}
[Serializable]
public class SettingJson
{
    public int version;
    public SettingJsonSystem System;
    public SettingJsonLm LM;
}
[Serializable]
public class SettingJsonSystem
{
    public int state;
    public int selfie;
    public int upsideDown;
}
[Serializable]
public class SettingJsonLm
{
    public int line;
}
