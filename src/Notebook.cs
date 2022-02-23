using Godot;
using System;
using System.Collections.Generic;

public class Notebook : Node2D {
	private List<NotebookInfo> tempInfo = new List<NotebookInfo>();
	private List<NotebookInfo> info = new List<NotebookInfo>();
	private List<Label> infoStatic = new List<Label>();
	private string[] infoNames = {"Prenom", "Nom", "Adresse", "Num", "Conjoint", "Enfants", "Metier"};
	
	private bool hidden = true;
	private bool mapOpen = false;
	private AudioStreamPlayer ASP;
	
	public CharacterInfo_t characterInfo = new CharacterInfo_t(-1);
	public InfoValue_t correctInfo = new InfoValue_t(false);
	
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
		ASP = GetNode<AudioStreamPlayer>("../NotebookClick");
		//Fetch all info
		foreach(var infoName in infoNames) {
			info.Add(GetNode<NotebookInfo>("Sprite/" + infoName));
			infoStatic.Add(GetNode<Label>("Sprite/" + infoName + "Static"));
		}
		
		//Initially hide all static info
		foreach(var infS in infoStatic) {
			infS.Hide();
		}
		FillCharInfo();
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
	}
	
	public void _on_CutsceneEnd(NPC questNPC) {
		//Sanity check
		if(questNPC == null) {
			return;
		}
		//Update the characterInfo
		FillCharInfo();
		
		//Request an info evaluation from the NPC
		correctInfo = questNPC._CompareSolutions(characterInfo);
		_UpdateNotebook(correctInfo);
	}
	
	public void _on_SendInfoToQuestNPC(Player p, NPC questNPC) {
		//Update the characterInfo
		FillCharInfo();
		
		//Request an info evaluation from the NPC
		correctInfo = questNPC._EvaluateQuest(p, characterInfo);
		_UpdateNotebook(correctInfo);
	}
	
	public void _on_NotebookController_pressed() {
		if(ASP.Playing == false) {
			ASP.Play();
		}
		if(hidden) {
			Show();
			AudioServer.SetBusMute(2, true);
		} else {
			Hide();
			AudioServer.SetBusMute(2, false);
		}
		hidden = !hidden;
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
	}
}



