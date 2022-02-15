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
    private TextMeshProUGUI highesPointText;

    [SerializeField]
    private GameObject starBurstParticle;
    [SerializeField]
    private Transform sparkleAreaParticle;
    [SerializeField]
    private Transform lostPanel;
    [SerializeField]
    private Transform undoPanel;
    [SerializeField]
    private Transform restartPanel;
    [SerializeField]
    private Transform adRemovePanel;
    [SerializeField]
    private GameObject adRemoveBtn;
    [SerializeField]
    private Transform tcPanel;
    [SerializeField]
    private RectTransform highestPointsImg;
    [SerializeField]
    private RectTransform pointsImg;
    [SerializeField]
    private GameObject showerParticle;

    private float xpBarMaskWidth;
    private RawImage xpBarImg;

    private int lastCelebration = 0;

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
        if (ES3.Load("noAds", false))
        {
            adRemoveBtn.SetActive(false);
        }
        if (!ES3.Load("tc", false))
        {
            TurnOnTcPanel();
        }
    }

    public void OpenTc()
    {
        Application.OpenURL("https://www.privacypolicygenerator.info/live.php?token=4QOVojPiwoj5tldVu9j86nsS3VkzPmil");
    }
    
    public void TurnOnTcPanel()
    {
        tcPanel.localScale = Vector3.zero;
        tcPanel.gameObject.SetActive(true);
        AudioManager.Instance.PlayMergeClip();
        DOVirtual.Float(0, 1, 0.5f, value => { tcPanel.localScale = Vector3.one * value; });
    }
    
    public void TurnOffTcPanel()
    {
        ES3.Save("tc",true);
        AudioManager.Instance.PlayMergeClip();
        DOVirtual.Float(1, 0, 0.5f, value => { tcPanel.localScale = Vector3.one * value; })
            .OnComplete(() => { tcPanel.gameObject.SetActive(false); });
    }

    public void TurnOnUndoPanel()
    {
        if (!UndoSystem.Instance.CanPerformUndo())
        {
            ToastSystem.Instance.Toast(
                "No Undo Available. If any blocks is matched previously, it will clear all undo record");
            return;
        }

        if (!AdController.Instance.rewardIsReady)
        {
            ToastSystem.Instance.Toast("No Ad is available. So, Can not perform undo.");
            return;
        }
        undoPanel.localScale = Vector3.zero;
        undoPanel.gameObject.SetActive(true);
        AudioManager.Instance.PlayMergeClip();
        DOVirtual.Float(0, 1, 0.5f, value => { undoPanel.localScale = Vector3.one * value; });
    }

    public void TurnOffUndoPanel()
    {
        AudioManager.Instance.PlayMergeClip();
        DOVirtual.Float(1, 0, 0.5f, value => { undoPanel.localScale = Vector3.one * value; })
            .OnComplete(() => { undoPanel.gameObject.SetActive(false); });
    }

    public void TurnOnRestartPanel()
    {
        restartPanel.localScale = Vector3.zero;
        restartPanel.gameObject.SetActive(true);
        AudioManager.Instance.PlayMergeClip();
        DOVirtual.Float(0, 1, 0.5f, value => { restartPanel.localScale = Vector3.one * value; });
    }

    public void TurnOffRestartPanel()
    {
        AudioManager.Instance.PlayMergeClip();
        DOVirtual.Float(1, 0, 0.5f, value => { restartPanel.localScale = Vector3.one * value; })
            .OnComplete(() => { restartPanel.gameObject.SetActive(false); });
    }
    
    public void TurnOnAdRemovePanel()
    {
        adRemovePanel.localScale = Vector3.zero;
        adRemovePanel.gameObject.SetActive(true);
        AudioManager.Instance.PlayMergeClip();
        DOVirtual.Float(0, 1, 0.5f, value => { adRemovePanel.localScale = Vector3.one * value; });
    }

    public void TurnOffAdRemovePanel()
    {
        AudioManager.Instance.PlayMergeClip();
        DOVirtual.Float(1, 0, 0.5f, value => { adRemovePanel.localScale = Vector3.one * value; })
            .OnComplete(() => { adRemovePanel.gameObject.SetActive(false); });
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
        EventManager.OnGameLost += OnGameLost;
        EventManager.OnUndoPerformed += OnUndoPerformed;
    }

    private void OnUndoPerformed(Recorder recorder)
    {
        TurnOffUndoPanel();
    }


    private void OnGameLost()
    {
        lostPanel.gameObject.SetActive(true);
        lostPanel.localScale = Vector3.zero;
        AudioManager.Instance.PlayMergeClip();
        DOVirtual.Float(0, 1, 0.5f, value => { lostPanel.localScale = Vector3.one * value; });
    }

    private void OnDisable()
    {
        EventManager.OnUpdateNextBlock -= OnUpdateNextBlock;
        EventManager.OnXpValueUpdated -= OnXpValueUpdated;
        EventManager.OnGameLost -= OnGameLost;
        EventManager.OnUndoPerformed -= OnUndoPerformed;
    }

    private void OnXpValueUpdated(XpHolder xpHolder)
    {
        var totalLevelIncreased = xpHolder.CurrentsLevels - xpHolder.OldLevels;

        if (xpHolder.PointsInAGame - lastCelebration >= 50)
        {
            ToastSystem.Instance.Toast(QuoteGenerator.GetCelebrationQuote(), 7);
            showerParticle.SetActive(true);
            AudioManager.Instance.PlayMergeClip();
            lastCelebration = xpHolder.PointsInAGame;
            if (totalLevelIncreased == 0)
            {
                cam.DOShakePosition(0.15f, 0.6f).OnComplete(() => { cam.DOShakeRotation(0.15f, 0.6f); });
            }
        }

        pointsText.text = "Points: " + xpHolder.PointsInAGame;
        highesPointText.text = "Highest Points: " + xpHolder.HighestPointsInAGame;
        highestPointsImg.gameObject.SetActive(true);
        pointsImg.gameObject.SetActive(true);
        if (xpHolder.HighestPointsInAGame == 0)
        {
            highestPointsImg.localScale = Vector3.one;
            pointsImg.localScale = Vector3.zero;
        }
        else if (xpHolder.HighestPointsInAGame >= xpHolder.PointsInAGame)
        {
            highestPointsImg.localScale = Vector3.one;
            var ratio = xpHolder.PointsInAGame / (float)xpHolder.HighestPointsInAGame;
            pointsImg.DOScale(Vector3.one * ratio, 1f);
        }
        else
        {
            pointsImg.localScale = Vector3.one;
            var ratio = xpHolder.HighestPointsInAGame / (float)xpHolder.PointsInAGame;
            highestPointsImg.DOScale(Vector3.one * ratio, 1f);
        }

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