/*!
# SPDX-FileCopyrightText: © 2023 Dolphin Co.,Ltd. <http://lidar.jp>
#
# SPDX-License-Identifier: MIT
*/

using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class IntervalControl
{
    private bool check = false;
    public bool IntervalState
    {
        get {
            if (true == check)
            {
                check = false;
                return true;
            }
            else
            {
                return false;
            }
        }
        private set
        {
            check = value;
        }
    }

    private bool threadExitFlag = false;

    public IntervalControl(string className, int setIntervalMs)
    {
        string intervalClassName;
        int intervalSec;
        int intervalMs;

        if("" != className)
        {
            intervalClassName = className;
        }
        else
        {
            intervalClassName = "";
        }

        if (0 < setIntervalMs)
        {
            intervalSec = (int)(setIntervalMs / 1000);
            intervalMs = (setIntervalMs % 1000);
        }
        else
        {
            intervalSec = 0;
            intervalMs = 200;
        }

        object[] obj = new object[3];
        obj[0] = intervalClassName;
        obj[1] = intervalSec;
        obj[2] = intervalMs;
        Thread th = new(new ParameterizedThreadStart(IntervalControlMethod));
        th.Priority = System.Threading.ThreadPriority.Lowest;
        th.Start(obj);
    }

    public void Abort()
    {
        threadExitFlag = true;
    }

    private async void IntervalControlMethod(object args)
    {
        object[] temp = (object[])args;
        string intervalClassName = (string)temp[0];
        int intervalSec = (int)temp[1];
        int intervalMs = (int)temp[2];
        TimeSpan ts;

        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();

        sw.Reset();
        sw.Start();

        while (true)
        {
            if(threadExitFlag)
            {
                break;
            }
            else
            {
                sw.Restart();
                await Task.Delay(intervalMs + (intervalSec * 1000));
                ts = sw.Elapsed;
                if ("" != intervalClassName)
                {
                    Debug.Log("[" + intervalClassName + "]" + "sec: " + ts.Seconds.ToString() + " " + "ms: " + ts.Milliseconds.ToString() + " " + "intervalMs: " + (intervalMs + intervalSec * 1000).ToString());
                }
                IntervalState = true;
            }
        }
    }
}
