using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryManager : MonoBehaviour
{   
    [SerializeField] private GameObject itemCursor;

    [SerializeField] private GameObject slotHolder;
    [SerializeField] private GameObject hotbarSlotHolder;
    [SerializeField] private ItemClass itemToAdd;
    [SerializeField] private ItemClass itemToRemove;

    [SerializeField] private SlotClass[] startingItems;

    private SlotClass[] items;

    private GameObject[] slots;
    private GameObject[] hotbarSlots;

    private SlotClass movingSlot;
    private SlotClass tempSlot;
    private SlotClass originalSlot;

    public ItemClass primaryItem;
    public ItemClass secondaryItem;
    public ItemClass consumableItem;
    public ItemClass armorItem;

    public PlayerInventoryController playerController;

    bool isMovingItem;

    private void Start(){
        slots = new GameObject[slotHolder.transform.childCount];
        items = new SlotClass[slots.Length];

        hotbarSlots = new GameObject[hotbarSlotHolder.transform.childCount];

        for (int i = 0; i < hotbarSlots.Length; i++){
            hotbarSlots[i] = hotbarSlotHolder.transform.GetChild(i).gameObject;
        }

        //initialize slots
        for (int i = 0; i < items.Length; i++){
            items[i] = new SlotClass();
        }

        for (int i = 0; i < startingItems.Length; i++){
            items[i] = startingItems[i];
        }

        //set all the slots
        for (int i = 0; i < slotHolder.transform.childCount; i++){
            slots[i] = slotHolder.transform.GetChild(i).gameObject;
        }
        Add(itemToAdd, 1);
        Remove(itemToRemove);
        RefreshUI();
    }

    private void Update() {

        itemCursor.SetActive(isMovingItem);
        itemCursor.transform.position = Input.mousePosition;
        if (isMovingItem)
            itemCursor.GetComponent<Image>().sprite = movingSlot.GetItem().itemIcon;

        if (Input.GetMouseButtonDown(0)){ //left clicked
            //find the closest slot (the slot we clicked on)
            if (isMovingItem) {
                EndItemMove();
            }
                //end item move
            else BeginItemMove();

            primaryItem = items[slots.Length - hotbarSlots.Length].GetItem();
            secondaryItem = items[1 + slots.Length - hotbarSlots.Length].GetItem();
            consumableItem = items[2 + slots.Length - hotbarSlots.Length].GetItem();
            armorItem = items[3 + slots.Length - hotbarSlots.Length].GetItem();

        } else if (Input.GetMouseButtonDown(1)){ //right clicked
            //find the closest slot (the slot we clicked on)
            if (isMovingItem) {
                EndItemMove_Single();
            }
                //end item move
            else {
                BeginItemMove_Half();
            }
        } 


        // //VAIHA PRIMARY ITEMIKS JA JOKASELLE ITEMILLE OMA SLOTTI
        // //OPTIMOI JOS MAHOLLISTA
        // primaryItem = items[slots.Length - hotbarSlots.Length].GetItem();
        // secondaryItem = items[1 + slots.Length - hotbarSlots.Length].GetItem();
        // consumableItem = items[2 + slots.Length - hotbarSlots.Length].GetItem();
        // armorItem = items[3 + slots.Length - hotbarSlots.Length].GetItem();
    }

    #region Inventory Utils
    public void RefreshUI(){
        for (int i = 0; i < slots.Length; i++)
        {
            try {
                //Jos on esine
                slots[i].transform.GetChild(0).GetComponent<Image>().enabled = true;
                slots[i].transform.GetChild(0).GetComponent<Image>().sprite = items[i].GetItem().itemIcon;
                
                if (items[i].GetItem().isStackable)
                    slots[i].transform.GetChild(1).GetComponent<TMP_Text>().text = items[i].GetQuantity() + "";
                else slots[i].transform.GetChild(1).GetComponent<TMP_Text>().text = "";
            } catch {
                //Jos ei esinettä
                slots[i].transform.GetChild(0).GetComponent<Image>().sprite = null;
                slots[i].transform.GetChild(0).GetComponent<Image>().enabled = false;
                slots[i].transform.GetChild(1).GetComponent<TMP_Text>().text = "";
            }
            
        }

        RefreshHotbar();
    }

    public void RefreshHotbar(){
        for (int i = 0; i < hotbarSlots.Length; i++){
            try {
                //Jos on esine
                hotbarSlots[i].transform.GetChild(0).GetComponent<Image>().enabled = true;
                hotbarSlots[i].transform.GetChild(0).GetComponent<Image>().sprite = items[i + slots.Length - hotbarSlots.Length].GetItem().itemIcon;
                    
                if (items[i + slots.Length - hotbarSlots.Length].GetItem().isStackable)
                    hotbarSlots[i].transform.GetChild(1).GetComponent<TMP_Text>().text = items[i + slots.Length - hotbarSlots.Length].GetQuantity() + "";
                else
                    hotbarSlots[i].transform.GetChild(1).GetComponent<TMP_Text>().text = "";
            } catch {
                //Jos ei esinettä
                hotbarSlots[i].transform.GetChild(0).GetComponent<Image>().sprite = null;
                hotbarSlots[i].transform.GetChild(0).GetComponent<Image>().enabled = false;
                hotbarSlots[i].transform.GetChild(1).GetComponent<TMP_Text>().text = "";
            }
        }
    }

    public bool Add(ItemClass item, int quantity){
        //items.Add(item);
        //check if inventory contains item
        SlotClass slot = Contains(item);
        if (slot != null && slot.GetItem().isStackable){
            slot.AddQuantity(quantity);
        } 
        else{
            for (int i = 0; i < items.Length; i++){
                if (items[i].GetItem() == null) // this is an empty slot
                {
                    items[i].AddItem(item, quantity);
                    break;
                }
            }
            // if (items.Count < slots.Length)
            //     items.Add(new SlotClass(item, 1));
            // else
            //     return false;
        }
        RefreshUI();
        return true;
    }

    public bool Remove(ItemClass item){
        //items.Remove(item);
        SlotClass temp = Contains(item);
        if (temp != null){
            if (temp.GetQuantity() >= 1){
                temp.SubQuantity(1);
            } else {

                int slotToRemoveIndex = 0;

                for (int i = 0; i < items.Length; i++){
                    if (items[i].GetItem() == item){
                        slotToRemoveIndex = i;
                        break;
                    }
                }

                items[slotToRemoveIndex].Clear();
            }
            
        } 
        else{
            return false;
        }

        RefreshUI();
        return true;
    }

    public void Consume(){
        items[2 + slots.Length - hotbarSlots.Length].SubQuantity(1);
        RefreshUI();
    }

    public SlotClass Contains(ItemClass item){
        for (int i = 0; i < items.Length; i++){
            if(items[i].GetItem() == item)
                return items[i];
        }

        return null;
    }
    #endregion Inventory Utils

    #region Moving Stuff

    private bool BeginItemMove(){
        originalSlot = GetClosestSlot();
        if (originalSlot == null || originalSlot.GetItem() == null){
            return false; //No item to move
        }

        movingSlot = new SlotClass(originalSlot);
        originalSlot.Clear();
        isMovingItem = true;
        RefreshUI();
        return true;
    }

    private bool BeginItemMove_Half(){
        originalSlot = GetClosestSlot();
        if (originalSlot == null || originalSlot.GetItem() == null){
            return false; //No item to move
        }

        movingSlot = new SlotClass(originalSlot.GetItem(), Mathf.CeilToInt(originalSlot.GetQuantity() / 2f));
        originalSlot.SubQuantity(Mathf.CeilToInt(originalSlot.GetQuantity() / 2f));
        if (originalSlot.GetQuantity() == 0){
            originalSlot.Clear();
        }
        isMovingItem = true;
        RefreshUI();
        return true;
    }

    private bool EndItemMove() {
        originalSlot = GetClosestSlot();

        if (originalSlot == null) {
            Add(movingSlot.GetItem(), movingSlot.GetQuantity());
            movingSlot.Clear();

        }
        else {

            if (originalSlot.GetItem() != null)
            {
                if (originalSlot.GetItem() == movingSlot.GetItem()){ //same item
                    if (originalSlot.GetItem().isStackable){
                        originalSlot.AddQuantity(movingSlot.GetQuantity());
                        movingSlot.Clear();
                    }
                    else
                        return false;
                } else {
                    tempSlot = new SlotClass(originalSlot);//a = b
                    originalSlot.AddItem(movingSlot.GetItem(), movingSlot.GetQuantity());//b = c
                    movingSlot.AddItem(tempSlot.GetItem(), tempSlot.GetQuantity());//a = c
                    RefreshUI();
                    return true;
                }
            } else { //place item as usual
                originalSlot.AddItem(movingSlot.GetItem(), movingSlot.GetQuantity());
                movingSlot.Clear();
            }
        }

        isMovingItem = false;
        RefreshUI();
        return true;
    }

    private bool EndItemMove_Single() {
        originalSlot = GetClosestSlot();

        if (originalSlot == null || movingSlot.GetItem().isStackable == false){
            return false; //No item to move
        }
        
        if (originalSlot.GetItem() != null && originalSlot.GetItem() != movingSlot.GetItem()) {
            return false;
        }

        movingSlot.SubQuantity(1);

        if (originalSlot.GetItem() != null && originalSlot.GetItem() == movingSlot.GetItem()) {
            originalSlot.AddQuantity(1);
        } else {
            originalSlot.AddItem(movingSlot.GetItem(), 1);
        }

        if (movingSlot.GetQuantity() < 1){
            isMovingItem = false;
            movingSlot.Clear();
        } else {
            isMovingItem = true;
        }
        RefreshUI();
        return true;
    }

    private SlotClass GetClosestSlot(){
        for (int i = 0; i < slots.Length; i++){
            if (Vector2.Distance(slots[i].transform.position, Input.mousePosition) <= 32){
                return items[i];
            }
        }
        return null;
    }

    #endregion Moving Stuff
}
