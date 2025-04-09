/*!
# SPDX-FileCopyrightText: © 2023 Dolphin Co.,Ltd. <http://lidar.jp>
#
# SPDX-License-Identifier: MIT
*/

using System;
using System.Collections.Generic;

public class IpConfig
{
    public string rxAddressStr = null;
    public string txAddressStr = null;
    public int rxPortNumber;
    public List<int> txPortNumber;

    public IpConfig(string rxAddress, int rxPort, string txAddress, IEnumerable<int> txPort)
    {
        rxAddressStr = rxAddress;
        rxPortNumber = rxPort;
        if (null != txAddress)
        {
            txAddressStr = txAddress;
            txPortNumber = new List<int>();
            foreach (var value in txPort)
            {
                txPortNumber.Add(value);
            }
        }
    }
}