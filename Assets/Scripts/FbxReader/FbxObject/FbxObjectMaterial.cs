using UnityEngine;

public sealed class FbxObjectMaterial: IFbxObject
{
    public FbxDocument FbxDocument;
    public FbxObjectId Id;
    public string Name;
    public FbxObjectMaterial(FbxDocument fbxDocument, FbxNode node)
    {
        FbxDocument = fbxDocument;
        Id = new FbxObjectId {Id = (long) node.Properties[0]};
        Name = (string) node.Properties[1];
        
        FbxDocument.ObjectCache.Add(this, Id);
    }

    public FbxObjectTexture GetTexture()
    {
        foreach(var dist in FbxDocument.ConnectionCache.ConnectByDist(Id))
        {
            if (FbxDocument.ObjectCache.FbxObject(dist) is FbxObjectTexture texture)
            {
                return texture;
            }
        }

        return null;
    }

    public FbxObjectType FbxObjectType => FbxObjectType.Material;
}
