using Godot;
using System;
using System.Linq;

public class Pigeon : KinematicBody2D {
	
	enum PigeonState {INIT, FOUNTAIN, FLYING, ROOF};
	private PigeonState curState;
	
	private AnimationPlayer AP;
	private AnimationTree AT;
	private AnimationNodeStateMachinePlayback AS;
	private AudioStreamPlayer2D ASP;
	
	private Random rand = new Random();
	
	private Vector2 InitPos = Vector2.Zero;
	private Vector2 Destination = Vector2.Zero;
	private Vector2 InputVec = Vector2.Zero;
	private Vector2 Velocity = Vector2.Zero;
	private const int ACC = 950;
	private const int FRIC = 1000;
	
	private float cooldown;

	[Export]
	public Vector2[] RoofTops = new Vector2[4] {Vector2.Zero, Vector2.Zero, Vector2.Zero, Vector2.Zero};
	[Export]
	public Vector2 FountainSpot = new Vector2();
	[Export]
	public float RoofTime = 10.0f;
	[Export]
	public int FlySpeed = 100;
	[Export]
	public bool RandomStartDir = true;
	[Export]
	public int RightDirProb = 50;
	[Export]
	public int IdleStartProb = 25;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		//Set init pos and cooldown
		InitPos = Position;
		Destination = RoofTops[0];
		cooldown = (rand.Next(100)/100.0f) * RoofTime;
		curState = PigeonState.INIT;
		
		//Fetch nodes
		AP = GetNode<AnimationPlayer>("AnimationPlayer");
		AT = GetNode<AnimationTree>("AnimationTree");
		AS = (AnimationNodeStateMachinePlayback)AT.Get("parameters/playback");
		ASP = GetNode<AudioStreamPlayer2D>("TakeOff");
	}
	
	private void HandleMovement(float delta) {
		//Update velocity
		if(InputVec == Vector2.Zero) {
			Velocity = Velocity.MoveToward(Vector2.Zero, FRIC * delta);
		} else {
			//Set blend positions for animation
			Velocity = Velocity.MoveToward(InputVec * FlySpeed, ACC * delta);
		}
	}
	
	private void HandleStates(float delta) {
		switch(curState) {
			case PigeonState.INIT:
				if(rand.Next(100) < IdleStartProb) {
					AT.Active = true;
					curState = PigeonState.FOUNTAIN;
					if(RandomStartDir) {
						if(rand.Next(100) > RightDirProb) {
							AS.Travel("Idle");
						} else {
							AS.Travel("IdleRight");
						}
					}
				}
				break;
			case PigeonState.FOUNTAIN:
				break;
			case PigeonState.FLYING:
				//Go to destination
				if(Position.DistanceTo(Destination) <= 5.0f) {
					InputVec = Vector2.Zero;
					
					//Check where the destination is 
					if(RoofTops.Contains(Destination)) {
						curState = PigeonState.ROOF;
						cooldown = RoofTime;
					} else {
						curState = PigeonState.FOUNTAIN;
					}
					//Clip onto destination
					if(rand.Next(100) > RightDirProb) {
						AS.Travel("Idle");
					} else {
						AS.Travel("IdleRight");
					}
				} else {
					//Set input vector
					InputVec = (Destination - Position).Normalized();
				}
				break;
			case PigeonState.ROOF:
				if(cooldown > 0) {
					cooldown -= delta; 
				} else {
					cooldown = 0.0f;
					//Go to the fountain
					Destination = FountainSpot;
					curState = PigeonState.FLYING;
					if(Destination.x - Position.x < 0) {
						AS.Travel("FlyLeft");
					} else {
						AS.Travel("FlyRight");
					}
				}
				break;
			default:
				break;
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(float delta) {
		//Handle states
		HandleStates(delta);
		
		//Handle movement
		HandleMovement(delta);
		if(Velocity != Vector2.Zero) {
			//Scale velocity and move
			Velocity = MoveAndSlide(Velocity);
		}
	}
	
	private void _on_Area2D_area_entered(Area2D hb) {
		if(hb.Owner is Player || hb.Owner is NPC) {
			if(curState == PigeonState.FOUNTAIN) {
				curState = PigeonState.FLYING;
				ASP.Play();
				Destination = RoofTops[rand.Next(RoofTops.Length)];
				if(Destination.x - Position.x < 0) {
					AS.Travel("FlyLeft");
				} else {
					AS.Travel("FlyRight");
				}
			}
		}
	}
}

