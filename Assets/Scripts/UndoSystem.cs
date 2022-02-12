using System;
using System.Collections.Generic;
using UnityEngine;

public class UndoSystem : MonoBehaviour
{
    private Stack<Recorder> RecorderStack = new Stack<Recorder>();
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
        RecorderStack.Push(recorder);
    }

    public void PerformUndo()
    {
        if (RecorderStack.Count > 0 && GameManager.Instance.GameState == GameState.WaitingInput)
        {
            EventManager.UpdateUndoPerformed(RecorderStack.Pop());
        }
    }

    public void ClearAllRecord()
    {
        RecorderStack.Clear();
    }
}

public class Recorder
{
    public readonly List<Block> NewlyAddedBlocks = new List<Block>(3);
    public readonly List<BlockType> NextSpawnBlocks = new List<BlockType>(3);
    public Node CurrentNode;
    public Node PreviousNode;
}