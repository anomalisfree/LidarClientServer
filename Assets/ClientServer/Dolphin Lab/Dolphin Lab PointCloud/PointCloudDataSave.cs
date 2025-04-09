/*!
# SPDX-FileCopyrightText: © 2023 Dolphin Co.,Ltd. <http://lidar.jp>
#
# SPDX-License-Identifier: MIT
*/

using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

using DllPointCloudDataSaveReadonly;
using DllSaveDataParser;

public class PointCloudDataSave : MonoBehaviour
{
    private PointCloudData pointCloudData;
    private SaveDataParser saveDataParser;

    private Vector3[] maskPos;

    public static readonly string newLineCode = Environment.NewLine;

    //public PointCloudDataSave()
    void Start()
    {
        pointCloudData = PointCloudStatic.gameObject.GetComponent<PointCloudData>();
        saveDataParser = new();

        maskPos = new Vector3[8];
        for (int i = 0; i < 8; i++)
        {
            maskPos[i] = new Vector3(0.0f, 0.0f, 0.0f);
        }
    }

    public void SavePly(bool triangle, DateTime now, int cubeSizeRatioX, int cubeSizeRatioY)
    {
        PlyData plyData;
        string fileNameCommon = PointCloudDataSaveStatic.GetCommonPrefixName(now);
        string fileName;
        string path;

        string saveFolder = PointCloudDataSaveStatic.GetSaveFolder(now);
        string dataPath = PointCloudDataSaveStatic.GetDataPath();

        Directory.CreateDirectory(Path.Combine(dataPath, saveFolder));
        Vector3[] mask;
        mask = new Vector3[8];

        for (int k = 0; k < 8; k++)
        {
            mask[k] = maskPos[k];
        }

        plyData = saveDataParser.CreatePlyData(
            pointCloudData.OutputNumScanLine,
            pointCloudData.OutputNumPointsPerLine,
            pointCloudData.DataBuffer[pointCloudData.DrawFrameBufferNum],
            pointCloudData.ColorBuffer[pointCloudData.DrawFrameBufferNum],
            mask, triangle, cubeSizeRatioX, cubeSizeRatioY
            );

        if (true == triangle)
        {
            fileName = fileNameCommon + PointCloudDataSaveReadonly.filenameSuffixPlyBox + PointCloudDataSaveReadonly.extensionPly;
            path = dataPath + "/" + saveFolder + "/" + fileName;
            Debug.Log("path = " + path);

            StringBuilder sb = new();

            int vertex = 0;
            try
            {
                vertex = plyData.point.Count;
                for (int i = 0; i < vertex; i++)
                {
                    string x = plyData.point[i].x.ToString();
                    string y = plyData.point[i].y.ToString();
                    string z = plyData.point[i].z.ToString();

                    string r = plyData.color[i].r.ToString();
                    string g = plyData.color[i].g.ToString();
                    string b = plyData.color[i].b.ToString();

                    string st = "\n" + x + " " + y + " " + z + " " + r + " " + g + " " + b;
                    sb.Append(st);
                }
            }
#if UNITY_STANDALONE_WIN
            catch{}
#else
            catch (Exception ex)
            {
                Debug.Log("ply save Exception = " + ex.Message);
            }
#endif

            uint face = 0;
            try
            {
                int index;
                face = plyData.triangleNumber;
                for (int i = 0; i < face; i++)
                {
                    index = i * 3;
                    string indexNum = "3";
                    string idx0 = plyData.indecies[index + 0].ToString();
                    string idx1 = plyData.indecies[index + 1].ToString();
                    string idx2 = plyData.indecies[index + 2].ToString();

                    string st = "\n" + indexNum + " " + idx0 + " " + idx1 + " " + idx2;
                    sb.Append(st);
                }
            }
#if UNITY_STANDALONE_WIN
            catch{}
#else
            catch (Exception ex)
            {
                Debug.Log("ply save Exception = " + ex.Message);
            }
#endif

            StreamWriter sw = new(path, false); // overwrite
            sw.Write("ply\n");
            sw.Write("format ascii 1.0\n");
            sw.Write("comment Dolphin Point Cloud\n");
            sw.Write("element vertex " + vertex.ToString() + "\n");
            sw.Write("property float x\n");
            sw.Write("property float y\n");
            sw.Write("property float z\n");
            sw.Write("property uchar red\n");
            sw.Write("property uchar green\n");
            sw.Write("property uchar blue\n");
            sw.Write("element face " + face.ToString() + "\n");
            sw.Write("property list uchar uint vertex_indices\n");
            sw.Write("end_header");
            sw.Write(sb.ToString());
            sw.Flush();
            sw.Close();
        }
        else
        {
            fileName = fileNameCommon + PointCloudDataSaveReadonly.filenameSuffixPlyPoint + PointCloudDataSaveReadonly.extensionPly;
            path = dataPath + "/" + saveFolder + "/" + fileName;
            Debug.Log("path = " + path);


            StringBuilder sb = new();

            int vertex = 0;
            try
            {
                vertex = plyData.point.Count;
                for (int i = 0; i < vertex; i++)
                {
                    string x = plyData.point[i].x.ToString();
                    string y = plyData.point[i].y.ToString();
                    string z = plyData.point[i].z.ToString();

                    string r = plyData.color[i].r.ToString();
                    string g = plyData.color[i].g.ToString();
                    string b = plyData.color[i].b.ToString();

                    string st = "\n" + x + " " + y + " " + z + " " + r + " " + g + " " + b;
                    sb.Append(st);
                }
            }
#if UNITY_STANDALONE_WIN
            catch{}
#else
            catch (Exception ex)
            {
                Debug.Log("ply save Exception = " + ex.Message);
            }
#endif
            uint face = 0;

            StreamWriter sw = new(path, false); // overwrite
            sw.Write("ply\n");
            sw.Write("format ascii 1.0\n");
            sw.Write("comment Dolphin Point Cloud\n");
            sw.Write("element vertex " + vertex.ToString() + "\n");
            sw.Write("property float x\n");
            sw.Write("property float y\n");
            sw.Write("property float z\n");
            sw.Write("property uchar red\n");
            sw.Write("property uchar green\n");
            sw.Write("property uchar blue\n");
            sw.Write("element face " + face.ToString() + "\n");
            sw.Write("property list uchar uint vertex_indices\n");
            sw.Write("end_header");
            sw.Write(sb.ToString());
            sw.Flush();
            sw.Close();
        }
    }

    public void SaveStl(DateTime now, int cubeSizeRatioX, int cubeSizeRatioY)
    {
        StlData stlData;
        string fileNameCommon = PointCloudDataSaveStatic.GetCommonPrefixName(now);
        string fileName;
        string path;

        string saveFolder = PointCloudDataSaveStatic.GetSaveFolder(now);
        string dataPath = PointCloudDataSaveStatic.GetDataPath();

        Directory.CreateDirectory(Path.Combine(dataPath, saveFolder));

        byte[] header = new byte[80];

        int i;

        Vector3[] mask;
        mask = new Vector3[8];

        // front
        // 0, 1, 2, 3
        // back
        // 4, 5, 6, 7
        // top
        // 2, 3, 4, 5
        // bottom
        // 0, 1, 6, 7
        // right
        // 1, 2, 5, 6
        // left
        // 0, 3, 4, 7
        for (i = 0; i < 8; i++)
        {
            mask[i] = maskPos[i];
        }

        stlData = saveDataParser.CreateStlData(
            pointCloudData.OutputNumScanLine,
            pointCloudData.OutputNumPointsPerLine,
            pointCloudData.DataBuffer[pointCloudData.DrawFrameBufferNum],
            mask, cubeSizeRatioX, cubeSizeRatioY
            );

        for (i = 0; i < 80; i++)
        {
            header[i] = Convert.ToByte(i);
        }

        fileName = fileNameCommon + PointCloudDataSaveReadonly.extensionStl;

        path = dataPath + "/" + saveFolder + "/" + fileName;
        Debug.Log("ScreenShot" + "path = " + path);

        var writer = new BinaryWriter(new FileStream(path, FileMode.Create));
        try
        {
            writer.Write(header, 0, 80);

            writer.Write(stlData.triangleNumber);
            for (i = 0; i < stlData.triangleNumber; i++)
            {
                //x component of the normal vector
                writer.Write(stlData.normalVector[i].x);
                //y component of the normal vector
                writer.Write(stlData.normalVector[i].y);
                //z component of the normal vector
                writer.Write(stlData.normalVector[i].z);
                //1st vertex coordinate x
                writer.Write(stlData.vertex[i][0].x);
                //1st vertex coordinate y
                writer.Write(stlData.vertex[i][0].y);
                //1st vertex coordinate z
                writer.Write(stlData.vertex[i][0].z);
                //2nd vertex coordinate X
                writer.Write(stlData.vertex[i][1].x);
                //2nd vertex coordinate y
                writer.Write(stlData.vertex[i][1].y);
                //2nd vertex coordinate z
                writer.Write(stlData.vertex[i][1].z);
                //3rd vertex coordinate x
                writer.Write(stlData.vertex[i][2].x);
                //3rd vertex coordinate y
                writer.Write(stlData.vertex[i][2].y);
                //3rd vertex coordinate z
                writer.Write(stlData.vertex[i][2].z);
                //unused data
                writer.Write((UInt16)0);
            }
        }
        catch
        {

        }
        finally
        {
            writer.Close();
        }

    }

    public void SaveCsv(DateTime now, bool wtScreenShot)
    {
        string fileNameCommon = PointCloudDataSaveStatic.GetCommonPrefixName(now);
        string fileName;
        string path;

        string saveFolder = PointCloudDataSaveStatic.GetSaveFolder(now);
        string dataPath = PointCloudDataSaveStatic.GetDataPath();

        Directory.CreateDirectory(Path.Combine(dataPath, saveFolder));

        // save screenshot
        if (wtScreenShot)
        {
            fileName = fileNameCommon + PointCloudDataSaveReadonly.extensionPng;
            path = dataPath + "/" + saveFolder + "/" + fileName;
            Debug.Log("ScreenShot" + "path = " + path);

            try
            {
                Canvas canvas = GameObject.Find(PointCloudDataSaveReadonly.canvasName).GetComponent<Canvas>();
                canvas.enabled = false;

#if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
                ScreenCapture.CaptureScreenshot(Application.productName + "/" + saveFolder + "/" + fileName); // "jp.lidar.DolphinLab/files/Dolphin Lab/
#elif UNITY_STANDALONE_OSX
                ScreenCapture.CaptureScreenshot(path);
#else
                ScreenCapture.CaptureScreenshot(path);
#endif
                Task.Delay(100);

                canvas.enabled = true;
            }
#if UNITY_STANDALONE_WIN
            catch{}
#else
            catch (Exception ex)
            {
                Debug.Log("xxxxxx ScreenCapture.CaptureScreenshot xxxxxx");
                Debug.Log("message" + "," + ex.Message);
                Task.Delay(500);
            }
#endif
        }

        fileName = fileNameCommon + PointCloudDataSaveReadonly.extensionCsv;
        path = dataPath + "/" + saveFolder + "/" + fileName;

        Debug.Log("saveCsvPath" + "path = " + path);

        StreamWriter sw = new(path, false); // overwrite
        sw.WriteLine("Date and time of data" + "," + now.ToString(PointCloudDataSaveReadonly.dateFormatAsCsvDate));
        sw.WriteLine("");
        sw.WriteLine("" + "," + "No of line" + "," + "point" + "," + "x" + "," + "y" + "," + "z");
        
        string stCommonData = saveDataParser.CreateCsvData(
            pointCloudData.OutputNumScanLine,
            pointCloudData.OutputNumPointsPerLine,
            pointCloudData.DataBuffer[pointCloudData.DrawFrameBufferNum]
            );

        sw.Write(stCommonData);
        sw.Flush();
        sw.Close();
    }

    public void SaveXyz(DateTime now)
    {
        string fileNameCommon = PointCloudDataSaveStatic.GetCommonPrefixName(now);
        string fileName;
        string path;

        string saveFolder = PointCloudDataSaveStatic.GetSaveFolder(now);
        string dataPath = PointCloudDataSaveStatic.GetDataPath();
        string stCommonData;

        Directory.CreateDirectory(Path.Combine(dataPath, saveFolder));

        fileName = fileNameCommon + PointCloudDataSaveReadonly.extensionXyz;
        stCommonData = "";

        path = dataPath + "/" + saveFolder + "/" + fileName;

        Debug.Log("saveCsvPath" + "path = " + path);

        StreamWriter sw = new(path, false); // overwrite

        stCommonData += saveDataParser.CreateXyzData(
            pointCloudData.OutputNumScanLine,
            pointCloudData.OutputNumPointsPerLine,
            pointCloudData.DataBuffer[pointCloudData.DrawFrameBufferNum]
            );

        sw.Write(stCommonData);
        sw.Flush();
        sw.Close();
    }

    public void SavePts(DateTime now)
    {
        string fileNameCommon = PointCloudDataSaveStatic.GetCommonPrefixName(now);
        string fileName;
        string path;

        string saveFolder = PointCloudDataSaveStatic.GetSaveFolder(now);
        string dataPath = PointCloudDataSaveStatic.GetDataPath();

        string stCommonData;

        Directory.CreateDirectory(Path.Combine(dataPath, saveFolder));

        fileName = fileNameCommon + PointCloudDataSaveReadonly.extensionPts;
        stCommonData = (pointCloudData.OutputNumScanLine * pointCloudData.OutputNumPointsPerLine).ToString() + newLineCode;

        path = dataPath + "/" + saveFolder + "/" + fileName;

        Debug.Log("saveCsvPath" + "path = " + path);

        StreamWriter sw = new(path, false); // overwrite

        stCommonData += saveDataParser.CreateXyzData(
            pointCloudData.OutputNumScanLine,
            pointCloudData.OutputNumPointsPerLine,
            pointCloudData.DataBuffer[pointCloudData.DrawFrameBufferNum]
            );

        sw.Write(stCommonData);
        sw.Flush();
        sw.Close();
    }
}

