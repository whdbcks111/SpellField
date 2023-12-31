using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FloorLoopGenerator : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _loopPrefab;
    [SerializeField] private Sprite[] _loopSprites;

    private Vector3 _loopSize;

    private Vector2 _leftBottom, _rightTop;
    private readonly Dictionary<Vector2Int, SpriteRenderer> _loopObjectMap = new();
    private readonly Queue<SpriteRenderer> _unusedObjects = new();

    private void Awake()
    {
        _loopSize = _loopPrefab.bounds.size;
        CheckFloor();
    }

    private void Update()
    {
        CheckFloor();
    }

    private int GetLoopIndex(Vector2Int pos)
    {
        const float step = 2.3f;
        var perlin = Mathf.PerlinNoise(pos.x * step, pos.y * step);

        return Mathf.Clamp((int)(perlin * _loopSprites.Length), 0, _loopSprites.Length - 1);
    }

    private void PlaceLoopObject(Vector2Int pos)
    {
        if (_loopObjectMap.ContainsKey(pos)) return;
        SpriteRenderer target;
        if (_unusedObjects.Count > 0) target = _unusedObjects.Dequeue();
        else target = Instantiate(_loopPrefab, transform);

        target.sprite = _loopSprites[GetLoopIndex(pos)];
        // 반복감을 없애기 위해 특정한 계산식으로 땅의 스프라이트를 정함

        target.gameObject.SetActive(true);
        target.transform.position = new(pos.x * _loopSize.x, pos.y * _loopSize.y);
        _loopObjectMap[pos] = target;
    }

    private void DestroyLoopObject(Vector2Int pos)
    {
        if (!_loopObjectMap.ContainsKey(pos)) return;
        SpriteRenderer target = _loopObjectMap[pos];
        target.gameObject.SetActive(false);
        _unusedObjects.Enqueue(target);
        _loopObjectMap.Remove(pos);
    }

    private void CheckFloor()
    {
        _leftBottom = Camera.main.ViewportToWorldPoint(new(0, 0));
        _rightTop = Camera.main.ViewportToWorldPoint(new(1, 1));

        int minX = (int)(_leftBottom.x / _loopSize.x - 2), maxX = (int)(_rightTop.x / _loopSize.x + 2);
        int minY = (int)(_leftBottom.y / _loopSize.y - 2), maxY = (int)(_rightTop.y / _loopSize.y + 2);

        var keys = _loopObjectMap.Keys.ToArray();
        foreach (var pos in keys)
        {
            if (pos.x < minX || pos.x > maxX || pos.y < minY || pos.y > maxY)
            {
                DestroyLoopObject(pos);
            }
        }

        for (int x = minX; x <= maxX; ++x)
        {
            for(int y = minY; y <= maxY; ++y)
            {
                PlaceLoopObject(new(x, y));
            }
        }
    }
}
