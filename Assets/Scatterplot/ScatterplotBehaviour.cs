using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScatterplotBehaviour : MonoBehaviour
{
    public Mesh DataPointMesh;

    public Material DataPointMaterial;

    private uint[] args = new uint[5] { 0, 0, 0, 0, 0 };

    private ComputeBuffer positionBuffer;
    private ComputeBuffer argsBuffer;
    private ComputeBuffer colorBuffer;

    private const int BUFFER_STRIDE = 16;

    private const string POSITIONS_BUFFER_NAME = "positionBuffer";

    private const string COLORS_BUFFER_NAME = "colorBuffer";

    void Start()
    {
        argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);

        PlotFakeData();
    }

    private void PlotFakeData()
    {
        int pointsCount = 20000;

        Vector4[] positions = new Vector4[pointsCount];
        Vector4[] colors = new Vector4[pointsCount];

        for (int i = 0; i < pointsCount; i++)
        {
            float size = Random.Range(0.05f, 0.25f);
            var position = Random.insideUnitSphere * Random.Range(5.0f, 10.0f);
            positions[i] = new Vector4(position.x, position.y, position.z, size);
            colors[i] = new Vector4(Random.value, Random.value, Random.value, 1f);
        }

        Plot(positions, colors);
    }

    void Update()
    {
        Graphics.DrawMeshInstancedIndirect(DataPointMesh, 0, DataPointMaterial, new Bounds(Vector3.zero, new Vector3(100.0f, 100.0f, 100.0f)), argsBuffer);
    }

    void OnDisable()
    {
        if (positionBuffer != null) positionBuffer.Release();
        positionBuffer = null;

        if (colorBuffer != null) colorBuffer.Release();
        colorBuffer = null;

        if (argsBuffer != null) argsBuffer.Release();
        argsBuffer = null;
    }

    void Plot(Vector4[] positions, Vector4[] colors)
    {
        if (positions.Length != colors.Length)
        {
            Debug.Log("Make sure that positions and colors have the same length before plotting.");
            return;
        }

        var pointsCount = positions.Length;

        if (positionBuffer != null) positionBuffer.Release();
        if (colorBuffer != null) colorBuffer.Release();

        positionBuffer = new ComputeBuffer(pointsCount, BUFFER_STRIDE);
        colorBuffer = new ComputeBuffer(pointsCount, BUFFER_STRIDE);

        positionBuffer.SetData(positions);
        colorBuffer.SetData(colors);

        DataPointMaterial.SetBuffer(POSITIONS_BUFFER_NAME, positionBuffer);
        DataPointMaterial.SetBuffer(COLORS_BUFFER_NAME, colorBuffer);

        uint numIndices = (DataPointMesh != null) ? (uint)DataPointMesh.GetIndexCount(0) : 0;
        args[0] = numIndices;
        args[1] = (uint)pointsCount;
        argsBuffer.SetData(args);
    }
}
