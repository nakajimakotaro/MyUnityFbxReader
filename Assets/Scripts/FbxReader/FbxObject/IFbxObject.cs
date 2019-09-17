public interface IFbxObject
{
    FbxObjectType FbxObjectType { get; }
}

public enum FbxObjectType
{
    Geometry,
    Material,
    Texture,
}
