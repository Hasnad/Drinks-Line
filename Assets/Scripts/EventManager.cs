using System;
using System.Collections.Generic;

public static class EventManager
{
    public static event Action<List<BlockType>> OnUpdateNextBlock;
    public static event Action<Block> OnBlockSelected;
    public static event Action<Block> OnBlockDeselected;
    public static event Action<Block> OnBlockSpawned;
    public static event Action<List<Block>> OnBlockMatchFound;
    public static event Action<XpHolder> OnXpValueUpdated;


    public static void UpdateNextBlock(List<BlockType> blockTypes)
    {
        OnUpdateNextBlock?.Invoke(blockTypes);
    }

    public static void UpdateBlockSelected(Block selectedBlock)
    {
        OnBlockSelected?.Invoke(selectedBlock);
    }

    public static void UpdateBlockDeselected(Block blockToDeselect)
    {
        OnBlockDeselected?.Invoke(blockToDeselect);
    }

    public static void UpdateBlocksMatchFound(List<Block> blocks)
    {
        OnBlockMatchFound?.Invoke(blocks);
    }

    public static void UpdateBlockSpawned(Block block)
    {
        OnBlockSpawned?.Invoke(block);
    }

    public static void UpdateXpValueUpdated(XpHolder xpHolder)
    {
        OnXpValueUpdated?.Invoke(xpHolder);
    }
}