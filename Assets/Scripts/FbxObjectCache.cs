using System;
using System.Collections.Generic;

public class FbxObjectCache
{
    Dictionary<FbxObjectId, FbxNode> Map = new Dictionary<FbxObjectId, FbxNode>();

    public FbxNode Get(FbxObjectId id)
    {
        return Map[id];
    }
    public FbxObjectId Id(FbxNode node)
    {
        return new FbxObjectId{Id = (long) node.Properties[0]};
    }
    public static FbxObjectCache Build(FbxData data)
    {
        var res = new FbxObjectCache();
        
        var objects = GetChildNode(data.Node, "Objects").Childs;
        foreach (var node in objects)
        {
            res.Map.Add(new FbxObjectId{Id = (long)node.Properties[0]}, node);
        }

        return res;
    }

    static FbxNode GetChildNode(FbxNode node, string name)
    {
        foreach (var child in node.Childs)
        {
            if (child.Name == name)
                return child;
        }

        throw new Exception();
    }
}
