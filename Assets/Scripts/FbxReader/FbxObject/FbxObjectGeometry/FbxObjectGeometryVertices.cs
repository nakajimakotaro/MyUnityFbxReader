public class FbxObjectGeometryVertices
{
    public double[] Vertices;

    public FbxObjectGeometryVertices(FbxNode node)
    {
        Vertices = (double[])node.Properties[0];
    }

    public void IndexToVertex(FbxObjectGeometryPolygonVertexIndex index)
    {
        Vertices = MeshOperator.IndexToVertex(Vertices, 3, index.Array);
    }
}

