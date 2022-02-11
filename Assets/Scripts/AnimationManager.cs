using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class AnimationManager : MonoBehaviour
{
    private void OnEnable()
    {
        EventManager.OnBlockSelected += OnBlockSelected;
        EventManager.OnBlockDeselected += OnBlockDeselected;
        EventManager.OnBlockSpawned += OnBlockSpawned;
    }

    private void OnBlockSpawned(Block block)
    {
        DOVirtual.Vector3(Vector3.zero, Vector3.one, 0.5f, v =>
        {
            block.transform.localScale = v;
        });
    }

    private void OnBlockDeselected(Block block)
    {
        block.transform.DOKill();
        block.transform.localScale = Vector3.one;
    }

    private void OnBlockSelected(Block block)
    {
        block.transform.DOScale(Vector2.one * .8f, .5f).SetEase(Ease.Linear).SetLoops(-1, LoopType.Yoyo);
    }

    private void OnDisable()
    {
        EventManager.OnBlockSelected -= OnBlockSelected;
        EventManager.OnBlockDeselected -= OnBlockDeselected;
        EventManager.OnBlockSpawned -= OnBlockSpawned;
    }
}