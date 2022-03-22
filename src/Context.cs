using Godot;
using System;
using System.Diagnostics;
using System.Collections.Generic;

public enum GameStates {INIT, PALUD, OTHERS};
public enum Locations {PALUD, BRASSERIE, CASINO};

// Storage for all persistent data in the game
public class Context : Node {
	private List<CharacterInfo_t> NotebookCharInfo = new List<CharacterInfo_t>();
	private List<InfoValue_t> NotebookCorrectInfo = new List<InfoValue_t>();
	private GameStates GameState = GameStates.INIT;
	private Locations CurrentLocation = Locations.PALUD;
	public const int N_TABS = 6;
	
	public override void _Ready() {
		NotebookCharInfo.Add(new CharacterInfo_t(
			"", "Trüschel", "", 0, "", 0, ""
		));
		NotebookCorrectInfo.Add(new InfoValue_t(
			false, true, false, false, false, false, false
		));
		NotebookCharInfo.Add(new CharacterInfo_t(
			"", "Perregaux", "Rue St-François", 9, "", 0, ""
		));
		NotebookCorrectInfo.Add(new InfoValue_t(
			false, true, true, true, false, false, false
		));
		NotebookCharInfo.Add(new CharacterInfo_t(
			"", "De Montolieu", "", 0, "Veuf.ve", 0, ""
		));
		NotebookCorrectInfo.Add(new InfoValue_t(
			false, true, false, true, true, false, false
		));
		NotebookCharInfo.Add(new CharacterInfo_t(
			"", "Mercier", "", 0, "Marié.e", 3, "Négociant.e"
		));
		NotebookCorrectInfo.Add(new InfoValue_t(
			false, true, false, false, true, true, true
		));
		NotebookCharInfo.Add(new CharacterInfo_t(
			"", "Rochat", "", 0, "", 0, ""
		));
		NotebookCorrectInfo.Add(new InfoValue_t(
			false, true, false, false, false, false, false
		));
	}
	
	public void _UpdateLocation(string id) {
		switch(id) {
			case "Palud/ProtoPalud":
				CurrentLocation = Locations.PALUD;
				_StartGame();
				break;
			case "Casino/Casino":
				CurrentLocation = Locations.CASINO;
				_SwitchScenes();
				break;
			case "Brasserie/Brasserie":
				CurrentLocation = Locations.BRASSERIE;
				_SwitchScenes();
				break;
			default:
				break;
		}
	}
	
	public Locations _GetLocation() {
		return CurrentLocation;
	}
	
	public void _StartGame() {
		GameState = GameStates.PALUD;
	}
	
	public void _SwitchScenes() {
		GameState = GameStates.OTHERS;
	}
	
	public GameStates _GetGameState() {
		return GameState;
	}

	public List<CharacterInfo_t> _GetNotebookCharInfo() {
		return NotebookCharInfo;
	}
	
	public CharacterInfo_t _GetNotebookCharInfo(int id) {
		Debug.Assert(0 <= id && id < N_TABS);
		return NotebookCharInfo[id];
	}
	
	public List<InfoValue_t> _GetNotebookCorrectInfo() {
		return NotebookCorrectInfo;
	}
	
	public InfoValue_t _GetNotebookCorrectInfo(int id) {
		Debug.Assert(0 <= id && id < N_TABS);
		return NotebookCorrectInfo[id];
	}
	
	public void _UpdateNotebookCharInfo(int id, CharacterInfo_t data) {
		Debug.Assert(0 <= id && id < N_TABS);
		NotebookCharInfo[id] = data;
	}
	
	public void _UpdateNotebookCorrectInfo(int id, InfoValue_t data) {
		Debug.Assert(0 <= id && id < N_TABS);
		NotebookCorrectInfo[id] = data;
	}
}