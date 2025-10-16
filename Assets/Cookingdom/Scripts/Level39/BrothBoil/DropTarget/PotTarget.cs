using UnityEngine;

public class PotTarget : DropTargetBase
{
    public override bool CanAccept(DraggableItem item)
    {
        var go = item.gameObject;
        var c = ClayPotController.Instance;
        var a = this.GetComponent<DraggableItem>().lastSnappedTarget;
        if (a == null) 
            return false;
        else
        {
            if (a.gameObject.name != "Gas Stove") 
                return false;
        }
        if (!c.ClayPot.HasWater) return false;
        if (item.name == "Chicken") return item.GetComponent<Chicken>().canCook;// phải có nước mới được thả
        return c.CanAcceptIngredient(go);
        
    }
    public override void OnItemDropped(DraggableItem item)
    {
        var c = ClayPotController.Instance;
        // 1) chỉ add (nếu hợp lệ)
        c.AddIngredientStep1(item.gameObject);
        // 2) xử lý hậu kỳ rơi vào nồi (chìm + rung nếu đang bật lửa)
        c.OnIngredientDropped(item.gameObject);
    }
}
