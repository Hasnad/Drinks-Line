using System;
using UnityEngine;

public class Block : MonoBehaviour
{
    public int value;

    public Node occupiedNode;
    public Vector2 Pos => transform.position;
    [SerializeField]
    private SpriteRenderer bgRenderer;
    [SerializeField]
    private SpriteRenderer frontRenderer;
    private GameManager gameManager;
    
    private void Start()
    {
        gameManager = GameManager.Instance;
    }
    
    public void SetBlockData(BlockType blockType)
    {
        value = blockType.value;
        bgRenderer.color = blockType.color;
        frontRenderer.sprite = blockType.sprite;
    }
    
    public void SetOccupiedNode(Node node)
    {
        occupiedNode = node;
    }

    public void SetAlphaToHalf()
    {
        var color = frontRenderer.color;
        color.a *= 0.8f;
        frontRenderer.color = color;
    }
}
[Serializable]
public struct BlockType
{
    public int value;
    public Color color;
    public Sprite sprite;
}