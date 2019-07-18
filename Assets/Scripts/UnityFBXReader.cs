using System;
using System.Collections.Generic;
using UnityEngine;

public class UnityFBXReader : MonoBehaviour
{
    [SerializeField] MeshFilter MeshFilter;
    [SerializeField] MeshRenderer MeshRenderer;
    void Start()
    {
        var data = FbxReader.ReadFromFile("Assets/test.fbx");

        var geometry = data.Object.Geometry[0];
        geometry.ToTrainable();

        var vertices = ConvertVector3(geometry.Vertices.Vertices);
        var normals = ConvertVector3(geometry.Normal.Normal);
        var uv = ConvertVector2(geometry.UV.UV);
        
        var materials = geometry.MaterialIndex.MaterialIndex;
        var matList = new List<List<int>>();
        for (var i = 0;i < materials.Length;i++)
        {
            while (materials[i] >= matList.Count)
            {
                matList.Add(new List<int>());
            }
            matList[materials[i]].Add(i);
        }

        var mesh = new Mesh {vertices = vertices, subMeshCount = matList.Count, normals = normals, uv = uv};

        for(var i = 0;i < matList.Count; i++)
        {
            mesh.SetTriangles(matList[i], i);
        }
        
        mesh.RecalculateTangents();
        
        MeshFilter.mesh = mesh;
    }

//    T[] ReadLayerElement<T, E>(FbxNode meshNode, string nodeName, string propertyName, PolygonMap polygonMap,
//        Func<E[], T[]> convertFunc)
//    {
//        var node = GetChildNode(meshNode, nodeName);
//        var info = ReadLayerElementInfo(node);
//        var polygonArray = convertFunc(GetProperty<E[]>(node, propertyName));
//
//        if (info.MappingInformationType == MappingInformationType.ByPolygon)
//        {
//            polygonArray = PolygonToPolygonIndex(polygonMap, polygonArray);
//            return TriangleDivision(polygonMap, polygonArray);
//        }
//        
//        switch (info.ReferenceInformationType)
//        {
//            case ReferenceInformationType.IndexToDirect:
//            {
//                var polygonIndex = GetProperty<int[]>(node, propertyName + "Index");
//                var triangleIndex = TriangleDivision(polygonMap, polygonIndex);
//                return IndexToVertex(polygonArray, triangleIndex);
//            }
//
//            case ReferenceInformationType.Direct:
//                return TriangleDivision(polygonMap, polygonArray);
//            default:
//                throw new Exception();
//        }
//    }

    static Vector2[] ConvertVector2(double[] array)
    {
        var res = new Vector2[array.Length/2];
        for (var i = 0; i < res.Length; i++)
        {
            res[i] = new Vector2(
                (float)array[i*2],
                (float)array[i*2+1]
            );
        }

        return res;
    }
    
    static Vector3[] ConvertVector3(double[] array)
    {
        var res = new Vector3[array.Length/3];
        for (var i = 0; i < res.Length; i++)
        {
            res[i] = new Vector3(
                (float)array[i*3],
                (float)array[i*3+1],
                (float)array[i*3+2]
            );
        }

        return res;
    }
}