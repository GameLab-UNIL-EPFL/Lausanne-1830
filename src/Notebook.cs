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
using System.Collections.Generic;

public class Notebook : Node2D {
	private List<NotebookInfo> tempInfo = new List<NotebookInfo>();
	private List<NotebookInfo> info = new List<NotebookInfo>();
	private List<Label> infoStatic = new List<Label>();
	private string[] infoNames = {"Prenom", "Nom", "Adresse", "Num", "Conjoint", "Enfants", "Metier"};
	
	private List<Sprite> tabSprites = new List<Sprite>();
	private List<Button> tabButtons = new List<Button>(); 
	private List<Sprite> Portraits = new List<Sprite>();
	
	private bool hidden = true;
	private bool mapOpen = false;
	private AudioStreamPlayer ASP;
	
	private Node2D M;
	
	private Context context;
	private Player p;
	
	//Currently opened tab
	public CharacterInfo_t characterInfo = new CharacterInfo_t(-1);
	public InfoValue_t correctInfo = new InfoValue_t(false);
	
	private int curTabId = 0;
	
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
					characterInfo.num = Int32.Parse(inf.Text == "" ? "-1" : inf.Text);
					break;
				case "conjoint":
					characterInfo.conjoint = inf.Text;
					break;
				case "enfants":
					characterInfo.enfants = Int32.Parse(inf.Text == "" ? "4" : inf.Text);
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
		
		//Make sure that the context has the questNPC
		context._FetchQuestNPC();
		for(int i = 1; i < Context.N_TABS; ++i) {
			Portraits.Add(GetNode<Sprite>("Portrait" + i));
		}
		//Fetch all info
		foreach(var infoName in infoNames) {
			info.Add(GetNode<NotebookInfo>("Sprite/" + infoName));
			infoStatic.Add(GetNode<Label>("Sprite/" + infoName + "Static"));
		}
		
		//Gather all tabs
		for(int i = 1; i <= Context.N_TABS; ++i) {
			tabSprites.Add(GetNode<Sprite>("Tab" + i));
			
			//Retrieve tab and connect its pressed signal
			Button tab = GetNode<Button>("Tab" + i + "Button");
			tab.Connect("pressed", this, "_on_Tab" + i + "Button_pressed");
			tabButtons.Add(tab);
		}
		curTabId = 0;
		
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
		foreach(var inf in info) {
			inf.Show();
		}
		foreach(var inf in infoStatic) {
			inf.Show();
		}
		for(int i = 0; i < 4; ++i) {
			tabSprites[i].Show();
			tabButtons[i].Show();
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
		for(int i = 0; i < 4; ++i) {
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
			if(!p.isCutscene) {
				_on_MapB_pressed();
			}
		}
	}
	
	private void PressTabButton(int buttonid) {
		if(hidden || mapOpen) return;
		Debug.Assert(0 <= buttonid && buttonid < Context.N_TABS);
		Debug.Assert(0 <= curTabId && curTabId < Context.N_TABS);
		
		//Fill char info one last time
		FillCharInfo();
		
		//Update Context
		context._UpdateNotebookCharInfo(curTabId, characterInfo);
		context._UpdateNotebookCorrectInfo(curTabId, correctInfo);
		
		//Sanity check
		Debug.Assert(context._GetNotebookCharInfo(curTabId).Equals(characterInfo));
		Debug.Assert(context._GetNotebookCorrectInfo(curTabId).Equals(correctInfo));
		
		//Fetch data from context
		characterInfo = context._GetNotebookCharInfo(buttonid);
		correctInfo = context._GetNotebookCorrectInfo(buttonid);
		
		//Update the Notebook display
		tabSprites[curTabId].Frame = 1;
		tabSprites[buttonid].Frame = 0;
		
		//Update current tab id
		curTabId = buttonid;
		
		//Update the characterInfo
		FillNotebook(characterInfo);
		_UpdateNotebook(correctInfo);
	}
	
	private void _Change_Portrait(int num) {
		for(int i = 1; i <= 5; ++i) {
			var P = GetNode<Sprite>("Portrait" + i);
			P.Hide();
		}
		Portraits[num].Show();
	}
	
	public void _on_Tab1Button_pressed() {
		PressTabButton(0);
		_Change_Portrait(0);
		
	}
	public void _on_Tab2Button_pressed() {
		PressTabButton(1);
		_Change_Portrait(1);
		
	}
	public void _on_Tab3Button_pressed() {
		PressTabButton(2);
		_Change_Portrait(2);
		
	}
	public void _on_Tab4Button_pressed() {
		PressTabButton(3);
		_Change_Portrait(3);
	}
	public void _on_Tab5Button_pressed() {
		PressTabButton(4);
		_Change_Portrait(4);
	}
}

