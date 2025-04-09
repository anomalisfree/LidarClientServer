/*!
# SPDX-FileCopyrightText: © 2023 Dolphin Co.,Ltd. <http://lidar.jp>
#
# SPDX-License-Identifier: MIT
*/

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;

using DllBinaryParser;
using DllOscParserAccelerated;
using DllOscReadonly;
using DllXYRadianConversion;

public class OcsConverter : MonoBehaviour
{
    private readonly DebugOut debugOut = new(false, typeof(OcsConverter).Name);

    private bool udpMethodExitFlag = false;

    private Action<byte[]> OnRxCallBack;
    private Action<BinaryDataInformation, Vector3[]> OnLocalCallBack;

    private bool bkState = false;
    public bool State { get; private set; } = false;
    public bool UdpConnection { get; private set; } = false;
    public int MaxLineNumber { get; private set; } = 0;
    public int FrameNumber { get; private set; } = 0;
    public int WidthValue { get; private set; } = 0;

    public bool InfoGetDone { get; private set; } = false;

    // view in Unity Editor
    private string rxIpAddressViewer;
    private int rxPortViewer;
    private string txIpAddressViewer;
    private List<int> txPortsViewer;
    public int rxLineNumber = 0;
    public int totalLineNumber = 0;
    public int pointsPerLine = 0;
    private string oscAddressTofXyzLine;

    public DateTime nowDateTime = DateTime.Now;

    public XYRadianConversion XyRad { get; private set; }
    public float[][] TofData { get; private set; }

    // view in Unity Editor
    private float xPhysicalAngle = 58.0f;
    private float bkXPhysicalAngle = 0.0f;
    private float xPhysicalRadian;
    private float xLogicalAngleCorrection = 0.0f;
    private float xLogicalRadianCorrection;

    private float yMvPerDegree = 14.0f;
    private float YmvPerDegree { get { return yMvPerDegree; } }
    private int yVtop = 0;
    private Int32 YVtop { get { return yVtop; } set { yVtop = value; } }
    private int yVref = 0;
    private Int32 YVref { get { return yVref; } set { yVref = value; } }
    private int yVbtm = 0;
    private Int32 YVbtm { get { return yVbtm; } set { yVbtm = value; } }
    private int yStep = 0;
    private Int32 YStep { get { return yStep; } set { yStep = value; } }

    private bool bkUpsideDown = false;
    public bool UpsideDown { get; set; } = false;
    private Int32 UpsideDownValue
    {
        get
        {
            if (UpsideDown)
            {
                return 1;
            }
            else
            {
                return -1;
            }
        }
    }

    private bool bkSelfie = false;
    public bool Selfie { get; set; } = false;
    private Int32 SelfieValue
    {
        get
        {
            if (Selfie)
            {
                return -1;
            }
            else
            {
                return 1;
            }
        }
    }

    private TimeMeasurement timeMeasurement;
    public List<float> oscConvertlapTimesMs;

    private float xNumerator = 0.7f;
    private float bkXNumerator = 4.0f;
    private float XNumerator { get { return xNumerator; } set { xNumerator = value; } }
    private float xDenominator = 15.0f;
    private float bkXDenominator = 0.0f;
    private float XDenominator { get { return xDenominator; } set { xDenominator = value; } }

    private float zNumerator = 0.0f;
    private float bkZNumerator = 4.0f;
    private float ZNumerator { get { return zNumerator; } set { zNumerator = value; } }
    private float zDenominator = 15.0f;
    private float bkZDenominator = 0.0f;
    private float ZDenominator { get { return zDenominator; } set { zDenominator = value; } }

    // Start is called before the first frame update
    void Start()
    {
        debugOut.Print("[" + MethodBase.GetCurrentMethod().Name + "]");

        XyRad = new XYRadianConversion();
    }

    void OnDestroy()
    {
        debugOut.Print("[" + MethodBase.GetCurrentMethod().Name + "]");

        udpMethodExitFlag = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (bkState != State)
        {
            bkState = State;

            if (!State)
            {
                nowDateTime = DateTime.Now;
            }
        }
    }

    public void StartConverter(bool debugOutOsc, IpConfig ipConfig, Action<byte[]> rxAction, Action<BinaryDataInformation, Vector3[]> localAction)
    {
        debugOut.Print("");

        timeMeasurement = new TimeMeasurement();

        OnRxCallBack += rxAction;
        OnLocalCallBack += localAction;

        debugOut.Print(OscReadonly.oscDataDiscription);

        udpMethodExitFlag = false;
        Task.Run(() => UdpMethod(debugOutOsc, ipConfig.rxAddressStr, ipConfig.rxPortNumber, ipConfig.txAddressStr, ipConfig.txPortNumber));
    }


    private void UpdateUdpRxMethodPublic(Int32 line, Int32 maxLine, Int32 points)
    {
        rxLineNumber = line;
        totalLineNumber = maxLine;
        pointsPerLine = points;
    }

    private void TxMethod(List<IPEndPoint> eps, byte[] dgram)
    {
        if (0 != eps.Count)
        {
            UdpClient udpTx = new();

            foreach (var value in eps)
            {
                try
                {
                    udpTx.SendAsync(dgram, dgram.Length, value);
                }
#if UNITY_STANDALONE_WIN
                catch { }
#else
                catch (Exception ex)
                {
                    Debug.Log(ex.Message);
                }
#endif
            }
        }
        else
        {
            try
            {
                OnRxCallBack(dgram);
            }
#if UNITY_STANDALONE_WIN
            catch { }
#else
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
            }
#endif
        }
    }

    private bool CheckDebugYParameterChange()
    {
        if (
            (bkXNumerator != xNumerator) ||
            (bkXDenominator != xDenominator) ||
            (bkZNumerator != zNumerator) ||
            (bkZDenominator != zDenominator))
        {
            bkXNumerator = xNumerator;
            bkXDenominator = xDenominator;
            bkZNumerator = zNumerator;
            bkZDenominator = zDenominator;

            return true;
        }
        else
        {
            return false;
        }
    }

    private bool CheckDebugXParameterChange()
    {
        if (
            (bkXPhysicalAngle != xPhysicalAngle))
        {
            XyRad.XPhysicalAngle = xPhysicalAngle;
            xPhysicalRadian = Mathf.Deg2Rad * xPhysicalAngle;
            XyRad.XPhysicalRadian = xPhysicalRadian;

            bkXPhysicalAngle = xPhysicalAngle;

            return true;
        }
        else
        {
            return false;
        }
    }

    private void UdpMethod(bool debugOutOsc, string rxIpAddress, Int32 rxPort, string txIpAddress, List<Int32> txPorts)
    {
        debugOut.Print("[start]");
        Debug.Log("[rx]" + DebugStatic.GetMessageString(rxIpAddress, rxPort));

        UdpClient udpRx = null;
        BinaryParser binaryParser = new();
        OscParser oscParser = new();
        OscParserAccelerated oscParserAccelerated = new();

        InfoJson jsonObj = new();

        Int32 dataFormat = OscReadonly.receiveDataFormatUnDef;

        Int32 frameNumber = 0;
        Int32 maxLineNumber;
        Int32 lineNumber;
        Int32 dataCount;

        float width = 0.0f;
        Int32 maxPointNumber = 0;
        Int32 bkFrameNumber = 0;
        Int32 bkLineNumber = 0;

        Int32 stateCheckCount = 0;

        rxIpAddressViewer = rxIpAddress;
        rxPortViewer = rxPort;

        oscAddressTofXyzLine = OscReadonly.oscAddressTofXyzLine;

        Int32 index;
        List<IPEndPoint> txEps = new();

        if (null != txIpAddress)
        {
            txIpAddressViewer = txIpAddress;

            if (null != txPorts)
            {
                txPortsViewer = txPorts;

                index = -1;
                foreach (var value in txPorts)
                {
                    index += 1;
                    Debug.Log("[tx]" + "[" + index.ToString() + "]" + DebugStatic.GetMessageString(txIpAddress, value));

                    txEps.Add(new(IPAddress.Parse(txIpAddress), value));
                }
            }
            else
            {
                Debug.Log("osc send port not set...");
                udpMethodExitFlag = true;
            }
        }
        else
        {
            if (null != OnRxCallBack)
            {
                debugOut.Print("callBack: " + ((Action<byte[]>)OnRxCallBack).Method.Name.ToString());
            }
            else
            {
                debugOut.Print("osc send callback not set...");
                udpMethodExitFlag = true;
            }
        }

        while (true)
        {
            if (udpMethodExitFlag)
            {
                break;
            }
            else
            {
                if (null == udpRx)
                {
                    try
                    {
                        udpRx = new UdpClient(new IPEndPoint(IPAddress.Parse(rxIpAddress), rxPort));
                        udpRx.Client.ReceiveTimeout = 500;
                    }
                    catch
                    {
                        debugOut.Print("waiting for connection..." + " " + DebugStatic.GetMessageString(rxIpAddress, rxPort));
                        System.Threading.Thread.Sleep(1000);

                        udpRx = null;
                    }
                }
                else
                {
                    try
                    {
                        IPEndPoint remoteEp = null;
                        byte[] data = udpRx.Receive(ref remoteEp);

                        UdpConnection = true;

                        stateCheckCount = 0;

                        try
                        {
                            if (true == binaryParser.CheckBinaryTofData(data))
                            {
                                dataFormat = OscReadonly.receiveDataFormatBin;
                            }
                            else
                            {
                                dataFormat = OscReadonly.receiveDataFormatUnDef;
                            }
                        }
                        catch
                        {
                            dataFormat = OscReadonly.receiveDataFormatUnDef;
                        }

                        if (OscReadonly.receiveDataFormatUnDef == dataFormat)
                        {
                            try
                            {
                                /*
                                 * Can you do it faster than D? ;-)
                                 */
                                if (DebugStatic.SpeedingUp)
                                {
                                    if (true == oscParserAccelerated.GetLineDataAcceleratedCode(false, data))
                                    {
                                        dataFormat = OscReadonly.receiveDataFormatOsc;
                                    }
                                    else
                                    {
                                        dataFormat = OscReadonly.receiveDataFormatUnDef;
                                    }
                                }
                                else
                                {
                                    if (true == oscParser.GetLineData(false, data))
                                    {
                                        dataFormat = OscReadonly.receiveDataFormatOsc;
                                    }
                                    else
                                    {
                                        dataFormat = OscReadonly.receiveDataFormatUnDef;
                                    }
                                }

                            }
                            catch
                            {
                                dataFormat = OscReadonly.receiveDataFormatUnDef;
                            }
                        }

                        if (OscReadonly.receiveDataFormatUnDef == dataFormat)
                        {
                            try
                            {
                                jsonObj = JsonParser.JsonDeserialize<InfoJson>(data);
                                dataFormat = OscReadonly.receiveDataFormatJson;
                            }
                            catch
                            {
                                dataFormat = OscReadonly.receiveDataFormatUnDef;
                            }
                        }

                        if (!InfoGetDone)
                        {
                            if (OscReadonly.receiveDataFormatJson == dataFormat)
                            {
                                frameNumber = (Int32)jsonObj.spec.seqCnt;
                                width = (Int32)jsonObj.spec.adj5;
                                MaxLineNumber = (Int32)jsonObj.spec.maxLine;

                                YStep = (Int32)jsonObj.spec.lmStep;
                                YVtop = jsonObj.spec.lmArea4Top;
                                YVref = jsonObj.spec.lmArea4Typ;
                                YVbtm = jsonObj.spec.lmArea4Btm;

                                WidthValue = (Int32)jsonObj.spec.adj5;

                                XyRad.GenerateYRad(XNumerator, XDenominator, ZNumerator, ZDenominator, MaxLineNumber, YmvPerDegree, YStep, YVtop, YVref);
                                XyRad.GenerateXRad(MaxLineNumber, maxPointNumber);

                                bkXPhysicalAngle = xPhysicalAngle;
                                bkUpsideDown = UpsideDown;
                                bkSelfie = Selfie;

                                if (0 != jsonObj.spec.state)
                                {
                                    State = true;

                                    maxPointNumber = (Int32)jsonObj.spec.maxPoint;

                                    TofData = new float[MaxLineNumber][];
                                    for (int i = 0; i < MaxLineNumber; i++)
                                    {
                                        TofData[i] = new float[maxPointNumber];
                                    }

                                    XyRad.GenerateTransformationCoefficient(SelfieValue, UpsideDownValue, MaxLineNumber, maxPointNumber);
                                }
                                else
                                {
                                    State = false;
                                }


                                InfoGetDone = true;
                                debugOut.Print("connection completed.");

                                //oscConvertlapTimesMs = timeMeasurement.StopStopWatch();

                                if (DolphinLabCoreStatic.converterMode)
                                {
                                    TxMethod(txEps, data);
                                }
                            }
                        }
                        else
                        {
                            if (OscReadonly.receiveDataFormatBin == dataFormat)
                            {
                                try
                                {
                                    FrameNumber = (int)binaryParser.info.frameNumber;
                                    maxLineNumber = (Int32)binaryParser.info.maxLineNumber;
                                    lineNumber = (Int32)binaryParser.info.lineNumber;
                                    dataCount = (Int32)binaryParser.info.dataCount;

                                    maxPointNumber = (Int32)dataCount;

                                    timeMeasurement.StartStopWatch();

                                    string oscAddr = OscReadonly.oscAddressTofXyzLine;

                                    byte[] oscData;
                                    Vector3[] xyz = new Vector3[dataCount];

                                    /*
                                     * Can you do it faster than D? ;-)
                                     */
                                    if (DebugStatic.SpeedingUp)
                                    {
                                        oscData = oscParserAccelerated.GenerateBinary2OscTransformationAcceleratedCode(data, binaryParser, oscAddr, XyRad.coefficientAry[lineNumber]);
                                    }
                                    else
                                    {
                                        oscData = oscParser.GenerateBinary2OscTransformation(data, binaryParser, oscAddr, XyRad.coefficientAry[lineNumber]);
                                    }

                                    timeMeasurement.AddLapTime();

                                    if (null == oscData)
                                    {
                                        debugOut.Print("null == oscData");
                                    }
                                    TxMethod(txEps, oscData);

                                    oscConvertlapTimesMs = timeMeasurement.StopStopWatch();

                                    UpdateUdpRxMethodPublic(lineNumber, maxLineNumber, dataCount);


                                    if (debugOutOsc)
                                    {
                                        if (bkFrameNumber != (Int32)binaryParser.info.frameNumber)
                                        {
                                            debugOut.Print("frameNumber: " + binaryParser.info.frameNumber.ToString() + ", " + "maxLineNumber: " + maxLineNumber.ToString() + ", " + "oscAddr:" + oscAddr);
                                            bkFrameNumber = (Int32)binaryParser.info.frameNumber;
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    debugOut.ExPrint("[Exception]" + "[DolphinLabOscStatic.receiveDataFormatBin == dataFormat]" + ex.Message);
                                }
                            }
                            else if (OscReadonly.receiveDataFormatOsc == dataFormat)
                            {
                                maxLineNumber = (Int32)oscParser.dataInfo.maxLineNumber;
                                lineNumber = (Int32)oscParser.dataInfo.currentLineNumber;
                                dataCount = (Int32)oscParser.dataInfo.pointsPerLine;

                                maxPointNumber = (Int32)dataCount;

                                timeMeasurement.StartStopWatch();

                                TxMethod(txEps, data);

                                if ((bkLineNumber + 1) != lineNumber)
                                {
                                    if ((bkLineNumber + 1) != maxLineNumber)
                                    {
                                        debugOut.Print("bkLineNumber: " + bkLineNumber.ToString() + ", " + "lineNumber: " + lineNumber.ToString());
                                    }
                                    bkLineNumber = lineNumber;
                                }

                                UpdateUdpRxMethodPublic(lineNumber, maxLineNumber, dataCount);

                                oscConvertlapTimesMs = timeMeasurement.StopStopWatch();
                            }
                            else if (OscReadonly.receiveDataFormatJson == dataFormat)
                            {
                                if (0 < jsonObj.spec.upsideDown)
                                {
                                    UpsideDown = true;
                                }
                                else if (jsonObj.spec.upsideDown < 0)
                                {
                                    UpsideDown = false;
                                }

                                if (0 < jsonObj.spec.selfie)
                                {
                                    Selfie = true;
                                }
                                else if (jsonObj.spec.selfie < 0)
                                {
                                    Selfie = false;
                                }

                                if (State)
                                {
                                    frameNumber = (Int32)jsonObj.spec.seqCnt;
                                    if ((bkFrameNumber + 1) != frameNumber)
                                    {
                                        debugOut.Print("bkFrameNumber: " + bkFrameNumber.ToString() + ", " + "frameNumber: " + frameNumber.ToString());
                                    }
                                    bkFrameNumber = frameNumber;

                                    if (
                                        (CheckDebugYParameterChange()) ||
                                        (bkUpsideDown != UpsideDown) ||
                                        (MaxLineNumber != (Int32)jsonObj.spec.maxLine) ||
                                       CheckDebugXParameterChange() ||
                                       (bkSelfie != Selfie) ||
                                       (maxPointNumber != (Int32)jsonObj.spec.maxPoint)
                                        )
                                    {
                                        MaxLineNumber = (Int32)jsonObj.spec.maxLine;
                                        YStep = (Int32)jsonObj.spec.lmStep;
                                        YVtop = jsonObj.spec.lmArea4Top;
                                        YVref = jsonObj.spec.lmArea4Typ;
                                        YVbtm = jsonObj.spec.lmArea4Btm;

                                        XyRad.GenerateYRad(XNumerator, XDenominator, ZNumerator, ZDenominator, MaxLineNumber, YmvPerDegree, YStep, YVtop, YVref);
                                        XyRad.GenerateXRad(MaxLineNumber, maxPointNumber);
                                        XyRad.GenerateTransformationCoefficient(SelfieValue, UpsideDownValue, MaxLineNumber, maxPointNumber);

                                        bkUpsideDown = UpsideDown;

                                        TofData = new float[MaxLineNumber][];
                                        for (int i = 0; i < MaxLineNumber; i++)
                                        {
                                            TofData[i] = new float[maxPointNumber];
                                        }

                                        if (0 < (Int32)jsonObj.spec.maxPoint)
                                        {
                                            maxPointNumber = (Int32)jsonObj.spec.maxPoint;

                                            XyRad.GenerateXRad(MaxLineNumber, maxPointNumber);
                                            XyRad.GenerateTransformationCoefficient(SelfieValue, UpsideDownValue, MaxLineNumber, maxPointNumber);

                                            bkSelfie = Selfie;
                                        }
                                    }
                                }

                                if (0 != jsonObj.spec.state)
                                {
                                    State = true;
                                }
                                else
                                {
                                    State = false;
                                }

                                YStep = (Int32)jsonObj.spec.lmStep;
                                YVtop = jsonObj.spec.lmArea4Top;
                                YVref = jsonObj.spec.lmArea4Typ;
                                YVbtm = jsonObj.spec.lmArea4Btm;

                                WidthValue = (Int32)jsonObj.spec.adj5;

                                if (DolphinLabCoreStatic.converterMode)
                                {
                                    TxMethod(txEps, data);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        stateCheckCount++;
                        if (4 < stateCheckCount)
                        {
                            State = false;
                            InfoGetDone = false;
                            stateCheckCount = 0;
                            UdpConnection = false;
                        }
                        debugOut.ExPrint("[Exception]" + ex.Message);

                        //oscConvertlapTimesMs = timeMeasurement.StopStopWatch();
                    }
                }
            }
        }

        if (null != udpRx)
        {
            udpRx.Close();
            udpRx.Dispose();
        }

        debugOut.Print("[exit]");
    }

}
