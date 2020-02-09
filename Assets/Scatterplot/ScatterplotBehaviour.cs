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

    private MaterialPropertyBlock block;
    private const string MATRIX_PROPERTY_NAME = "_TransformMatrix";

    void Start()
    {
        UpdateTransformationMatrix();

        argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);

        PlotFakeData();
    }

    private void PlotFakeData()
    {
        int gridResolution = 50;

        float size = 0.5f;
        int pointsCount = gridResolution * gridResolution * gridResolution;

        Vector4[] positions = new Vector4[pointsCount];
        Vector4[] colors = new Vector4[pointsCount];

        for (int i = 0, z = 0; i < gridResolution; i++)
        {
            for (int j = 0; j < gridResolution; j++)
            {
                for (int k = 0; k < gridResolution; k++, z++)
                {
                    positions[z] = new Vector4(
                        i - (gridResolution - 1) * 0.5f,
                        j - (gridResolution - 1) * 0.5f,
                        k - (gridResolution - 1) * 0.5f,
                        size
                    );

                    colors[z] = new Vector4(
                        (float)i / gridResolution * 0.5f,
                        (float)j / gridResolution * 0.5f,
                        (float)k / gridResolution * 0.5f
                    );
                }
            }
        }

        // int pointsCount = 100000;
        // for (int i = 0; i < pointsCount; i++)
        // {
        //     float size = Random.Range(0.05f, 0.25f);
        //     var position = Random.insideUnitSphere * Random.Range(5.0f, 10.0f);
        //     positions[i] = new Vector4(position.x, position.y, position.z, size);
        //     colors[i] = new Vector4(Random.value, Random.value, Random.value, 1f);
        // }

        PlotData(positions, colors);
    }

    public void UpdateTransformationMatrix()
    {
        if (block == null)
        {
            block = new MaterialPropertyBlock();
        }
        block.SetMatrix(MATRIX_PROPERTY_NAME, transform.localToWorldMatrix);
    }

    void Update()
    {
        UpdateTransformationMatrix();

        if (DataPointMesh != null && DataPointMaterial != null)
        {
            Graphics.DrawMeshInstancedIndirect(
                DataPointMesh,
                0,
                DataPointMaterial,
                new Bounds(Vector3.zero, new Vector3(100.0f, 100.0f, 100.0f)),
                argsBuffer,
                0,
                block
            );
        }
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

    void PlotData(Vector4[] positions, Vector4[] colors)
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
