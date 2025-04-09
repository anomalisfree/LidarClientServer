/*!
# SPDX-FileCopyrightText: © 2023 Dolphin Co.,Ltd. <http://lidar.jp>
#
# SPDX-License-Identifier: MIT
*/

using System;
using UnityEngine;

using DllOscReadonly;

public class OscStatic
{
    public static bool debugOutOsc = false;

    public static GameObject gameObject = GameObject.Find(OscReadonly.gameObjectName);
}
