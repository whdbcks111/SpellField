using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldObject : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private float _fadeTime;

    private void Awake()
    {
        var col = _spriteRenderer.color;
        col.a = 0f;
        _spriteRenderer.color = col;
        FadeInTask().Forget();
    }

    public void Destroy()
    {
        DestroyTask().Forget();
    }

    private async UniTask FadeInTask()
    {
        while (_spriteRenderer.color.a < 1f)
        {
            var col = _spriteRenderer.color;
            col.a += Time.deltaTime / Mathf.Max(Time.deltaTime, _fadeTime);
            _spriteRenderer.color = col;
            await UniTask.Yield();
        }
    }

    private async UniTask DestroyTask()
    {
        while(_spriteRenderer.color.a > 0f)
        {
            var col = _spriteRenderer.color;
            col.a -= Time.deltaTime / Mathf.Max(Time.deltaTime, _fadeTime);
            _spriteRenderer.color = col;
            await UniTask.Yield();
        }
        Destroy(gameObject);
    }
}
