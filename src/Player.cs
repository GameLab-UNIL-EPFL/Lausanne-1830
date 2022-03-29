using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;

public enum PlayerStates { IDLE, WALKING, RUNNING, BLOCKED, NOTEBOOK };

public class Player : KinematicBody2D {
	[Signal]
	public delegate void SendInfoToQuestNPC(NPC questNPC);
	
	[Signal]
	public delegate void CutsceneEnd();
	
	[Signal]
	public delegate void SlideInNotebookController();
	
	[Signal]
	public delegate void OpenNotebook();
	
	//Player FSM
	public PlayerStates CurrentState = PlayerStates.IDLE;
	
	private bool NotebookOpen = false;
	private PlayerStates PrevState = PlayerStates.IDLE;
	
	// Cutscene state
	
	private bool isCutsceneConv = false;
	private ColorRect FadeIn;
	
	[Export]
	public int WalkSpeed = 100; //Pixels per second
	[Export]
	public int RunSpeed = 150;
	[Export]
	public float RunTime = 3.0f; // Seconds
	[Export]
	public bool isCutscene;
	
	//Empirical acceleration and friction amounts
	private const int ACC = 950;
	private const int FRIC = 1000;
	
	private int Speed = 0;
	private bool RunRequest = false;
	private float RunCooldown = 0.0f;
	
	public Vector2 Velocity = Vector2.Zero;
	private Vector2 InputVec = Vector2.Zero;
	
	private AnimationPlayer animation;
	private AnimationTree animationTree; 
	private AnimationNodeStateMachinePlayback animationState;
	
	private List<Item> itemsInRange = new List<Item>();
	
	private List<NPC> subs = new List<NPC>();
	private List<NPC> subsWithAuto = new List<NPC>();
	
	private Notebook NB;
	private Context context;
	private NPC lastNearest = null;
	
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
	private void HandleCutscene(float delta) {
		if(isCutsceneConv) {
			if(Input.IsActionJustPressed("ui_interact")) {
				NearestSub()._Notify(this);
			}
		} else {
			InputVec.x = 0.0f;
			InputVec.y = -1.0f;
			if(subs.Count != 0 && QuestGiverIsSubbed()) {
				var nearestNPC = NearestSub();
				if(nearestNPC.isQuestNPC) {
					InputVec.y = 0.0f;
					nearestNPC._Notify(this);
				}
			} 
		}
		InputVec = InputVec.Normalized();
		HandleMovement(delta);
	}
	
	/**
	 * @brief Checks for player input and updates its velocity accordingly
	 * @param delta, the time elapsed since the last update
	 */
	private void HandleInput(float delta) {
		if(CurrentState != PlayerStates.BLOCKED && CurrentState != PlayerStates.NOTEBOOK) {
			//Handle movement
			InputVec.x = Input.GetActionStrength("ui_right") - Input.GetActionStrength("ui_left");
			InputVec.y = Input.GetActionStrength("ui_down") - Input.GetActionStrength("ui_up");
			InputVec = InputVec.Normalized();
			
			//Check for sprint
			RunRequest = Input.IsActionPressed("ui_shift");
			HandleMovement(delta);
		} else {
			InputVec = Vector2.Zero;
			Velocity = Vector2.Zero;
		}
		
		//Check for interaction
		if(((subs.Count != 0) || (itemsInRange.Count != 0)) &&
			(CurrentState != PlayerStates.NOTEBOOK)) {
				
			if(Input.IsActionJustPressed("ui_interact")) {
				NotifySubs();
				NotifyItems();
			}
		}
		
		//Check for map
		if(Input.IsActionJustPressed("ui_map")) {
			NB._on_MapB_pressed();
		}
		
		//Check for tab
		if(Input.IsActionJustPressed("ui_focus_next")) {
			EmitSignal(nameof(OpenNotebook));
		}
		
		//Check for tab switches
		if(Input.IsActionJustPressed("ui_1")) {
			NB._on_Tab1Button_pressed();
		}
		if(Input.IsActionJustPressed("ui_2")) {
			NB._on_Tab2Button_pressed();
		}
		if(Input.IsActionJustPressed("ui_3")) {
			NB._on_Tab3Button_pressed();
		}
		if(Input.IsActionJustPressed("ui_4")) {
			NB._on_Tab4Button_pressed();
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
		return subs.Count == 0;
	}
	
	public void BlockPlayer() {
		CurrentState = PlayerStates.NOTEBOOK;
	}
	
	public void UnBlockPlayer() {
		CurrentState = PlayerStates.IDLE;
	}
	
	/**
	 * @brief Updates the player's state according to the current actions taken
	 * @param delta, the time elapsed since the last update
	 */
	private void HandleState(float delta) {
		switch(CurrentState) {
			case PlayerStates.IDLE:
				//Rest run cooldown faster
				RunCooldown = (RunCooldown < RunTime) ? RunCooldown + (2.0f * (float)delta) : RunTime;
				
				//Check for state change
				if(Velocity != Vector2.Zero) {
					CurrentState = PlayerStates.WALKING;
					animationState.Travel("Walk");
				}
				break;
				
			case PlayerStates.WALKING:
				//Rest run cooldown
				RunCooldown = (RunCooldown < RunTime) ? RunCooldown + (float)delta : RunTime;
				
				//Check for sprint
				if(RunRequest && RunCooldown == RunTime) {
					CurrentState = PlayerStates.RUNNING;
					
					//Update animation to match state change
					animationState.Travel("Run");
				} 
		
				CheckIdle();
				break;
				
			case PlayerStates.RUNNING:
				//Burn cooldown if running
				RunCooldown -= (float)delta;
				if(RunCooldown <= 0.0f) {
					RunCooldown = 0.0f;
					CurrentState = PlayerStates.WALKING;
					
					//Update animation to match state change
					animationState.Travel("Walk");
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
		RunCooldown = RunTime;
		CurrentState = PlayerStates.IDLE;
		
		//Fetch nodes
		NB = GetNode<Notebook>("../../Notebook");
		context = GetNode<Context>("/root/Context");
		animation = GetNode<AnimationPlayer>("AnimationPlayer");
		animationTree = GetNode<AnimationTree>("AnimationTree");
		animationState = (AnimationNodeStateMachinePlayback)animationTree.Get("parameters/playback");
		
		if(context._GetGameState() != GameStates.INIT) {
			isCutscene = false;
			if(context._GetGameState() == GameStates.PALUD) {
				Position = new Vector2(Position.x, Position.y - 200);
			}
		}
		if(!isCutscene) {
			EmitSignal(nameof(SlideInNotebookController));
		}
	}
	
	// Called on every physics engine tick
	public override void _Process(float delta) {
		if(isCutscene) {
			HandleCutscene(delta);
		} else {
			//Handle input
			HandleInput(delta);
		}
		
		//Update player state
		HandleState(delta);
		
		//Scale velocity and move
		Velocity = MoveAndSlide(Velocity);
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
		if(subs.Count == 0) {
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
		var nearestNPC = NearestSub();
		if(nearestNPC == null) return;
		
		if(nearestNPC.isQuestNPC) {
			EmitSignal(nameof(SendInfoToQuestNPC), nearestNPC);
		} else {
			nearestNPC._Notify(this);
		}
	}
	
	public void _EndDialogue() {
		CurrentState = PlayerStates.IDLE;
		
		if(context._IsGameComplete()) {
			var dooropen = GetNode<ColorRect>("../../Collisions/HotelDeVilleDoor/OpenEndDoor");
			var doorcolision = GetNode<CollisionShape2D>("../../Collisions/HotelDeVilleDoor/EndDoor");
			
			//Show opened door and remove collisions
			dooropen.Show();
			doorcolision.Disabled = true;
		}
		
		// If the cutscene is still going, end it
		if(isCutscene) {
			isCutscene = false;
			EmitSignal(nameof(CutsceneEnd));
			EmitSignal(nameof(SlideInNotebookController));
			context._StartGame();
		}
	}
	
	private void _on_EnterEndZone_area_entered(object area) {
		SceneChanger SC = GetNode<SceneChanger>("/root/SceneChanger");
		SC.GotoScene("res://scenes/Interaction/EndScreen.tscn");
	}
	
	public void _StartDialogue() {
		CurrentState = PlayerStates.BLOCKED;
		
		foreach(var sub in subs) {
			sub._StopTalking();
		}
		
		if(isCutscene) {
			isCutsceneConv = true;
		}
	}
	
	/**
	 * @brief Makes sure that the player can't move when the notebook is open 
	 */
	private void _on_NotebookController_pressed() {
		if(NotebookOpen) {
			NotebookOpen = false;
			//Restore the state the previous state before the notebook was opened
			CurrentState = PrevState;
		} else {
			NotebookOpen = true;
			PrevState = CurrentState;
			CurrentState = PlayerStates.BLOCKED;
		}
	}
}
