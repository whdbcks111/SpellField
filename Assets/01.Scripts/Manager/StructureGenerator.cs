using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StructureGenerator : MonoBehaviour
{
    public const string StructureKey = "structure";

    [Header("Stones")]
    [SerializeField] private Structure _bigStoneStructure;
    [SerializeField] private Structure _smallStoneStructure;
    [SerializeField][Range(1, 10)] private int _maxSmallStoneCount;
    [SerializeField][Range(1, 400)] private float _stoneDistance;

    [Header("Bush")]
    [SerializeField] private Structure _bushStructure;
    [SerializeField][Range(1, 400)] private float _bushDistance;

    [Header("Grass")]
    [SerializeField] private Structure _grassStructure;
    [SerializeField][Range(1, 10)] private int _maxGrassCount;
    [SerializeField][Range(1, 400)] private float _grassDistance;

    [Header("Treasure")]
    [SerializeField] private Structure[] _treasureStructures;
    [SerializeField][Range(1, 400)] private float _treasureDistance;

    [Header("BloodStone")]
    [SerializeField] private Structure _bloodStoneStructure;
    [SerializeField][Range(1, 400)] private float _bloodStoneDistance;

    [Header("EnhanceTree")]
    [SerializeField] private Structure _enhanceTreeStructure;
    [SerializeField][Range(1, 400)] private float _enhanceTreeDistance;

    private float RandomRange(float minInclusive, float maxExclusive)
    {
        return GameManager.Instance.GetSeedRandomRange(StructureKey, minInclusive, maxExclusive);
    }

    private int RandomRange(int minInclusive, int maxExclusive)
    {
        return GameManager.Instance.GetSeedRandomRange(StructureKey, minInclusive, maxExclusive);
    }

    public Vector2 GetRandomPosInCircle(float radius)
    {
        Vector2 pos;
        do
        {
            pos = new Vector2(RandomRange(-radius, radius), RandomRange(-radius, radius));
        }
        while (pos.magnitude > radius);
        return pos;
    }

    public void Generate(float radius)
    {
        for (int i = 0; i < (radius * radius) / (_stoneDistance * _stoneDistance); i++)
        {
            Vector2 pos = GetRandomPosInCircle(radius);
            Structure.SpawnStructure(_bigStoneStructure, pos);
            if (RandomRange(0f, 1f) < 0.7f)
            {
                int stoneCount = RandomRange(1, _maxSmallStoneCount + 1);
                for (int j = 0; j < stoneCount; j++)
                {
                    Structure.SpawnStructure(_smallStoneStructure, pos + new Vector2(RandomRange(-8f, 8f), RandomRange(-7, -3)));
                }
            }
        }

        for (int i = 0; i < (radius * radius) / (_bushDistance * _bushDistance); i++)
        {
            Vector2 pos = GetRandomPosInCircle(radius);
            Structure.SpawnStructure(_bushStructure, pos);
        }

        for (int i = 0; i < (radius * radius) / (_treasureDistance * _treasureDistance); i++)
        {
            Vector2 pos = GetRandomPosInCircle(radius);
            Structure.SpawnStructure(_treasureStructures[RandomRange(0, _treasureStructures.Length)], pos);
        }

        for (int i = 0; i < (radius * radius) / (_bloodStoneDistance * _bloodStoneDistance); i++)
        {
            Vector2 pos = GetRandomPosInCircle(radius);
            Structure.SpawnStructure(_bloodStoneStructure, pos);
        }

        for (int i = 0; i < (radius * radius) / (_enhanceTreeDistance * _enhanceTreeDistance); i++)
        {
            Vector2 pos = GetRandomPosInCircle(radius);
            Structure.SpawnStructure(_enhanceTreeStructure, pos);
        }

        for (int i = 0; i < (radius * radius) / (_grassDistance * _grassDistance); i++)
        {
            Vector2 pos = GetRandomPosInCircle(radius);
            int count = RandomRange(1, _maxGrassCount + 1);
            for (int j = 0; j < count; j++)
            {
                Structure.SpawnStructure(_grassStructure, pos + new Vector2(RandomRange(-8f, 8f), RandomRange(-7f, 5f)));
            }
        }
    }
}
