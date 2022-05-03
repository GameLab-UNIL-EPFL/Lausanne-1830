/*
Historically accurate educational video game based in 1830s Lausanne.
Copyright (C) 2021  GameLab UNIL-EPFL

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/
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
	private Button close;
	private Button CloseNBList;
	private Sprite closeSprite;
	
	
	// NumPad nodes
	private VBoxContainer NumVC;
	//private Label InputNum;
	private LineEdit InputNum;
	
	// Used to not respawn labels in case of opening and reclosing the same attribute
	private string curAttribute;
	private const string NUM = "num";
	private const string ENFANTS = "enfants";
	
	private void HideAll() {
		bgSprite.Hide();
		NumVC.Hide();
		sC.Hide();
		vBC.Hide();
		
		//Hide all labels
		foreach(var label in labels) {
			label.Hide();
		}
		
		// Hide the parent node
		Hide();
	}
	
	private void ShowVerticalNameList() {
		NumVC.Hide();
		sC.Show();
		vBC.Show();
		
		closeSprite.Frame = 0;
		
		List<string> shownOptions = new List<string>();
		
		//Show all labels
		foreach(var label in labels) {
			if(!shownOptions.Contains(label.Text)) {
				label.Show();
				shownOptions.Add(label.Text);
			}
		}
	}
	
	private void ShowNumpad() {
		// Show the buttons
		NumVC.Show();
		sC.Hide();
		
		// Don't forget to reset the text
		InputNum.Text = "";
	}
	
	private void ShowAll() {
		bgSprite.Show();
		
		closeSprite.Frame = 0;
		
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
		).Count() + (
			from attr in characterAttributes.Root.Descendants("solution")
			select attr
		).Count();
	}
	
	// Propagate the request to update the notebook info
	private void _on_UpdateNotebookInfo(string newVal) {
		EmitSignal(nameof(UpdateInfo), curAttribute, newVal);
		_on_Close_button_up();
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
		if(labels.Count() != options.Length) {
			throw new Exception("Labels-Attributes size missmatch! n_Labels = " +
						 labels.Count() + ", n_options = " + options.Length);
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
		close = GetNode<Button>("Close");
		closeSprite = GetNode<Sprite>("Close/CloseSprite");
		CloseNBList = GetNode<Button>("CloseNBList");
		
		//Connect background button
		CloseNBList.Connect("pressed", this, "_on_Close_button_up");
		
		// Fetch numpad nodes
		NumVC = GetNode<VBoxContainer>("BgSprite/NumberVC");
		//InputNum = GetNode<Label>("BgSprite/NumberVC/InputNumber");
		InputNum = GetNode<LineEdit>("BgSprite/NumberVC/LineEdit");
		
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
		// Check if the numpad should be show instead of the list
		if(attributeName == NUM || attributeName == ENFANTS) {
			curAttribute = attributeName;
			ShowNumpad();
		} else {
			// Make sure to not respawn labels for nothing
			if(curAttribute == attributeName) {
				ShowVerticalNameList();
			} else {
				// Get all options for a given attribute
				string[] options = QueryCharacterAttribute(attributeName);
				
				// Fill and show the labels
				FillLabels(options);
				curAttribute = attributeName;
				ShowVerticalNameList();
			}
		}
		ShowAll();
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
					
		var querySolution = from solution in characterAttributes.Root.Descendants("solution")
							select solution.Attribute(attributeName).Value;
					
		List<string> res = new List<string>();
		
		//Painfully convert the IEnumeration to a usable list
		//This step is necessary, since one can't obtain the size of an 
		//IEnumeration without iterating through it #lazyevaluation
		foreach(var elem in query) {
			res.Add(elem);
		}
		foreach(var e in querySolution) {
			res.Add(e);
		}
		
		// Cache result for future use
		string[] result = res.ToArray();
		string[] finalRes = result.OrderBy(x => x).ToArray();
		attributesCache.Add(attributeName, finalRes);
		
		return finalRes;
	}
	
	private void _on_Close_button_down() {
		closeSprite.Frame = 1;
	}
	
	private void _on_Close_button_up() {
		closeSprite.Frame = 0;
		HideAll();
	}
	
	private void _on_InsertNumber(int num) {
		if(InputNum.Text.Length < 3) {
			InputNum.Text += num.ToString();
		}
	}
	
	private void _on_RemoveNumber() {
		if(InputNum.Text.Length != 0)  {
			InputNum.Text = InputNum.Text.Substring(0, InputNum.Text.Length - 1);
		}
	}
	
	private void _on_EnterNumber() {
		if(InputNum.Text.Length != 0) {
			EmitSignal(nameof(UpdateInfo), curAttribute, InputNum.Text);
			_on_Close_button_up();
		}
	}
	private void _on_LineEdit_text_entered(String new_text) {
		_on_EnterNumber();
	}
}
