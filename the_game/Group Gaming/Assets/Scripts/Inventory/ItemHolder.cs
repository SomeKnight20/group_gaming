using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemHolderScript : MonoBehaviour
{
    public ItemClass itemiData;

    public int itemiAmount;

    public GameObject teksticanvas;

    private bool collectable = false;

    // public ItemClass[] allItems;

    public SpriteRenderer spriteRenderer;

    public InventoryManager inventory;

    private void Start() {
        // inventory.Add(itemiData, 1);

        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();

        inventory = GameObject.Find("InventoryManager").GetComponent<InventoryManager>();

        // itemiData = allItems[Random.Range(0, allItems.Length)];

        spriteRenderer.sprite = itemiData.itemIcon;
    }

    // private void OnCollisionEnter2D(Collision2D other) {
    //     Debug.Log(other.gameObject.tag);
    //     if (other.gameObject.tag == "Player")
    //     {
    //         inventory.Add(itemiData, 1);
    //         Destroy(gameObject);
    //     }
    // }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (collectable)
            {
                inventory.Add(itemiData, itemiAmount);
                Destroy(gameObject);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.tag == "Player")
        {
            teksticanvas.SetActive(true);
            collectable = true;
            // inventory.Add(itemiData, 1);
            // Destroy(gameObject);
        }
    }

    private void OnTriggerExit2D(Collider2D other) {
        if (other.gameObject.tag == "Player")
        {
            teksticanvas.SetActive(false);
            collectable = false;
        }
    }
}