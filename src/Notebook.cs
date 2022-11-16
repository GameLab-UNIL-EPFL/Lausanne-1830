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
using System.Diagnostics;
using System.Xml;
using System.Xml.Linq;
using System.Linq;
using System.Collections.Generic;

public class Notebook : Node2D {
	[Signal]
	public delegate void CloseNotebook();

	[Signal]
	public delegate void PageComplete();

	private List<NotebookInfo> tempInfo = new List<NotebookInfo>();
	private List<NotebookInfo> info = new List<NotebookInfo>();
	private List<Label> infoStatic = new List<Label>();
	private string[] infoNames = {"Prenom", "Nom", "Adresse", "Num", "Conjoint", "Enfants", "Metier"};
	
	private List<Sprite> tabSprites = new List<Sprite>();
	private List<Button> tabButtons = new List<Button>(); 
	private List<Sprite> Portraits = new List<Sprite>();
	
	private Button closeNB;
	private Label closeLabel;
	private Node2D NBL;
	
	private bool hidden = true;
	private bool mapOpen = false;
	private AudioStreamPlayer ASP;
	private Sprite Stamp;
	private AnimationPlayer AP;
	private AnimationPlayer AP2;
	
	private Node2D M;
	
	private Context context;
	private Player p;
	
	private string infoFilePath;
	private XDocument InfoXML;
	private RichTextLabel Quest;
	
	//Currently opened tab
	public CharacterInfo_t characterInfo = new CharacterInfo_t(-1);
	public InfoValue_t correctInfo = new InfoValue_t(false);
	
	private int curTabId = 0;
	
	public bool _IsMapOpen() {
		return mapOpen;
	}
	
	public bool _IsNotebookOpen() {
		return !hidden && !mapOpen;
	}
	
	public bool _TutoPageIsComplete() {
		foreach(Label info in infoStatic) {
			if(info.Text.Equals("Elisabeth")) {
				return true;
			}
		}
		return false;
	}
	
	private void FillNotebook(CharacterInfo_t cI) {
		foreach(var inf in info) {
			switch(inf.AttributeName) {
				case "prenom":
					inf.Text = cI.prenom; 
					break;
				case "nom":
					inf.Text = cI.nom;
					break;
				case "adresse":
					inf.Text = cI.adresse;
					break;
				case "num":
					inf.Text = cI.num.ToString();
					break;
				case "conjoint":
					inf.Text = cI.conjoint;
					break;
				case "enfants":
					inf.Text = cI.enfants.ToString();
					break;
				case "metier":
					inf.Text = cI.metier;
					break;
				default:
					break;
			}
		}
	}
	
	private void FillCharInfo() {
		foreach(var inf in info) {
			switch(inf.AttributeName) {
				case "prenom":
					characterInfo.prenom = inf.Text;
					break;
				case "nom":
					characterInfo.nom = inf.Text;
					break;
				case "adresse":
					characterInfo.adresse = inf.Text;
					break;
				case "num":
					//Make sure the entry is valid
					try {
						characterInfo.num = Int32.Parse(inf.Text == "" ? "-1" : inf.Text);
					} catch {
						//When a non-char is entered, display the default INVALID number
						characterInfo.num = -1;
						inf.Text = "";
					}
					break;
				case "conjoint":
					characterInfo.conjoint = inf.Text;
					break;
				case "enfants":
					//Make sure only valid entries are used
					try {
						characterInfo.enfants = Int32.Parse(inf.Text == "" ? "4" : inf.Text);
					} catch {
						//Otherwise default to -1
						characterInfo.enfants = -1;
						inf.Text = "";
					}
					break;
				case "metier":
					characterInfo.metier = inf.Text;
					break;
				default:
					break;
			}
		}
	}
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		Hide();
		context = GetNode<Context>("/root/Context");
		p = GetNode<Player>("../YSort/Player");
		ASP = GetNode<AudioStreamPlayer>("../NotebookClick");
		M = GetNode<Node2D>("Map");
		AP = GetNode<AnimationPlayer>("AnimationPlayer");
		AP2 = GetNode<AnimationPlayer>("AnimationPlayer2");
		Stamp = GetNode<Sprite>("Stamp");
		closeNB = GetNode<Button>("ColorRect/CloseNotebook");
		closeLabel = GetNode<Label>("Fermer");
		Quest = GetNode<RichTextLabel>("Quest");
		NBL = GetNode<Node2D>("NotebookList");
		infoFilePath = "res://db/" + context._GetLanguageAbbrv() + "/characters/infoCharacters.xml";
		
		//Load in character info XML
		DialogueController._ParseXML(ref InfoXML, infoFilePath);
		
		//Make sure that the context has the questNPC
		context._FetchQuestNPC();
		curTabId = context._GetCurrentTab();
		for(int i = 0; i < Context.N_TABS; ++i) {
			Sprite portrait = GetNode<Sprite>("Portrait" + i);
			Portraits.Add(portrait);
			
			// Only show the portrait from the current tab
			if(i == curTabId) {
				portrait.Show();
			} else {
				portrait.Hide();
			}
		}
		//Fetch all info
		foreach(var infoName in infoNames) {
			info.Add(GetNode<NotebookInfo>("Sprite/" + infoName));
			infoStatic.Add(GetNode<Label>("Sprite/" + infoName + "Static"));
		}
		
		//Gather all tabs
		for(int i = 0; i <= Context.N_TABS; ++i) {
			tabSprites.Add(GetNode<Sprite>("Tab" + i));
			
			//Retrieve tab and connect its pressed signal
			Button tab = GetNode<Button>("Tab" + i + "Button");
			tab.Connect("pressed", this, "_on_Tab" + i + "Button_pressed");
			tabButtons.Add(tab);
		}
		
		//Load in current character info and correct info
		characterInfo = context._GetNotebookCharInfo(curTabId);
		correctInfo = context._GetNotebookCorrectInfo(curTabId);
		
		//Initially hide all static info
		foreach(var infS in infoStatic) {
			infS.Hide();
		}
		
		//Initialize display
		FillNotebook(characterInfo);
		_UpdateNotebook(correctInfo);
		
		//Check if tabs are complete
		for(int i = 0; i < Context.N_TABS; ++i) {
			if(i == curTabId) {
				tabSprites[i].Frame = context._IsTabCorrect(i) ? 1 : 0;
			} else {
				tabSprites[i].Frame = context._IsTabCorrect(i) ? 3 : 2;
			}
		}

		//Set the objective (override it by charInfo if tab was completed)
		SetObjective((context._GetQuest() == Quests.TUTORIAL &&
				context._GetQuestStatus() != QuestStatus.COMPLETE) ?
					context._GetQuestStateId() : 1);
		Stamp.Hide();

		//Update the objective to show char info
		if(context._IsTabCorrect(curTabId)) {
			SetCharInfo();
			Stamp.Show();
		} 
	}
	
	private int AttributeToIdx(string attr) {
		int idx = -1;
		switch(attr) {
			case "prenom":
				idx = 0;
				break;
			case "nom":
				idx = 1;
				break;
			case "adresse":
				idx = 2;
				break;
			case "num":
				idx = 3;
				break;
			case "conjoint":
				idx = 4;
				break;
			case "enfants":
				idx = 5;
				break;
			case "metier":
				idx = 6;
				break;
			default:
				break;
		}
		return idx;
	}
	
	private void EvaluateAndUpdateNB(bool cutscene = true) {
		//Update the characterInfo
		FillCharInfo();
		
		//Request an info evaluation from the NPC
		var tmpcorrect = context._GetQuestNPC()._CompareSolutions(characterInfo, curTabId);
		if(tmpcorrect.IsCorrect() || cutscene) {
			correctInfo = tmpcorrect;
			_UpdateNotebook(correctInfo);
			if(!Stamp.Visible) {
				MusicPlayer MP = (MusicPlayer)GetNode("/root/MusicPlayer");
				MP.MusicFadeOut();
				AP.Play("Stamp");
				AP2.Play("EraseText");
				
			}
			
			//Update Context
			context._UpdateNotebookCharInfo(curTabId, characterInfo);
			context._UpdateNotebookCorrectInfo(curTabId, correctInfo);
			
			//Update tab if needed
			tabSprites[curTabId].Frame = context._IsTabCorrect(curTabId) ? 1 : 0;
		}
	}
	
	//Evaluate the notebook infor on every update
	private void _on_NotebookInfo_UpdateInfo() {
		//Used to evaluate and update the notebook
		EvaluateAndUpdateNB(false);
	}
	
	public void _UpdateNotebook(InfoValue_t vals) {
		//Change correct results to static
		foreach(var v in vals.FoundAttributes()) {
			int idx = AttributeToIdx(v);
			//Sanity check
			if(idx == -1)  {
				throw new Exception("Illegal attribute name!");
			}
			
			infoStatic[idx].Text = info[idx].Text;
			info[idx].Hide();
			infoStatic[idx].Show();
		}
		
		foreach(var v in vals.Outliers()) {
			int idx = AttributeToIdx(v);
			//Sanity check
			if(idx == -1)  {
				throw new Exception("Illegal attribute name!");
			}
			
			infoStatic[idx].Hide();
			info[idx].Show();
		}
	}
	
	private void DisableNonTutoTabs() {
		var pressTab = GetNode<Sprite>("PressTab");
		pressTab.Hide();
		closeNB.Disabled = true;
		closeLabel.Hide();
		for(int i = 1; i < Context.N_TABS; ++i) {
			tabSprites[i].Hide();
			tabButtons[i].Hide();
		}
	}
	
	public void _on_CutsceneEnd() {
		EvaluateAndUpdateNB();
	}
	
	public void _on_SendInfoToQuestNPC(NPC questNPC) {
		//Update the characterInfo
		FillCharInfo();
		
		//Request an info evaluation from the NPC
		correctInfo = questNPC._EvaluateQuest(p, characterInfo, curTabId);
	}
	
	public void _on_NotebookController_pressed() {
		if(mapOpen) {
			_on_MapB_pressed();
			hidden = true;
			Show();
		}
		if(ASP.Playing == false) {
			ASP.Play();
		}
		if(hidden) {
			Show();
			ShowAll();
			AudioServer.SetBusMute(2, true);
			p.BlockPlayer();
		} else {
			Hide();
			AudioServer.SetBusMute(2, false);
			p.UnBlockPlayer();
			EmitSignal(nameof(CloseNotebook));
		}
		hidden = !hidden;
		
		//Update Context
		FillCharInfo();
		_UpdateNotebook(correctInfo);
		context._UpdateNotebookCharInfo(curTabId, characterInfo);
		context._UpdateNotebookCorrectInfo(curTabId, correctInfo);
	}
	
	private void ShowAll() {
		var bg = GetNode<Sprite>("Sprite");
		bg.Show();
		var pressTab = GetNode<Sprite>("PressTab");
		pressTab.Show();
		closeNB.Disabled = false;
		closeLabel.Show();
		foreach(var inf in info) {
			inf.Show();
		}
		foreach(var inf in infoStatic) {
			inf.Show();
		}
		for(int i = 0; i < Context.N_TABS; ++i) {
			tabSprites[i].Show();
			tabButtons[i].Show();
		}
		
		//Hide non-tutorial stuff in case of tuto
		if(context._GetGameState() == GameStates.INIT) {
			DisableNonTutoTabs();

			//Set objective (it might later be overridden by charInfo if needed)
			SetObjective((context._GetQuest() == Quests.TUTORIAL &&
				context._GetQuestStatus() != QuestStatus.COMPLETE) ?
					context._GetQuestStateId() : 1);
			Stamp.Hide();

			//Set tutorial char info if tab is completed
			if(context._IsTabCorrect(curTabId)) {
				SetCharInfo();
				Stamp.Show();
			} 
		}
	}
	
	private void HideAll() {
		var bg = GetNode<Sprite>("Sprite");
		bg.Hide();
		var pressTab = GetNode<Sprite>("PressTab");
		pressTab.Hide();
		foreach(var inf in info) {
			inf.Hide();
		}
		foreach(var inf in infoStatic) {
			inf.Hide();
		}
		for(int i = 0; i < Context.N_TABS; ++i) {
			tabSprites[i].Hide();
			tabButtons[i].Hide();
		}
	}
	
	public void _on_MapB_pressed() {
		_on_MapButton_pressed();
		M.Show();
		var space = GetNode<Sprite>("PressSpace");
		space.Show();
		
		p._Map_B_Pressed();
		
		if(mapOpen) {
			Show();
			HideAll();
		} else {
			M.Hide();
			space.Hide();
			Hide();
		}
	}
	
	public void _on_MapButton_pressed() {
		
		if(ASP.Playing == false) {
			ASP.Play();
		}
		if(mapOpen) {
			//Show all temps
			foreach(var i in tempInfo) {
				i.Show();
			}
			tempInfo.Clear();
		} else {
			//Copy activated buttons to temp
			foreach(var i in info) {
				if(i.Visible)  {
					tempInfo.Add(i);
				}
				i.Hide();
			}
		}
		mapOpen = !mapOpen;
		
		//Update Context
		FillCharInfo();
		context._UpdateNotebookCharInfo(curTabId, characterInfo);
		context._UpdateNotebookCorrectInfo(curTabId, correctInfo);
	}
	
	private void _on_OpenMapZone_area_entered(Area2D tb) {
		if(tb.Owner is Player) {
			Player p = (Player)(tb.Owner);
			if(!p.isCutscene && !p.isEnterAnim) {
				_on_MapB_pressed();
			}
		}
	}
	
	private void PressTabButton(int buttonid) {
		if(hidden || mapOpen || NBL.Visible || AP.IsPlaying()) return;
		Debug.Assert(0 <= buttonid && buttonid < Context.N_TABS);
		Debug.Assert(0 <= curTabId && curTabId < Context.N_TABS);
		
		//Fill char info one last time
		FillCharInfo();
		
		//Update Context
		context._UpdateNotebookCharInfo(curTabId, characterInfo);
		context._UpdateNotebookCorrectInfo(curTabId, correctInfo);
		context._UpdateCurrentTab(buttonid);
		
		//Sanity check
		Debug.Assert(context._GetNotebookCharInfo(curTabId).Equals(characterInfo));
		Debug.Assert(context._GetNotebookCorrectInfo(curTabId).Equals(correctInfo));
		
		//Fetch data from context
		characterInfo = context._GetNotebookCharInfo(buttonid);
		correctInfo = context._GetNotebookCorrectInfo(buttonid);
		
		//Update the Notebook display
		tabSprites[curTabId].Frame = context._IsTabCorrect(curTabId) ? 3 : 2;
		tabSprites[buttonid].Frame = context._IsTabCorrect(buttonid) ? 1 : 0;
		
		//Update current tab id
		curTabId = buttonid;
		
		//Update the characterInfo
		FillNotebook(characterInfo);
		_UpdateNotebook(correctInfo);
		
		//Set object (will be overriden by charInfo is needed)
		SetObjective((context._GetQuest() == Quests.TUTORIAL &&
				context._GetQuestStatus() != QuestStatus.COMPLETE) ?
					context._GetQuestStateId() : 1);
		Stamp.Hide();

		//Update the objective to the charInfo if tab is completed
		if(context._IsTabCorrect(curTabId)) {
			SetCharInfo();
			Stamp.Show();
		}
	}
	
	private void SetObjective(int id) {
		//Query character info
		var infoTextQuery = from charInfo in InfoXML.Root.Descendants("objectif")
			where int.Parse(charInfo.Attribute("id").Value) == id
			select charInfo.Value;
		
		//Load in new info text
		foreach(string infoText in infoTextQuery) {
			Quest.BbcodeText = infoText;
			//break; // Need to do this to convert query result to string
		}
	}
	
	private void SetCharInfo() {
		//Query character info
		var infoTextQuery = from charInfo in InfoXML.Root.Descendants("personnage")
				where int.Parse(charInfo.Attribute("id").Value) == curTabId
				select charInfo.Value;
		
		//Load in new info text
		foreach(string infoText in infoTextQuery) {
			Quest.BbcodeText = infoText;
			break; // Need to do this to convert query result to string
		}
	}
	
	private void _on_AnimationPlayer_animation_finished(String anim_name) {
		if(anim_name == "Stamp") {
			MusicPlayer MP = (MusicPlayer)GetNode("/root/MusicPlayer");
			MP.MusicFadeIn(-10);
			
			//Signal that the page is complete
			EmitSignal(nameof(PageComplete));
		}
	}
	
	private void _on_AnimationPlayer2_animation_finished(String anim_name) {
		if(anim_name == "EraseText") {
			// Set new text from xml
			AP2.Play("WriteText");
			
			SetCharInfo();
		}
	}
	
	private void _Change_Portrait(int num) {
		if(!NBL.Visible && !AP.IsPlaying()) {
			for(int i = 0; i < Context.N_TABS; ++i) {
				var P = GetNode<Sprite>("Portrait" + i);
				P.Hide();
			}
			Portraits[num].Show();	
		}
	}
	
	public void _on_Tab0Button_pressed() {
		PressTabButton(0);
		_Change_Portrait(0);
		
	}
	public void _on_Tab1Button_pressed() {
		PressTabButton(1);
		_Change_Portrait(1);
		
	}
	public void _on_Tab2Button_pressed() {
		PressTabButton(2);
		_Change_Portrait(2);
		
	}
	public void _on_Tab3Button_pressed() {
		PressTabButton(3);
		_Change_Portrait(3);
		
	}
	public void _on_Tab4Button_pressed() {
		PressTabButton(4);
		_Change_Portrait(4);
	}
	public void _on_Tab5Button_pressed() {
		PressTabButton(5);
		_Change_Portrait(5);
	}
}

