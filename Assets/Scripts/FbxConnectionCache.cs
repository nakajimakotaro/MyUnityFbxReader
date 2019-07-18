using System;
using System.Collections.Generic;

public class FbxConnectionCache
{
    Dictionary<FbxObjectId, List<FbxObjectId>> SourceMap = new Dictionary<FbxObjectId, List<FbxObjectId>>();
    Dictionary<FbxObjectId, List<FbxObjectId>> DistMap = new Dictionary<FbxObjectId, List<FbxObjectId>>();

    public static FbxConnectionCache Build(FbxData data)
    {
        var res = new FbxConnectionCache();
        
        var node = GetChildNode(data.Node, "Connections");
        foreach (var conn in node.Childs)
        {
            var sourceId = new FbxObjectId{Id = (long)conn.Properties[1]};
            var distId   = new FbxObjectId{Id = (long)conn.Properties[2]};
            if (!res.SourceMap.ContainsKey(sourceId)) res.SourceMap.Add(sourceId, new List<FbxObjectId>());
            if (!res.DistMap  .ContainsKey(distId  )) res.DistMap  .Add(distId  , new List<FbxObjectId>());

            res.SourceMap[sourceId].Add(distId);
            res.DistMap  [distId  ].Add(sourceId);
        }

        return res;
    }

    public List<FbxObjectId> DistList(FbxObjectId source)
    {
        return SourceMap[source];
    }
    public List<FbxObjectId> SourceList(FbxObjectId dist)
    {
        return DistMap[dist];
    }

    static T GetProperty<T>(FbxNode node, string name)
    {
        foreach (var e in node.Childs)
        {
            if (e.Name == name)
            {
                return (T) e.Properties[0];
            }
        }

        throw new Exception();
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