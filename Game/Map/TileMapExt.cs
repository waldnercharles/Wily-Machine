using System.Collections.Generic;
using Godot;

namespace WilyMachine;

public static class TileMapExt
{
    public static IEnumerable<Vector2I> GetCellsWithCustomData(this TileMap tileMap, string layerName, Variant value)
    {
        for (var layer = 0; layer < tileMap.GetLayersCount(); layer++)
        {
            foreach (var cell in tileMap.GetUsedCells(layer))
            {
                var cellData = tileMap.GetCellTileData(layer, cell);
                var customData = cellData?.GetCustomData(layerName);

                if (customData.HasValue && customData.Value.VariantEquals(value))
                {
                    yield return cell;
                }
            }
        }
    }
}
