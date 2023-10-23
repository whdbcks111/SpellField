using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class Minimap : MonoBehaviour
{
    private Image _image;
    [SerializeField] private RectTransform _selfPointPrefab, _otherPointPrefab;

    private float _width;
    public readonly Dictionary<string, RectTransform> _points = new();

    private void Awake()
    {
        _image = GetComponent<Image>();
        _width = _image.rectTransform.sizeDelta.x;
    }

    private Vector2 ConvertToAnchoredPos(Vector2 pos)
    {
        return _width * pos / (2 * GameManager.Instance.MapRadius);
    }

    private void Update()
    {

        if (GameManager.Instance.SelfPlayer == null) return;

        foreach(string key in _points.Keys )
        {
            if (!Player.PlayerMap.ContainsKey(key))
            {
                Destroy(_points[key].gameObject);
                _points.Remove(key);
            }
        }

        foreach(string uid in Player.PlayerMap.Keys)
        {
            if(!_points.ContainsKey(uid)) {
                if(uid == GameManager.Instance.SelfPlayer.ClientInfo.UID)
                    _points[uid] = Instantiate(_selfPointPrefab, _image.transform);
                else
                    _points[uid] = Instantiate(_otherPointPrefab, _image.transform);
            }
        }

        foreach(var entry in _points)
        {
            var uid = entry.Key;
            var point = entry.Value;
            point.anchoredPosition = ConvertToAnchoredPos(Player.PlayerMap[uid].transform.position);
        }
    }
}
