/*!
# SPDX-FileCopyrightText: © 2023 Dolphin Co.,Ltd. <http://lidar.jp>
#
# SPDX-License-Identifier: MIT
*/

using System;
using UnityEngine;

using DllPointCloudReadonly;

public class PointCloudColor
{
    public Color[] colorAry;

    private readonly float nearColorRange = 20.0f; //20m
    private readonly float nearColorCycleUnit = 10.0f; //10m
    private readonly float farColorCycleUnit = 10.0f; //10m

    public Int32 ColorLoopStep { private get; set; } = 0;

    private UInt32 ViewColorNumber { get { return PointCloudReadonly.viewColorNumber; } }
    private float ScaleParameter { get { return PointCloudReadonly.scaleParameter; } }

    public PointCloudColor()
    {
        InitColor();
    }

    private Color ConvertRgb2Color(Int32 red, Int32 green, Int32 blue)
    {
        return new Color(red / 255f, green / 255f, blue / 255f);
    }

    private void InitColor()
    {
        colorAry = new Color[ViewColorNumber];
        UInt32 offset = ViewColorNumber / 2;

        // color map
        colorAry[0] = ConvertRgb2Color(255, 185, 0);
        colorAry[1] = ConvertRgb2Color(255, 140, 0);
        colorAry[2] = ConvertRgb2Color(247, 99, 12);
        colorAry[3] = ConvertRgb2Color(220, 80, 16);
        colorAry[4] = ConvertRgb2Color(218, 59, 1);
        colorAry[5] = ConvertRgb2Color(239, 105, 80);
        colorAry[6] = ConvertRgb2Color(209, 52, 56);
        colorAry[7] = ConvertRgb2Color(255, 67, 67);
        colorAry[8] = ConvertRgb2Color(231, 72, 86);
        colorAry[9] = ConvertRgb2Color(232, 17, 35);
        colorAry[10] = ConvertRgb2Color(234, 0, 94);
        colorAry[11] = ConvertRgb2Color(195, 0, 82);
        colorAry[12] = ConvertRgb2Color(227, 0, 140);
        colorAry[13] = ConvertRgb2Color(191, 0, 119);
        colorAry[14] = ConvertRgb2Color(194, 57, 179);
        colorAry[15] = ConvertRgb2Color(154, 0, 137);
        colorAry[16] = ConvertRgb2Color(0, 120, 215);
        colorAry[17] = ConvertRgb2Color(0, 99, 177);
        colorAry[18] = ConvertRgb2Color(142, 140, 216);
        colorAry[19] = ConvertRgb2Color(107, 105, 214);
        colorAry[20] = ConvertRgb2Color(135, 100, 184);
        colorAry[21] = ConvertRgb2Color(116, 77, 169);
        colorAry[22] = ConvertRgb2Color(177, 70, 194);
        colorAry[23] = ConvertRgb2Color(136, 23, 152);
        colorAry[24] = ConvertRgb2Color(0, 153, 188);
        colorAry[25] = ConvertRgb2Color(45, 125, 154);
        colorAry[26] = ConvertRgb2Color(0, 183, 195);
        colorAry[27] = ConvertRgb2Color(3, 131, 135);
        colorAry[28] = ConvertRgb2Color(0, 178, 148);
        colorAry[29] = ConvertRgb2Color(1, 133, 116);
        colorAry[30] = ConvertRgb2Color(0, 204, 106);
        colorAry[31] = ConvertRgb2Color(16, 137, 62);

        colorAry[offset + 0] = colorAry[offset - 1];
        colorAry[offset + 1] = colorAry[offset - 2];
        colorAry[offset + 2] = colorAry[offset - 3];
        colorAry[offset + 3] = colorAry[offset - 4];
        colorAry[offset + 4] = colorAry[offset - 5];
        colorAry[offset + 5] = colorAry[offset - 6];
        colorAry[offset + 6] = colorAry[offset - 7];
        colorAry[offset + 7] = colorAry[offset - 8];
        colorAry[offset + 8] = colorAry[offset - 9];
        colorAry[offset + 9] = colorAry[offset - 10];
        colorAry[offset + 10] = colorAry[offset - 11];
        colorAry[offset + 11] = colorAry[offset - 12];
        colorAry[offset + 12] = colorAry[offset - 13];
        colorAry[offset + 13] = colorAry[offset - 14];
        colorAry[offset + 14] = colorAry[offset - 15];
        colorAry[offset + 15] = colorAry[offset - 16];
        colorAry[offset + 16] = colorAry[offset - 17];
        colorAry[offset + 17] = colorAry[offset - 18];
        colorAry[offset + 18] = colorAry[offset - 19];
        colorAry[offset + 19] = colorAry[offset - 20];
        colorAry[offset + 20] = colorAry[offset - 21];
        colorAry[offset + 21] = colorAry[offset - 22];
        colorAry[offset + 22] = colorAry[offset - 23];
        colorAry[offset + 23] = colorAry[offset - 24];
        colorAry[offset + 24] = colorAry[offset - 25];
        colorAry[offset + 25] = colorAry[offset - 26];
        colorAry[offset + 26] = colorAry[offset - 27];
        colorAry[offset + 27] = colorAry[offset - 28];
        colorAry[offset + 28] = colorAry[offset - 29];
        colorAry[offset + 29] = colorAry[offset - 30];
        colorAry[offset + 30] = colorAry[offset - 31];
        colorAry[offset + 31] = colorAry[offset - 32];
    }

    private UInt32 GetColorLoopItem(bool colorLoopEnable, UInt32 divUnit)
    {
        Int32 item;

        if (colorLoopEnable)
        {
            item = (Int32)divUnit + ColorLoopStep;

            if (((Int32)ViewColorNumber - 1) <= item)
            {
                item -= ((Int32)ViewColorNumber - 1);
            }

            return (UInt32)item;
        }
        else
        {
            return divUnit;
        }
    }

    public Color GetColor(float posZ)
    {
        float divMaskUnit;
        float colorz;
        UInt32 divUnit;
        float remain;
        Color setColor;

        if (posZ < (nearColorRange * ScaleParameter))
        {
            divMaskUnit = (nearColorCycleUnit * ScaleParameter) / ViewColorNumber;
            colorz = (posZ % (nearColorCycleUnit * ScaleParameter));
        }
        else
        {
            divMaskUnit = (farColorCycleUnit * ScaleParameter) / ViewColorNumber;
            colorz = (posZ % (farColorCycleUnit * ScaleParameter));
        }

        divUnit = (UInt32)(colorz / divMaskUnit);

        if (divUnit < (ViewColorNumber - 1))
        {
            remain = (colorz % divMaskUnit) / divMaskUnit;
            divUnit = GetColorLoopItem(false, divUnit);
            setColor = Color.Lerp(colorAry[divUnit], colorAry[divUnit + 1], remain);
        }
        else
        {
            divUnit = GetColorLoopItem(false, divUnit);
            setColor = colorAry[divUnit];
        }

        return setColor;
    }
}
