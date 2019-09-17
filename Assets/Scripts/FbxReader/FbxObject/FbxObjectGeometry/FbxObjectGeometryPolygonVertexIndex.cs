public class FbxObjectGeometryPolygonVertexIndex
{
    public int[] Array;
    
    public FbxObjectGeometryPolygonVertexIndex(FbxNode node, FbxObjectGeometryPolygonMap map)
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
    static void VertexIndexNegativeToPositive(int[] indexes, FbxObjectGeometryPolygonMap map)
    {
        int lastPolygonIndex = -1;
        foreach (var polygonNum in map.VertexNumArray)
        {
            lastPolygonIndex += polygonNum;
            indexes[lastPolygonIndex] = ~indexes[lastPolygonIndex];
        }
    }

    public void ToTrainable(FbxObjectGeometryPolygonMap map)
    {
        Array = MeshOperator.TriangleDivision(Array, 1, map);
    }
}

