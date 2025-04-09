/*!
# SPDX-FileCopyrightText: © 2023 Dolphin Co.,Ltd. <http://lidar.jp>
#
# SPDX-License-Identifier: MIT
*/

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering;

using DllBinaryParser;
using DllOscParserAccelerated;
using DllPointCloudGeneratorAccelerated;
using DllPointCloudReadonly;

public class PointCloudData : MonoBehaviour
{
    private readonly DebugOut debugOut = new(false, typeof(PointCloudData).Name);

    public ConcurrentQueue<DolphinLabCoreStatic.PointCloudOutputData> Queue { get; set; }
    private int maxQueueCount = 16;
    public bool DataOutputEnable { get; set; } = false;

    public Vector3[][][] DataBuffer { get; private set; }
    public bool[][][] DataEnableBuffer { get; private set; }
    public Color[][][] ColorBuffer { get; private set; }
    public float[][][] ColorAlphaBuffer { get; private set; }

    public Vector3[] Vertices { get { return vertices; } private set { vertices = value; } }
    private Vector3[] vertices;

    public float ColorAlpha { get; set; } = 1.0f;
    public float PointCloudDrawRatioValue { get; set; } = 1.0f;

    private PointCloudGenerator pointCloudGenerator;
    private PointCloudColor pointCloudColor;
    private PointCloudGeneratorAccelerated pointCloudGeneratorAccelerated;

    private OscParser oscParser;
    private OscParserAccelerated oscParserAccelerated;

    private Mesh pointCloudMesh;
    private MeshFilter pointCloudMeshFilter;
    private MeshCollider pointCloudMeshCollider;

    public float DrawRatio { get { return drawRatio; } set { drawRatio = value; } }
    public Int32 CubeSizeRatioX { get { return (Int32)qubeSizeRatioX; } set { qubeSizeRatioX = (UInt32)value; } }
    public Int32 CubeSizeRatioY { get { return (Int32)qubeSizeRatioY; } set { qubeSizeRatioY = (UInt32)value; } }

    public bool DrawPointCloud { get; set; } = true;
    public Int32 DrawFrameBufferNum { get; private set; }
    private UInt32 setDrawFrameBufferNum;
    private Int32 oldSeqCnt = 0;

    public Vector3[][] OutputDataBuffer { get; private set; }
    public Int32 OutputDataFrame { get; private set; }
    public Int32 OutputScanFrame { get; private set; }
    public Int32 OutputNumScanLine { get; private set; }
    public Int32 OutputNumPointsPerLine { get; private set; }

    private Int32 bkMaxLine = PointCloudReadonly.numScanLineMax;
    private Int32 bkNumPoints = PointCloudReadonly.numPointsPerLineMax;

    public float MaxZ{
        get { return maxZbuffer; }
    }
    public float PointRatio
    {
        get
        {
            if(0 != totalPoints)
            {
                return ((float)existingPoints / (float)totalPoints * 100);
            }
            else
            {
                return 0.0f;
            }
        }
    }
    private Int32 existingPointsBuffer = 0;
    private float maxZbuffer = 0.0f;

    private bool drawFrameFlag = false;

    private System.Diagnostics.Stopwatch sw;
    private TimeSpan ts;
    public float Fps {
        get {
            if(0.0f != (float)ts.TotalMilliseconds)
            {
                return (1000.0f / (float)ts.TotalMilliseconds);
            }
            else
            {
                return 0.0f;
            }
        }
    }

    public List<Action> OnUICallBacks { get; private set; }

    private Int32 NumFrameBufferMax { get { return PointCloudReadonly.numFrameBufferMax; } }
    private Int32 NumScanLineMax { get { return PointCloudReadonly.numScanLineMax; } }
    private Int32 NumPointsPerLineMax { get { return PointCloudReadonly.numPointsPerLineMax; } }

    private bool bkColliderEnable;
    public bool ColliderEnable { get { return colliderEnable; } set { colliderEnable = value; } }


    /*
     * view in Unity Editor
     */
    public float maxDistance = 0.0f;
    public UInt32 totalPoints = 0;
    public UInt32 existingPoints = 0;
    public float existPointRatio = 0.0f;
    public float fpsData = 0.0f;
    private bool colliderEnable = true;
    private float drawRatio = 1.5f;
    private UInt32 qubeSizeRatioX = 240;
    private UInt32 qubeSizeRatioY = 120;

    private TimeMeasurement timeMeasurement;
    public List<float> drawlapTimesMs;

    private void Awake()
    {
        pointCloudColor = new PointCloudColor();
        oscParser = new OscParser();
        oscParserAccelerated = new OscParserAccelerated();

        sw = new System.Diagnostics.Stopwatch();
        sw.Reset();

        InitBuffer();
        ClearUIActions();

        timeMeasurement = new TimeMeasurement();
        //InitMesh();

        Queue = new ConcurrentQueue<DolphinLabCoreStatic.PointCloudOutputData>();
    }

    private void InitMesh()
    {
        pointCloudMeshFilter = PointCloudStatic.gameObject.AddComponent<MeshFilter>();
        pointCloudMeshCollider = PointCloudStatic.gameObject.AddComponent<MeshCollider>();
        MeshRenderer meshRenderer = PointCloudStatic.gameObject.AddComponent<MeshRenderer>();
        meshRenderer.material = Resources.Load<Material>("PointCloud/PointCube");
        // Lighting
        meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
        meshRenderer.receiveShadows = false;
        // Probes
        meshRenderer.lightProbeUsage = LightProbeUsage.Off;
        meshRenderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
        // Additional Settings
        meshRenderer.motionVectorGenerationMode = MotionVectorGenerationMode.Camera;

        pointCloudMesh = new();
    }

    void Start()
    {
        debugOut.Print("[" + MethodBase.GetCurrentMethod().Name + "]");

        pointCloudGenerator = PointCloudStatic.gameObject.GetComponent<PointCloudGenerator>();
        pointCloudGeneratorAccelerated = PointCloudStatic.gameObject.GetComponent<PointCloudGeneratorAccelerated>();

        DrawFrameBufferNum = 0;
        setDrawFrameBufferNum = 0;
        oldSeqCnt = -1;

        //bkColliderEnable = ColliderEnable;
        //if(ColliderEnable)
        //{
        //    pointCloudMeshCollider.enabled = true;
        //}
        //else
        //{
        //    pointCloudMeshCollider.enabled = false;
        //}

    }

    public void AddUIActions(List<Action> actions)
    {
        foreach (var val in actions)
        {
            OnUICallBacks.Add(val);
        }
    }

    public void ClearUIActions()
    {
        OnUICallBacks = new List<Action>();
    }

    private void UpdatePublic()
    {
        existPointRatio = PointRatio;
        fpsData = Fps;
        maxDistance = MaxZ / 100;
    }

    void Update()
    {
        if (ReadyFrameData())
        {
            timeMeasurement.StartStopWatch();

            //if (bkColliderEnable != ColliderEnable)
            //{
            //    if (ColliderEnable)
            //    {
            //        pointCloudMeshCollider.enabled = true;
            //    }
            //    else
            //    {
            //        pointCloudMeshCollider.enabled = false;
            //    }

            //    bkColliderEnable = ColliderEnable;
            //}

            //Destroy(pointCloudMesh);
            //pointCloudMesh = null;

            //if(DrawNewFrame())
            //{
            //    /*
            //     * Can you do it faster than D? ;-)
            //     */
            //    if (DebugStatic.SpeedingUp)
            //    {
            //        var tupleData = pointCloudGeneratorAccelerated.CreatePointCloudAsMeshAcceleratedCode(
            //            OutputNumScanLine,
            //            OutputNumPointsPerLine,
            //            DataBuffer[DrawFrameBufferNum],
            //            DataEnableBuffer[DrawFrameBufferNum],
            //            ColorBuffer[DrawFrameBufferNum],
            //            ColorAlphaBuffer[DrawFrameBufferNum],
            //            DrawRatio, CubeSizeRatioX, CubeSizeRatioY
            //            );

            //        pointCloudMesh = tupleData.Item1;
            //        Vertices = tupleData.Item2;
            //    }
            //    else
            //    {
            //        var tupleData = pointCloudGenerator.CreatePointCloudAsMesh(
            //            OutputNumScanLine,
            //            OutputNumPointsPerLine,
            //            DataBuffer[DrawFrameBufferNum],
            //            ColorBuffer[DrawFrameBufferNum],
            //            DrawRatio, CubeSizeRatioX, CubeSizeRatioY
            //            );
            //        pointCloudMesh = tupleData.Item1;
            //        Vertices = tupleData.Item2;
            //    }
            //    pointCloudMeshFilter.sharedMesh = pointCloudMesh;
            //    if (ColliderEnable)
            //    {
            //        pointCloudMeshCollider.sharedMesh = pointCloudMesh;
            //    }
            //}
            
            drawlapTimesMs = timeMeasurement.StopStopWatch();

            ClearDrawFrame();

            foreach (var val in OnUICallBacks)
            {
                val();
            }

            UpdatePublic();

        }
        else if(!DrawPointCloud)
        {
            if(null != pointCloudMesh)
            {
                Destroy(pointCloudMesh);
                pointCloudMesh = null;
            }
        }
    }

    public void OscRecevieData(byte[] oscData)
    {
        Int32 sequenceCouter;
        Int32 maxLineNumber;
        Int32 lineNumber;
        Int32 dataCount;

        /*
         * Can you do it faster than D? ;-)
         */
        if (DebugStatic.SpeedingUp)
        {
            if (true == oscParserAccelerated.GetLineDataAcceleratedCode(true, oscData))
            {
                sequenceCouter = (Int32)oscParserAccelerated.dataInfo.frameNumber;
                maxLineNumber = (Int32)oscParserAccelerated.dataInfo.maxLineNumber;
                lineNumber = (Int32)oscParserAccelerated.dataInfo.currentLineNumber;
                dataCount = (Int32)oscParserAccelerated.dataInfo.pointsPerLine;
                Vector3[] originDataV = oscParserAccelerated.xyzData;

                CreatePointCloudData(
                    sequenceCouter,
                    maxLineNumber,
                    lineNumber,
                    dataCount,
                    originDataV
                    );
            }
        }
        else
        {
            if (true == oscParser.GetLineData(true, oscData))
            {
                sequenceCouter = (Int32)oscParser.dataInfo.frameNumber;
                maxLineNumber = (Int32)oscParser.dataInfo.maxLineNumber;
                lineNumber = (Int32)oscParser.dataInfo.currentLineNumber;
                dataCount = (Int32)oscParser.dataInfo.pointsPerLine;
                List<Vector3> originDataVL = oscParser.xyzDataOld;

                CreatePointCloudDataGeneralCode(
                    sequenceCouter,
                    maxLineNumber,
                    lineNumber,
                    dataCount,
                    originDataVL
                    );
            }
        }
    }

    public void XyzRecevieData(BinaryDataInformation info, Vector3[] originDataV)
    {
        Int32 sequenceCouter = (Int32)info.frameNumber;
        Int32 maxLineNumber = (Int32)info.maxLineNumber;
        Int32 lineNumber = (Int32)info.lineNumber;
        Int32 dataCount = (Int32)info.dataCount;

        CreatePointCloudData(
            sequenceCouter,
            maxLineNumber,
            lineNumber,
            dataCount,
            originDataV
            );
    }

    private void InitBuffer()
    {
        DataBuffer = new Vector3[NumFrameBufferMax][][];
        DataEnableBuffer = new bool[NumFrameBufferMax][][];
        ColorBuffer = new Color[NumFrameBufferMax][][];
        ColorAlphaBuffer = new float[NumFrameBufferMax][][];
        for (int i = 0; i < NumFrameBufferMax; i++)
        {
            DataBuffer[i] = new Vector3[NumScanLineMax][];
            DataEnableBuffer[i] = new bool[NumScanLineMax][];
            ColorBuffer[i] = new Color[NumScanLineMax][];
            ColorAlphaBuffer[i] = new float[NumScanLineMax][];

            for (int j = 0; j < NumScanLineMax; j++)
            {
                DataBuffer[i][j] = new Vector3[NumPointsPerLineMax];
                DataEnableBuffer[i][j] = new bool[NumPointsPerLineMax];
                ColorBuffer[i][j] = new Color[NumPointsPerLineMax];
                ColorAlphaBuffer[i][j] = new float[NumPointsPerLineMax];
            }
        }
    }

    private void InitBuffer(Int32 index, Int32 maxLineNumber, Int32 dataCount)
    {
        for (int i = 0; i < maxLineNumber; i++)
        {
            for (int j = 0; j < dataCount; j++)
            {
                DataBuffer[index][i][j] = new Vector3(0.0f, 0.0f, 0.0f);
                DataEnableBuffer[index][i][j] = true;
                ColorBuffer[index][i][j] = new Color(0.0f, 0.0f, 0.0f);
                ColorAlphaBuffer[index][i][j] = 1.0f;
            }
        }
    }

    private void DrawBufferUpdate()
    {
        setDrawFrameBufferNum++;
        if (NumFrameBufferMax <= setDrawFrameBufferNum)
        {
            setDrawFrameBufferNum = 0;
        }
    }

    private bool ReadyFrameData()
    {
        return drawFrameFlag;
    }

    private bool DrawNewFrame()
    {
        if(DrawPointCloud)
        {
            return drawFrameFlag;
        }
        else
        {
            return false;
        }
    }

    private void ClearDrawFrame()
    {
        drawFrameFlag = false;
    }

    private void CreatePointCloudDataGeneralCode(Int32 sequenceCouter, Int32 maxLineNumber, Int32 lineNumber, Int32 dataCount, List<Vector3> originDataV)
    {
        Color setColor;

        if (oldSeqCnt != sequenceCouter)
        {
            sw.Stop();
            ts = sw.Elapsed;
            sw.Restart();

            DrawFrameBufferNum = (Int32)setDrawFrameBufferNum;
            drawFrameFlag = true;

            OutputDataFrame = DrawFrameBufferNum;
            OutputScanFrame = oldSeqCnt;
            OutputNumScanLine = bkMaxLine;
            OutputNumPointsPerLine = bkNumPoints;

            OutputDataBuffer = new Vector3[OutputNumScanLine][];

            for (int i = 0; i < OutputNumScanLine; i++)
            {
                OutputDataBuffer[i] = new Vector3[OutputNumPointsPerLine];
                OutputDataBuffer[i] = DataBuffer[OutputDataFrame][i][0..OutputNumPointsPerLine];
            }

            if(DataOutputEnable)
            {
                if(maxQueueCount <= Queue.Count)
                {
                    Queue.Clear();
                }
                Queue.Enqueue(new DolphinLabCoreStatic.PointCloudOutputData(
                    OutputScanFrame,
                    OutputNumScanLine,
                    OutputNumPointsPerLine,
                    OutputDataBuffer));
            }

            maxZbuffer = 0.0f;

            totalPoints = (uint)(maxLineNumber * dataCount);
            existingPoints = (uint)existingPointsBuffer;
            existingPointsBuffer = 0;

            DrawBufferUpdate();

            oldSeqCnt = sequenceCouter;

            InitBuffer((int)setDrawFrameBufferNum, maxLineNumber, dataCount);
        }

        bkMaxLine = maxLineNumber;
        bkNumPoints = dataCount;

        for (int i = 0; i < dataCount; i++)
        {
            DataBuffer[setDrawFrameBufferNum][lineNumber][i] = originDataV[i];

            if(maxZbuffer < originDataV[i].z)
            {
                maxZbuffer = originDataV[i].z;
            }

            if(0 != originDataV[i].z)
            {
                existingPointsBuffer++;
            }

            setColor = pointCloudColor.GetColor(originDataV[i].z);

            ColorBuffer[setDrawFrameBufferNum][lineNumber][i] = setColor;
        }
    }

    private void CreatePointCloudData(Int32 sequenceCouter, Int32 maxLineNumber, Int32 lineNumber, Int32 dataCount, Vector3[] originDataV)
    {
        Color setColor;

        if (oldSeqCnt != sequenceCouter)
        {
            sw.Stop();
            ts = sw.Elapsed;
            sw.Restart();

            DrawFrameBufferNum = (Int32)setDrawFrameBufferNum;
            drawFrameFlag = true;

            OutputDataFrame = DrawFrameBufferNum;
            OutputScanFrame = oldSeqCnt;
            OutputNumScanLine = bkMaxLine;
            OutputNumPointsPerLine = bkNumPoints;
            OutputDataBuffer = new Vector3[OutputNumScanLine][];

            for(int i = 0; i < OutputNumScanLine; i++)
            {
                OutputDataBuffer[i] = new Vector3[OutputNumPointsPerLine];
                OutputDataBuffer[i] = DataBuffer[OutputDataFrame][i][0..OutputNumPointsPerLine];
            }

            if (DataOutputEnable)
            {
                if (maxQueueCount <= Queue.Count)
                {
                    Queue.Clear();
                }
                Queue.Enqueue(new DolphinLabCoreStatic.PointCloudOutputData(
                    OutputScanFrame,
                    OutputNumScanLine,
                    OutputNumPointsPerLine,
                    OutputDataBuffer));
            }

            maxZbuffer = 0.0f;

            totalPoints = (uint)(maxLineNumber * dataCount);
            existingPoints = (uint)existingPointsBuffer;
            existingPointsBuffer = 0;

            DrawBufferUpdate();

            oldSeqCnt = sequenceCouter;

            InitBuffer((int)setDrawFrameBufferNum, maxLineNumber, dataCount);

        }

        bkMaxLine = maxLineNumber;
        bkNumPoints = dataCount;

        for (int i = 0; i < dataCount; i++)
        {
            DataBuffer[setDrawFrameBufferNum][lineNumber][i] = originDataV[i];

            if (maxZbuffer < originDataV[i].z)
            {
                maxZbuffer = originDataV[i].z;
            }

            if (0 != originDataV[i].z)
            {
                existingPointsBuffer++;
            }

            setColor = pointCloudColor.GetColor(originDataV[i].z);
            ColorAlphaBuffer[setDrawFrameBufferNum][lineNumber][i] = ColorAlpha;
            ColorBuffer[setDrawFrameBufferNum][lineNumber][i] = setColor;
        }
    }
}
