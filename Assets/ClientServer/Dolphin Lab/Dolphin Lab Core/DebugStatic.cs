/*!
# SPDX-FileCopyrightText: © 2023 Dolphin Co.,Ltd. <http://lidar.jp>
#
# SPDX-License-Identifier: MIT
*/

using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

public class DebugStatic
{
    /*
     speedingUp : true  : *AcceleratedCode
                : false : *GeneralCode
     */
    public static bool SpeedingUp { get; set; }

    public static string GetMessageString(string ipAddress, int port)
    {
        return "ipAddress: " + ipAddress + ", " + "port: " + port.ToString();
    }
}

public class DebugOut
{
    private readonly string logClassName = typeof(DebugOut).Name;
    private readonly bool outPutEnable = false;
    
    public DebugOut(bool enable, string className)
    {
        logClassName = className;
        outPutEnable = enable;
    }

    public void Print(
        string st,
        [CallerMemberName] string callerMethodName = "",
        [CallerLineNumber] int callerLineNumber = -1
        )
    {
        if (outPutEnable)
        {
            Debug.Log("[" + logClassName + "]" + "[" + callerMethodName + "]" + ">" + callerLineNumber + ", " + st);
        }
    }

    public void ExPrint(
        string st,
        [CallerMemberName] string callerMethodName = "",
        [CallerLineNumber] int callerLineNumber = -1
        )
    {
        if (outPutEnable)
        {
            Debug.Log("<color=#ff0000ff>" + "[" + logClassName + "]" + "[" + callerMethodName + "]" + ">" + callerLineNumber + ", " + st + "</color>");
        }
    }
}
