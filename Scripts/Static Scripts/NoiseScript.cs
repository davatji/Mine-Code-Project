using UnityEngine;
using System;

public static class NoiseScript
{

    //receives world absolute pos and additional properties to determine the characteristic of perlin sample. returns the perlin noise value
    public static float GetPerlinNoise2D(Vector2 position, float scale, float offset, int worldLengthInChunk, int worldWidthInChunk){
        return Mathf.PerlinNoise((position.x + 0.1f)/ worldLengthInChunk * scale + offset, (position.y + 0.1f) / worldWidthInChunk * scale + offset);
    }

    //the same logic as GetPerlinNoise2D, except that the value it returns is boolean (whether the perlinNoise is bigger than or equal to the threshold)
    public static bool GetPerlinNoise3D(Vector3 position, float scale, float offset, float threshold, int worldLengthInChunk, int worldWidthInChunk){
        float permutationXY = GetPerlinNoise2D(new Vector2(position.x, position.y), scale, offset, worldLengthInChunk, worldWidthInChunk);
        float permutationYZ = GetPerlinNoise2D(new Vector2(position.y, position.z), scale, offset, worldLengthInChunk, worldWidthInChunk);
        float permutationXZ = GetPerlinNoise2D(new Vector2(position.x, position.z), scale, offset, worldLengthInChunk, worldWidthInChunk);

        float permutationYX = GetPerlinNoise2D(new Vector2(position.y, position.x), scale, offset, worldLengthInChunk, worldWidthInChunk);
        float permutationZY = GetPerlinNoise2D(new Vector2(position.z, position.y), scale, offset, worldLengthInChunk, worldWidthInChunk);
        float permutationZX = GetPerlinNoise2D(new Vector2(position.z, position.x), scale, offset, worldLengthInChunk, worldWidthInChunk);

        float meanPerlin = (permutationXY + permutationYZ + permutationXZ + permutationYX + permutationZY + permutationZX) / 6;
        return meanPerlin >= threshold;
    }
}
