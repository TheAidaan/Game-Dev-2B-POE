﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;

class Node //for doubly
{
    public ListDialogueNode data; // found in DialogueManager.cs
    public Node previous;
    public Node next;
    public Node(ListDialogueNode data)
    {
        this.data = data;
    }
}
public class DoublyLinkedList  // can add, clear, return data both ways
{
    Node _head; // the start of the list

    Node _temp; // random point

    Node _pointer; // the working point 

    Node _foot;

    public void AddNode(ListDialogueNode data)//adds to the end
    {
        Node Node = new Node(data);

        if (_head == null) //is there a list
        {
            _head = Node; //make a list that starts with _head
            _head.next = null;
            _head.previous = null;
        }
        else
        {
            _pointer = _head; //currently working with _head

            while (_pointer.next != null)
            {
                _pointer = _pointer.next; //current is actually last
            }

            _pointer.next = Node;// tell the last node whats next
            Node.previous = _pointer;// tell the new node what's behind
            Node.next = null;//emphasize that the new data is at the end

            _foot = Node;
        }

        _pointer = _head; //point to _head when adding(for now(maybe))
    }
    public void Clear()//kills 'em all
    {
        _temp = _head;
        while (_temp != null)// cant clear whats already clear
        {
            _pointer = _temp.next; // point to the next node

            _temp.previous = _temp.next = null; // delete delete nodes
            _temp.data = null; //delete data

            _temp = _pointer;
        }
        _head = null; // makes it offical 
        _pointer = null;

    }
    public ListDialogueNode Start() //the function that starts it all
    {
        return _head.data;
    }

    public ListDialogueNode Next()//moves along
    {
        if (_pointer != null) // is there evens a list
        {
            if (_pointer.next != null)  // if i have data to send then there could be data following this...
            {
                _pointer = _pointer.next;  // ... so point to the next node
            }
            else
            {
                return null; //if the pointer gave a null value then the list is done and the node sent was the last node in the value  
            }

            _temp = _pointer;
            return _temp.data; //here's what you asked for           
        }
        else
        {
            return null;//it's pointing to the end or the list is clear
        }
    }
    public ListDialogueNode Previous()//comes back
    {
        if (_pointer != null) // is the player at the end of the list?
        {
            if (_pointer != _head) // if you're not pointing to the head
            {
                if (_pointer.previous != null) // if you're not at the beginning..
                {
                    _pointer = _pointer.previous; //...move backwards
                }

                _temp = _pointer;

                return _temp.data; //here's what you asked for   
            }
            else
            {
                return null; // if you're pointing to the end then send the head and don't do anything else
            }

        }
        else
        {
            _pointer = _foot.previous; //point to the second last entry
            _temp = _pointer;
            _pointer = _temp.previous;
            return _temp.data;//return the last entry
        }
    }
}
