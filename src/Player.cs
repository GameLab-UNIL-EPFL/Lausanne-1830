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
using System.Collections.Generic;
using System.Diagnostics;

public enum PlayerStates { IDLE, WALKING, RUNNING, BLOCKED, NOTEBOOK, ENTER };

public class Player : KinematicBody2D {
	[Signal]
	public delegate void SendInfoToQuestNPC(NPC questNPC);
	
	[Signal]
	public delegate void CutsceneEnd();
	
	[Signal]
	public delegate void SlideInNotebookController();
	
	[Signal]
	public delegate void OpenNotebook();
	
	[Signal]
	public delegate void OpenTutorial();
	
	//Player FSM
	public PlayerStates CurrentState = PlayerStates.IDLE;
	
	private bool NotebookOpen = false;
	private bool MapOpen = false;
	private PlayerStates PrevState = PlayerStates.IDLE;
	
	// Cutscene state
	private bool isCutsceneConv = false;
	
	[Export]
	public int WalkSpeed = 100; //Pixels per second
	[Export]
	public int RunSpeed = 150;
	[Export]
	public bool isCutscene;
	[Export]
	public float FootstepPitch = 1.0f;

	public float CloseNotebookTimer = 4.0f;
	
	public bool isBrewEnd = false;
	public bool isEnterAnim = true;
	
	//Empirical acceleration and friction amounts
	private const int ACC = 950;
	private const int FRIC = 1000;
	
	private int Speed = 0;
	private bool RunRequest = false;
	
	public Vector2 Velocity = Vector2.Zero;
	private Vector2 InputVec = Vector2.Zero;
	
	private AnimationPlayer animation;
	private AnimationTree animationTree; 
	private AnimationNodeStateMachinePlayback animationState;
	private AudioStreamPlayer ASP;
	private Timer T;
	
	private List<Item> itemsInRange = new List<Item>();
	
	private List<NPC> subs = new List<NPC>();
	private List<NPC> subsWithAuto = new List<NPC>();
	
	private Notebook NB;
	private Button CloseNB;
	private Context context;
	private NPC lastNearest = null;
	private NPC speaker = null;

	private bool wasBlocked = false;
	
	// Returns whether or not the quest giver is in the sub list
	private bool QuestGiverIsSubbed() {
		foreach(var sub in subs) {
			if(sub.isQuestNPC) {
				return true;
			}
		}
		return false;
	}
	
	private void HandleMovement(float delta) {
		Speed = CurrentState == PlayerStates.RUNNING ? RunSpeed : WalkSpeed;
		
		//Update velocity
		if(InputVec == Vector2.Zero) {
			Velocity = Velocity.MoveToward(Vector2.Zero, FRIC * delta);
		} else {
			//Set blend positions for animation
			animationTree.Set("parameters/Walk/blend_position", InputVec);
			animationTree.Set("parameters/Run/blend_position", InputVec);
			animationTree.Set("parameters/Idle/blend_position", InputVec);
			Velocity = Velocity.MoveToward(InputVec * Speed, ACC * delta);
		}
	}
	
	// Walk up to the quest giver and interact
	private void HandleTutorial(float delta) {

		if(CurrentState != PlayerStates.NOTEBOOK) {
			if(Input.IsActionJustPressed("ui_interact")) {
				//Only allow interaction with questNPC if in initial stage
				NPC nearestNPC = NearestSub();
				var item = NearestItem();

				if(nearestNPC != null) {
					//Halt the player's movement BEFORE the interaction
					InputVec = Vector2.Zero;
					//Check which element is nearest to the player
					if(item == null || isNearer(nearestNPC, item)) {
						NotifySubs();
					} else if(!isNearer(nearestNPC, item)) {
						NotifyItems();
					}	
					
				//Interact with item if nearer to it 
				} else if(item != null) {
					NotifyItems();
				}
				
			}
		} 
		//Check for tab
		if(Input.IsActionJustPressed("ui_focus_next") &&
			context._GetQuestStateId() >= QuestController.TALK_TO_QUEST_NPC_OBJECTIVE &&
			CurrentState != PlayerStates.BLOCKED) {
			if(CurrentState == PlayerStates.NOTEBOOK) {
				CurrentState = PlayerStates.IDLE;
			} else {
				CurrentState = PlayerStates.BLOCKED;
			}
			//Open book
			EmitSignal(nameof(OpenNotebook));
		}
		
		HandleMovementInput(delta);
		InputVec = InputVec.Normalized();
		HandleMovement(delta);
	}
	
	private void HandleMovementInput(float delta) {
		if(CurrentState != PlayerStates.BLOCKED && CurrentState != PlayerStates.NOTEBOOK) {
			//Handle movement
			InputVec.x = Input.GetActionStrength("ui_right") - Input.GetActionStrength("ui_left");
			InputVec.y = Input.GetActionStrength("ui_down") - Input.GetActionStrength("ui_up");
			InputVec = InputVec.Normalized();
			
			//Check for sprint
			RunRequest = Input.IsActionPressed("ui_shift");
			HandleMovement(delta);
		} else {
			//Player shouldn't move if blocked
			InputVec = Vector2.Zero;
			Velocity = Vector2.Zero;

			animationState.Travel("Idle");
		}
	}
	
	private void HandleInteractionInput(float delta) {
		if(((subs.Count != 0) || (itemsInRange.Count != 0)) &&
			(CurrentState != PlayerStates.NOTEBOOK)) {
				
			if(Input.IsActionJustPressed("ui_interact")) {
				NotifySubs();
				NotifyItems();
			}
		}
	}
	
	private void HandleEscapeInput() {
		if(NB._IsMapOpen()) {
			NB._on_MapB_pressed();
		} else if(NB._IsNotebookOpen()) {
			NB._on_NotebookController_pressed();
		}
	}
	
	/**
	 * @brief Checks for player input and updates its velocity accordingly
	 * @param delta, the time elapsed since the last update
	 */
	private void HandleInput(float delta) {
		HandleMovementInput(delta);
		
		//Check for interaction (and handle cooldown)
		HandleInteractionInput(delta);

		//Check for escape
		if(Input.IsActionJustPressed("ui_cancel")) {
			HandleEscapeInput();
		}
		
		if(CurrentState != PlayerStates.BLOCKED) {
			//Check for map
			if(Input.IsActionJustPressed("ui_map")) {
				NB._on_MapB_pressed();
			}
		}
		//Check for tab
		if(Input.IsActionJustPressed("ui_focus_next")) {
			if(CurrentState == PlayerStates.BLOCKED) {
				wasBlocked = true;
			}
			EmitSignal(nameof(OpenNotebook));
		}
		
		if(CurrentState == PlayerStates.NOTEBOOK) {
			//Check for tab switches
			if(Input.IsActionJustPressed("ui_1")) {
				NB._on_Tab0Button_pressed();
			}
			if(Input.IsActionJustPressed("ui_2")) {
				NB._on_Tab1Button_pressed();
			}
			if(Input.IsActionJustPressed("ui_3")) {
				NB._on_Tab2Button_pressed();
			}
			if(Input.IsActionJustPressed("ui_4")) {
				NB._on_Tab3Button_pressed();
			}
			if(Input.IsActionJustPressed("ui_5")) {
				NB._on_Tab4Button_pressed();
			}
			if(Input.IsActionJustPressed("ui_6")) {
				NB._on_Tab5Button_pressed();
			}
		}
	}
	
	private void CheckIdle() {
		//Become idle if player stops moving
		if(Velocity == Vector2.Zero) {
			//Update state and animation
			CurrentState = PlayerStates.IDLE;
			animationState.Travel("Idle");
		}
	}
	
	public bool _CanInteract() {
		return subs.Count == 0 || context._GetQuest() == Quests.TUTORIAL;
	}
	
	public void BlockPlayer() {
		CurrentState = PlayerStates.NOTEBOOK;
	}
	
	//Unblock the player such that they return to the correct state from before the notebook opening
	public void UnBlockPlayer() {
		if(wasBlocked) {
			CurrentState = PlayerStates.BLOCKED;
			wasBlocked = false;
		} else {
			CurrentState = PlayerStates.IDLE;
		}
	}
	
	/**
	 * @brief Updates the player's state according to the current actions taken
	 * @param delta, the time elapsed since the last update
	 */
	private void HandleState(float delta) {
		switch(CurrentState) {
			case PlayerStates.IDLE:
				
				//Check for state change
				if(Velocity != Vector2.Zero) {
					CurrentState = PlayerStates.WALKING;
					animationState.Travel("Walk");
				}
				break;
				
			case PlayerStates.WALKING:
				
				//Check for sprint
				if(RunRequest) {
					CurrentState = PlayerStates.RUNNING;
					
					//Update animation to match state change
					animationState.Travel("Walk");
				} 
				
				if(T.TimeLeft <= 0) {
					ASP.PitchScale = FootstepPitch;
					ASP.PitchScale = (float)GD.RandRange(ASP.PitchScale - 0.1f, ASP.PitchScale + 0.1f);
					ASP.Play();
					T.Start(0.26f);
				}
		
				CheckIdle();
				break;
				
			case PlayerStates.RUNNING:
				if(!RunRequest) {
					CurrentState = PlayerStates.WALKING;
					
					//Update animation to match state change
					animationState.Travel("Walk");
				}
				
				if(T.TimeLeft <= 0) {
					ASP.PitchScale = FootstepPitch;
					ASP.PitchScale = (float)GD.RandRange(ASP.PitchScale - 0.1f, ASP.PitchScale + 0.2f);
					ASP.Play();
					T.Start(0.23f);
				}
				
				CheckIdle();
				break;
			case PlayerStates.NOTEBOOK:
			case PlayerStates.BLOCKED:
				animationState.Travel("Idle");
				break;
				
			default:
				//Update state and animation
				CurrentState = PlayerStates.IDLE;
				animationState.Travel("Idle");
				break;
		}
	}
	
	public override void _Ready() {
		//Initialize variables
		Speed = WalkSpeed;
		
		//Fetch nodes
		NB = GetNode<Notebook>("../../Notebook");
		CloseNB = GetNode<Button>("../../Notebook/ColorRect/CloseNotebook");
		context = GetNode<Context>("/root/Context");
		animation = GetNode<AnimationPlayer>("AnimationPlayer");
		animationTree = GetNode<AnimationTree>("AnimationTree");
		animationState = (AnimationNodeStateMachinePlayback)animationTree.Get("parameters/playback");
		ASP = GetNode<AudioStreamPlayer>("AudioStreamPlayer");
		T = GetNode<Timer>("Timer");
		
		//Connect close notebook signal
		CloseNB.Connect("pressed", this, nameof(HandleEscapeInput));
		
		if(context._GetGameState() != GameStates.INIT) {
			isCutscene = false;
			if(context._GetLocation() == Locations.PALUD) {
				Position = new Vector2(Position.x, Position.y - 200);
			}
			if(context._GetLocation() == Locations.BRASSERIE && context._IsBrewGameCutscene()) {
 				Position = context._GetPlayerPreviousPos();
				NPC brewer = GetNode<NPC>("../NPC/Brewer");
				brewer.Position = context._GetBrewerPreviousPos();
				subs.Add(brewer);
				isBrewEnd = true;
			}
		}
		if(!isCutscene) {
			EmitSignal(nameof(SlideInNotebookController));
		}
		
		if(context._IsGameComplete() && context._GetLocation() == Locations.PALUD) {
			var dooropen = GetNode<ColorRect>("../../Collisions/HotelDeVilleDoor/OpenEndDoor");
			var doorcolision = GetNode<CollisionShape2D>("../../Collisions/HotelDeVilleDoor/EndDoor");
			
			//Show opened door and remove collisions
			dooropen.Show();
			doorcolision.Disabled = true;
		}
		
		var EnterPos = context._GetPlayerPosition();
		if(EnterPos != Vector2.Zero && context._GetGameState() != GameStates.INIT) {
			Position = EnterPos;
		}

		//Set initial player state
		CurrentState = PlayerStates.IDLE;
		isEnterAnim = !isCutscene;
	}

	//Have the player walk up until they are interupted by a PlayerEnterArea
	private void HandleEnter(float delta) {
		InputVec = Vector2.Zero;
		InputVec[1] = -1.0f;
		HandleMovement(delta);
	}
	
	// Called on every physics engine tick
	public override void _Process(float delta) {
		if(isCutscene) {
			HandleTutorial(delta);
		} else if(isEnterAnim) {
			HandleEnter(delta);
		} else {
			//Handle input
			HandleInput(delta);
		}
		if(isBrewEnd) {
			NPC brewer = GetNode<NPC>("../NPC/Brewer");
			NotifySubs();
			subs.Remove(brewer);
			isBrewEnd = false;
		}
		
		//Update player state
		HandleState(delta);
		
		//Scale velocity and move
		Velocity = MoveAndSlide(Velocity);

		//Make sure that the player isn't blocked if he's not in a dialogue
		if(subs.Count == 0 && itemsInRange.Count == 0) {
			if(CurrentState != PlayerStates.NOTEBOOK) {
				CheckIdle();
			}
		}
	}
	
	public void _Subscribe(NPC npc) {
		if(itemsInRange.Count == 0) {
			subs.Add(npc);
			if(npc.HasAutoDialogue) {
				subsWithAuto.Add(npc);
			}
		}
		
		//Check to see if AutoDialogue exists
		var nearest = NearestSub(true);
		if(nearest != null) {
			if(lastNearest != null && nearest != lastNearest) {
				lastNearest._EndAutoDialogue();
			}
			lastNearest = nearest;
			nearest._RequestAutoDialogue();
		}
	}
	
	public void _Unsubscribe(NPC npc) {
		if(subs.Contains(npc)) {
			subs.Remove(npc);
		}
		if(subsWithAuto.Contains(npc)) {
			subsWithAuto.Remove(npc);
		}
	}
	
	public void _AddItemInRange(Item i) {
		if((subs.Count == 0 && context._GetQuest() != Quests.TUTORIAL) ||
			(context._GetQuest() == Quests.TUTORIAL && 
				context._GetQuestStateId() >= QuestController.CONFIRM_OPEN_NOTEBOOK_OBJECTIVE)) {
			itemsInRange.Add(i);
		} 
	}
	
	public void _RemoveItemInRange(Item i) {
		itemsInRange.Remove(i);
	}
	
	// Finds the nearest item to the player
	private Item NearestItem() {
		if(itemsInRange.Count == 0) return null;
		
		float minDistance = float.MaxValue;
		Item nearest = itemsInRange[0];
		
		// Iterate through all subs and keep the one with the shortest distance to player
		foreach(Item i in itemsInRange) {
			var distance = Position.DistanceTo(i.Position);
			if(distance < minDistance) {
				minDistance = distance;
				nearest = i;
			}
		}
		return nearest;
	}

	private bool isNearer(NPC a, Item b) {
		float distToa = Position.DistanceTo(a.Position);;
		float distTob = Position.DistanceTo(b.Position);
		return (distToa < distTob);
	}
	
	// Finds the nearest sub to the player
	private NPC NearestSub(bool withAuto = false) {
		List<NPC> subL = withAuto ? subsWithAuto : subs;
		if(subL.Count == 0) return null;
		
		float minDistance = float.MaxValue;
		NPC nearest = subL[0];
		
		// Iterate through all subs and keep the one with the shortest distance to player
		foreach(NPC sub in subL) {
			var distance = Position.DistanceTo(sub.Position);
			if(distance < minDistance) {
				minDistance = distance;
				nearest = sub;
			}
		}
		return nearest;
	}
	
	private void NotifyItems() {
		var item = NearestItem();
		if(item == null) return;
		
		item._Notify(this);
	}
	
	private void NotifySubs() {
		// Only notify the nearest sub
		var nearestNPC = speaker == null ? NearestSub() : speaker;
		if(nearestNPC == null) return;
		
		if(nearestNPC.isQuestNPC) {
			//Check for tutorial
			if(context._GetQuest() == Quests.TUTORIAL) {
				nearestNPC._NotifyQuest(this);
			} else {
				EmitSignal(nameof(SendInfoToQuestNPC), nearestNPC);
			}
		} else {
			nearestNPC._Notify(this);
		}
	}
	
	public void _EndDialogue() {
		CurrentState = PlayerStates.IDLE;
		
		if(speaker.isBrewer) {
			context._UpdatePlayerPreviousPos(Position);
		}
		
		// If the cutscene is still going, end it
		if(isCutscene) {
			if(context._GetQuestStateId() == QuestController.TALK_TO_QUEST_NPC_OBJECTIVE) {
				EmitSignal(nameof(SlideInNotebookController));
			}

			if(context._GetQuestStatus() == QuestStatus.COMPLETE) {
				isCutscene = false;
				EmitSignal(nameof(CutsceneEnd));
				context._StartGame();
				
				//Find and open the door
				StaticBody2D Door = GetNode<StaticBody2D>("../../Door");
				CollisionShape2D DoorCol = GetNode<CollisionShape2D>("../../Door/DoorCol");
				Door.Hide();
				DoorCol.Disabled = true;
				
				//Force one more interaction with the NPC
				NotifySubs();

				//Slide in the Map
				EmitSignal(nameof(SlideInNotebookController));
			}
		}
		speaker = null;
	}
	
	private void _on_EnterEndZone_area_entered(object area) {
		SceneChanger SC = GetNode<SceneChanger>("/root/SceneChanger");
		SC.GotoScene("res://scenes/Interaction/EndScreen.tscn");
	}
	
	public void _StartDialogue() {
		if(speaker == null) {
			speaker = NearestSub();
		} else {
			return;
		}
		CurrentState = PlayerStates.BLOCKED;
		
		foreach(var sub in subs) {
			sub._StopTalking();
		}
		
		if(isCutscene) {
			isCutsceneConv = true;
		}
	}
	
	public void _Map_B_Pressed() {
		if(MapOpen) {
			MapOpen = false;
			CurrentState = PrevState;
		} else {
			MapOpen = true;
			if(NotebookOpen) {
				PrevState = CurrentState;
			}
			CurrentState = PlayerStates.NOTEBOOK;
		}
	}
	
	/**
	 * @brief Makes sure that the player can't move when the notebook is open 
	 */
	public void _on_NotebookController_pressed() {
		if(NotebookOpen) {
			NotebookOpen = false;
			//Restore the state the previous state before the notebook was opened
			CurrentState = PrevState;
		} else {
			NotebookOpen = true;
			PrevState = CurrentState;
			CurrentState = PlayerStates.NOTEBOOK;
		}
	}
	
	private void _on_Intro_Exit_area_entered(Area2D area) {
		if(area.Owner is Player && !isEnterAnim) {
			NB._on_MapB_pressed();
		}
	}
	private void _on_IntroAreaDoor_area_entered(Area2D area) {
		if(area.Owner is Player) {
			SceneChanger SC = GetNode<SceneChanger>("/root/SceneChanger");
			SC.GotoScene("res://scenes/Intro/Intro.tscn");
				
			context._UpdateLocation("Intro/Intro");
		}
	}
	
	//On entrance to the Moulin, TP the player
	private void _on_Door_area_entered(Area2D area) {
		if(area.Owner is Player) {
			SceneChanger SC = GetNode<SceneChanger>("/root/SceneChanger");
			SC.GotoScene("res://scenes/Flon/Moulin.tscn");
			context._UpdateLocation("Flon/Moulin");
		}
	}
	
	private void _on_Exit_area_entered(Area2D area) {
		if(area.Owner is Player) {
			SceneChanger SC = GetNode<SceneChanger>("/root/SceneChanger");
			SC.GotoScene("res://scenes/Flon/Flon.tscn");
			context._UpdateLocation("Flon/Flon");
		}
	}
	
	//Stops the player's intro animation
	private void _on_PlayerEnterArea_area_entered(Area2D area) {
		if(isEnterAnim) {
			InputVec = Vector2.Zero;
			Velocity = Vector2.Zero;

			//Set player to IDLE
			CurrentState = PlayerStates.IDLE;

			//Update animation to match state change
			animationState.Travel("Idle");

			isEnterAnim = false;
		}

	}
}

