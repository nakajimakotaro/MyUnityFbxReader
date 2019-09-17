using System.Collections.Generic;

/// <summary>
/// FbxObjectGeometryPolygonVertexIndexの頂点
/// </summary>
public sealed class FbxObjectGeometryPolygonMap
{
    /// <summary>
    /// ポリゴンを形成する頂点の数のリスト
    /// </summary>
    public int[] VertexNumArray;
    /// <summary>
    /// 三角形にしたときの頂点数の合計
    /// </summary>
    public int VertexNum;

    public FbxObjectGeometryPolygonMap(FbxNode indexNode)
    {
        var map = new List<int>();

        var count = 0;
        var vertexNum = 0;
        foreach (var elem in (int[])indexNode.Properties[0])
        {
            count++;
            if (elem < 0)
            {
                map.Add(count);
                vertexNum += (count - 2) * 3;
                count = 0;
            }
        }

        VertexNumArray = map.ToArray();
        VertexNum = vertexNum;
    }
}