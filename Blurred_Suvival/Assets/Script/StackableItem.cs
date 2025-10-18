public static class StackableItem
{
    public static bool IsStackable(ItemType itemType)
    {
        if (itemType == ItemType.Weapon || itemType == ItemType.Vest || itemType == ItemType.Trouser || itemType == ItemType.Shoe)
        {
            return false;
        }
        return true;
    }
}
