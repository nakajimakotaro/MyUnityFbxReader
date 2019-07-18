using System.Collections.Generic;

public sealed class PolygonMap
{
    public int[] VertexNumArray;
    public int VertexNum;

    public PolygonMap(FbxNode indexNode)
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