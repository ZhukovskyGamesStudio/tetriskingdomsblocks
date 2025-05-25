public static class ResourcesUtils {
    public static  ResourceType ResourceByCellType(CellType type) {
        switch (type) {
            case CellType.Forest:
                return ResourceType.Wood;
            case CellType.Mountain:
                return ResourceType.Rocks;
        }

        return ResourceType.None;
    }
}
