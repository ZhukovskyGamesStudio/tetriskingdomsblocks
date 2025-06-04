public static class ResourcesUtils {
    //
    public static  ResourceType ResourceByCellType(CellType type) {
        switch (type) {
            case CellType.Forest:
                return ResourceType.Wood;
            case CellType.Mountain:
                return ResourceType.Rocks;
            case CellType.Village:
                return ResourceType.Food;
        }

        return ResourceType.None;
    }
}
