using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlacementGrid : MonoBehaviour
{
    [SerializeField] int _gridSize = 100;
    Material lineMaterial;

    private (Vector3, Vector3)[] _gridVertices;

    [SerializeField] private Transform _targetTransform;
    [SerializeField] private bool _snapToCenter = false;

    Plane plane;

    private void Awake()
    {
        CreateLineMaterial();
        plane = new Plane(Vector3.up, 0);
        _gridVertices = new (Vector3, Vector3)[_gridSize * 2 + 2];
        int lineLength = _gridSize * GridUtilities.TileSize;
        int index = 0;
        for (int i = 0; i < _gridSize + 1; i++, index++)
        {
            int lineOffset = i * GridUtilities.TileSize;
            _gridVertices[index] = (new Vector3(lineOffset, 0, 0), new Vector3(lineOffset, 0, lineLength));
        }
        for (int i = 0; i < _gridSize + 1; i++, index++)
        {
            int lineOffset = i * GridUtilities.TileSize;
            _gridVertices[index] = (new Vector3(0, 0, lineOffset), new Vector3(lineLength, 0, lineOffset));
        }
    }

    void CreateLineMaterial()
    {
        // simple colored things.
        var shader = Shader.Find("Hidden/Internal-Colored");
        lineMaterial = new Material(shader);
        lineMaterial.hideFlags = HideFlags.HideAndDontSave;
        // Turn on alpha blending
        lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        // Turn backface culling off
        lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
        // Turn off depth writes
        lineMaterial.SetInt("_ZWrite", 0);
    }

    void OnRenderObject()
    {
        lineMaterial.SetPass(0);
        GL.PushMatrix();
        GL.MultMatrix(transform.localToWorldMatrix); // not needed if already in worldspace
        GL.Begin(GL.LINES);
        GL.Color(Color.red);
        for (int i = 0; i < _gridVertices.Length; i++)
        {
            var pair = _gridVertices[i];
            GL.Vertex(pair.Item1);
            GL.Vertex(pair.Item2);
        }

        GL.End();
        GL.PopMatrix();
    }

    private void Update()
    {
        float distance;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (plane.Raycast(ray, out distance))
        {
            Vector3 worldPos = ray.GetPoint(distance);
            if (_snapToCenter)
            {
                _targetTransform.position = GridUtilities.GetTileCenterFromWorldXZ(worldPos);
            }
            else
            {
                _targetTransform.position = GridUtilities.GetGridPosition(worldPos);
            }
        }
    }
}
