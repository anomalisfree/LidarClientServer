/*!
# SPDX-FileCopyrightText: © 2023 Dolphin Co.,Ltd. <http://lidar.jp>
#
# SPDX-License-Identifier: MIT
*/

using System;

public class DolphinLabCoreIpConfigStatic : IpConfigStatic
{
    public static IpConfig OscUserConfig
    {
        get
        {
            if (DolphinLabCoreStatic.converterMode)
            {
                return oscConfigConverter;
            }
            else
            {
                if (DolphinLabCoreStatic.standaloneMode)
                {
                    return oscConfigStandalone;
                }
                else
                {
                    return oscConfigViaOsc;
                }
            }
        }
    }

    public static IpConfig ControlUserConfig
    {
        get
        {
            if (DolphinLabCoreStatic.converterMode)
            {
                return controlConfigConverter;
            }
            else
            {
                if (DolphinLabCoreStatic.standaloneMode)
                {
                    return controlConfigStandalone;
                }
                else
                {
                    return controlConfigViaOsc;
                }
            }
        }
    }
}
