using Godot;
using System;
using System.Xml;
using System.Xml.Linq;
using System.Linq;
using System.Collections.Generic;

public struct Text_t {
	bool hasOptions;
	string[] options;
};

public struct Dialogue_t {
	Text_t[] texts;
	string type; //onDemand or onApproach
	string target1; //Name of person talking
	
	public Dialogue_t(Text_t[] t, string tpe, string tgt1) {
		texts = t;
		type = tpe;
		target1 = tgt1;
	}
};

public class DialogueController : Node {
	
	//File at which the scene's dialogue is stored
	[Export]
	public string SceneDialogueFile = "res://assets/04_dialogues/01_Palud/testDialogue.xml";
	
	//Local XDocument containing a parsed version of the dialogue
	private XDocument dialogueTree;
	
	private XMLParser xmlp = new XMLParser();
	
	//Used to lock the Dialogue controller during an interaction
	private bool IsOccupied
	{get; set;}
	
	//Used to store the texts of each target
	private Queue<String> target0Text;
	private Queue<String> target1Text;
	
	public const string ON_DEMAND = "onDemand";
	public const string ON_APPROACH = "onApproach";
	
	/**
	 * @brief Parses the XML file and loads it into a local XDocument
	 */
	public static void _ParseXML(ref XDocument targetXML, string filePath) {
		if(filePath == null) {
			throw new Exception("No xml file was input for the scene!");
		}
		
		//Load XML file into a XDocument for querying
		var xml = XDocument.Load(filePath);
		
		//Sanity check
		if(xml != null) {
			targetXML = xml;
		} else {
			throw new Exception("Unable to load xml file!");
		}
	}
	
	public static void _GDParseXML(ref XMLParser xmlp, string filePath) {
		if(filePath == null) {
			throw new Exception("No xml file was input for the scene!");
		}
		
		var xml = xmlp.Open(filePath);
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		IsOccupied = false;
		_ParseXML(ref dialogueTree, SceneDialogueFile);
	}
	
	/**
	 * @brief Queries the local XDocument for a given dialogue
	 * @param dialogueID, the id of the dialogue being queried
	 * @return a string array containing all of the lines of the dialogue
	 */
	private string[] QueryDialogue(string dialogueID, string type, int tagetNum = 0) {
		// Query the data and write out resulting texts as a string array
		var query = from dialogue in dialogueTree.Root.Descendants("dialogue")
					where dialogue.Attribute("id").Value == dialogueID
					select dialogue.Elements("text");
					
		List<string> res = new List<string>();
		
		//Check for options
		//if(query.Elements("option").isEmpty()) {
			//Map the queried XElements to their respective values
			var newList = query.Select(x => x.Select(y => y.Value));
			
			//Painfully convert the IEnumeration to a usable list
			//This step is necessary, since one can't obtain the size of an 
			//IEnumeration without iterating through it #lazyevaluation
			foreach(var elem in newList) {
				foreach(var text in elem) {
					res.Add(text);
				}
			}
		//}
		
		return res.ToArray();
	}
	
	private void FillQueue(string dialogueID, int id = 0, bool isApproach = false) {
		//Fetch initial dialogues and fill queues
		string[] texts = QueryDialogue(
				dialogueID, 
				isApproach ? ON_APPROACH : ON_DEMAND,
				id == 0 ? 0 : 1 //Make sure its only 0 or 1
		);
		for(int i = 0; i < texts.Length ; ++i) {
			if(id == 0) {
				target0Text.Enqueue(texts[i]);
			} else {
				target1Text.Enqueue(texts[i]);
			}
		}
	}
	
	/**
	 * @brief Starts the given dialogue and returns the first text
	 * @param dialogueId, the id of the dialogue that must be started 
	 * @returns the first text of the dialogue
	 */
	public string _StartDialogue(string dialogueId, bool isApproach = false, bool dualTargets = false) {
		//Check for lock
		if(IsOccupied) {
			return null;  
		} else {
			//Clear the queues
			target0Text.Clear();
			target1Text.Clear();
			
			//Grab lock if not onApproach
			if(!isApproach) {
				IsOccupied = true;
			}
			
			//Fetch initial dialogues and fill queues
			FillQueue(dialogueId, 0, isApproach);
			
			//Check for second target
			if(dualTargets) {
				FillQueue(dialogueId, 1, isApproach);
			}
			
			//Target0 always starts the conversation
			return target0Text.Dequeue();
		}
	}
	
	/**
	 * @brief Fetches the next text in the dialogue for the given npc
	 * @param tragetId, the id of the npc requesting the text
	 * @returns the text required to continue the dialogue
	 */
	public string _NextDialogue(int targetId) {
		var Q = targetId == 0 ? ref target0Text : ref target1Text;
		try {
			return Q.Dequeue();
		} catch(InvalidOperationException e) {
			if(targetId == 0) _EndDialogue();
			return null;
		}
	}  
	
	/**
	 * @brief Ends the currently playing dialogue
	 */
	public void _EndDialogue() {
		IsOccupied = false;
		
		//Clear the queues
		target0Text.Clear();
		target1Text.Clear();
	}
}
