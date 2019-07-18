using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

public class FbxHeader
{
    public uint FileVersion;
}

public class FbxData
{
    public FbxHeader Header;
    public FbxNode Node;
    public FbxConnectionCache ConnectionCache;
    public FbxObjectCache ObjectCache;
}

public class FbxNode
{
    public string Name;
    public object[] Properties;
    public FbxNode[] Childs;

    public FbxNode GetChild(string name)
    {
        foreach (var child in Childs)
        {
            if (child.Name == name)
            {
                return child;
            }
        }
        throw new Exception();
    }
    public T GetProperty<T>(int index)
    {
        return (T)Properties[index];
    }
}

public class FbxPropertyHeader
{
    public ulong EndOffset;
    public ulong NumProperties;
    public ulong PropertyListLen;
    public ushort NameLen;
}

public static class FbxDocumentReader
{
    public static FbxData ReadFromFile(string path)
    {
        using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read))
        using (var reader = new BinaryReader(stream))
        {
            return Read(reader);
        }
    }

    static FbxData Read(BinaryReader reader)
    {
        var data = new FbxData();
        data.Header = ReadHeader(reader);
        var nodes = new List<FbxNode>();
        while (true)
        {
            var node = ReadNode(reader, data.Header.FileVersion);
            if (node == null) break;
            nodes.Add(node);
        }
        data.Node = new FbxNode();
        data.Node.Childs = nodes.ToArray();
        data.ConnectionCache = FbxConnectionCache.Build(data);
        data.ObjectCache = FbxObjectCache.Build(data);
        
        return data;
    }

    static FbxHeader ReadHeader(BinaryReader reader)
    {
        var buffer = new byte[21];
        reader.Read(buffer, 0, buffer.Length);
        
        buffer = new byte[2];
        reader.Read(buffer, 0, buffer.Length);
        
        var fileVersion = reader.ReadUInt32();

        var header = new FbxHeader();
        header.FileVersion = fileVersion;

        return header;
    }

    static FbxPropertyHeader ReadNodeHeader(BinaryReader reader, uint version)
    {
        var header = new FbxPropertyHeader();
        if (version == 7400)
        {
            header.EndOffset = reader.ReadUInt32();
            header.NumProperties = reader.ReadUInt32();
            header.PropertyListLen = reader.ReadUInt32();
            header.NameLen =reader.ReadByte();
        }else if (version == 7500)
        {
            header.EndOffset = reader.ReadUInt64();
            header.NumProperties = reader.ReadUInt64();
            header.PropertyListLen = reader.ReadUInt64();
            header.NameLen =reader.ReadByte();
        }
        return header;
    }
    
    static FbxNode ReadNode(BinaryReader reader, uint version)
    {
        var header = ReadNodeHeader(reader, version);
        if (header.EndOffset == 0) return default;
        
        var buffer = new byte[header.NameLen];
        reader.Read(buffer, 0, buffer.Length);
        var name = Encoding.ASCII.GetString(buffer);
        
        var properties = new object[header.NumProperties];
        for (ulong i = 0; i < header.NumProperties; i++)
        {
            var type = reader.ReadChar();
            switch (type)
            {
                case 'C':
                    properties[i] = (reader.ReadByte() & 1) == 1;
                    break;
                case 'Y': properties[i] = reader.ReadInt16(); break;
                case 'I': properties[i] = reader.ReadInt32(); break;
                case 'L': properties[i] = reader.ReadInt64(); break;
                case 'F': properties[i] = reader.ReadSingle(); break;
                case 'D': properties[i] = reader.ReadDouble(); break;
                
                case 'y':
                case 'i':
                case 'l':
                case 'f':
                case 'd':
                    properties[i] = ReadArrayProperty(reader, type);
                    break;
                
                case 'S': properties[i] = ReadStringProperty(reader); break;
                case 'R': properties[i] = ReadRawProperty(reader); break;
            }
        }


        var isContainNode = header.EndOffset != (ulong)reader.BaseStream.Position;

        var childList = new List<FbxNode>();
        if (isContainNode)
        {
            while (true)
            {
                var nestNode = ReadNode(reader, version);
                if (nestNode == null) break;
                childList.Add(nestNode);
            }
        }

        var node = new FbxNode();
        node.Name = name;
        node.Properties = properties;
        node.Childs = childList.ToArray();
        return node;
    }

    static string ReadStringProperty(BinaryReader reader)
    {
        var length = reader.ReadUInt32();
        var buffer = new byte[length];
        reader.Read(buffer, 0, buffer.Length);
        return Encoding.ASCII.GetString(buffer);
    }

    static byte[] ReadRawProperty(BinaryReader reader)
    {
        int length = reader.ReadInt32();
        var buffer = new byte[length];
        reader.Read(buffer, 0, length);
        return buffer;
    }

    static object ReadArrayProperty(BinaryReader reader, char type)
    {
        var arrayLength = reader.ReadUInt32();
        var encoding = reader.ReadUInt32();
        var compressedLength = reader.ReadUInt32();

        int byteNum;
        switch (type)
        {
            case 'y': byteNum = 2; break;
            case 'i': byteNum = 4; break;
            case 'l': byteNum = 8; break;
            case 'f': byteNum = 4; break;
            case 'd': byteNum = 8; break;
            case 'b': byteNum = 1; break;
            default: throw new Exception("");
        }
        var buffer = new byte[arrayLength*byteNum];
        
        if (encoding == 1)
        {
            var combuffer = new byte[compressedLength-2];
            reader.Read(new byte[2], 0, 2);
            reader.Read(combuffer, 0, combuffer.Length);
            
            using (var bufferStream = new MemoryStream(combuffer))
            using (var unCompress = new DeflateStream(bufferStream, CompressionMode.Decompress))
            {
                unCompress.Read(buffer, 0, buffer.Length);
            }
        }
        else
        {
            reader.Read(buffer, 0, buffer.Length);
        }

        T[] ReadFromBuffer<T>(Func<byte[], int, T> f)
        {
            var list = new T[arrayLength];
            for (int i = 0; i < arrayLength; i++)
            {
                list[i] = f(buffer, i*byteNum);
            }

            return list;
        }

        object res = null;
        switch (type)
        {
            case 'y': res = ReadFromBuffer(BitConverter.ToInt16); break;
            case 'i': res = ReadFromBuffer(BitConverter.ToInt32); break;
            case 'l': res = ReadFromBuffer(BitConverter.ToInt64); break;
            case 'f': res = ReadFromBuffer(BitConverter.ToSingle); break;
            case 'd': res = ReadFromBuffer(BitConverter.ToDouble); break;
            case 'b': res = ReadFromBuffer((b, i)=>(b[i]&1)==1); break;
        }

        return res;
    }
}
