using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class Node : MonoBehaviour
{
    public Node pathFindingParent;

    public Block occupiedBlock;
    public bool IsOccupied => occupiedBlock != null;
    public Vector2 Pos => transform.position;

    public int gCost;
    public int hCost;
    public int FCost => gCost + hCost;
    
    public void SetOccupiedBlock(Block block)
    {
        occupiedBlock = block;
        block.SetOccupiedNode(this);
    }

    public int Distance(Node otherNode)
    {
        return Mathf.RoundToInt(Vector2.Distance(Pos, otherNode.Pos) * 10);
    }

    public List<Node> GetNeighbours(bool fourSidesOnly = true)
    {
        var nodes = new List<Node>();
        for (var y = -1; y <= 1; y++)
        {
            for (var x = -1; x <= 1; x++)
            {
                if (fourSidesOnly)
                {
                    if ((x == 0 && y == 0) || (x == -1 && (y == -1 || y == 1)) ||
                        (x == 1 && (y == 1 || y == -1))) continue;
                }
                else
                {
                    if (x == 0 && y == 0) continue;
                }

                var newPos = new Vector2(Pos.x + x, Pos.y + y);
                var node = GameManager.Instance.GetNodeFromPosition(newPos);
                if (node != null)
                {
                    nodes.Add(node);
                }
            }
        }

        return nodes;
    }
}