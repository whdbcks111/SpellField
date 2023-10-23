using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StructureGenerator : MonoBehaviour
{
    public const string StructureKey = "structure";

    [SerializeField] private Structure _bigStoneStructure, _smallStoneStructure;
    [SerializeField][Range(1, 10)] private int _maxSmallStoneCount;
    [SerializeField][Range(1, 400)] private float _stoneDistance;

    private float RandomRange(float minInclusive, float maxExclusive)
    {
        return GameManager.Instance.GetSeedRandomRange(StructureKey, minInclusive, maxExclusive);
    }

    private int RandomRange(int minInclusive, int maxExclusive)
    {
        return GameManager.Instance.GetSeedRandomRange(StructureKey, minInclusive, maxExclusive);
    }

    public void Generate(float radius)
    {
        for (int i = 0; i < (radius * radius) / (_stoneDistance * _stoneDistance); i++)
        {
            float angle = RandomRange(0, 2 * Mathf.PI);
            Vector2 pos = RandomRange(0, radius) * new Vector2(
                Mathf.Cos(angle),
                Mathf.Sin(angle)
                );
            Structure.SpawnStructure(_bigStoneStructure, pos);
            if(RandomRange(0f, 1f) < 0.7f)
            {
                int stoneCount = RandomRange(1, _maxSmallStoneCount);
                for (int j = 0; j < stoneCount; j++)
                {
                    Structure.SpawnStructure(_smallStoneStructure, pos + new Vector2(RandomRange(-8f, 8f), RandomRange(-7, -3)));
                }
            } 
        }


        foreach (var player in Player.GetPlayers())
        {
            player.RemoveNearStructures();
        }
    }
}
