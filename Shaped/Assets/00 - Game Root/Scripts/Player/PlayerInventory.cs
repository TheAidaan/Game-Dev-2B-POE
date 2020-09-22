﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInventory : MonoBehaviour
{
    HashTable _inventory = new HashTable(); //the player inventory

    GameObject[] _itemSlotsUI = new GameObject[6];//to hold all six itemSlots

    static PlayerInventory instance;//single...
    private void Awake()
    {
        instance = this; //..ton
    }
    void Start()
    { 
        for (int i = 0; i < 6; i++)
        {
            _itemSlotsUI[i] = transform.GetChild(i).gameObject; // gather all item slots
        }

        _inventory.Add(InventoryItems.Circle);

        DisplayItems();
    }

    void DisplayItems()
    {
        HashData[] displayItems = _inventory.Display(); // length should always be the amount of items in the hashtable

        int x = 0; // to present the items from top to bottom

        for(int i=0;i< displayItems.Length; i++)
        {
            ItemSlot itemSlot = _itemSlotsUI[x].GetComponent<ItemSlot>(); 

            if (itemSlot != null)
            {
                if (displayItems[i] != null) // only show the stored items in the array, fuck the nulls
                {
                    itemSlot.SetItem(displayItems[i].data); // show the item on screen
                    x++; //only if item is added, can the next itemslot be used
                }
                
            }
            else
            {
                Debug.Log("Item Slot " + i + " doesn't have an ItemSlot.sc attached"); // just in case
            }
        }

        HideUnusedSlots(x);
    }

    void HideUnusedSlots(int index)
    {
        if (index< _itemSlotsUI.Length)// if all slots are filled then there are not unused slots to hide
        {
            while (index < _itemSlotsUI.Length)
            {
                _itemSlotsUI[index].SetActive(false);
                index++;
            }
        }
        

    }

    /*              PUBLIC STATICS              */

    public static void Add(InventoryItem item) // anything can add an item to the inventory
    {
        bool added =  instance._inventory.Add(item); // the add, might be successful, might not 

        if (added)
        {
            instance.DisplayItems();
        }else
        {
            Debug.Log("Item not added");
        }
    }
}
