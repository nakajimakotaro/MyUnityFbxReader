using System;
using System.Collections.Generic;
using UnityEngine;

public class FbxConnectionCache
{
    Dictionary<FbxObjectId, List<FbxObjectId>> ConnectBySourceMap = new Dictionary<FbxObjectId, List<FbxObjectId>>();
    Dictionary<FbxObjectId, List<FbxObjectId>> ConnectByDistMap = new Dictionary<FbxObjectId, List<FbxObjectId>>();

    public static FbxConnectionCache Build(FbxData data)
    {
        var res = new FbxConnectionCache();
        
        var node = data.Node.GetChild("Connections");
        foreach (var conn in node.Childs)
        {
            var sourceId = new FbxObjectId{Id = (long)conn.Properties[1]};
            var distId   = new FbxObjectId{Id = (long)conn.Properties[2]};
            if (!res.ConnectBySourceMap.ContainsKey(sourceId)) res.ConnectBySourceMap.Add(sourceId, new List<FbxObjectId>());
            if (!res.ConnectByDistMap  .ContainsKey(distId  )) res.ConnectByDistMap  .Add(distId  , new List<FbxObjectId>());

            res.ConnectBySourceMap[sourceId].Add(distId);
            res.ConnectByDistMap  [distId  ].Add(sourceId);
        }

        return res;
    }

    public List<FbxObjectId> ConnectBySource(FbxObjectId source)
    {
        ConnectBySourceMap.TryGetValue(source, out var res);
        if(res==null)return new List<FbxObjectId>();
        return res;
    }
    public List<FbxObjectId> ConnectByDist(FbxObjectId dist)
    {
        ConnectByDistMap.TryGetValue(dist, out var res);
        if(res==null)return new List<FbxObjectId>();
        return res;
    }
}