using System;

/// <summary>
/// FbxLayerのベースクラス
/// UVやVertexなどを頂点やポリゴンごとに持つ
/// </summary>
/// <typeparam name="T">配列要素の型</typeparam>
public abstract class FbxObjectGeometryLayer<T>
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

    void PolygonToPolygonIndex(FbxObjectGeometryPolygonMap map)
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
    
    void TriangleDivision(FbxObjectGeometryPolygonMap map)
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

    public void ToTriangleVertex(FbxObjectGeometryPolygonMap map)
    {
        PolygonToPolygonIndex(map);
        TriangleDivision(map);
        IndexToVertex();
    }

    protected FbxObjectGeometryLayer(FbxNode node, int arrayElemNum)
    {
        Node = node;
        MappingInformationType = GetMappingInfomationType(node);
        ReferenceInformationType = GetReferenceInformationType(node);
        ArrayElemNum = arrayElemNum;
    }
}

public class FbxObjectGeometryLayerElementUv : FbxObjectGeometryLayer<double>
{
    public double[] UV=>Array;

    public FbxObjectGeometryLayerElementUv(FbxNode node) : base(node, 2)
    {
        Array = node.GetChild("UV").GetProperty<double[]>(0);
        Index = node.GetChild("UVIndex").GetProperty<int[]>(0);
    }
}

public class FbxObjectGeometryLayerElementMaterial : FbxObjectGeometryLayer<int>
{
    public int[] MaterialIndex => Array;
    public FbxObjectGeometryLayerElementMaterial(FbxNode node) : base(node, 1)
    {
        Array = node.GetChild("Materials").GetProperty<int[]>(0);
    }

}

public class FbxObjectGeometryLayerElementNormal : FbxObjectGeometryLayer<double>
{
    public double[] Normal => Array;

    public FbxObjectGeometryLayerElementNormal(FbxNode node, FbxObjectGeometryPolygonMap map) : base(node, 3)
    {
        Array = node.GetChild("Normals").GetProperty<double[]>(0);
    }
}
