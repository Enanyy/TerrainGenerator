using UnityEngine;
using System.Collections;


public class MeshData
{
    private Vector3[] mVertices;
    private int[] mTriangles;
    private Vector2[] mUVs;
    private Vector3[] mBakedNormals;

    private int mTriangleIndex;

    private bool mUseFlatShading;

    public MeshData(int meshWidth, int meshHeight, bool useFlatShading)
    {
        mUseFlatShading = useFlatShading;
        mVertices = new Vector3[meshWidth * meshHeight];
        mUVs = new Vector2[meshWidth * meshHeight];
        mTriangles = new int[(meshWidth - 1) * (meshHeight - 1) * 6];
    }

    public void AddVertice(int index, Vector3 vertice, Vector2 uv)
    {
        mVertices[index] = vertice;
        mUVs[index] = uv;
    }

    public void AddTriangle(int a, int b, int c)
    {
        mTriangles[mTriangleIndex] = a;
        mTriangles[mTriangleIndex + 1] = b;
        mTriangles[mTriangleIndex + 2] = c;
        mTriangleIndex += 3;
    }


    Vector3[] CalculateNormals()
    {

        Vector3[] vertexNormals = new Vector3[mVertices.Length];
        int triangleCount = mTriangles.Length / 3;
        for (int i = 0; i < triangleCount; i++)
        {
            int normalTriangleIndex = i * 3;
            int vertexIndexA = mTriangles[normalTriangleIndex];
            int vertexIndexB = mTriangles[normalTriangleIndex + 1];
            int vertexIndexC = mTriangles[normalTriangleIndex + 2];

            Vector3 triangleNormal = SurfaceNormalFromIndices(vertexIndexA, vertexIndexB, vertexIndexC);
            vertexNormals[vertexIndexA] += triangleNormal;
            vertexNormals[vertexIndexB] += triangleNormal;
            vertexNormals[vertexIndexC] += triangleNormal;
        }

        
        for (int i = 0; i < vertexNormals.Length; i++)
        {
            vertexNormals[i].Normalize();
        }

        return vertexNormals;

    }

    Vector3 SurfaceNormalFromIndices(int indexA, int indexB, int indexC)
    {
        Vector3 pointA =  mVertices[indexA];
        Vector3 pointB =  mVertices[indexB];
        Vector3 pointC =  mVertices[indexC];

        Vector3 sideAB = pointB - pointA;
        Vector3 sideAC = pointC - pointA;
        return Vector3.Cross(sideAB, sideAC).normalized;
    }

    public void ProcessMesh()
    {
        if (mUseFlatShading)
        {
            FlatShading();
        }
        else
        {
            BakeNormals();
        }
    }

    void BakeNormals()
    {
        mBakedNormals = CalculateNormals();
    }

    void FlatShading()
    {
        Vector3[] flatShadedVertices = new Vector3[mTriangles.Length];
        Vector2[] flatShadedUvs = new Vector2[mTriangles.Length];

        for (int i = 0; i < mTriangles.Length; i++)
        {
            flatShadedVertices[i] = mVertices[mTriangles[i]];
            flatShadedUvs[i] = mUVs[mTriangles[i]];
            mTriangles[i] = i;
        }

        mVertices = flatShadedVertices;
        mUVs = flatShadedUvs;
    }

    public Mesh CreateMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = mVertices;
        mesh.triangles = mTriangles;
        mesh.uv = mUVs;
        if (mUseFlatShading)
        {
            mesh.RecalculateNormals();
        }
        else
        {
            mesh.normals = mBakedNormals;
        }
        return mesh;
    }
}