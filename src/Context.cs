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

public enum GameStates {INIT, PLAYING, COMPLETE};
public enum Locations {INTRO, PALUD, BRASSERIE, CASINO, MOULIN, FLON};
public enum Quests {NONE, TUTORIAL};
public enum QuestStatus {NONE, ON_GOING, COMPLETE, NOT_STARTED};

// Storage for all persistent data in the game
public class Context : Node {
	//Notebook data
	private List<CharacterInfo_t> NotebookCharInfo = new List<CharacterInfo_t>();
	private List<InfoValue_t> NotebookCorrectInfo = new List<InfoValue_t>();
	public int CurrentTab = 0;
	
	//Game state data
	private GameStates GameState = GameStates.INIT;
	private Locations CurrentLocation = Locations.INTRO;
	private Quests CurrentQuest = Quests.TUTORIAL;
	private QuestStatus CurrentQuestStatus = QuestStatus.NOT_STARTED;
	private int QuestStateId = -2;
	
	//Quest NPC ref
	private NPC QuestNPC = null;
	
	//Constants
	public const int N_TABS = 6;
	
	//Brewery minigame variables
	private float BrewGameScore = -1.0f;
	private bool BrewGameCutscene = false;
	private Vector2 BrewerPreviousPos = Vector2.Zero;
	private Vector2 PlayerPreviousPos = Vector2.Zero;
	private bool wasInMoulin = false;
	
	//Player scene positions
	private Vector2 IntroEnterPosition = new Vector2(397, 310);
	private Vector2 PaludEnterPosition = new Vector2(562, 450);
	private Vector2 MoulinEnterPosition = new Vector2(469, 319);
	private Vector2 MoulinExitPosition = new Vector2(407, 230);
	private Vector2 FlonEnterPosition = new Vector2(188, 446);
	private Vector2 BrasserieEnterPosition = new Vector2(264, 393);
	private Vector2 CasinoEnterPosition = new Vector2(191, 432);
	
	public override void _Ready() {
		NotebookCharInfo.Add(new CharacterInfo_t(
			"", "De Cerjeat", "", 1, "Célibataire", 0, "Rentier.ère" 
		));
		NotebookCorrectInfo.Add(new InfoValue_t(
			false, true, false, true, true, true, true
		));
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
			"", "Mercier", "", 0, "Marié.e", 3, ""
		));
		NotebookCorrectInfo.Add(new InfoValue_t(
			false, true, false, false, true, true, false
		));
		NotebookCharInfo.Add(new CharacterInfo_t(
			"", "Rochat", "", 1, "", 0, ""
		));
		NotebookCorrectInfo.Add(new InfoValue_t(
			false, true, false, true, false, false, false
		));
	}
	
	public void _Clear() {
		//Clear all elements
		NotebookCharInfo = new List<CharacterInfo_t>();
		NotebookCorrectInfo = new List<InfoValue_t>();
		GameState = GameStates.INIT;
		CurrentLocation = Locations.INTRO;
		
		//Reload context
		_Ready();
	}
	
	public void _UpdateLocation(string id) {
		switch(id) {
			case "Intro/Intro":
				CurrentLocation = Locations.INTRO;
				wasInMoulin = false;
				_SwitchScenes();
				break;
			case "Palud/ProtoPalud":
				CurrentLocation = Locations.PALUD;
				wasInMoulin = false;
				_SwitchScenes();
				break;
			case "Casino/Casino":
				CurrentLocation = Locations.CASINO;
				wasInMoulin = false;
				_SwitchScenes();
				break;
			case "Brasserie/Brasserie":
				CurrentLocation = Locations.BRASSERIE;
				wasInMoulin = false;
				_SwitchScenes();
				break;
			case "Flon/Moulin":
				CurrentLocation = Locations.MOULIN;
				wasInMoulin = false;
				_SwitchScenes();
				break;
			case "Flon/Flon":
				wasInMoulin = CurrentLocation == Locations.MOULIN;
				CurrentLocation = Locations.FLON;
				_SwitchScenes();
				break;
			default:
				break;
		}
	}
	
	public void _FetchQuestNPC() {
		if(QuestNPC == null) {
			QuestNPC = GetNode<NPC>("/root/Intro/YSort/QuestNPC");
		}
	}
	
	public NPC _GetQuestNPC() {
		return QuestNPC;
	}
	
	public Vector2 _GetPlayerPosition() {
		if(GameState != GameStates.INIT) {
			switch(CurrentLocation) {
				case Locations.PALUD:
					return PaludEnterPosition;
				case Locations.INTRO:
					return IntroEnterPosition;
				case Locations.MOULIN:
					return MoulinEnterPosition;
				case Locations.FLON:
					return wasInMoulin ? MoulinExitPosition : FlonEnterPosition;
				case Locations.BRASSERIE:
					if(BrewerPreviousPos != Vector2.Zero) 
						return BrewerPreviousPos;
					return BrasserieEnterPosition;
				case Locations.CASINO:
					return CasinoEnterPosition;
				default:
					return Vector2.Zero;
			}
		}
		return Vector2.Zero;
	}
	
	public void _UpdateQuest(Quests q) {
		CurrentQuest = q;
	}
	
	public void _UpdateQuestStatus(QuestStatus qs) {
		CurrentQuestStatus = qs;
	}
	
	public void _UpdateQuestStateId(int id) {
		QuestStateId = id;
	}
	
	public Quests _GetQuest() {
		return CurrentQuest;
	}
	
	public QuestStatus _GetQuestStatus() {
		return CurrentQuestStatus;
	}
	
	public int _GetQuestStateId() {
		return QuestStateId;
	}
	
	public int _GetCurrentTab() {
		return CurrentTab;
	}
	
	public void _UpdateCurrentTab(int tabNum) {
		CurrentTab = tabNum;
	}
	
	public Locations _GetLocation() {
		return CurrentLocation;
	}
	
	public void _StartGame() {
		GameState = GameStates.PLAYING;
	}
	
	public void _SwitchScenes() {
		GameState = GameStates.PLAYING;
		CurrentQuest = Quests.NONE;
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
	
	public bool _IsTabCorrect(int id) {
		Debug.Assert(0 <= id && id < N_TABS);
		return NotebookCorrectInfo[id].IsCorrect();
	}
	
	public void _UpdateNotebookCharInfo(int id, CharacterInfo_t data) {
		Debug.Assert(0 <= id && id < N_TABS);
		NotebookCharInfo[id] = data;
	}
	
	public bool _IsGameComplete() {
		return GameState == GameStates.COMPLETE;
	}
	
	public int _GetNotCorrectTabs() {
		int n_corrects = 0;
		for(int i = 0; i < N_TABS; ++i) {
			if(!NotebookCorrectInfo[i].IsCorrect()) n_corrects++;
		}
		return n_corrects;
	} 
	
	public bool _AllTabsCorrect() {
		return _GetNotCorrectTabs() == 0;
	}
	
	private bool CheckGameOver() {
		for(int i = 0; i < N_TABS; ++i) {
			if(!NotebookCorrectInfo[i].IsCorrect()) return false;
		}
		return true;
	}
	
	public void _UpdateNotebookCorrectInfo(int id, InfoValue_t data) {
		Debug.Assert(0 <= id && id < N_TABS);
		NotebookCorrectInfo[id] = data;
		
		//Check if the game is complete
		if(CheckGameOver()) {
			GameState = GameStates.COMPLETE;
		} 
	}
	
	public void _EndBrewGameCutscene() {
		BrewGameCutscene = false;
	}
	
	public bool _IsBrewGameCutscene() { 
		return BrewGameCutscene;
	}
	
	public void _UpdateBrewBurn(int burn) {
		BrewGameScore = (float)burn;
		BrewGameCutscene = true;
	}
	
	public float _CheckBrewBurn() {
		return BrewGameScore;
	}
	
	public Vector2 _GetBrewerPreviousPos() {
		return new Vector2(BrewerPreviousPos.x, BrewerPreviousPos.y);
	}
	
	public void _UpdateBrewerPreviousPos(Vector2 pos) {
		BrewerPreviousPos = new Vector2(pos.x, pos.y);
	}
	
	public Vector2 _GetPlayerPreviousPos() {
		return new Vector2(PlayerPreviousPos.x, PlayerPreviousPos.y);
	}
	
	public void _UpdatePlayerPreviousPos(Vector2 pos) {
		PlayerPreviousPos = new Vector2(pos.x, pos.y);
	}
}
