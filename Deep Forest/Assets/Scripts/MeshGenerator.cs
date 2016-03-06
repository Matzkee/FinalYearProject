using UnityEngine;
using System.Collections;
using System.Collections.Generic;
/* 
    Class acting as a mesh generator for different mesh structures
*/
public class MeshGenerator{

    public GameObject GenerateTreeMesh(List<BranchSegment> branchSegments, List<BranchTip> branchTips, Material treeBark)
    {
        GameObject treeStructure = new GameObject("Tree Structure");
        Mesh mesh;
        MeshRenderer meshRenderer;

        mesh = treeStructure.AddComponent<MeshFilter>().mesh;
        meshRenderer = treeStructure.AddComponent<MeshRenderer>();
        mesh.Clear();

        // Get the number of point per circle
        int pointsPerCircle = branchSegments[0].startCircle.circlePoints.Count;
        // Adding 1 extra vertex for better uv mapping
        int vertsPerSide = pointsPerCircle + 1;
        // 3 triangle indexes per polygon
        int trianglePoints = 3;

        int triangleCount = ((vertsPerSide * (trianglePoints * 2)) * branchSegments.Count) + 
            ((vertsPerSide * trianglePoints) * branchTips.Count);
        // Since we use reduced number of vertices we calculate their optimal amount
        // For counting branch tips we add and extra 1 for the tip vertex itself
        int optimalVertsCount = (2 * (vertsPerSide) * branchSegments.Count) +
           ((vertsPerSide + 1) * branchTips.Count);

        // Alocate new arrays
        Vector3[] vertices = new Vector3[optimalVertsCount];
        Vector2[] uvs = new Vector2[optimalVertsCount];
        int[] triangles = new int[triangleCount];

        int vertexIndex = 0;
        int vertexIndexUV = 0;
        int sideCounter = 0;
        int triangleIndex = 0;
        float tilling = (float)(sideCounter++) / pointsPerCircle;

        // Set triangle indexes
        int tLeft, bLeft, tRight, bRight, centre;

        Vector2 uvBottom = new Vector2(tilling, 0f);
        Vector2 uvTop = new Vector2(tilling, 1f / pointsPerCircle);

        // Each time we add vertices to the array we always add extra for better uv mapping
        // of textures. vertsPerSide is our preset variable for that

        foreach (BranchSegment s in branchSegments)
        {
            tLeft = vertexIndex;
            bLeft = vertexIndex + 1;
            tRight = vertexIndex + 2;
            bRight = vertexIndex + 3;
            for (int i = 0; i < vertsPerSide; i++)
            {
                vertices[vertexIndex++] = s.endCircle.circlePoints[i % pointsPerCircle];
                vertices[vertexIndex++] = s.startCircle.circlePoints[i % pointsPerCircle];
            }

            for (int i = 0; i < vertsPerSide; i++)
            {
                // Assign uv control nodes to its corresponding vertices
                uvs[vertexIndexUV++] = uvBottom;
                uvs[vertexIndexUV++] = uvTop;

                // Calculate next uv offset
                tilling = (float)(sideCounter++) / pointsPerCircle;
                uvBottom = new Vector2(tilling, 0f);
                uvTop = new Vector2(tilling, 1f / pointsPerCircle);

                // Assign triangle indexes
                triangles[triangleIndex++] = tLeft;
                triangles[triangleIndex++] = bLeft;
                triangles[triangleIndex++] = bRight;
                triangles[triangleIndex++] = tLeft;
                triangles[triangleIndex++] = bRight;
                triangles[triangleIndex++] = tRight;

                // Rearrange triangle indexes
                tLeft = tRight;
                tRight += 2;
                tRight = (tRight >= vertexIndex) ? tRight - (vertsPerSide * 2) : tRight;
                bLeft = bRight;
                bRight += 2;
                bRight = (bRight >= vertexIndex) ? bRight - (vertsPerSide * 2) : bRight;
            }
        }

        //Create the mesh for cones
        foreach (BranchTip c in branchTips)
        {
            sideCounter = 0;
            bLeft = vertexIndex;
            bRight = vertexIndex + 1;
            for (int i = 0; i < vertsPerSide; i++)
            {
                vertices[vertexIndex++] = c.startCircle.circlePoints[i % pointsPerCircle];
            }
            // Add extra vertex as centre for uv mapping
            vertices[vertexIndex++] = c.end;
            centre = vertexIndex - 1;
            // Add the centre and set its index
            for (int i = 0; i < pointsPerCircle + 1; i++)
            {
                // Assign uv control nodes to its corresponding vertices & calculate next offset
                tilling = (float)(sideCounter++) / pointsPerCircle;
                uvBottom = new Vector2(tilling, 1f / pointsPerCircle);
                uvs[vertexIndexUV++] = uvBottom;

                // Assign triangle indexes
                triangles[triangleIndex++] = bLeft;
                triangles[triangleIndex++] = bRight;
                triangles[triangleIndex++] = centre;


                // Rearrange triangle indexes
                bLeft = bRight;
                bRight += 1;
                bRight = (bRight >= vertexIndex) ? bRight - vertsPerSide : bRight;
            }
            // Use 0.5f for now later on trace circle points on mesh and assign values this way
            Vector2 uvEndPoint = new Vector2(0.5f, 0.5f);
            uvs[vertexIndexUV++] = uvEndPoint;
        }

        // Assign values to the mesh
        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        meshRenderer.material = treeBark;

        return treeStructure;
    }

    public GameObject GenerateTreeLeaves(List<Leaf> leaves, Material leafMaterial)
    {
        GameObject treeLeaves = new GameObject("Tree Leaves");
        Mesh mesh;
        MeshRenderer meshRenderer;

        mesh = treeLeaves.AddComponent<MeshFilter>().mesh;
        meshRenderer = treeLeaves.AddComponent<MeshRenderer>();
        mesh.Clear();

        int vertexCount = leaves.Count * 4;
        int triangleCount = leaves.Count * 6;
        // Indexes
        int vertexIndex = 0;
        int triangleIndex = 0;
        int uvIndex = 0;
        // Triangle indexes
        int tLeft = vertexIndex;
        int bLeft = vertexIndex + 1;
        int tRight = vertexIndex + 2;
        int bRight = vertexIndex + 3;
        // Alocate new arrays
        Vector3[] vertices = new Vector3[vertexCount];
        Vector2[] uvs = new Vector2[vertexCount];
        int[] triangles = new int[triangleCount];

        foreach (Leaf t in leaves)
        {
            // Apply vertices
            vertices[vertexIndex++] = t.tLeft;
            vertices[vertexIndex++] = t.bLeft;
            vertices[vertexIndex++] = t.tRight;
            vertices[vertexIndex++] = t.bRight;
            // Apply uvs
            uvs[uvIndex++] = new Vector2(0, 1);
            uvs[uvIndex++] = new Vector2(0, 0);
            uvs[uvIndex++] = new Vector2(1, 1);
            uvs[uvIndex++] = new Vector2(1, 0);

            // Apply triangles
            triangles[triangleIndex++] = tLeft;
            triangles[triangleIndex++] = bRight;
            triangles[triangleIndex++] = bLeft;
            triangles[triangleIndex++] = tLeft;
            triangles[triangleIndex++] = tRight;
            triangles[triangleIndex++] = bRight;

            tLeft = vertexIndex;
            bLeft = vertexIndex + 1;
            tRight = vertexIndex + 2;
            bRight = vertexIndex + 3;
        }

        // Assign arrays to the mesh
        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        // Assign leaves to the tree structure
        meshRenderer.material = leafMaterial;

        return treeLeaves;
    }
}
