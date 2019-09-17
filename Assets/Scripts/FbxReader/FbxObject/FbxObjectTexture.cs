
using UnityEngine;

public sealed class FbxObjectTexture: IFbxObject
{
    public FbxObjectId Id;
    public string Name;
    public string RelativeFilename;
    FbxDocument FbxDocument;

    public FbxObjectTexture(FbxDocument fbxDocument, FbxNode node)
    {
        FbxDocument = fbxDocument;
        Id = new FbxObjectId{Id = (long) node.Properties[0]};
        Name = (string) node.Properties[1];
        RelativeFilename = (string) node.GetChild("RelativeFilename").Properties[0];
        
        FbxDocument.ObjectCache.Add(this, Id);
    }

    public FbxObjectType FbxObjectType => FbxObjectType.Texture;
}
