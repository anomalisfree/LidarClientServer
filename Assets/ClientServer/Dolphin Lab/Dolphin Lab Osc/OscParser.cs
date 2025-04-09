/*!
# SPDX-FileCopyrightText: © 2023 Dolphin Co.,Ltd. <http://lidar.jp>
#
# SPDX-License-Identifier: MIT
*/

using System;
using System.Collections.Generic;
using UnityEngine;

using DllBinaryParser;
using DllOscReadonly;
using DllXYRadianConversion;

public class OscParser
{
    /////////////////////////////////////////////////
    // osc data stream
    /////////////////////////////////////////////////
    // address : /tof_xyz_line
    //
    //[dataAry]     [item]          [type]
    // 0~3      :frameNumber        : UInt32
    // 4~7      :maxLineNumber      : UInt32
    // 8~11     :currentLineNumber  : UInt32
    // 12~15    :pointsPerLine      : UInt32
    // 16~      :data               : 

    public OscDataInformation dataInfo;
    public OscStatusInformation statusInfo;

    public float[] tofData;
    public List<Vector3> xyzDataOld;
    public Vector3[] xyzData;

    protected const bool isBigEndian = false;

    const int arryCount = 600 * 3;
    protected readonly byte[] oscFormatTypeArry = new byte[arryCount];
    protected readonly byte[] oscFormatTypePaddingBase = { 0x00, 0x00, 0x00, 0x00 };
    protected readonly byte oscFormatAddrDelim = Convert.ToByte(',');
    protected readonly byte oscFormatAddrPadding = 0x00;

    protected readonly float magLM = 1.0f;

    public OscParser()
    {
        int i;
        for (i = 0; i < OscReadonly.typeDataIndex; i++)
        {
            oscFormatTypeArry[i] = Convert.ToByte('i');
        }
        for (; i < arryCount; i++)
        {
            oscFormatTypeArry[i] = Convert.ToByte('f');
        }
    }

    private UInt32 BitConverterToUInt32(bool isLittele, byte[] binData, Int32 index)
    {
        UInt32 rcd;

        if (isLittele)
        {
            rcd = BitConverter.ToUInt32(binData, index);
        }
        else
        {
            byte[] tempBytes = new byte[4];
            Buffer.BlockCopy(binData, index, tempBytes, 0, 4);
            Array.Reverse(tempBytes);
            rcd = BitConverter.ToUInt32(tempBytes);
        }

        return rcd;
    }

    private float BitConverterToSingle(bool isLittele, byte[] binData, Int32 index)
    {
        float rcd;

        if (isLittele)
        {
            rcd = BitConverter.ToSingle(binData, index);
        }
        else
        {
            byte[] tempBytes = new byte[4];
            Buffer.BlockCopy(binData, index, tempBytes, 0, 4);
            Array.Reverse(tempBytes);
            rcd = BitConverter.ToSingle(tempBytes);
        }

        return rcd;
    }

    private float BitConverterByType(byte type, byte[] binData, Int32 index)
    {
        float rcd = 0;

        if ('f' == type)
        {
            rcd = BitConverterToSingle(isBigEndian, binData, index);
        }
        else if ('i' == type)
        {
            rcd = (float)BitConverterToUInt32(isBigEndian, binData, index);
        }

        return rcd;
    }

    public bool GetLineData(bool allData, byte[] oscData)
    {
        Int32 originIndex;
        Int32 endIndex;
        Int32 arryLength;

        originIndex = 0;
        endIndex = 0;

        // address
        //----------------------------------------
        for (int i = 0; i < oscData.Length; i++)
        {
            if (0x00 == oscData[i])
            {
                endIndex = i;
                break;
            }
        }

        arryLength = endIndex - originIndex;
        byte[] address = new byte[arryLength];
        Buffer.BlockCopy(oscData, originIndex, address, 0, arryLength);

        //ASCII encode
        //----------------------------------------
        string addressText = System.Text.Encoding.ASCII.GetString(address);

        if (OscReadonly.oscAddressTofXyzLine != addressText)
        {
            return false;
        }

        for (int i = endIndex; i < oscData.Length; i++)
        {
            if (0x2c == oscData[i])
            {
                endIndex = i;
                break;
            }
        }

        originIndex = endIndex + 1;

        // type
        //----------------------------------------
        for (int i = originIndex; i < oscData.Length; i++)
        {
            if (0x00 == oscData[i])
            {
                endIndex = i;
                break;
            }
        }
        arryLength = endIndex - originIndex;
        byte[] type = new byte[arryLength];
        Buffer.BlockCopy(oscData, originIndex, type, 0, arryLength);

        originIndex = endIndex;

        // alignment
        //----------------------------------------
        int mod;
        mod = originIndex % 4;
        if (0 != mod)
        {
            originIndex += (4 - mod);
        }
        else
        {
            originIndex += 4;
        }

        // frameNumber;
        //----------------------------------------
        dataInfo.frameNumber = BitConverterToUInt32(isBigEndian, oscData, originIndex + OscReadonly.idFrameNum);
        // maxLineNumber;
        //----------------------------------------
        dataInfo.maxLineNumber = BitConverterToUInt32(isBigEndian, oscData, originIndex + OscReadonly.idMaxLineNum);
        // currentLineNumber;
        //----------------------------------------
        dataInfo.currentLineNumber = BitConverterToUInt32(isBigEndian, oscData, originIndex + OscReadonly.idLineNum);
        // pointsPerLine
        //----------------------------------------
        dataInfo.pointsPerLine = BitConverterToUInt32(isBigEndian, oscData, originIndex + OscReadonly.idPoints);

        // Return path for data format confirmation
        if (!allData)
        {
            return true;
        }

        // xyz
        //----------------------------------------
        originIndex += OscReadonly.idData;

        xyzDataOld = new();
        byte typeX, typeY, typeZ;
        for (int i = 0, j = OscReadonly.typeDataIndex; i < dataInfo.pointsPerLine; i++)
        {
            typeX = type[j++];
            typeY = type[j++];
            typeZ = type[j++];

            Vector3 vd = new();
            vd.x = BitConverterByType(typeX, oscData, originIndex);
            originIndex += 4;
            vd.y = BitConverterByType(typeY, oscData, originIndex);
            originIndex += 4;
            vd.z = BitConverterByType(typeZ, oscData, originIndex);
            originIndex += 4;
            xyzDataOld.Add(vd);
        }

        return true;
    }

    public byte[] GenerateBinary2OscTransformation(byte[] binData, BinaryParser binInfo, string oscAddr, List<TransformationCoefficient> coeffAry)
    {
        byte[] oscByte = Array.Empty<byte>();
        int mod, cnt, i, st;
        byte[] workByte;

        // address
        //----------------------------------------
        byte[] addrByte = System.Text.Encoding.UTF8.GetBytes(oscAddr);
        mod = addrByte.Length % 4;
        if (0 == mod)
        {
            cnt = 4;
        }
        else
        {
            cnt = 4 - mod;
        }
        st = addrByte.Length;
        Array.Resize(ref addrByte, addrByte.Length + cnt + 1);
        for (i = 0; i < cnt; i++)
        {
            addrByte[st + i] = oscFormatAddrPadding;
        }
        addrByte[st + i] = oscFormatAddrDelim;

        // type
        //----------------------------------------
        cnt = OscReadonly.typeDataIndex + (Int32)(binInfo.info.dataCount * 3);
        byte[] typeByte = new byte[cnt];
        Array.Copy(oscFormatTypeArry, 0, typeByte, 0, cnt);

        mod = (addrByte.Length + cnt) % 4;
        if (0 == mod)
        {
            cnt = 4;
        }
        else
        {
            cnt = 4 - mod;
        }
        workByte = new byte[cnt];
        Array.Copy(oscFormatTypePaddingBase, 0, workByte, 0, cnt);
        st = typeByte.Length;
        Array.Resize(ref typeByte, typeByte.Length + cnt);
        Array.Copy(workByte, 0, typeByte, st, cnt);

        // info
        //----------------------------------------
        byte[] infoByte = new byte[OscReadonly.typeDataIndex * sizeof(Int32)];
        workByte = new byte[sizeof(Int32)];
        st = 0;
        cnt = 4;
        workByte = BitConverter.GetBytes((Int32)binInfo.info.frameNumber);
        Array.Reverse(workByte);
        Array.Copy(workByte, 0, infoByte, st, cnt);

        st += cnt;
        cnt = 4;
        workByte = BitConverter.GetBytes((Int32)binInfo.info.maxLineNumber);
        Array.Reverse(workByte);
        Array.Copy(workByte, 0, infoByte, st, cnt);

        st += cnt;
        cnt = 4;
        workByte = BitConverter.GetBytes((Int32)binInfo.info.lineNumber);
        Array.Reverse(workByte);
        Array.Copy(workByte, 0, infoByte, st, cnt);

        st += cnt;
        cnt = 4;
        workByte = BitConverter.GetBytes((Int32)binInfo.info.dataCount);
        Array.Reverse(workByte);
        Array.Copy(workByte, 0, infoByte, st, cnt);

        // xyz
        //----------------------------------------
        byte[] xyzByte = new byte[binInfo.info.dataCount * 3 * sizeof(Single)];
        workByte = new byte[sizeof(Single)];
        float tof;
        //Vector3 xyz;

        Int32 dataUnitSize;
        if (binInfo.Type_uint16 == binInfo.info.dataType)
        {
            dataUnitSize = 2;
        }
        else
        {
            dataUnitSize = 2;
        }

        workByte = new byte[4];
        for (i = 0, cnt = 0, st = 0; i < binInfo.info.dataCount; i++)
        {
            UInt16 tofOrigin = BitConverter.ToUInt16(binData, binInfo.Data_st + (i * dataUnitSize));
            tof = PointCloudStatic.GetTofNsec2Meter(tofOrigin);

            Vector3 pointcloud = new Vector3(
                    tof * coeffAry[i].x,
                    tof * coeffAry[i].y,
                    tof * coeffAry[i].z
                    );
            // x
            //----------------------------------------
            st += cnt;
            cnt = 4;
            workByte = BitConverter.GetBytes(pointcloud.x);
            Array.Reverse(workByte);
            Array.Copy(workByte, 0, xyzByte, st, cnt);

            // y
            //----------------------------------------
            st += cnt;
            cnt = 4;
            workByte = BitConverter.GetBytes(pointcloud.y);
            Array.Reverse(workByte);
            Array.Copy(workByte, 0, xyzByte, st, cnt);

            // z
            //----------------------------------------
            st += cnt;
            cnt = 4;
            workByte = BitConverter.GetBytes(pointcloud.z);
            Array.Reverse(workByte);
            Array.Copy(workByte, 0, xyzByte, st, cnt);

        }

        // generate osc data
        //----------------------------------------
        Array.Resize(ref oscByte, addrByte.Length + typeByte.Length + infoByte.Length + xyzByte.Length);
        cnt = 0;

        st = 0;
        cnt += addrByte.Length;
        Array.Copy(addrByte, 0, oscByte, st, cnt);

        st += cnt;
        cnt = typeByte.Length;
        Array.Copy(typeByte, 0, oscByte, st, cnt);

        st += cnt;
        cnt = infoByte.Length;
        Array.Copy(infoByte, 0, oscByte, st, cnt);

        st += cnt;
        cnt = xyzByte.Length;
        Array.Copy(xyzByte, 0, oscByte, st, cnt);

        return oscByte;
    }
}
