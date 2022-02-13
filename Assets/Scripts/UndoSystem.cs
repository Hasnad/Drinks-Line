using System;
using System.Collections.Generic;
using UnityEngine;

public class UndoSystem : MonoBehaviour
{
    private readonly Stack<Recorder> recorderStack = new Stack<Recorder>();
    public static UndoSystem Instance;

    private void Awake()
    {
        Instance = this;
    }

    public void Record(Node previousNode, Node currentNode)
    {
        var recorder = new Recorder
        {
            PreviousNode = previousNode,
            CurrentNode = currentNode
        };
        EventManager.UpdateNewRecorderCreated(recorder);
        recorderStack.Push(recorder);
    }

    public void PerformUndo()
    {
        if (CanPerformUndo())
        {
            EventManager.UpdateUndoPerformed(recorderStack.Pop());
        }
    }

    public bool CanPerformUndo()
    {
        return recorderStack.Count > 0 && GameManager.Instance.GameState == GameState.WaitingInput;
    }

    public void ClearAllRecord()
    {
        recorderStack.Clear();
    }
}

public class Recorder
{
    public readonly List<Block> NewlyAddedBlocks = new List<Block>(3);
    public readonly List<BlockType> NextSpawnBlocks = new List<BlockType>(3);
    public Node CurrentNode;
    public Node PreviousNode;
}