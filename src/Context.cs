using Godot;
using System;
using System.Diagnostics;
using System.Collections.Generic;

// Storage for all persistent data in the game
public class Context : Node {
	public List<CharacterInfo_t> NotebookCharInfo = new List<CharacterInfo_t>();
	public List<InfoValue_t> NotebookCorrectInfo = new List<InfoValue_t>();
	
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
