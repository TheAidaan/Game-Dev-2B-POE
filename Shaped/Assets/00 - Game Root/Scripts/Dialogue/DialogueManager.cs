﻿using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class DialogueManager : MonoBehaviour
{
    const float PLAYER_TEXT_DELAY = 0.04f;

    public static DialogueManager instance; //single...

    Graph _dialogueGraph = new Graph();
    Vertex _currentDialogueVertex;
   
    DoublyLinkedList _dialogueList = new DoublyLinkedList();
    ListDialogueNode _currentDialogueNode;


    RectTransform _npcArea, _playerArea;
    TextMeshProUGUI _npcNameTxt, _DialogueTxt,_playerTxt;
    Image _npcIcon;

    DialogueChoiceManager _choices;

    DialogueAlert _alert;
    DialogueBox _dialogueBox;

    Character _currentNPC;

    bool _typing, _stoptyping,_branchedNarrative, _npcSpeaking;
    static bool _activeDialogue;
    public static bool activeDialogue { get { return _activeDialogue; } }

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
       //GameObject dialogueBox = GetComponentInChildren<Image>().gameObject;

        _dialogueBox = GetComponentInChildren<DialogueBox>();

        _npcArea = (RectTransform)_dialogueBox.transform.GetChild(0);
        _npcNameTxt = _npcArea.GetComponent<TextMeshProUGUI>();
        _npcIcon = _npcNameTxt.GetComponentInChildren<Image>();

        _playerArea = (RectTransform)_dialogueBox.transform.GetChild(1);
        _playerTxt = _playerArea.GetComponent<TextMeshProUGUI>();

        _DialogueTxt = _dialogueBox.transform.GetChild(2).GetComponent<TextMeshProUGUI>();

        _choices = GetComponentInChildren<DialogueChoiceManager>();

        _alert = GetComponentInChildren<DialogueAlert>();
        _activeDialogue = false;


    }

    // Update is called once per frame
    void Update()
    {
        if (_activeDialogue)
        {
            if (Input.GetKeyDown(KeyCode.Q))    // player can't move player character anymore so A only moves the dialogue backwards
                if (_typing)
                    _stoptyping = true;

            _alert.Hide();//player should see that they are able to choose to talk to the npc they are currently talking to

            if (Input.GetKeyDown(KeyCode.E) && !_branchedNarrative) // move the conversation forward
                    NextExchange(); 
        }
    }

    void NextExchange()           //List
    {
        if (_npcSpeaking)
        {
            PlayerResponse(); // every npc response has a player response 
        }
        else
        {
            _currentDialogueNode = _dialogueList.Next(); //npc response is always first 
            if (_currentDialogueNode != null)
                NPCResponse();
            else
                EndDialogue(); 

        }
        
        
    }


    IEnumerator RunDialogue(string NPCText, float delay)             //ALL        
    {
        _typing = true; // lets everybody know its typing

        for (int i = 0;i < NPCText.Length + 1; i++)
        {
            _DialogueTxt.text = NPCText.Substring(0,i); //add a character to the end of the text
            yield return new WaitForSeconds(delay); // waits a while

            if (_stoptyping)
            {
                _stoptyping = false; // stopped typing
                _typing = false; // stopped typing
                _DialogueTxt.text = NPCText; // show the full text that was stopped

                break;
            }
        }
        yield return new WaitForSeconds(_currentNPC.TextDelay); // waits a while

        if (_branchedNarrative)
        {
            _dialogueBox.ShowDialogueBox();
            _choices.ActivateButtons(_currentDialogueVertex.Data.Responses);
        }
        else if (_currentDialogueNode != null)
            if (_currentDialogueNode.Interupt && !_npcSpeaking)
                NextExchange();
    }
    void PlayerResponse()
    {
        _npcSpeaking = false;

        _npcArea.gameObject.SetActive(false);
        _playerArea.gameObject.SetActive(true);
        _DialogueTxt.alignment = TextAlignmentOptions.TopRight;

        StartCoroutine(RunDialogue(_currentDialogueNode.Response, PLAYER_TEXT_DELAY));       ///List
    }

    void NPCResponse()
    {
        _npcSpeaking = true;

        _playerArea.gameObject.SetActive(false);
        _npcArea.gameObject.SetActive(true);
        _DialogueTxt.alignment = TextAlignmentOptions.TopLeft;

        StartCoroutine(RunDialogue(_currentDialogueNode.NPCText, _currentNPC.TextDelay));       ///List
    }
    void EndDialogue()          //All       
    {
        _currentNPC.IsTalking = false;

        _dialogueGraph.Clear();
        _dialogueList.Clear();

        _currentDialogueVertex = null;
        _activeDialogue = false;

        _currentDialogueNode = null;

        _playerArea.gameObject.SetActive(false);  //preparing fot the next NPC text
        _npcArea.gameObject.SetActive(true);
        _DialogueTxt.alignment = TextAlignmentOptions.TopLeft;

        _dialogueBox.HideDialogueBox();

        PlayerInventory.Static_DisplayItems();
    }

    /*              PUBLIC STATICS RECEIVERS             */                                                 /*              PUBLIC STATICS RECEIVERS             */                                             /*              PUBLIC STATICS RECEIVERS             */

    
    void LoadGraph(TextAsset asset)     //graph
    {
        GraphDialogue JsonNodes = new GraphDialogue();

        JsonNodes = JsonUtility.FromJson<GraphDialogue>(asset.text); // put it into a generic list

        foreach (GraphDialogueNode node in JsonNodes.Dialogue)
        {
            instance.AddToGraph(node);
        }
        instance.ActivateDialogue(); // send the NPC name 
    }

    void AddToGraph(GraphDialogueNode node)     //graph
    {
        _dialogueGraph.AddNode(node);
    }

    void Response(int responseID)       //graph
    {
        if (!_dialogueGraph.Empty)
            if (!_currentDialogueVertex.Edges.Any())
                EndDialogue();
            else
            {
                if (_currentDialogueVertex.Edges.Count() <= responseID)
                    EndDialogue();
                else
                {
                    _currentDialogueVertex = _currentDialogueVertex.Edges.ElementAt(responseID);
                    StartCoroutine(RunDialogue(_currentDialogueVertex.Data.NPCText, _currentNPC.TextDelay));
                }
            }
    }

    public void ChangeDialogueOptionText(string message)            //All
    {
        if (message == string.Empty)
        {
            _alert.Hide(); // Hide the alert
        }
        else // activate the text and display the message
        {
            _alert.Show(message);
        }
    }
    void SetNPC(Character NPC)       //All
    {
        _currentNPC = NPC;
        _currentNPC.IsTalking = true;

        _npcNameTxt.text = _currentNPC.Name;

        Sprite npcIcon = GameManager.sprites[NPC.IconID];

        if (npcIcon != null) // is there even an asset
        {
            _npcIcon.sprite = npcIcon; // if yes the show it
        }
        else
        {
            Debug.Log("No NPC icon"); // this is why it's not working
        }
    }
    void ActivateDialogue()         //All                                
    {
        _activeDialogue = true;
        _dialogueBox.ShowDialogueBox();

        if (_branchedNarrative)
        {
            _currentDialogueVertex = _dialogueGraph.Start();
            StartCoroutine(RunDialogue(_currentDialogueVertex.Data.NPCText, _currentNPC.TextDelay));
        }
        else
        {
            _currentDialogueNode = _dialogueList.Start();
            NPCResponse();
        }

        _npcSpeaking = true;
        PlayerInventory.Static_HideAllSlots();

    }
    void LoadList(TextAsset asset)      //list
    {
        ListDialogueNodes JsonNodes = new ListDialogueNodes();

        JsonNodes = JsonUtility.FromJson<ListDialogueNodes>(asset.text); // put it into a generic list


        foreach (ListDialogueNode node in JsonNodes.Dialogue)
            instance.AddToList(node); // puts it into linked list

        instance.ActivateDialogue(); // send the NPC name 
    }
    void AddToList(ListDialogueNode node)       //list
    {
        _dialogueList.AddNode(node);
    }

    
    /*              PUBLIC STATICS             */                                                 /*              PUBLIC STATICS             */                                             /*              PUBLIC STATICS             */


    public static void LoadFile(Character NPC) //anyone can call this = anyone can speak
    {
        TextAsset asset = Resources.Load<TextAsset>("DialogueFiles/" + NPC.File); // get the text asset with the NPC file name 
        if (asset != null) //was there a text asset?
        {
            NarrativeTypeCheck  check = JsonUtility.FromJson<NarrativeTypeCheck>(asset.text); // checking if the file should be loaded into a graph or a linked list

            instance.SetNPC(NPC);
            instance._branchedNarrative = check.BranchedNarrative;

            if (check.BranchedNarrative)
                instance.LoadGraph(asset);
            else
                instance.LoadList(asset);        
        }else
        {
            Debug.Log("No json file for " + NPC.Name + " file name" + NPC.File);
        }
            
        
    }
    public static void Static_Response(int responseID)          //Graph
    {
        instance.Response(responseID);
    }

    public static void GiveDialogueOption(string name) // get the NPC name that the player might talk to 
    {
        if (name != string.Empty)
        {
            instance.ChangeDialogueOptionText("Press [SPACE] to talk to " + name); // format the message if the string has a name
        }
        else
        {
            instance.ChangeDialogueOptionText(string.Empty); // dont format the name and send an empty string if there's no npc name given
        }
    }
}
