using System.Collections.Generic;
using UnityEngine;

public class FbxObjectCache
{
    Dictionary<FbxObjectId, IFbxObject> IdToNode = new Dictionary<FbxObjectId, IFbxObject>();
    Dictionary<IFbxObject, FbxObjectId> NodeToId = new Dictionary<IFbxObject, FbxObjectId>();
    
    public void Add(IFbxObject node, FbxObjectId Id)
    {
        IdToNode.Add(Id, node);
        NodeToId.Add(node, Id);
    }
    public IFbxObject FbxObject(FbxObjectId id)
    {
        IdToNode.TryGetValue(id, out var res);
        return res;
    }
    public FbxObjectId Id(IFbxObject node)
    {
        return NodeToId[node];
    }
}
