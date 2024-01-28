using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoomOptionUI : MonoBehaviour
{
    [SerializeField] private Slider _mapRadiusSlider;
    [SerializeField] private TextMeshProUGUI _mapRadiusDisplay;
    [SerializeField] private Slider _gameTimeSlider;
    [SerializeField] private TextMeshProUGUI _gameTimeDisplay;
    [SerializeField] private Slider _obtainTimeSlider;
    [SerializeField] private TextMeshProUGUI _obtainTimeDisplay;

    private void Awake()
    {
        _mapRadiusSlider.value = GameManager.DefaultMapRadius;
        _gameTimeSlider.value = GameManager.DefaultGameTime;
        _obtainTimeSlider.value = GameManager.DefaultObtainSpan;

        var roomState = NetworkManager.Instance.PingData.RoomState;

        if (roomState.TryGetValue(GameManager.GameTimeOptionKey, out string timeStr)
            && float.TryParse(timeStr, out float time))
            _gameTimeSlider.value = time;

        if (roomState.TryGetValue(GameManager.ObtainSpanOptionKey, out string obtainSpanStr)
            && float.TryParse(obtainSpanStr, out float obtainSpan))
            _obtainTimeSlider.value = obtainSpan;

        if (roomState.TryGetValue(GameManager.MapRadiusOptionKey, out string mapRadiusStr)
            && float.TryParse(mapRadiusStr, out float mapRadius))
            _mapRadiusSlider.value = mapRadius;

        _mapRadiusSlider.onValueChanged.AddListener(value =>
        {
            NetworkManager.Instance.SetRoomState(GameManager.MapRadiusOptionKey, value.ToString());
        });

        _gameTimeSlider.onValueChanged.AddListener(value =>
        {
            NetworkManager.Instance.SetRoomState(GameManager.GameTimeOptionKey, value.ToString());
        });

        _obtainTimeSlider.onValueChanged.AddListener(value =>
        {
            NetworkManager.Instance.SetRoomState(GameManager.ObtainSpanOptionKey, value.ToString());
        });
    }

    private void Update()
    {
        _mapRadiusDisplay.SetText($"{_mapRadiusSlider.value:F0}블록");
        _gameTimeDisplay.SetText($"{(int)(_gameTimeSlider.value / 60f)}분 {_gameTimeSlider.value % 60f:F0}초");
        _obtainTimeDisplay.SetText($"{(int)(_obtainTimeSlider.value / 60f)}분 {_obtainTimeSlider.value % 60f:F0}초");
    }
}
