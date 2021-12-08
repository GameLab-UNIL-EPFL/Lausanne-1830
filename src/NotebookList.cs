using Godot;
using System;
using System.Xml;
using System.Xml.Linq;
using System.Linq;
using System.Collections.Generic;

public class NotebookList : Node2D {
	[Signal]
	public delegate void UpdateInfo(string attribute, string newVal);
	
	[Export]
	public string DBFilePath;
	
	// Used to cache xml lookup results, in order to speed up second lookups
	private Dictionary<string, string[]> attributesCache;
	
	// Local XDocument containing a parsed version of the dialogue
	private XDocument characterAttributes;
	
	// Children Nodes
	private Sprite bgSprite;
	private ScrollContainer sC;
	private VBoxContainer vBC;
	private List<InfoChoiceButton> labels;
	
	// Used to not respawn labels in case of opening and reclosing the same attribute
	private string curAttribute;
	
	private void HideAll() {
		bgSprite.Hide();
		sC.Hide();
		vBC.Hide();
		
		//Hide all labels
		foreach(var label in labels) {
			label.Hide();
		}
		
		// Hide the parent node
		Hide();
	}
	
	private void ShowAll() {
		bgSprite.Show();
		sC.Show();
		vBC.Show();
		
		//Show all labels
		foreach(var label in labels) {
			label.Show();
		}
		
		// Show the parent node
		Show();
	}
	
	/**
	 * @brief Counts the number of characters in the xml file
	 */
	private int CountCharacters() {
		return (
			from attr in characterAttributes.Root.Descendants("personnage")
			select attr
		).Count();
	}
	
	// Propagate the request to update the notebook info
	private void _on_UpdateNotebookInfo(string newVal) {
		EmitSignal(nameof(UpdateInfo), curAttribute, newVal);
	}
	
	/**
	 * @brief Spawns the same amount of labels as there are characters in the xml.
	 */
	private void SpawnLabels() {
		int nLabels = CountCharacters();
		
		// Spawn a new text label for each character
		for(int i = 0; i < nLabels; ++i) {
			InfoChoiceButton txt = new InfoChoiceButton();
			txt.Connect("UpdateNotebookInfo", this, "_on_UpdateNotebookInfo");
			vBC.AddChild(txt);
			labels.Add(txt);
		}
	}
	
	/**
	 * @brief Fills the labels with the contents of the given options array
	 * @param options, the strings that will populate the labels
	 */
	private void FillLabels(string[] options) {
		// Sanity check
		if(labels.Count() !=  options.Length) {
			throw new Exception("Labels-Attributes size missmatch!");
		} else {
			for(int i = 0; i < options.Length; ++i) {
				labels[i].Text = options[i];
			}
		}
	}
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		// Sanity Check
		if(DBFilePath == null) {
			throw new Exception("DB file path must be set !!");
		}
		
		// Parse the XML file and store result in characterAttributes
		DialogueController._ParseXML(ref characterAttributes, DBFilePath);
		attributesCache = new Dictionary<string, string[]>();
		
		// Fetch children nodes
		bgSprite = GetNode<Sprite>("BgSprite");
		sC = GetNode<ScrollContainer>("BgSprite/ScrollContainer");
		vBC = GetNode<VBoxContainer>("BgSprite/ScrollContainer/AttributeList");
		
		// Spawn a label for each character
		labels = new List<InfoChoiceButton>();
		SpawnLabels();
		
		// Initially these elements should be hidden
		HideAll();
	}
	
	/**
	 * @brief Request to open all of options for a given attribute
	 * @param attributeName, the attribute for which we want all options
	 */
	public void _on_OpenOptions(string attributeName) {
		// Make sure to not respawn labels for nothing
		if(curAttribute == attributeName) {
			ShowAll();
		} else {
			// Get all options for a given attribute
			string[] options = QueryCharacterAttribute(attributeName);
			
			// Fill and show the labels
			FillLabels(options);
			curAttribute = attributeName;
			ShowAll();
		}
	}
	
	/**
	 * @brief Queries the local XDocument for all values of a given attribute
	 * @param attributeName, the attribute, e.g. `prenom` being queried
	 * @return a string array containing all of the values requested
	 */
	private string[] QueryCharacterAttribute(string attributeName) {
		// Check for cached result
		if(attributesCache.ContainsKey(attributeName)) {
			return attributesCache[attributeName];
		}
		
		// Query the data and write out resulting texts as a string array
		var query = from personnage in characterAttributes.Root.Descendants("personnage")
					select personnage.Attribute(attributeName).Value;
					
		List<string> res = new List<string>();
		
		//Painfully convert the IEnumeration to a usable list
		//This step is necessary, since one can't obtain the size of an 
		//IEnumeration without iterating through it #lazyevaluation
		foreach(var elem in query) {
			res.Add(elem);
		}
		// Cache result for future use
		string[] finalRes = res.ToArray();
		attributesCache.Add(attributeName, finalRes);
		
		return finalRes;
	}
	
	/**
	 * @brief Reaction to the close button being pressed. Should hide all elements.
	 */
	private void _on_Close_pressed() {
		HideAll();
	}
}

