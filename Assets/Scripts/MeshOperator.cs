using System;

public static class MeshOperator
{
    /// <summary>
    /// indexに従ってarrayの要素を割り当てていく
    /// </summary>
    /// <param name="array"></param>
    /// <param name="triangleIndex"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T[] IndexToVertex<T>(T[] array, int num, int[] triangleIndex)
    {
        var res = new T[triangleIndex.Length*num];
        for (var i = 0; i < triangleIndex.Length; i++)
        {
            for (var e = 0; e < num; e++)
            {
                res[i*num+e] = array[triangleIndex[i]*num+e];
            }
        }

        return res;
    }

    public static T[] TriangleDivision<T>(T[] array, int num, PolygonMap map)
    {
        var triangle = new T[map.VertexNum*num];
        var vertexCount = 0;
        var triangleCount = 0;
        foreach (var vertexNum in map.VertexNumArray)
        {
            var firstIndex = vertexCount;
            for (var i = 0; i < vertexNum-2; i++)
            {
                var secondIndex = vertexCount+i+1;
                var thirdIndex = vertexCount+i+2;
                for (var e = 0; e < num; e++)
                {
                    try
                    {
                        triangle[triangleCount    *num+e] = array[firstIndex*num+e];
                        triangle[(triangleCount+1)*num+e] = array[secondIndex*num+e];
                        triangle[(triangleCount+2)*num+e] = array[thirdIndex*num+e];
                    }
                    catch (Exception exception)
                    {
                        Console.WriteLine(exception);
                        throw;
                    }
                }
                triangleCount += 3;
            }

            vertexCount += vertexNum;
        }
        
        return triangle;
    }
    
    public static T[] PolygonToPolygonIndex<T>(T[] array, int num, PolygonMap map)
    {
        var res = new T[map.VertexNum];

        var resIndex=0;
        var polygonIndex=0;
        for(var i = 0;i < array.Length/num;i++)
        {
            for (var k = 0; k < map.VertexNumArray[polygonIndex]; k++)
            {
                for (var e = 0; e < num; e++)
                {
                    res[resIndex*num+e] = array[i*num+e];
                }
                resIndex++;
            }

            polygonIndex++;
        }

        return res;
    }
}