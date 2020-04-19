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

    public readonly int width;

    private Mesh mMesh;

    public MeshData(int meshWidth, bool useFlatShading)
    {
        width = meshWidth;
        mUseFlatShading = useFlatShading;
        mVertices = new Vector3[meshWidth * meshWidth];
        mUVs = new Vector2[meshWidth * meshWidth];
        mTriangles = new int[(meshWidth - 1) * (meshWidth - 1) * 6];
        mBakedNormals = new Vector3[mVertices.Length];
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

    public void  Clear()
    {
        mTriangleIndex = 0;
    }

    void CalculateNormals()
    {
        int triangleCount = mTriangles.Length / 3;
        for (int i = 0; i < triangleCount; i++)
        {
            int normalTriangleIndex = i * 3;
            int vertexIndexA = mTriangles[normalTriangleIndex];
            int vertexIndexB = mTriangles[normalTriangleIndex + 1];
            int vertexIndexC = mTriangles[normalTriangleIndex + 2];

            Vector3 triangleNormal = SurfaceNormalFromIndices(vertexIndexA, vertexIndexB, vertexIndexC);
            mBakedNormals[vertexIndexA] += triangleNormal;
            mBakedNormals[vertexIndexB] += triangleNormal;
            mBakedNormals[vertexIndexC] += triangleNormal;
        }

        
        for (int i = 0; i < mBakedNormals.Length; i++)
        {
            mBakedNormals[i].Normalize();
        }

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
            CalculateNormals();
        }
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
        if(mMesh == null)
        {
            mMesh = new Mesh();
        }
        mMesh.Clear();

        mMesh.vertices = mVertices;
        mMesh.triangles = mTriangles;
        mMesh.uv = mUVs;
        if (mUseFlatShading)
        {
            mMesh.RecalculateNormals();
        }
        else
        {
            mMesh.normals = mBakedNormals;
        }
        return mMesh;
    }
}