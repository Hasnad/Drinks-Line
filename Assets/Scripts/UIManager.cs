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


    private float xpBarMaskWidth;
    private RawImage xpBarImg;

    private void Awake()
    {
        xpBarImg = xpBarMaskRect.GetChild(0).GetComponent<RawImage>();
        xpBarMaskWidth = xpBarMaskRect.sizeDelta.x;
        DOVirtual.Float(0, 1, 15, value =>
        {
            var uvRect = xpBarImg.uvRect;
            uvRect.x = value;
            xpBarImg.uvRect = uvRect;
        }).SetEase(Ease.Linear).SetLoops(-1, LoopType.Incremental);
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

            totalLevelIncreased--;

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