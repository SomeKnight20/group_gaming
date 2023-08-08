using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SlotClass
{
    [SerializeField] private ItemClass item;
    [SerializeField] private int quantity;


    public SlotClass (){
        item = null;
        quantity = 0;
    }

    public SlotClass(SlotClass slot){
        item = slot.item;
        quantity = slot.quantity;
    }

    public SlotClass (ItemClass _item, int _quantity){
        item = _item;
        quantity = _quantity;
    }

    public void Clear(){
        this.item = null;
        this.quantity = 0;
    }

    public ItemClass GetItem() {
        return item;
    }

    public int GetQuantity(){
        return quantity;
    }

    public void AddQuantity(int _quantity){
        quantity += _quantity;
    }

    public void SubQuantity(int _quantity) {
        quantity -= _quantity;
        if (quantity <= 0){
            //quantity = 0
            Clear();
        }
    }

    public void AddItem(ItemClass item, int quantity){
        this.item = item;
        this.quantity = quantity;
    }
}
