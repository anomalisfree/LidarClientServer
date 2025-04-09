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

public class OscController : MonoBehaviour
{
    private readonly DebugOut debugOut = new(false, typeof(OscController).Name);

    private bool udpMethodExitFlag = false;

    // view in Unity Editor
    private string rxIpAddressViewer;
    private Int32 rxPortViewer;
    private string txIpAddressViewer;
    private List<Int32> txPortsViewer;

    private Int32 lineValue = 0;
    public Int32 LineMode
    {
        private get
        {
            Int32 value = lineValue;
            lineValue = 0;
            return value;
        }
        set
        {
            if (0 < value)
            {
                lineValue = 1;
            }
            else if (value < 0)
            {
                lineValue = -1;
            }
            else
            {
                lineValue = 0;
            }
        }
    }

    public Controller stateMode;

    private bool bkSelfie = false;
    public bool Selfie { get; set; }
    private Int32 SelfieValue
    {
        get
        {
            if (bkSelfie != Selfie)
            {
                Int32 value;
                if (Selfie)
                {
                    value = 1;
                }
                else
                {
                    value = -1;
                }
                bkSelfie = Selfie;
                return value;
            }
            else
            {
                return 0;
            }
        }
    }
    public bool DebugSelfie { get; private set; } = false;

    private bool bkUpsideDown = false;
    public bool UpsideDown { get; set; }
    private Int32 UpsideDownValue
    {
        get
        {
            if (bkUpsideDown != UpsideDown)
            {
                Int32 value;
                if (UpsideDown)
                {
                    value = 1;
                }
                else
                {
                    value = -1;
                }
                bkUpsideDown = UpsideDown;
                return value;
            }
            else
            {
                return 0;
            }
        }
    }
    public bool DebugUpsideDown { get; private set; } = false;

    private int markerPositionValue = 0;
    public int MarkerPositionValue
    {
        private get
        {
            int value = markerPositionValue;
            markerPositionValue = 0;
            return value;
        }
        set
        {
            markerPositionValue = value;
        }
    }

    private Int32 controlValue = 0;
    public Int32 ControlFlag
    {
        get
        {
            Int32 value = controlValue;
            controlValue = 0;
            return value;
        }
        set { controlValue = value; }
    }

    public Controller controlFlag;
    private Func<Int32, Int32, byte[]> controlCallBack = null;

    private void Awake()
    {
        stateMode = new Controller(true);
        controlFlag = new Controller(false);
    }

    // Use this for initialization
    void Start()
    {
        debugOut.Print("[" + MethodBase.GetCurrentMethod().Name + "]");
    }

    void OnDestroy()
    {
        debugOut.Print("[" + MethodBase.GetCurrentMethod().Name + "]");

        udpMethodExitFlag = true;
    }

    public void StartController(IpConfig ipConfig)
    {
        udpMethodExitFlag = false;
        Task.Run(() => UdpMethod(ipConfig.rxAddressStr, ipConfig.rxPortNumber, ipConfig.txAddressStr, ipConfig.txPortNumber));
    }

    public void SetControlCallBack(Func<int, int, byte[]> callBack)
    {
        controlCallBack += callBack;
    }

    // Update is called once per frame
    void Update()
    {
    }

    void TxMethod(List<IPEndPoint> eps, byte[] dgram)
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
                catch{}
#else
                catch (Exception ex)
                {
                    Debug.Log(ex.Message);

                }
#endif
            }
        }
    }

    void UdpMethod(string rxIpAddress, Int32 rxPort, string txIpAddress, List<int> txPorts)
    {
        debugOut.Print("[start]");

        UdpClient udpRx = null;
        ControlJson controlJson;
        InfoJson infoJson;
        byte[] dgram;

        Int32 index;
        Int32 value;
        List<IPEndPoint> txEps = new();

        if (null != rxIpAddress)
        {
            rxIpAddressViewer = rxIpAddress;

            if (0 < rxPort)
            {
                rxPortViewer = rxPort;
                debugOut.Print("[rx]" + DebugStatic.GetMessageString(rxIpAddress, rxPort));
            }
            else
            {
                debugOut.Print("control send port not set...");
                udpMethodExitFlag = true;
            }
        }
        else
        {
            if (null != controlCallBack)
            {
                debugOut.Print("callBack: " + ((Func<Int32, Int32, byte[]>)controlCallBack).Method.Name.ToString());
            }
            else
            {
                debugOut.Print("control send callback not set...");
            }
        }

        if (null != txIpAddress)
        {
            txIpAddressViewer = txIpAddress;

            if (null != txPorts)
            {
                txPortsViewer = txPorts;

                index = -1;
                foreach (var port in txPorts)
                {
                    index += 1;
                    debugOut.Print("[tx]" + "[" + index.ToString() + "]" + DebugStatic.GetMessageString(txIpAddress, port));

                    txEps.Add(new(IPAddress.Parse(txIpAddress), port));
                }
            }
            else
            {
                debugOut.Print("osc send port not set...");
                udpMethodExitFlag = true;
            }
        }

        while (true)
        {
            System.Threading.Thread.Sleep(500);

            if (udpMethodExitFlag)
            {
                break;
            }
            else
            {
                /*
                 * LineMode
                 */
                value = LineMode;
                if (0 != value)
                {
                    debugOut.Print("value: " + value);

                    controlJson = GetLineControlJson(value);
                    dgram = JsonParser.JsonSerialize(controlJson);
                    TxMethod(txEps, dgram);
                }

                /*
                 * SelfieValue
                 */
                value = SelfieValue;
                if (0 != value)
                {
                    if (DolphinLabCoreStatic.standaloneMode)
                    {
                        if (0 < value)
                        {
                            DebugSelfie = true;
                        }
                        else if (value < 0)
                        {
                            DebugSelfie = false;
                        }
                    }
                    else
                    {
                        infoJson = GetSelfieInfoJson(value);
                        dgram = JsonParser.JsonSerialize(infoJson);
                        TxMethod(txEps, dgram);
                    }
                    debugOut.Print("DebugSelfie: " + DebugSelfie.ToString());
                }

                /*
                 * UpsideDownValue
                 */
                value = UpsideDownValue;
                if (0 != value)
                {
                    if (DolphinLabCoreStatic.standaloneMode)
                    {
                        if (0 < value)
                        {
                            DebugUpsideDown = true;
                        }
                        else if (value < 0)
                        {
                            DebugUpsideDown = false;
                        }
                    }
                    else
                    {
                        infoJson = GetUpsideDownInfoJson(value);
                        dgram = JsonParser.JsonSerialize(infoJson);
                        TxMethod(txEps, dgram);
                    }
                    debugOut.Print("DebugUpsideDown: " + DebugUpsideDown.ToString());
                }

                if (controlFlag.ControlEnable)
                {
                    if (null != controlCallBack)
                    {
                        dgram = controlCallBack(controlFlag.ModeId, controlFlag.ControlValue);
                        TxMethod(txEps, dgram);
                    }
                }

                if (stateMode.ControlEnable)
                {
                    debugOut.Print("StateMode");

                    controlJson = GetStateControlJson();
                    dgram = JsonParser.JsonSerialize(controlJson);
                    TxMethod(txEps, dgram);
                }
                else if (null != rxIpAddress)
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
                            /*
                             * Rx Control
                             */
                            IPEndPoint remoteEp = null;
                            dgram = udpRx.Receive(ref remoteEp);

                            try
                            {
                                infoJson = JsonParser.JsonDeserialize<InfoJson>(dgram);

                                if (0 < infoJson.spec.upsideDown)
                                {
                                    DebugUpsideDown = true;
                                }
                                else if (infoJson.spec.upsideDown < 0)
                                {
                                    DebugUpsideDown = false;
                                }

                                if (0 < infoJson.spec.selfie)
                                {
                                    DebugSelfie = true;
                                }
                                else if (infoJson.spec.selfie < 0)
                                {
                                    DebugSelfie = false;
                                }
                            }
                            catch { }

                            TxMethod(txEps, dgram);
                        }
                        catch { }
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

    public void SetStartStop(Int32 mode)
    {
        stateMode.ControlValue = mode;
    }

    public void SetLineMode(Int32 mode)
    {
        LineMode = mode;
    }

    public void ToggleSelfie()
    {
        Selfie = !Selfie;
    }

    private ControlJson InitControlJson()
    {
        ControlJson controlJson = new();
        controlJson.Setting = new SettingJson
        {
            System = new SettingJsonSystem(),
            LM = new SettingJsonLm(),
        };
        return controlJson;
    }

    private ControlJson GetLineControlJson(Int32 mode)
    {
        ControlJson controlJson = InitControlJson();

        if (mode < 0)
        {
            controlJson.Setting.LM.line = -1;
        }
        else
        {
            controlJson.Setting.LM.line = 1;
        }
        return controlJson;
    }

    private ControlJson GetStateControlJson()
    {
        ControlJson controlJson = InitControlJson();
        controlJson.Setting.System.state = 1;
        return controlJson;
    }

    // For future use after firmware update
    private ControlJson GetSelfieControlJson(Int32 value)
    {
        ControlJson controlJson = InitControlJson();
        controlJson.Setting.System.selfie = value;
        return controlJson;
    }

    // For future use after firmware update
    private ControlJson GetUpsideDownControlJson(Int32 value)
    {
        ControlJson controlJson = InitControlJson();
        controlJson.Setting.System.upsideDown = value;
        return controlJson;
    }

    private InfoJson InitInfoJson()
    {
        InfoJson infoJson = new();
        infoJson.spec = new InfoJsonSpec();
        return infoJson;
    }

    private InfoJson GetSelfieInfoJson(Int32 value)
    {
        InfoJson infoJson = InitInfoJson();
        infoJson.spec.selfie = value;
        return infoJson;
    }

    private InfoJson GetUpsideDownInfoJson(Int32 value)
    {
        InfoJson infoJson = InitInfoJson();
        infoJson.spec.upsideDown = value;
        return infoJson;
    }
}
