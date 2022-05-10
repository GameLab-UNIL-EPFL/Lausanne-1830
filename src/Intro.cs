using Godot;
using System;

public class Intro : Node2D {
	
	Context context;
	CollisionShape2D DoorCol;
	StaticBody2D Door;
	Label dirTuto;
	Label interactTuto;
	
	//Hide the door if we're no longer in the tutorial
	public override void _Ready() {
		Door = GetNode<StaticBody2D>("Door");
		DoorCol = GetNode<CollisionShape2D>("Door/DoorCol");
		context = GetNode<Context>("/root/Context");
		dirTuto = GetNode<Label>("Label");
		interactTuto = GetNode<Label>("Label2");
		
		//Check if we're still in the intro or not
		if(context._GetGameState() != GameStates.INIT) {
			//Hide the door
			Door.Hide();
			DoorCol.Disabled = true;
			
			//Hide the tutorial text
			dirTuto.Hide();
			interactTuto.Hide();
		}
	}
}
