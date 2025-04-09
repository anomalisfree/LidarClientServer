/*!
# SPDX-FileCopyrightText: © 2023 Dolphin Co.,Ltd. <http://lidar.jp>
#
# SPDX-License-Identifier: MIT
*/

using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

using DllIpConfigReadonly;

public class IpConfigSetting
{
    /*
     * For raw data communication
     */
    private string rawDataRxIpAddress = IpConfigReadonly.rawDataDefaultRxIpAddress;

    /*
     * For OSC data communication
     */
    private int oscDataRxPort = IpConfigReadonly.oscDataDefaultRxPort;
    private int oscDataTxPort0 = IpConfigReadonly.oscDataDefaultRxPort;
    private int oscDataTxPort1 = IpConfigReadonly.oscDataDefaultTxPort1;
    private int oscDataTxPort2 = IpConfigReadonly.oscDataDefaultTxPort2;

    /*
     * For control data communication
     */
    private string controlDataTxIpAddress = IpConfigReadonly.controlDataDefaultTxIpAddress;

    [Serializable]
    private class IpConfigJsonConverter
    {
        public string rawDataRxIpAddress;
        public IpConfigJsonConverterTxPorts oscDataTxPorts;
        public string controlDataTxIpAddress;

        public IpConfigJsonConverter()
        {
            rawDataRxIpAddress = IpConfigReadonly.rawDataDefaultRxIpAddress;
            List<int> port = new();
            port.Add(IpConfigReadonly.oscDataDefaultTxPort0);
            port.Add(IpConfigReadonly.oscDataDefaultTxPort1);
            port.Add(IpConfigReadonly.oscDataDefaultTxPort2);
            oscDataTxPorts = new(port);
            controlDataTxIpAddress = IpConfigReadonly.controlDataDefaultTxIpAddress;
        }
    }

    [Serializable]
    private class IpConfigJsonConverterTxPorts
    {
        public List<int> port;

        public IpConfigJsonConverterTxPorts(List<int> ports)
        {
            port = ports;
        }
    }

    [Serializable]
    private class IpConfigJsonStandalone
    {
        public string rawDataRxIpAddress;
        public string controlDataTxIpAddress;

        public IpConfigJsonStandalone()
        {
            rawDataRxIpAddress = IpConfigReadonly.rawDataDefaultRxIpAddress;
            controlDataTxIpAddress = IpConfigReadonly.controlDataDefaultTxIpAddress;
        }
    }

    [Serializable]
    private class IpConfigJsonViaOsc
    {
        public int oscDataRxPort;

        public IpConfigJsonViaOsc()
        {
            oscDataRxPort = IpConfigReadonly.oscDataDefaultRxPort;
        }
    }

    public IpConfigSetting()
    {
        IpConfigStatic.oscConfigStandalone = new(IpConfigReadonly.rawDataDefaultRxIpAddress, IpConfigReadonly.rawDataDefaultRxPort, null, null);
        IpConfigStatic.oscConfigConverter = new(IpConfigReadonly.rawDataDefaultRxIpAddress, IpConfigReadonly.rawDataDefaultRxPort, IpConfigReadonly.oscDataDefaultLocalHostTxIpAddress, new[] { oscDataTxPort0, oscDataTxPort1, oscDataTxPort2 });
        IpConfigStatic.oscConfigViaOsc = new(IpConfigReadonly.oscDataDefaultLocalHostRxIpAddress, oscDataRxPort, null, null);

        IpConfigStatic.controlConfigConverter = new(IpConfigReadonly.controlDataDefaultRxIpAddress, IpConfigReadonly.controlDataDefaultRxPort, IpConfigReadonly.controlDataDefaultTxIpAddress, new[] { IpConfigReadonly.controlDataDefaultTxPort });
        IpConfigStatic.controlConfigStandalone = new(null, -1, IpConfigReadonly.controlDataDefaultTxIpAddress, new[] { IpConfigReadonly.controlDataDefaultTxPort });
        IpConfigStatic.controlConfigViaOsc = new(null, -1, IpConfigReadonly.controlDataDefaultLocalHostTxIpAddress, new[] { IpConfigReadonly.controlDataDefaultLocalHostTxPort });
    }

    private bool CheckIpAddress(string ipAddressText)
    {
        bool rcd;
        var defIpArySt = IpConfigReadonly.rawDataDefaultRxIpAddress.Split('.');
        List<int> defIpList = new();
        for (int i = 0; i < 4; i++)
        {
            defIpList.Add(Convert.ToInt32(defIpArySt[i]));
        }

        try
        {
            var ipAry = ipAddressText.Split('.');
            if (4 == ipAry.Length)
            {
                int value;

                value = Convert.ToInt32(ipAry[0]);
                if (value != defIpList[0])
                {
                    rcd = false;
                    Debug.Log("[IP address setting range error]" + "syntax: " + ipAddressText);
                    Debug.Log("[IP address setting range]" + defIpList[0].ToString() + "." + defIpList[1].ToString() + "." + "XXX.XXX");
                }
                else
                {
                    value = Convert.ToInt32(ipAry[1]);
                    if (value != defIpList[1])
                    {
                        rcd = false;
                        Debug.Log("[IP address range error]" + "syntax: " + ipAddressText);
                        Debug.Log("[IP address range]" + defIpList[0].ToString() + "." + defIpList[1].ToString() + "." + "XXX.XXX");
                    }
                    else
                    {
                        value = Convert.ToInt32(ipAry[2]);
                        if ((value < 0) || (255 < value))
                        {
                            rcd = false;
                            Debug.Log("[IP address value error]" + "syntax: " + ipAddressText);
                            Debug.Log("[IP address value]" + "0 to 255");
                        }
                        else
                        {
                            value = Convert.ToInt32(ipAry[3]);
                            if ((value < 1) || (254 < value))
                            {
                                rcd = false;
                                Debug.Log("[IP address value error]" + "syntax: " + ipAddressText);
                                Debug.Log("[IP address value]" + "1 to 254");
                            }
                            else
                            {
                                rcd = true;
                            }
                        }
                    }
                }
            }
            else
            {
                rcd = false;
                Debug.Log("[IP address syntax error]" + "syntax: " + ipAddressText);
                Debug.Log("[IP address syntax]" + defIpList[0].ToString() + "." + defIpList[1].ToString() + "." + "XXX.XXX");
            }
        }
        catch
        {
            rcd = false;
            Debug.Log("[IP address syntax error]" + "syntax: " + ipAddressText);
            Debug.Log("[IP address syntax]" + defIpList[0].ToString() + "." + defIpList[1].ToString() + "." + "XXX.XXX");
        }

        return rcd;
    }

    private string InitIpConfigConverter()
    {
        IpConfigJsonConverter ipConfigJson = new();
        string jsonStr = JsonUtility.ToJson(ipConfigJson, true);
        return jsonStr;
    }

    private string InitIpConfigStandalone()
    {
        IpConfigJsonStandalone ipConfigJson = new();
        string jsonStr = JsonUtility.ToJson(ipConfigJson, true);
        return jsonStr;
    }

    private string InitIpConfigViaOsc()
    {
        IpConfigJsonViaOsc ipConfigJson = new();
        string jsonStr = JsonUtility.ToJson(ipConfigJson, true);
        return jsonStr;
    }

    private void CreateInitIpConfigFile(string rootPath, string jsonString)
    {
        string path = rootPath + "/" + IpConfigReadonly.initialIpConfigFileName;
        StreamWriter sw = new(path, false); // overwrite
        sw.Write(jsonString);
        sw.Flush();
        sw.Close();
    }

    public void InitIpConfigConverter(string rootPath)
    {
        string path;
        IpConfigJsonConverter jsonObj;
        string initJsonString;
        string userJsonString;

        initJsonString = InitIpConfigConverter();
        CreateInitIpConfigFile(rootPath, initJsonString);

        path = rootPath + "/" + IpConfigReadonly.ipConfigFileName;
        if (System.IO.File.Exists(path))
        {
            StreamReader sr = new(path);
            userJsonString = sr.ReadToEnd();
            try
            {
                jsonObj = JsonParser.JsonDeserialize<IpConfigJsonConverter>(userJsonString);
            }
            catch (Exception ex)
            {
                Debug.Log("[FileReadError]" + "file name: " + IpConfigReadonly.ipConfigFileName + ", " + "ex.Message: " + ex.Message);
                jsonObj = JsonParser.JsonDeserialize<IpConfigJsonConverter>(initJsonString);

            }
        }
        else
        {
            jsonObj = JsonParser.JsonDeserialize<IpConfigJsonConverter>(initJsonString);
        }

        if (CheckIpAddress(jsonObj.rawDataRxIpAddress))
        {
            rawDataRxIpAddress = jsonObj.rawDataRxIpAddress;
        }
        else
        {
            rawDataRxIpAddress = IpConfigReadonly.rawDataDefaultRxIpAddress;
        }

        if (CheckIpAddress(jsonObj.controlDataTxIpAddress))
        {
            controlDataTxIpAddress = jsonObj.controlDataTxIpAddress;
        }
        else
        {
            controlDataTxIpAddress = IpConfigReadonly.controlDataDefaultTxIpAddress;
        }

        List<int> txPort = jsonObj.oscDataTxPorts.port;

        IpConfigStatic.oscConfigConverter = new(rawDataRxIpAddress, IpConfigReadonly.rawDataDefaultRxPort, IpConfigReadonly.oscDataDefaultLocalHostTxIpAddress, txPort);
        IpConfigStatic.controlConfigConverter = new(IpConfigReadonly.controlDataDefaultRxIpAddress, IpConfigReadonly.controlDataDefaultRxPort, controlDataTxIpAddress, new[] { IpConfigReadonly.controlDataDefaultTxPort });
    }

    public void InitIpConfigStandalone(string rootPath)
    {
        string path;
        IpConfigJsonStandalone jsonObj;
        string initJsonString;
        string userJsonString;

        initJsonString = InitIpConfigStandalone();
        CreateInitIpConfigFile(rootPath, initJsonString);

        path = rootPath + "/" + IpConfigReadonly.ipConfigFileName;
        if (System.IO.File.Exists(path))
        {
            StreamReader sr = new(path);
            userJsonString = sr.ReadToEnd();
            try
            {
                jsonObj = JsonParser.JsonDeserialize<IpConfigJsonStandalone>(userJsonString);
            }
            catch (Exception ex)
            {
                Debug.Log("[FileReadError]" + "file name: " + IpConfigReadonly.ipConfigFileName + ", " + "ex.Message: " + ex.Message);
                jsonObj = JsonParser.JsonDeserialize<IpConfigJsonStandalone>(initJsonString);
            }
        }
        else
        {
            jsonObj = JsonParser.JsonDeserialize<IpConfigJsonStandalone>(initJsonString);
        }

        if (CheckIpAddress(jsonObj.rawDataRxIpAddress))
        {
            rawDataRxIpAddress = jsonObj.rawDataRxIpAddress;
        }
        else
        {
            rawDataRxIpAddress = IpConfigReadonly.rawDataDefaultRxIpAddress;
        }

        if (CheckIpAddress(jsonObj.controlDataTxIpAddress))
        {
            controlDataTxIpAddress = jsonObj.controlDataTxIpAddress;
        }
        else
        {
            controlDataTxIpAddress = IpConfigReadonly.controlDataDefaultTxIpAddress;
        }

        IpConfigStatic.oscConfigStandalone = new(rawDataRxIpAddress, IpConfigReadonly.rawDataDefaultRxPort, null, null);
        IpConfigStatic.controlConfigStandalone = new(null, -1, controlDataTxIpAddress, new[] { IpConfigReadonly.controlDataDefaultTxPort });
    }

    public void InitIpConfigViaOsc(string rootPath)
    {
        string path;
        IpConfigJsonViaOsc jsonObj;
        string initJsonString;
        string userJsonString;

        initJsonString = InitIpConfigViaOsc();
        CreateInitIpConfigFile(rootPath, initJsonString);

        path = rootPath + "/" + IpConfigReadonly.ipConfigFileName;
        if (System.IO.File.Exists(path))
        {
            StreamReader sr = new(path);
            userJsonString = sr.ReadToEnd();
            try
            {
                jsonObj = JsonParser.JsonDeserialize<IpConfigJsonViaOsc>(userJsonString);
            }
            catch (Exception ex)
            {
                Debug.Log("[FileReadError]" + "file name: " + IpConfigReadonly.ipConfigFileName + ", " + "ex.Message: " + ex.Message);
                jsonObj = JsonParser.JsonDeserialize<IpConfigJsonViaOsc>(initJsonString);
            }
        }
        else
        {
            jsonObj = JsonParser.JsonDeserialize<IpConfigJsonViaOsc>(initJsonString);
        }

        oscDataRxPort = jsonObj.oscDataRxPort;

        IpConfigStatic.oscConfigViaOsc = new(IpConfigReadonly.oscDataDefaultLocalHostRxIpAddress, oscDataRxPort, null, null);
        IpConfigStatic.controlConfigViaOsc = new(null, -1, IpConfigReadonly.controlDataDefaultLocalHostTxIpAddress, new[] { IpConfigReadonly.controlDataDefaultLocalHostTxPort });
    }
}
