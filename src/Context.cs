using Godot;
using System;
using System.Diagnostics;
using System.Collections.Generic;

public enum GameStates {INIT, PALUD, OTHERS};

// Storage for all persistent data in the game
public class Context : Node {
	public List<CharacterInfo_t> NotebookCharInfo = new List<CharacterInfo_t>();
	public List<InfoValue_t> NotebookCorrectInfo = new List<InfoValue_t>();
	public GameStates GameState = GameStates.INIT;
	
	public override void _Ready() {
		NotebookCharInfo.Add(new CharacterInfo_t(
			"", "Trüschel", "", 0, "", 4, "Propriétaire"
		));
		NotebookCorrectInfo.Add(new InfoValue_t(
			false, true, false, false, false, true, true
		));
		NotebookCharInfo.Add(new CharacterInfo_t(
			"", "Perregaux", "Rue St-François", 0, "", 0, ""
		));
		NotebookCorrectInfo.Add(new InfoValue_t(
			false, true, true, false, false, true, false
		));
		NotebookCharInfo.Add(new CharacterInfo_t(
			"Isabelle", "", "", 0, "Marié.e", 0, "Écrivain.e"
		));
		NotebookCorrectInfo.Add(new InfoValue_t(
			true, false, false, false, true, true, true
		));
		NotebookCharInfo.Add(new CharacterInfo_t(
			"", "", "Rue du Pré", 31, "", 0, "Négociant.e"
		));
		NotebookCorrectInfo.Add(new InfoValue_t(
			false, false, true, true, false, true, true
		));
		NotebookCharInfo.Add(new CharacterInfo_t(
			"", "", "Moulins de Pépinet", 1, "Veuf.ve", 0, ""
		));
		NotebookCorrectInfo.Add(new InfoValue_t(
			false, false, true, true, true, true, false
		));
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
		Debug.Assert(0 <= id && id < 5);
		return NotebookCharInfo[id];
	}
	
	public List<InfoValue_t> _GetNotebookCorrectInfo() {
		return NotebookCorrectInfo;
	}
	
	public InfoValue_t _GetNotebookCorrectInfo(int id) {
		Debug.Assert(0 <= id && id < 5);
		return NotebookCorrectInfo[id];
	}
	
	public void _UpdateNotebookCharInfo(int id, CharacterInfo_t data) {
		Debug.Assert(0 <= id && id < 5);
		NotebookCharInfo[id] = data;
	}
	
	public void _UpdateNotebookCorrectInfo(int id, InfoValue_t data) {
		Debug.Assert(0 <= id && id < 5);
		NotebookCorrectInfo[id] = data;
	}
}
