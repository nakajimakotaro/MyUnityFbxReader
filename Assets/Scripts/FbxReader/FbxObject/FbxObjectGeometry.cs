using UnityEngine.Assertions;

public class FbxGeometry: IFbxObject
{
    public FbxObjectId Id;
    
    public FbxObjectGeometryVertices ObjectGeometryVertices;
    public FbxObjectGeometryPolygonVertexIndex ObjectGeometryPolygonVertexIndex;
    public FbxObjectGeometryPolygonMap FbxObjectGeometryPolygonMap;
    public FbxObjectGeometryLayerElementUv UV;
    public FbxObjectGeometryLayerElementNormal Normal;
    public FbxObjectGeometryLayerElementMaterial MaterialIndex;
    bool IsTriangle;
    FbxDocument FbxDocument;

    public FbxGeometry(FbxDocument fbxDocument, FbxNode node)
    {
        FbxDocument = fbxDocument;
        FbxDocument.ObjectCache.Add(this, Id);
        
        Id = new FbxObjectId{Id = (long) node.Properties[0]};
        foreach (var child in node.Childs)
        {
            switch (child.Name)
            {
                case "Vertices":
                    ObjectGeometryVertices = new FbxObjectGeometryVertices(child);
                    break;
                case "PolygonVertexIndex":
                    FbxObjectGeometryPolygonMap = new FbxObjectGeometryPolygonMap(child);
                    ObjectGeometryPolygonVertexIndex = new FbxObjectGeometryPolygonVertexIndex(child, FbxObjectGeometryPolygonMap);
                    break;
                case "LayerElementNormal":
                    Normal = new FbxObjectGeometryLayerElementNormal(child, FbxObjectGeometryPolygonMap);
                    break;
                case "LayerElementUV":
                    UV = new FbxObjectGeometryLayerElementUv(child);
                    break;
                case "LayerElementMaterial":
                    MaterialIndex = new FbxObjectGeometryLayerElementMaterial(child);
                    break;
            }
        }
    }
    
    public void ToTrainable()
    {
        Assert.IsFalse(IsTriangle);
        IsTriangle = true;

        ObjectGeometryPolygonVertexIndex.ToTrainable(FbxObjectGeometryPolygonMap);
        ObjectGeometryVertices.IndexToVertex(ObjectGeometryPolygonVertexIndex);
        
        UV.ToTriangleVertex(FbxObjectGeometryPolygonMap);
        Normal.ToTriangleVertex(FbxObjectGeometryPolygonMap);
        MaterialIndex.ToTriangleVertex(FbxObjectGeometryPolygonMap);
    }

    public FbxObjectType FbxObjectType => FbxObjectType.Geometry;
}
