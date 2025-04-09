/*!
# SPDX-FileCopyrightText: © 2023 Dolphin Co.,Ltd. <http://lidar.jp>
#
# SPDX-License-Identifier: MIT
*/

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

using DllPointCloudReadonly;

public class PointCloudGenerator : MonoBehaviour
{
    private readonly DebugOut debugOut = new(false, typeof(PointCloudGenerator).Name);

    void Start()
    {
    }

    public (Mesh, Vector3[]) CreatePointCloudAsMesh(
        Int32 numLine, Int32 numPoints,
        Vector3[][] pos, Color[][] color,
        float drawRatio, Int32 qubeSizeRatioX, Int32 qubeSizeRatioY)
    {
        List<Vector3> vertices = new();
        List<int> indecies = new();
        List<Color> colors = new();

        int totalCnt,vaildPointCnt;

        try
        {
            totalCnt = 0;
            for (int i = 0; i < numLine; i++)
            {
                vaildPointCnt = 0;
                for (int j = 0; j < numPoints; j++)
                {
                    Vector3 sttPos = pos[i][j];

                    if (0.0f == sttPos.z)
                    {
                        continue;
                    }

                    float sizeRatioX = sttPos.z / (1 + (uint)qubeSizeRatioX);
                    float xRatio = 0.5f * sizeRatioX;

                    sttPos.x = pos[i][j].x - (xRatio / 2);

                    float sizeRatioY = sttPos.z / (1 + (uint)qubeSizeRatioY);
                    float yRatio = 0.5f * sizeRatioY;
                    float zRatio = 0.5f;

                    float rX = drawRatio * xRatio;
                    float rY = drawRatio * yRatio;
                    float rZ = drawRatio * zRatio;

                    Vector3[] posAry = {
                                new Vector3(sttPos.x, sttPos.y, sttPos.z),
                                new Vector3(sttPos.x + rX, sttPos.y, sttPos.z),
                                new Vector3(sttPos.x + rX, sttPos.y + rY, sttPos.z),
                                new Vector3(sttPos.x, sttPos.y + rY, sttPos.z),
                                new Vector3(sttPos.x, sttPos.y + rY, sttPos.z + rZ),
                                new Vector3(sttPos.x + rX, sttPos.y + rY, sttPos.z + rZ),
                                new Vector3(sttPos.x + rX, sttPos.y, sttPos.z + rZ),
                                new Vector3(sttPos.x, sttPos.y, sttPos.z + rZ)
                            };
                    // vertex settings
                    vertices.AddRange(posAry);

                    Color[] colorAry = {
                                color[i][j],
                                color[i][j],
                                color[i][j],
                                color[i][j],
                                color[i][j],
                                color[i][j],
                                color[i][j],
                                color[i][j],
                            };
                    colors.AddRange(colorAry);

                    int idFace, idTri;
                    int index = (totalCnt * 8) + (vaildPointCnt * 8);
                    int[] indexAry;

                    ////////////////////////////////////////////////////
                    //face front
                    ////////////////////////////////////////////////////
                    idFace = PointCloudReadonly.idFaceFront;
                    //------------------------------------------
                    idTri = PointCloudReadonly.idTri0;
                    //------------------------------------------
                    indexAry = new int[] {
                                index + PointCloudReadonly.cubeTriangles[idFace][idTri][0],
                                index + PointCloudReadonly.cubeTriangles[idFace][idTri][1],
                                index + PointCloudReadonly.cubeTriangles[idFace][idTri][2]
                            };
                    indecies.AddRange(indexAry);
                    //------------------------------------------
                    idTri = PointCloudReadonly.idTri1;
                    //------------------------------------------
                    indexAry = new int[] {
                                index + PointCloudReadonly.cubeTriangles[idFace][idTri][0],
                                index + PointCloudReadonly.cubeTriangles[idFace][idTri][1],
                                index + PointCloudReadonly.cubeTriangles[idFace][idTri][2]
                            };
                    //save triangle vertex index
                    indecies.AddRange(indexAry);

                    ////////////////////////////////////////////////////
                    //face top
                    ////////////////////////////////////////////////////
                    idFace = PointCloudReadonly.idFaceTop;
                    //------------------------------------------
                    idTri = PointCloudReadonly.idTri0;
                    //------------------------------------------
                    indexAry = new int[] {
                                index + PointCloudReadonly.cubeTriangles[idFace][idTri][0],
                                index + PointCloudReadonly.cubeTriangles[idFace][idTri][1],
                                index + PointCloudReadonly.cubeTriangles[idFace][idTri][2]
                            };
                    //save triangle vertex index
                    indecies.AddRange(indexAry);

                    //------------------------------------------
                    idTri = PointCloudReadonly.idTri1;
                    //------------------------------------------
                    indexAry = new int[] {
                                index + PointCloudReadonly.cubeTriangles[idFace][idTri][0],
                                index + PointCloudReadonly.cubeTriangles[idFace][idTri][1],
                                index + PointCloudReadonly.cubeTriangles[idFace][idTri][2]
                            };
                    //save triangle vertex index
                    indecies.AddRange(indexAry);

                    ////////////////////////////////////////////////////
                    //face right
                    ////////////////////////////////////////////////////
                    idFace = PointCloudReadonly.idFaceRight;
                    //------------------------------------------
                    idTri = PointCloudReadonly.idTri0;
                    //------------------------------------------
                    indexAry = new int[] {
                                index + PointCloudReadonly.cubeTriangles[idFace][idTri][0],
                                index + PointCloudReadonly.cubeTriangles[idFace][idTri][1],
                                index + PointCloudReadonly.cubeTriangles[idFace][idTri][2]
                            };
                    //save triangle vertex index
                    indecies.AddRange(indexAry);

                    //------------------------------------------
                    idTri = PointCloudReadonly.idTri1;
                    //------------------------------------------
                    indexAry = new int[] {
                                index + PointCloudReadonly.cubeTriangles[idFace][idTri][0],
                                index + PointCloudReadonly.cubeTriangles[idFace][idTri][1],
                                index + PointCloudReadonly.cubeTriangles[idFace][idTri][2]
                            };
                    //save triangle vertex index
                    indecies.AddRange(indexAry);

                    ////////////////////////////////////////////////////
                    //face left
                    ////////////////////////////////////////////////////
                    idFace = PointCloudReadonly.idFaceLeft;
                    //------------------------------------------
                    idTri = PointCloudReadonly.idTri0;
                    //------------------------------------------
                    indexAry = new int[] {
                                index + PointCloudReadonly.cubeTriangles[idFace][idTri][0],
                                index + PointCloudReadonly.cubeTriangles[idFace][idTri][1],
                                index + PointCloudReadonly.cubeTriangles[idFace][idTri][2]
                            };
                    //save triangle vertex index
                    indecies.AddRange(indexAry);

                    //------------------------------------------
                    idTri = PointCloudReadonly.idTri1;
                    //------------------------------------------
                    indexAry = new int[] {
                                index + PointCloudReadonly.cubeTriangles[idFace][idTri][0],
                                index + PointCloudReadonly.cubeTriangles[idFace][idTri][1],
                                index + PointCloudReadonly.cubeTriangles[idFace][idTri][2]
                            };
                    //save triangle vertex index
                    indecies.AddRange(indexAry);

                    ////////////////////////////////////////////////////
                    //face back
                    ////////////////////////////////////////////////////
                    idFace = PointCloudReadonly.idFaceBack;
                    //------------------------------------------
                    idTri = PointCloudReadonly.idTri0;
                    //------------------------------------------
                    indexAry = new int[] {
                                index + PointCloudReadonly.cubeTriangles[idFace][idTri][0],
                                index + PointCloudReadonly.cubeTriangles[idFace][idTri][1],
                                index + PointCloudReadonly.cubeTriangles[idFace][idTri][2]
                            };
                    //save triangle vertex index
                    indecies.AddRange(indexAry);

                    //------------------------------------------
                    idTri = PointCloudReadonly.idTri1;
                    //------------------------------------------
                    indexAry = new int[] {
                                index + PointCloudReadonly.cubeTriangles[idFace][idTri][0],
                                index + PointCloudReadonly.cubeTriangles[idFace][idTri][1],
                                index + PointCloudReadonly.cubeTriangles[idFace][idTri][2]
                            };
                    //save triangle vertex index
                    indecies.AddRange(indexAry);

                    ////////////////////////////////////////////////////
                    //face bottom
                    ////////////////////////////////////////////////////
                    idFace = PointCloudReadonly.idFaceBottom;
                    //------------------------------------------
                    idTri = PointCloudReadonly.idTri0;
                    //------------------------------------------
                    indexAry = new int[] {
                                index + PointCloudReadonly.cubeTriangles[idFace][idTri][0],
                                index + PointCloudReadonly.cubeTriangles[idFace][idTri][1],
                                index + PointCloudReadonly.cubeTriangles[idFace][idTri][2]
                            };
                    //save triangle vertex index
                    indecies.AddRange(indexAry);

                    //------------------------------------------
                    idTri = PointCloudReadonly.idTri1;
                    //------------------------------------------
                    indexAry = new int[] {
                                index + PointCloudReadonly.cubeTriangles[idFace][idTri][0],
                                index + PointCloudReadonly.cubeTriangles[idFace][idTri][1],
                                index + PointCloudReadonly.cubeTriangles[idFace][idTri][2]
                            };

                    //save triangle vertex index
                    indecies.AddRange(indexAry);

                    vaildPointCnt++;
                }

                totalCnt += vaildPointCnt;
            }
        }
#if UNITY_STANDALONE_WIN
        catch { }
#else
        catch (Exception ex)
        {
            debugOut.Print("[" + MethodBase.GetCurrentMethod().Name + "]" + "[ex.Message]" + ex.Message);
        }
#endif

        Mesh mesh = new();
        try
        {
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            mesh.Clear();
            mesh.vertices = vertices.ToArray();
            mesh.SetIndices(indecies, MeshTopology.Triangles, 0);
            mesh.colors = colors.ToArray();
            mesh.name = "DolphinPointCloudMesh";
        }
#if UNITY_STANDALONE_WIN
        catch { }
#else
        catch (System.Exception ex)
        {
            Debug.Log("[" + MethodBase.GetCurrentMethod().Name + "]" + "[mesh]" + "[ex.Message]" + ex.Message);
        }
#endif
        return (mesh, vertices.ToArray());
    }
}
