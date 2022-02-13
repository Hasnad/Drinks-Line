using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class XpManager : MonoBehaviour
{
    private readonly Xp xp = new Xp();
    public int pointsPerBlock = 2;


    private void Start()
    {
        xp.Init();
    }

    private void OnEnable()
    {
        EventManager.OnBlockMatchFound += OnBlockMatchFound;
    }

    private void OnDisable()
    {
        EventManager.OnBlockMatchFound -= OnBlockMatchFound;
    }


    private void OnBlockMatchFound(List<Block> blocks)
    {
        xp.IncreasePoints(blocks.Count * pointsPerBlock);
        UndoSystem.Instance.ClearAllRecord();
    }


    private class Xp
    {
        private int points;
        private int levels;
        private int pointsRequiredToLevelUp;
        private const int LevelUpMultiplier = 2;
        private int pointsInSingleMatch;
        private int highestPointsInSingleMatch;

        public void Init()
        {
            Load();
            var xpHolder = new XpHolder
            {
                OldPoints = 0,
                OldLevels = levels,
                OldNormalizedPoints = Normalize(),
                CurrentPoints = points,
                CurrentsLevels = levels,
                CurrentNormalizedPoints = Normalize(),
                PointsInAGame = 0,
                HighestPointsInAGame = highestPointsInSingleMatch
            };

            EventManager.UpdateXpValueUpdated(xpHolder);
        }

        private float Normalize()
        {
            return points / (float)pointsRequiredToLevelUp;
        }


        private void Save()
        {
            ES3.Save("points", points);
            ES3.Save("levels", levels);
            ES3.Save("levelUpPoints", pointsRequiredToLevelUp);
            ES3.Save("highestPoints", highestPointsInSingleMatch);
        }

        private void Load()
        {
            points = ES3.Load("points", 0);
            levels = ES3.Load("levels", 0);
            pointsRequiredToLevelUp = ES3.Load("levelUpPoints", 10);
            highestPointsInSingleMatch = ES3.Load("highestPoints", 0);
        }

        public void IncreasePoints(int amount)
        {
            var xpHolder = new XpHolder
            {
                OldPoints = points,
                OldLevels = levels,
                OldNormalizedPoints = Normalize()
            };
            points += amount;
            pointsInSingleMatch += amount;
            if (pointsInSingleMatch > highestPointsInSingleMatch)
            {
                highestPointsInSingleMatch = pointsInSingleMatch;
            }
            LevelUp();
            xpHolder.CurrentPoints = points;
            xpHolder.CurrentsLevels = levels;
            xpHolder.CurrentNormalizedPoints = Normalize();
            xpHolder.PointsInAGame = pointsInSingleMatch;
            xpHolder.HighestPointsInAGame = highestPointsInSingleMatch;
            Save();
            EventManager.UpdateXpValueUpdated(xpHolder);
        }

        private void LevelUp()
        {
            while (points >= pointsRequiredToLevelUp)
            {
                points -= pointsRequiredToLevelUp;
                levels++;
                pointsRequiredToLevelUp *= LevelUpMultiplier;
            }
        }
    }
}

public struct XpHolder
{
    public int OldPoints;
    public int CurrentPoints;
    public int OldLevels;
    public int CurrentsLevels;
    public float OldNormalizedPoints;
    public float CurrentNormalizedPoints;
    public int PointsInAGame;
    public int HighestPointsInAGame;
}