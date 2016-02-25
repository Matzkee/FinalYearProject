﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CustomCyllinder : MonoBehaviour {

    public float radius;
    public float thetaOffset;
    public int numberOfPoints;
    public float height;
    List<Circle> circles;

    public Material material;
    Mesh mesh;
    MeshRenderer meshRenderer;

    /*
        This class is used to study procedurall mesh generation
        
        Please Ignore
    */
    void Start()
    {
        mesh = gameObject.AddComponent<MeshFilter>().mesh;
        meshRenderer = gameObject.AddComponent<MeshRenderer>();
        mesh.Clear();
        // Save current transform position & rotation
        Quaternion saveRot = transform.rotation;
        Vector3 savePos = transform.position;

        // Create 2 circles by moving the transform
        circles = new List<Circle>();
        Circle newCircle = new Circle(CreateCircleAt(transform, radius, numberOfPoints));
        circles.Add(newCircle);
        transform.Rotate(Vector3.right * 30.0f);
        transform.Translate(Vector3.forward * (radius * 2));
        Circle anotherCircle = new Circle(CreateCircleAt(transform, radius, numberOfPoints));
        circles.Add(anotherCircle);

        // Restore transforms position and rotation
        transform.position = savePos;
        transform.rotation = saveRot;

        // Create the mesh and set a premade material
        GenerateMesh(circles[0], circles[1]);
        meshRenderer.material = material;
    }

    List<Vector3> CreateCircleAt(Transform _centre, float _radius, int _numpoints)
    {
        List<Vector3> newPoints = new List<Vector3>();
        float thetaInc = (Mathf.PI * 2.0f) / _numpoints;
        //// Save current transform's rotation
        Quaternion prevRot = _centre.rotation;
        for (int i = 0; i < _numpoints; i++)
        {
            // Rotate the transform by preset theta and get point directly above it
            _centre.Rotate(Vector3.forward * (thetaInc * Mathf.Rad2Deg));
            Vector3 newPoint = (_centre.position + (_centre.up * _radius));
            newPoints.Add(newPoint);
        }
        // Return the transform to previous rotation
        _centre.rotation = prevRot;
        //Vector3 axis = _centre.forward;
        //Vector3 startPoint = _centre.position + (_centre.up * _radius);
        //for (int i = 0; i < _numpoints; i++)
        //{
        //    float theta = i * thetaInc;
        //    Quaternion q = Quaternion.AngleAxis(theta * Mathf.Rad2Deg, axis);
        //    Vector3 newPoint = q * startPoint;
        //    newPoints.Add(newPoint);
        //}
        return newPoints;
    }

    void GenerateMesh(Circle _c1, Circle _c2)
    {
        int numOfPoints = _c1.circlePoints.Count;
        // 3 Vertices per triangle, 2 triangles
        int verticesPerCell = 6;
        int vertexCount = (verticesPerCell * 2 * numOfPoints);

        // Alocate new arrays
        Vector3[] vertices = new Vector3[_c1.circlePoints.Count * 2];
        int[] triangles = new int[vertexCount];
        Vector2[] uvs = new Vector2[vertices.Length];

        int vertexIndex = 0;

        //Assign vertices
        int index = 0;
        for (int i = 0; i < _c1.circlePoints.Count; i++)
        {
            vertices[index++] = _c2.circlePoints[i];
            vertices[index++] = _c1.circlePoints[i];
        }
        int tLeft = 0;
        int bLeft = 1;
        int tRight = 2;
        int bRight = 3;
        string nums = "";
        for (int i = 0; i < numOfPoints; i++)
        {



            Vector3 cellBottomLeft = _c1.circlePoints[i];
            Vector3 cellTopLeft = _c2.circlePoints[i];
            Vector3 cellTopRight = _c2.circlePoints[(i + 1)%numOfPoints];
            Vector3 cellBottomRight = _c1.circlePoints[(i + 1)%numOfPoints];

            int startVertex = vertexIndex;
            vertexIndex += 6;
            //vertices[vertexIndex++] = cellTopLeft;
            //vertices[vertexIndex++] = cellBottomLeft;
            //vertices[vertexIndex++] = cellBottomRight;
            //vertices[vertexIndex++] = cellTopLeft;
            //vertices[vertexIndex++] = cellBottomRight;
            //vertices[vertexIndex++] = cellTopRight;

            triangles[startVertex] = tLeft;
            triangles[startVertex + 1] = bLeft;
            triangles[startVertex + 2] = bRight;
            triangles[startVertex + 3] = tLeft;
            triangles[startVertex + 4] = bRight;
            triangles[startVertex + 5] = tRight;
            tLeft = tRight;
            tRight += 2; tRight = tRight % index;
            bLeft = bRight;
            bRight += 2; bRight = bRight % index;
            //// Make triangles
            //for (int j = 0; j < verticesPerCell; j++)
            //{
            //    triangles[startVertex + j] = startVertex + j;
            //}
        }
        Debug.Log("Number of vertices: "+ vertices.Length);
        Debug.Log("Number of polygons: " + (triangles.Length)/3);
        // Assign values to the mesh
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();

    }

    void OnDrawGizmos()
    {
        if (circles != null)
        {
            for (int i = 0; i < circles.Count; i++)
            {
                for (int j = 1; j <= circles[i].circlePoints.Count; j++)
                {
                    Vector3 prev = circles[i].circlePoints[j - 1];
                    Vector3 next = circles[i].circlePoints[j % circles[i].circlePoints.Count];
                    Gizmos.color = Color.green;
                    Gizmos.DrawLine(prev, next);
                }
            }
        }
    }
    /*
        Class for storing x amount of points along a circle
    */
    class Circle
    {
        public List<Vector3> circlePoints;

        public Circle(List<Vector3> _points)
        {
            circlePoints = _points;
        }
    }
}
