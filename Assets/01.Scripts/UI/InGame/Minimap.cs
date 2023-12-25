using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class Minimap : MonoBehaviour
{
    private Image _minimapImage;
    [SerializeField] private Image _selfPointPrefab, _otherPointPrefab;
    [SerializeField] private RectTransform _safeAreaCircle;
    [SerializeField] private float zoom = 2f;

    private float _width;
    public readonly Dictionary<string, Image> _points = new();

    private void Awake()
    {
        _minimapImage = GetComponent<Image>();
        _width = _minimapImage.rectTransform.sizeDelta.x;
    }

    private Vector2 ConvertToAnchoredPos(Vector2 pos)
    {
        return _width * (pos - (Vector2)GameManager.Instance.SelfPlayer.transform.position) / (2 * GameManager.Instance.MapRadius / zoom);
    }

    private float ConvertToMinimapSize(float size)
    {
        return size * _width / (2 * GameManager.Instance.MapRadius / zoom);
    }

    private void Update()
    {

        if (GameManager.Instance.SelfPlayer == null) return;

        var pointKeys = _points.Keys.ToArray();
        foreach (string key in pointKeys)
        {
            if (!Player.PlayerMap.ContainsKey(key))
            {
                Destroy(_points[key].gameObject);
                _points.Remove(key);
            }
        }

        _safeAreaCircle.anchoredPosition = ConvertToAnchoredPos(GameManager.Instance.SafeArea.Center);
        _safeAreaCircle.sizeDelta = Vector2.one * ConvertToMinimapSize(GameManager.Instance.SafeArea.Radius * 2);

        foreach (string uid in Player.PlayerMap.Keys)
        {
            var player = Player.PlayerMap[uid];
            if (!_points.ContainsKey(uid)) {
                if(player == GameManager.Instance.SelfPlayer)
                    _points[uid] = Instantiate(_selfPointPrefab, _minimapImage.transform);
                else
                    _points[uid] = Instantiate(_otherPointPrefab, _minimapImage.transform);
            }

            var point = _points[uid];

            var color = point.color;
            if (player != GameManager.Instance.SelfPlayer)
            {
                if (player.Mode == GameMode.Spectator)
                {
                    color.a = 0.2f;
                }
                else if (player.IsInState(PlayerState.Invisible))
                {
                    color.a = 0f;
                }
                else
                {
                    color.a = 1f;
                }
            }
            else 
            {
                color.a = 1f;
            }
            point.color = color;
        }

        foreach(var entry in _points)
        {
            var uid = entry.Key;
            var point = entry.Value;
            point.rectTransform.anchoredPosition = ConvertToAnchoredPos(Player.PlayerMap[uid].transform.position);
        }
    }
}
