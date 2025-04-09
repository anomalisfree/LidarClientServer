/*!
# SPDX-FileCopyrightText: © 2023 Dolphin Co.,Ltd. <http://lidar.jp>
#
# SPDX-License-Identifier: MIT
*/

using System;
using System.Collections.Generic;

public class TimeMeasurement
{
    private readonly System.Diagnostics.Stopwatch sw;
    private List<float> lapTimes;

    public TimeMeasurement()
    {
        sw = new System.Diagnostics.Stopwatch();
        ClearLapTime();
    }

    public void StartStopWatch()
    {
        ClearLapTime();
        sw.Restart();
    }

    public List<float> StopStopWatch()
    {
        lapTimes.Add((float)sw.Elapsed.TotalMilliseconds);
        sw.Stop();
        return lapTimes;
    }

    public void ClearLapTime()
    {
        lapTimes = new List<float>();
    }

    public void AddLapTime()
    {
        lapTimes.Add((float)sw.Elapsed.TotalMilliseconds);
    }
}
