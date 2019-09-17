using System;
using System.Collections.Generic;

public sealed class FbxReader
{
    public static FbxDocument ReadFromFile(string path)
    {
        var data = FbxDocumentReader.ReadFromFile(path);
        return new FbxDocument(data);
    }
}

public sealed class FbxDocument
{
    public FbxConnectionCache ConnectionCache;
    public FbxObjectCache ObjectCache;
    public FbxObject Object;

    public FbxDocument(FbxData data)
    {
        ObjectCache = new FbxObjectCache();
        ConnectionCache = FbxConnectionCache.Build(data);
        Object = new FbxObject(this, data);
    }
}

public sealed class FbxObject
{
    public List<FbxObjectMaterial> Materials = new List<FbxObjectMaterial>();
    public List<FbxObjectTexture> Textures = new List<FbxObjectTexture>();
    public List<FbxGeometry> Geometry = new List<FbxGeometry>();

    public FbxObject(FbxDocument fbxDocument, FbxData data)
    {
        var objNode = data.Node.GetChild("Objects");
        foreach (var child in objNode.Childs)
        {
            switch (child.Name)
            {
                case "Geometry":
                    Geometry.Add(new FbxGeometry(fbxDocument, child));
                    break;
                case "Material":
                    Materials.Add(new FbxObjectMaterial(fbxDocument, child));
                    break;
                case "Texture":
                    Textures.Add(new FbxObjectTexture(fbxDocument, child));
                    break;
            }
        }
    }
}
