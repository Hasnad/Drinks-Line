using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    private GameState gameState;

    public Vector2Int gridSize;
    public int minMatchSize = 3;

    [SerializeField]
    private Node nodePrefab;
    [SerializeField]
    private Block blockPrefab;
    [SerializeField]
    private SpriteRenderer boardPrefab;

    private readonly List<Node> nodes = new List<Node>();
    private List<Block> blocks = new List<Block>();

    [SerializeField]
    private List<BlockType> blockTypes;

    private readonly List<BlockType> nextSpawnBlocks = new List<BlockType>(3);
    private readonly List<Block> matchedBlocks = new List<Block>();
    private readonly List<Block> newlyAddedBlocks = new List<Block>();

    private Camera cam;
    private SpriteRenderer board;
    private AudioManager audioManager;

    private Block selectedBlock = null;

    private void Awake() => Instance = this;

    private void Start()
    {
        cam = Camera.main;
        audioManager = AudioManager.Instance;
        ChangeState(GameState.GenerateLevel);
    }

    private void GenerateLevel()
    {
        var nodeHolder = new GameObject("Node Holder").transform;
        nodeHolder.position = Vector3.zero;
        for (var y = 0; y < gridSize.y; y++)
        {
            for (var x = 0; x < gridSize.x; x++)
            {
                var node = Instantiate(nodePrefab, new Vector3(x, y, 0.1f), Quaternion.identity);
                node.transform.SetParent(nodeHolder);
                nodes.Add(node);
            }
        }

        var center = new Vector3(gridSize.x / 2f - 0.5f, gridSize.y / 2f - 0.5f, .2f);
        board = Instantiate(boardPrefab, center, Quaternion.identity);
        board.size = gridSize;
        cam.transform.position = new Vector3(center.x, center.y, -10);
        cam.orthographicSize = (gridSize.x + gridSize.y) / 3f;

        ChangeState(GameState.SpawningBlocks);
    }

    private void SpawnBlocks(int amount)
    {
        var freeNodes = nodes.FindAll(n => !n.IsOccupied).OrderBy(n => Random.value).Take(amount).ToList();

        if (freeNodes.Count < 3)
        {
            ChangeState(GameState.Loss);
            return;
        }

        if (nextSpawnBlocks.Count == 0)
        {
            for (var i = 0; i < amount; i++)
            {
                nextSpawnBlocks.Add(blockTypes[Random.Range(0, blockTypes.Count)]);
            }
        }

        newlyAddedBlocks.Clear();
        foreach (var node in freeNodes)
        {
            var blockType = nextSpawnBlocks[0];
            nextSpawnBlocks.RemoveAt(0);
            var block = Instantiate(blockPrefab, node.Pos, Quaternion.identity);
            block.SetBlockData(blockType);
            node.SetOccupiedBlock(block);
            blocks.Add(block);
            newlyAddedBlocks.Add(block);
            EventManager.UpdateBlockSpawned(block);
        }

        for (var i = 0; i < amount; i++)
        {
            nextSpawnBlocks.Add(blockTypes[Random.Range(0, blockTypes.Count)]);
        }

        EventManager.UpdateNextBlock(nextSpawnBlocks);
        foreach (var block in newlyAddedBlocks)
        {
            selectedBlock = block;
            BlockMerge(false);
        }

        selectedBlock = null;

        if (nodes.TrueForAll(n => n.IsOccupied))
        {
            ChangeState(GameState.Loss);
            return;
        }
        ChangeState(GameState.WaitingInput);
    }

    private void Update()
    {
        if (gameState != GameState.WaitingInput) return;
        if (Input.GetMouseButtonDown(0))
        {
            var hit = Physics2D.Raycast(cam.ScreenToWorldPoint(Input.mousePosition), Vector2.zero,
                100, LayerMask.GetMask("Block", "Node"));
            if (hit.collider == null) return;

            if (selectedBlock == null)
            {
                if (hit.collider.CompareTag("Block"))
                {
                    selectedBlock = hit.transform.GetComponent<Block>();
                    EventManager.UpdateBlockSelected(selectedBlock);
                }
            }
            else if (selectedBlock != null)
            {
                if (hit.collider.CompareTag("Block"))
                {
                    var block = hit.transform.GetComponent<Block>();
                    if (selectedBlock == block)
                    {
                        EventManager.UpdateBlockDeselected(selectedBlock);
                        selectedBlock = null;
                    }
                    else
                    {
                        EventManager.UpdateBlockDeselected(selectedBlock);
                        selectedBlock = block;
                        EventManager.UpdateBlockSelected(selectedBlock);
                    }
                }
                else if (hit.collider.CompareTag("Node"))
                {
                    var node = hit.transform.GetComponent<Node>();
                    if (!node.IsOccupied)
                    {
                        ChangeState(GameState.BlockMoving);
                        // nodes.ForEach(n =>
                        //     n.transform.GetChild(0).GetComponent<SpriteRenderer>().color = GetColorFromHex("#908B82"));
                        var path = PathFinding.FindPath(selectedBlock.occupiedNode, node);
                        if (path == null)
                        {
                            Debug.Log("No Path Found");
                            ChangeState(GameState.WaitingInput);
                            return;
                        }

                        var sequence = DOTween.Sequence();
                        foreach (var n in path)
                        {
                            // n.transform.GetChild(0).GetComponent<SpriteRenderer>().color = Color.black;
                            // selectedBlock.SetAlphaToHalf();
                            sequence.Append(selectedBlock.transform.DOMove(n.Pos, 0.05f)).SetEase(Ease.Linear);
                            audioManager.PlayMoveClip(.2f);
                        }

                        sequence.OnComplete(() =>
                        {
                            selectedBlock.occupiedNode.occupiedBlock = null;
                            node.SetOccupiedBlock(selectedBlock);
                            EventManager.UpdateBlockDeselected(selectedBlock);
                            ChangeState(GameState.BlockMerging);
                        });
                    }
                }
            }
        }
    }

    private void BlockMerge(bool changeToSpawnBlock = true)
    {
        matchedBlocks.Clear();
        var currentNode = selectedBlock.occupiedNode;
        var counter = 0;
        var checkSidesMatch = currentNode.GetNeighbours(false);
        foreach (var matchNode in checkSidesMatch)
        {
            if (matchNode.IsOccupied && matchNode.occupiedBlock.value == selectedBlock.value)
            {
                counter++;
                matchedBlocks.Add(matchNode.occupiedBlock);
                var xDist = gridSize.x - matchNode.Pos.x;
                var yDist = gridSize.y - matchNode.Pos.y;
                var direction = matchNode.Pos - currentNode.Pos;
                var nextNodePos = matchNode.Pos;
                while (true)
                {
                    nextNodePos += direction;
                    var node = GetNodeFromPosition(nextNodePos);
                    if (node != null && node.IsOccupied && node.occupiedBlock.value == selectedBlock.value)
                    {
                        counter++;
                        matchedBlocks.Add(node.occupiedBlock);
                    }
                    else
                    {
                        break;
                    }
                }

                nextNodePos = matchNode.Pos;
                while (true)
                {
                    nextNodePos -= direction;
                    var node = GetNodeFromPosition(nextNodePos);
                    if (node != null && node.IsOccupied && node.occupiedBlock.value == selectedBlock.value)
                    {
                        counter++;
                        matchedBlocks.Add(node.occupiedBlock);
                    }
                    else
                    {
                        break;
                    }
                }

                if (counter >= minMatchSize)
                {
                    MatchFound(matchedBlocks);
                    EventManager.UpdateBlocksMatchFound(matchedBlocks);
                    return;
                }

                counter = 0;
                matchedBlocks.Clear();
            }
        }

        selectedBlock = null;
        if (changeToSpawnBlock)
        {
            ChangeState(GameState.SpawningBlocks);
        }
    }

    private void MatchFound(List<Block> blockList)
    {
        var sequence = DOTween.Sequence();
        foreach (var block in blockList)
        {
            block.occupiedNode.occupiedBlock = null;
            blocks.Remove(block);
            sequence.Join(block.transform.DOScale(0.01f, 0.5f));
            audioManager.PlayMergeClip();
        }

        sequence.OnComplete(() =>
        {
            blockList.ForEach(b => Destroy(b.gameObject));
            selectedBlock = null;

            ChangeState(GameState.WaitingInput);
        });
    }

    private Color GetColorFromHex(string hexCode)
    {
        ColorUtility.TryParseHtmlString(hexCode, out var c);
        return c;
    }

    private void ResetLevel(bool sceneLoad)
    {
        if (sceneLoad)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            return;
        }

        Destroy(nodes[0].transform.parent.gameObject);
        nodes.Clear();
        nextSpawnBlocks.Clear();
        selectedBlock = null;
        Destroy(board.gameObject);
    }

    private BlockType GetBlockTypeByValue(int value) => blockTypes.First(b => b.value == value);

    public Node GetNodeFromPosition(Vector2 pos) => nodes.FirstOrDefault(n => n.Pos == pos);

    private void ChangeState(GameState state)
    {
        gameState = state;
        switch (gameState)
        {
            case GameState.GenerateLevel:
                GenerateLevel();
                break;
            case GameState.SpawningBlocks:
                SpawnBlocks(3);
                break;
            case GameState.WaitingInput:
                break;
            case GameState.BlockMoving:
                break;
            case GameState.BlockMerging:
                BlockMerge();
                break;
            case GameState.Win:
                print("You Win");
                break;
            case GameState.Loss:
                print("You lost");
                break;
            case GameState.Reset:
                ResetLevel(true);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(state), state, null);
        }
    }
}

public enum GameState
{
    GenerateLevel = 0,
    SpawningBlocks = 1,
    WaitingInput = 2,
    BlockMoving = 3,
    BlockMerging = 4,
    Win = 5,
    Loss = 6,
    Reset = 7,
}