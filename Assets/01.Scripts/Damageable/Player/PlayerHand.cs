using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHand : MonoBehaviour
{
    private bool _isSwinging = false;
    private float _swingTime = 0.5f;
    private float _rotateTarget = 0f;
    private float _resetTimer = 0f;
    private float _vel = 0f;

    private void Awake()
    {
        transform.localEulerAngles = Vector3.zero;
    }

    private void Update()
    {
        if(_resetTimer > 0f)
        {
            _resetTimer -= Time.deltaTime;
            if (_resetTimer <= 0f)
            {
                _swingTime = 1f;
                _rotateTarget = 0f;
            }
        }
        transform.localEulerAngles = new(0, 0, Mathf.SmoothDampAngle(transform.localEulerAngles.z, _rotateTarget, ref _vel, _swingTime));
    }

    public void Swing(float rot, float time)
    {
        if (_isSwinging) return;
        _resetTimer = 1f;
        _isSwinging = true;
        SwingTask(rot, time).Forget();
    }

    private async UniTask SwingTask(float rot, float time)
    {
        _rotateTarget = Mathf.Abs(Mathf.DeltaAngle(rot, transform.localEulerAngles.z)) < Mathf.Abs(rot / 2f) ? 0f : rot;
        _swingTime = time;
        await UniTask.Delay(TimeSpan.FromSeconds(time));
        _isSwinging = false;
    }
}
