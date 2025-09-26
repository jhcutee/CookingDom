using UnityEngine;
[System.Flags]
public enum ItemTag
{
    None = 0,
    Ingredient = 1 << 0,
    Tool = 1 << 1,
    Plug = 1 << 2,
    Sliceable = 1 << 4,
    Boilable = 1 << 5,
    Washable = 1 << 6,
}
public class ItemBase : MonoBehaviour
{
    [SerializeField] private ItemTag tags;
    public ItemTag Tags => tags;

    public bool HasTag(ItemTag tag) => (tags & tag) != 0;

    public virtual void OnDocked(DropTargetBase dropTargetBase) { }
    public virtual void OnUndocked(DropTargetBase dropTargetBase) { }
}
