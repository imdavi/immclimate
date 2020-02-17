using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScatterplotBehaviour : MonoBehaviour
{
    public Mesh DataPointMesh;

    public Material DataPointMaterial;

    public float MinimumPointSize = 0.5f;

    private uint[] args = new uint[5] { 0, 0, 0, 0, 0 };

    private ComputeBuffer positionBuffer;
    private ComputeBuffer argsBuffer;
    private ComputeBuffer colorBuffer;

    private const int BUFFER_STRIDE = 16;

    private const string POSITIONS_BUFFER_NAME = "positionBuffer";

    private const string COLORS_BUFFER_NAME = "colorBuffer";

    private MaterialPropertyBlock block;
    private const string MATRIX_PROPERTY_NAME = "_TransformMatrix";

    private const int MaxAmountOfDimensions = 5;

    void Start()
    {
        UpdateTransformationMatrix();

        argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
    }

    internal void PlotData(Data data)
    {
        var values = data.Values;
        var pointsCount = values.Length;

        Vector4[] positions = new Vector4[pointsCount];

        Vector4[] colors = new Vector4[pointsCount];

        for (int i = 0; i < pointsCount; i++)
        {
            var point = values[i];

            Vector4 position = new Vector4();
            Vector4 color = new Vector4();

            for (int dimensionIndex = 0; dimensionIndex < MaxAmountOfDimensions; dimensionIndex++)
            {
                float value = 0.0f;

                if (dimensionIndex < point.Length)
                {
                    value = point[dimensionIndex];
                }

                switch (dimensionIndex)
                {
                    case 0: // X
                        position.x = value;
                        break;
                    case 1: // Y
                        position.y = value;
                        break;
                    case 2: // Z
                        position.z = value;
                        break;
                    case 3: // Color
                        var pointColor = Color.HSVToRGB((value + 1) * MinimumPointSize, 1.0f, 1.0f);

                        color.x = pointColor.r;
                        color.y = pointColor.g;
                        color.z = pointColor.b;
                        color.w = 1.0f;
                        break;
                    case 4: // Size
                        position.w = (value + 1) * MinimumPointSize;
                        break;
                }
            }
            positions[i] = position;
            colors[i] = color;
        }

        PlotData(positions, colors);
    }

    public void UpdateTransformationMatrix()
    {
        if (block == null)
        {
            block = new MaterialPropertyBlock();
        }

        block.SetMatrix(MATRIX_PROPERTY_NAME, transform.localToWorldMatrix * Matrix4x4.Translate(new Vector3(-0.5f, -0.5f, -0.5f)));
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
