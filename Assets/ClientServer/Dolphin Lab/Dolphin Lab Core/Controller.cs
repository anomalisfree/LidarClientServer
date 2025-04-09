/*!
# SPDX-FileCopyrightText: © 2023 Dolphin Co.,Ltd. <http://lidar.jp>
#
# SPDX-License-Identifier: MIT
*/

using System;

public class Controller
{
    private readonly bool toggleFlag = false;

    public bool ControlEnable
    {
        get
        {
            if(0 != controlValue)
            {
                if (toggleFlag)
                {
                    controlValue = 0;
                }
                return true;
            }
            else
            {
                if(0 != modeId)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
    }

    private int controlValue = 0;
    public int ControlValue
    {
        get
        {
            int value = controlValue;
            controlValue = 0;
            return value;
        }
        set
        {
            if (0 < value)
            {
                controlValue = 1;
            }
            else
            {
                if(toggleFlag)
                {
                    controlValue = 0;
                }
                else if (value < 0)
                {
                    controlValue = -1;
                }
                else
                {
                    controlValue = 0;
                }
            }
        }
    }

    public int ControlFullValue
    {
        set
        {
            controlValue = value;
        }
    }

    private int modeId = 0;
    public int ModeId
    {
        get
        {
            int value = modeId;
            modeId = 0;
            return value;
        }
        set
        {
            modeId = value;
        }
    }

    public Controller(bool toggle)
    {
        toggleFlag = toggle;
    }

    public void SetValue(int id, int value)
    {
        ModeId = id;
        ControlValue = value;
    }

    public void SetFullValue(int id, int fullValue)
    {
        ModeId = id;
        ControlFullValue = fullValue;
    }
}
