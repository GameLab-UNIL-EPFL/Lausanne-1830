using Godot;
using System;
using System.Collections.Generic;

public class Player : KinematicBody2D {
	//Player FSM
	enum PlayerStates { IDLE, WALKING, RUNNING, BLOCKED };
	PlayerStates CurrentState = PlayerStates.IDLE;
	
	private bool NotebookOpen = false;
	private PlayerStates PrevState = PlayerStates.IDLE;
	
	[Export]
	public int WalkSpeed = 100; //Pixels per second
	[Export]
	public int RunSpeed = 150;
	[Export]
	public float RunTime = 3.0f; // Seconds
	
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
	
	private List<NPC> subs = new List<NPC>();
	private int nSubs = 0;
	
	private void HandleMovement(float delta) {
		//Handle movement
		InputVec.x = Input.GetActionStrength("ui_right") - Input.GetActionStrength("ui_left");
		InputVec.y = Input.GetActionStrength("ui_down") - Input.GetActionStrength("ui_up");
		InputVec = InputVec.Normalized();
		
		//Check for sprint
		RunRequest = Input.IsActionPressed("ui_shift");
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
	
	/**
	 * @brief Checks for player input and updates its velocity accordingly
	 * @param delta, the time elapsed since the last update
	 */
	private void HandleInput(float delta) {
		if(CurrentState != PlayerStates.BLOCKED) {
			HandleMovement(delta);
		} else {
			InputVec = Vector2.Zero;
			Velocity = Vector2.Zero;
		}
		
		//Check for interaction
		if(subs.Count != 0) {
			if(Input.IsActionJustPressed("ui_interact")) {
				NotifySubs();
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
		animation = GetNode<AnimationPlayer>("AnimationPlayer");
		animationTree = GetNode<AnimationTree>("AnimationTree");
		animationState = (AnimationNodeStateMachinePlayback)animationTree.Get("parameters/playback");
	}
	
	// Called on every physics engine tick
	public override void _Process(float delta) {
		//Handle input
		HandleInput(delta);
		
		//Update player state
		HandleState(delta);
		
		//Scale velocity and move
		Velocity = MoveAndSlide(Velocity);
	}
	
	public int _Subscribe(NPC npc) {
		subs.Add(npc);
		return nSubs++;
	}
	
	public void _Unsubscribe(NPC npc) {
		subs.Remove(npc);
		nSubs--;
	}
	
	private void NotifySubs() {
		foreach(NPC sub in subs) {
			sub._Notify(this);
		}
	}
	
	public void _EndDialogue() {
		CurrentState = PlayerStates.IDLE;
	}
	
	public void _StartDialogue() {
		CurrentState = PlayerStates.BLOCKED;
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




