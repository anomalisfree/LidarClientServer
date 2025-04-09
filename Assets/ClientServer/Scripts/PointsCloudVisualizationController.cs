using System.Collections.Generic;
using UnityEngine;

public class PointsCloudVisualizationController : MonoBehaviour
{
    [SerializeField] private CloudPointMeshView cloudPointMeshPrefab;
    [SerializeField] private Material cloudPointMaterial;
    [SerializeField] private Color pointColor = Color.red;
    [SerializeField] private float pointSize = 4f;

    private CloudPointMeshView _cloudPointMeshView;
    private Vector3[] _vertices;
    private int[] _indices;
    private Color[] _colors;

    public void VisualizePoints(List<Vector3> points)
    {
        if (_cloudPointMeshView == null)
        {
            _cloudPointMeshView = Instantiate(cloudPointMeshPrefab);
        }

        VisualizeCloudPoints(points, _cloudPointMeshView);
    }

    public void VisualizeCloudPoints(List<Vector3> points, CloudPointMeshView cloudPointMeshView)
    {
        cloudPointMaterial.SetFloat("_PointSize", pointSize);

        if (points.Count <= 0)
        {
            cloudPointMeshView.SetActive(false);
            return;
        }

        var mesh = cloudPointMeshView.GetMesh();
        int pointCount = points.Count;

        if (_vertices == null || _vertices.Length < pointCount)
        {
            _vertices = new Vector3[pointCount];
            _indices = new int[pointCount];
            _colors = new Color[pointCount];
        }

        mesh.Clear();

        for (var i = 0; i < pointCount; i++)
        {
            _vertices[i] = points[i];
            _indices[i] = i;
            _colors[i] = pointColor;
        }

        mesh.vertices = _vertices;
        mesh.colors = _colors;
        mesh.SetIndices(_indices, MeshTopology.Points, 0);

        cloudPointMeshView.SetActive(true);
    }
}
