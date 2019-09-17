using System.Collections.Generic;
using UnityEngine;

public class UnityFBXReader : MonoBehaviour
{
    [SerializeField] MeshFilter MeshFilter;
    [SerializeField] MeshRenderer MeshRenderer;
    static readonly int MainTex = Shader.PropertyToID("_MainTex");

    void Start()
    {
        var data = FbxReader.ReadFromFile("Assets/test.fbx");

        var geometry = data.Object.Geometry[0];
        geometry.ToTrainable();

        var vertices = ConvertVector3(geometry.ObjectGeometryVertices.Vertices);
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
        mesh.UploadMeshData(true);
        
        MeshFilter.mesh = mesh;

        for (var i = 0; i < data.Object.Materials.Count; i++)
        {
            var materialData = data.Object.Materials[i];
            var textureData = materialData.GetTexture();
            if (textureData == null)
            {
                continue;
            }

            var path = textureData.RelativeFilename.Replace("Resources\\", "").Replace(".png", "");
            var texture = Resources.Load<Texture>(path);
            var material = MeshRenderer.materials[i];
            material.SetTexture(MainTex, texture);
        }
    }

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