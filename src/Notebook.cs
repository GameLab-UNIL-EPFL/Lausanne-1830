using Godot;
using System;
using System.Diagnostics;
using System.Collections.Generic;

public class Notebook : Node2D {
	private const int N_TABS = 5;
	private List<NotebookInfo> tempInfo = new List<NotebookInfo>();
	private List<NotebookInfo> info = new List<NotebookInfo>();
	private List<Label> infoStatic = new List<Label>();
	private string[] infoNames = {"Prenom", "Nom", "Adresse", "Num", "Conjoint", "Enfants", "Metier"};
	
	private List<Sprite> tabSprites = new List<Sprite>();
	private List<Button> tabButtons = new List<Button>(); 
	
	private bool hidden = true;
	private bool mapOpen = false;
	private AudioStreamPlayer ASP;
	private Sprite Portrait;
	
	private Map M;
	
	private Context context;
	
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
		ASP = GetNode<AudioStreamPlayer>("../NotebookClick");
		M = GetNode<Map>("Map");
		Portrait = GetNode<Sprite>("Portrait");
		//Fetch all info
		foreach(var infoName in infoNames) {
			info.Add(GetNode<NotebookInfo>("Sprite/" + infoName));
			infoStatic.Add(GetNode<Label>("Sprite/" + infoName + "Static"));
		}
		
		//Gather all tabs
		for(int i = 1; i <= N_TABS; ++i) {
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
	
	public void _on_CutsceneEnd(NPC questNPC) {
		//Sanity check
		if(questNPC == null) {
			return;
		}
		//Update the characterInfo
		FillCharInfo();
		
		//Request an info evaluation from the NPC
		correctInfo = questNPC._CompareSolutions(characterInfo, curTabId);
		_UpdateNotebook(correctInfo);
	}
	
	public void _on_SendInfoToQuestNPC(Player p, NPC questNPC) {
		//Update the characterInfo
		FillCharInfo();
		
		//Request an info evaluation from the NPC
		correctInfo = questNPC._EvaluateQuest(p, characterInfo, curTabId);
		_UpdateNotebook(correctInfo);
	}
	
	public void _on_NotebookController_pressed() {
		Player p = GetNode<Player>("../YSort/Player");
		if(mapOpen) {
			_on_MapButton_pressed();
			M._on_MapButton_pressed();
		}
		
		if(ASP.Playing == false) {
			ASP.Play();
		}
		if(hidden) {
			Show();
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
		context._UpdateNotebookCharInfo(curTabId, characterInfo);
		context._UpdateNotebookCorrectInfo(curTabId, correctInfo);
	}
	
	public void _on_MapB_pressed() {
		if(hidden) {
			_on_NotebookController_pressed();
			
			_on_MapButton_pressed();
			M._on_MapButton_pressed();
		} else if(!hidden && !mapOpen) {
			_on_MapButton_pressed();
			M._on_MapButton_pressed();
		} else {
			_on_NotebookController_pressed();
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
	
	private void PressTabButton(int buttonid) {
		Debug.Assert(0 <= buttonid && buttonid < 5);
		Debug.Assert(0 <= curTabId && curTabId < 5);
		
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
		FillNotebook(characterInfo);
		_UpdateNotebook(correctInfo);
		
		//Update current tab id
		curTabId = buttonid;
	}
	
	public void _on_Tab1Button_pressed() {
		PressTabButton(0);
		Portrait.Show();
		Portrait.Texture = (Texture)GD.Load("res://assets/01_characters/03_pnjs/angeliqueTruschel.png");
	}
	public void _on_Tab2Button_pressed() {
		PressTabButton(1);
		Portrait.Show();
		Portrait.Texture = (Texture)GD.Load("res://assets/01_characters/03_pnjs/henriPerregaux.png");
	}
	public void _on_Tab3Button_pressed() {
		PressTabButton(2);
		//Portrait.Texture = (Texture)GD.Load("res://assets/01_characters/03_pnjs/randomWoman-Sheet.png");
		Portrait.Hide();
	}
	public void _on_Tab4Button_pressed() {
		PressTabButton(3);
		//Portrait.Texture = (Texture)GD.Load("res://assets/01_characters/03_pnjs/cityHallMan-Sheet.png");
		Portrait.Hide();
	}
	public void _on_Tab5Button_pressed() {
		PressTabButton(4);
		//Portrait.Texture = (Texture)GD.Load("res://assets/01_characters/03_pnjs/cityHallMan-Sheet.png");
		Portrait.Hide();
	}
}
