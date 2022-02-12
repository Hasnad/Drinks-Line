using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("General")]
    public float xpIncreaseTime;

    [Header("Reference")]
    [SerializeField]
    private RectTransform xpBarMaskRect;

    [SerializeField]
    private Image[] nextBlocksImg;
    [SerializeField]
    private TextMeshProUGUI pointsText;
    [SerializeField]
    private TextMeshProUGUI levelText;

    [SerializeField]
    private GameObject starBurstParticle;
    [SerializeField]
    private Transform sparkleAreaParticle;

    private float xpBarMaskWidth;
    private RawImage xpBarImg;

    private Camera cam;

    private void Awake()
    {
        cam = Camera.main;
        xpBarImg = xpBarMaskRect.GetChild(0).GetComponent<RawImage>();
        xpBarMaskWidth = xpBarMaskRect.sizeDelta.x;
        DOVirtual.Float(0, 1, 15, value =>
        {
            var uvRect = xpBarImg.uvRect;
            uvRect.x = value;
            xpBarImg.uvRect = uvRect;
        }).SetEase(Ease.Linear).SetLoops(-1, LoopType.Incremental);
    }

    private void Start()
    {
        Invoke(nameof(TurnOnFx), 2);
    }

    private void TurnOnFx()
    {
        starBurstParticle.transform.position = levelText.rectTransform.position;
        var pos = xpBarMaskRect.position;
        pos.x += 2.356f;
        sparkleAreaParticle.position = pos;

        sparkleAreaParticle.gameObject.SetActive(true);
    }

    private void OnEnable()
    {
        EventManager.OnUpdateNextBlock += OnUpdateNextBlock;
        EventManager.OnXpValueUpdated += OnXpValueUpdated;
    }

    private void OnDisable()
    {
        EventManager.OnUpdateNextBlock -= OnUpdateNextBlock;
        EventManager.OnXpValueUpdated -= OnXpValueUpdated;
    }

    private void OnXpValueUpdated(XpHolder xpHolder)
    {
        var totalLevelIncreased = xpHolder.CurrentsLevels - xpHolder.OldLevels;
        pointsText.text = "Points: " + xpHolder.PointsInAGame;
        if (totalLevelIncreased == 0)
        {
            levelText.text = xpHolder.CurrentsLevels.ToString();
        }

        if (totalLevelIncreased > 0)
        {
            var sequence = DOTween.Sequence();

            sequence.Append(DOVirtual.Float(xpBarMaskRect.sizeDelta.x / xpBarMaskWidth, 1, xpIncreaseTime, value =>
            {
                var sizeDelta = xpBarMaskRect.sizeDelta;
                sizeDelta.x = value * xpBarMaskWidth;
                xpBarMaskRect.sizeDelta = sizeDelta;
            }));

            levelText.text = (xpHolder.OldLevels + 1).ToString();
            starBurstParticle.SetActive(true);

            totalLevelIncreased--;

            cam.DOShakePosition(0.15f, 0.6f).OnComplete(() => { cam.DOShakeRotation(0.15f, 0.6f); });

            if (totalLevelIncreased > 0)
            {
                sequence.Append(DOVirtual.Float(0, 1, xpIncreaseTime, value =>
                {
                    var sizeDelta = xpBarMaskRect.sizeDelta;
                    sizeDelta.x = value * xpBarMaskWidth;
                    xpBarMaskRect.sizeDelta = sizeDelta;
                }).SetLoops(totalLevelIncreased, LoopType.Restart));

                sequence.Join(DOVirtual.Int(xpHolder.OldLevels + 1, xpHolder.CurrentsLevels,
                    xpIncreaseTime * (totalLevelIncreased + 1),
                    value =>
                    {
                        levelText.text = value.ToString();
                        starBurstParticle.SetActive(false);
                        starBurstParticle.SetActive(true);
                    }));
            }

            sequence.OnComplete(() => { xpBarMaskRect.DOSizeDelta(GetSizeDelta(xpHolder), xpIncreaseTime); });
        }
        else
        {
            xpBarMaskRect.DOSizeDelta(GetSizeDelta(xpHolder), xpIncreaseTime);
        }
    }

    private Vector2 GetSizeDelta(XpHolder xpHolder)
    {
        var sizeDelta = xpBarMaskRect.sizeDelta;
        sizeDelta.x = xpHolder.CurrentNormalizedPoints * xpBarMaskWidth;
        return sizeDelta;
    }


    private void OnUpdateNextBlock(List<BlockType> blockTypes)
    {
        for (var i = 0; i < nextBlocksImg.Length; i++)
        {
            // nextBlocksImg[i].color = blockTypes[i].color;
            nextBlocksImg[i].transform.GetChild(0).GetComponent<Image>().sprite = blockTypes[i].sprite;
            var i1 = i;
            DOVirtual.Vector3(Vector3.zero, Vector3.one, 0.5f,
                v => { nextBlocksImg[i1].transform.GetChild(0).localScale = v; });
        }
    }
}