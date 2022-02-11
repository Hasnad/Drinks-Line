using System.Collections.Generic;
using System.Linq;

public static class PathFinding
{
    private static List<Node> openedNode = new List<Node>();
    private static HashSet<Node> closedNode = new HashSet<Node>();


    public static List<Node> FindPath(Node startNode, Node targetNode)
    {
        openedNode.Clear();
        closedNode.Clear();
        openedNode.Add(startNode);

        while (openedNode.Count > 0)
        {
            var currentNode = openedNode.OrderBy(n => n.FCost).ThenBy(n => n.hCost).First();
            openedNode.Remove(currentNode);
            closedNode.Add(currentNode);

            if (currentNode == targetNode)
            {
                var lastNode = targetNode;
                var pathNode = new List<Node>();
                while (lastNode != startNode)
                {
                    pathNode.Add(lastNode);
                    lastNode = lastNode.pathFindingParent;
                }

                pathNode.Reverse();
                return pathNode;
            }

            var neighbourNodes = currentNode.GetNeighbours();
            foreach (var neighbourNode in neighbourNodes)
            {
                if (neighbourNode.IsOccupied || closedNode.Contains(neighbourNode)) continue;
                var newMoveCostToNeighbour = currentNode.gCost + currentNode.Distance(neighbourNode);
                if (newMoveCostToNeighbour < currentNode.gCost || !openedNode.Contains(neighbourNode))
                {
                    neighbourNode.gCost = newMoveCostToNeighbour;
                    neighbourNode.hCost = neighbourNode.Distance(targetNode);
                    neighbourNode.pathFindingParent = currentNode;
                    if (!openedNode.Contains(neighbourNode))
                    {
                        openedNode.Add(neighbourNode);
                    }
                }
            }
        }
        return null;
    }
}