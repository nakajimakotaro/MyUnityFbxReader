using System;
using System.Collections.Generic;
using UnityEngine.Assertions;

public class FbxReader
{
    public static FbxDocument ReadFromFile(string path)
    {
        var data = FbxDocumentReader.ReadFromFile(path);
        return new FbxDocument(data);
    }
}

public class FbxDocument
{
    public FbxObject Object;

    public FbxDocument(FbxData data)
    {
        Object = new FbxObject(data.Node.GetChild("Objects"));
    }
}

public class FbxObject
{
    public List<FbxMaterial> Materials = new List<FbxMaterial>();
    public List<FbxGeometry> Geometry = new List<FbxGeometry>();

    public FbxObject(FbxNode objNode)
    {
        foreach (var child in objNode.Childs)
        {
            switch (child.Name)
            {
                case "Material":
                    Materials.Add(new FbxMaterial(child));
                    break;
                case "Geometry":
                    Geometry.Add(new FbxGeometry(child));
                    break;
            }
        }
    }
}

public class FbxGeometry
{
    public FbxVertices Vertices;
    public FbxPolygonVertexIndex PolygonVertexIndex;
    public PolygonMap PolygonMap;
    public FbxLayerElementUV UV;
    public FbxLayerElementNormal Normal;
    public FbxLayerElementMaterial MaterialIndex;
    bool IsTriangle;

    public FbxGeometry(FbxNode node)
    {
        foreach (var child in node.Childs)
        {
            switch (child.Name)
            {
                case "Vertices":
                    Vertices = new FbxVertices(child);
                    break;
                case "PolygonVertexIndex":
                    PolygonMap = new PolygonMap(child);
                    PolygonVertexIndex = new FbxPolygonVertexIndex(child, PolygonMap);
                    break;
                case "LayerElementNormal":
                    Normal = new FbxLayerElementNormal(child, PolygonMap);
                    break;
                case "LayerElementUV":
                    UV = new FbxLayerElementUV(child);
                    break;
                case "LayerElementMaterial":
                    MaterialIndex = new FbxLayerElementMaterial(child);
                    break;
            }
        }
    }
    
    public void ToTrainable()
    {
        Assert.IsFalse(IsTriangle);
        IsTriangle = true;

        PolygonVertexIndex.ToTrainable(PolygonMap);
        Vertices.IndexToVertex(PolygonVertexIndex);
        
        UV.ToTriangleVertex(PolygonMap);
        Normal.ToTriangleVertex(PolygonMap);
        MaterialIndex.ToTriangleVertex(PolygonMap);
    }
}

public class FbxVertices
{
    public double[] Vertices;

    public FbxVertices(FbxNode node)
    {
        Vertices = (double[])node.Properties[0];
    }

    public void IndexToVertex(FbxPolygonVertexIndex index)
    {
        Vertices = MeshOperator.IndexToVertex(Vertices, 3, index.Array);
    }
}

public class FbxPolygonVertexIndex
{
    public int[] Array;
    
    public FbxPolygonVertexIndex(FbxNode node, PolygonMap map)
    {
        Array = (int[])node.Properties[0];
        VertexIndexNegativeToPositive(Array, map);
    }

    /// <summary>
    ///反転しているビットを戻す
    /// https://stackoverflow.com/questions/7736845/can-anyone-explain-the-fbx-format-for-me
    /// </summary>
    /// <param name="indexes"></param>
    /// <returns></returns>
    static void VertexIndexNegativeToPositive(int[] indexes, PolygonMap map)
    {
        int lastPolygonIndex = -1;
        foreach (var polygonNum in map.VertexNumArray)
        {
            lastPolygonIndex += polygonNum;
            indexes[lastPolygonIndex] = ~indexes[lastPolygonIndex];
        }
    }

    public void ToTrainable(PolygonMap map)
    {
        Array = MeshOperator.TriangleDivision(Array, 1, map);
    }
}

public abstract class FbxLayer<T>
{
    public MappingInformationType MappingInformationType;
    public ReferenceInformationType ReferenceInformationType;
    
    protected T[] Array;
    protected int[] Index;
    /// <summary>
    /// 1頂点ごとの要素数(Normalなら3,UVなら2)
    /// </summary>
    protected int ArrayElemNum;

    protected FbxNode Node;

    MappingInformationType GetMappingInfomationType(FbxNode node)
    {
        var typeStr = node.GetChild("MappingInformationType").GetProperty<string>(0);
        switch (typeStr)
        {
            case "ByPolygonVertex":
                return MappingInformationType.ByPolygonVertex;
            case "ByPolygon":
                return MappingInformationType.ByPolygon;
            default:
                throw new Exception();
        }
    }
    ReferenceInformationType GetReferenceInformationType(FbxNode node)
    {
        var typeStr = node.GetChild("ReferenceInformationType").GetProperty<string>(0);
        switch (typeStr)
        {
            case "Direct":
                return ReferenceInformationType.Direct;
            case "IndexToDirect":
                return ReferenceInformationType.IndexToDirect;
            default:
                throw new Exception();
        }
    }

    void PolygonToPolygonIndex(PolygonMap map)
    {
        if (MappingInformationType == MappingInformationType.ByPolygon)
        {
            Array = MeshOperator.PolygonToPolygonIndex(Array, ArrayElemNum, map);
        }
    }
    
    void IndexToVertex()
    {
        if (MappingInformationType == MappingInformationType.ByPolygonVertex && ReferenceInformationType == ReferenceInformationType.IndexToDirect)
        {
            Array = MeshOperator.IndexToVertex(Array, ArrayElemNum, Index);
        }
    }
    
    void TriangleDivision(PolygonMap map)
    {
        if (MappingInformationType == MappingInformationType.ByPolygon)
        {
            Array = MeshOperator.TriangleDivision(Array, ArrayElemNum, map);
        }
        else
        {
            if (ReferenceInformationType == ReferenceInformationType.IndexToDirect)
            {
                Index = MeshOperator.TriangleDivision(Index, 1, map);
            }
            else
            {
                Array = MeshOperator.TriangleDivision(Array, ArrayElemNum, map);
            }
        }
    }

    public void ToTriangleVertex(PolygonMap map)
    {
        PolygonToPolygonIndex(map);
        TriangleDivision(map);
        IndexToVertex();
    }

    protected FbxLayer(FbxNode node, int arrayElemNum)
    {
        Node = node;
        MappingInformationType = GetMappingInfomationType(node);
        ReferenceInformationType = GetReferenceInformationType(node);
        ArrayElemNum = arrayElemNum;
    }
}

public class FbxLayerElementUV : FbxLayer<double>
{
    public double[] UV=>Array;

    public FbxLayerElementUV(FbxNode node) : base(node, 2)
    {
        Array = node.GetChild("UV").GetProperty<double[]>(0);
        Index = node.GetChild("UVIndex").GetProperty<int[]>(0);
    }
}

public class FbxLayerElementMaterial : FbxLayer<int>
{
    public int[] MaterialIndex => Array;
    public FbxLayerElementMaterial(FbxNode node) : base(node, 1)
    {
        Array = node.GetChild("Materials").GetProperty<int[]>(0);
    }

}

public class FbxLayerElementNormal : FbxLayer<double>
{
    public double[] Normal => Array;

    public FbxLayerElementNormal(FbxNode node, PolygonMap map) : base(node, 3)
    {
        Array = node.GetChild("Normals").GetProperty<double[]>(0);
    }
}


public class FbxMaterial : FbxNode
{
    public FbxMaterial(FbxNode node)
    {
    }
}