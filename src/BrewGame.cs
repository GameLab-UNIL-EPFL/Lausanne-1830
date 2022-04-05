/*
Historically accurate educational video game based in 1830s Lausanne.
Copyright (C) 2022 GameLab UNIL-EPFL

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

public class BrewGame : Node2D {
	
	[Export]
	public float GameTime = 30.0f;
	[Export]
	public float MaxRotateSpeed = 5.0f;
	[Export]
	public float MinRotateSpeed = 0.3f;
	[Export]
	public float SlowdownPerSecond = 0.5f;
	[Export]
	public int Radius = 50;
	[Export]
	public float BoostIncrement = 0.5f;
	[Export]
	public float BoostErrorDecrement = 1.0f; //Slowdown due to wrong input
	[Export]
	public int NumberOfBoosts = 3;
	[Export]
	public float BurnThreshold = 1.0f;
	[Export]
	public float BurnLossPerSecond = 10.0f; 
	[Export]
	public float MaxBurn = 100.0f;
	
	private float Angle = 0;
	private float RotateSpeed = 0;
	
	private float InvBurnThreshold;
	private float InvMaxBurn;
	
	//Handle stick boost and burnability
	private float boost = 3.0f;
	private float burn = 0.0f;
	private float burnPercent = 0.0f;
	
	private Sprite Stick;
	private ColorRect HideStickBurnt;
	private Sprite Beer;
	private Sprite BeerBurnt;
	private Label Score;
	private Label Time;
	
	//Hit areas
	private Area2D TopArea;
	private Area2D LeftArea;
	private Area2D DownArea;
	private Area2D RightArea;
	
	private int BoostPressed = 0;
	private bool isGameOver = false;
	private float GameOverScreenTime = 3.0f;
	
	private enum StickLocation {DOWN, RIGHT, UP, LEFT};
	private StickLocation Location = StickLocation.DOWN;
	
	private Context context;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		SetProcess(true);
		
		//Fetch nodes
		Time = GetNode<Label>("Scoreboard/Time");
		Score = GetNode<Label>("Scoreboard/Score");
		context = GetNode<Context>("/root/Context");
		
		Beer = GetNode<Sprite>("Beer");
		Stick = GetNode<Sprite>("Stick");
		BeerBurnt = GetNode<Sprite>("BeerBurnt");
		HideStickBurnt = GetNode<ColorRect>("Stick/hideStickBurnt");
		
		TopArea = GetNode<Area2D>("arrowTop/Area2D");
		LeftArea = GetNode<Area2D>("arrowLeft/Area2D");
		DownArea = GetNode<Area2D>("arrowBottom/Area2D");
		RightArea = GetNode<Area2D>("arrowRight/Area2D");
		
		//Connect Signals
		TopArea.Connect("area_exited", this, nameof(_on_Area2D_Exited));
		LeftArea.Connect("area_exited", this, nameof(_on_Area2D_Exited));
		DownArea.Connect("area_exited", this, nameof(_on_Area2D_Exited));
		RightArea.Connect("area_exited", this, nameof(_on_Area2D_Exited));
		
		TopArea.Connect("area_entered", this, nameof(_on_Top_Entered));
		LeftArea.Connect("area_entered", this, nameof(_on_Left_Entered));
		DownArea.Connect("area_entered", this, nameof(_on_Down_Entered));
		RightArea.Connect("area_entered", this, nameof(_on_Right_Entered));
		
		//Init constants
		InvMaxBurn = 1.0f/MaxBurn;
		InvBurnThreshold = 1.0f/BurnThreshold;
	}
	
	/**
	 * @brief handles the circular movement of the stick
	 * @param delta, the time elapsed since the last update
	 */
	private void HandleStickMovement(float delta) {
		Angle += RotateSpeed * delta;
		
		Vector2 Offset = new Vector2(
			(float)Math.Sin(Angle) * Radius * 2.0f, //Account for elliptical shape of the cuve
			(float)Math.Cos(Angle) * Radius
		);
		Vector2 Pos = Beer.Position + Offset;
		Stick.Position = Pos;
	}
	
	private void Boost(float mult) {
		//Boost the stick
		boost += BoostIncrement * mult;
		boost = Math.Min(boost, MaxRotateSpeed);
		
		//Make sure you can't spam the boost
		BoostPressed++;
	}
	
	//Check that the correct key was pressed at the correct time
	private void HandleStickState(float delta) {
		if(BoostPressed < NumberOfBoosts) {
			float actionPressedD = Input.GetActionStrength("ui_right");
			float actionPressedR = Input.GetActionStrength("ui_up");
			float actionPressedU = Input.GetActionStrength("ui_left");
			float actionPressedL = Input.GetActionStrength("ui_down");
			float mult = 0.0f;
			switch(Location) {
				case StickLocation.DOWN:
					mult = actionPressedD - 
							(actionPressedR + actionPressedU + actionPressedL) * BoostErrorDecrement;
					break;
				case StickLocation.RIGHT:
					mult = actionPressedR - 
							(actionPressedD + actionPressedU + actionPressedL) * BoostErrorDecrement;
					break;
				case StickLocation.UP:
					mult = actionPressedU - 
							(actionPressedR + actionPressedD + actionPressedL) * BoostErrorDecrement;
					break;
				case StickLocation.LEFT:
					mult = actionPressedL - 
							(actionPressedR + actionPressedU + actionPressedD) * BoostErrorDecrement;
					break;
				default: 
					break;
			}
			if(mult != 0.0f) {
				Boost(mult);
			} 
		}
	}
	
	//Handles the burning of the beer
	private void HandleBurn(float delta) {
		//Lower speed
		boost -= SlowdownPerSecond * delta;
		boost = Math.Max(boost, MinRotateSpeed);
		
		//Check for burn
		if(boost < BurnThreshold) {
			//Map boost from [0, BurnThreshold] -> [0, 1]
			//Then burnt amount is the distance from the threshold
			float burnAmount = (1.0f - (boost * InvBurnThreshold)) * BurnLossPerSecond;
			
			//Change the amount the beer is burnt
			burn += burnAmount * delta;
			burnPercent = (burn * InvMaxBurn);
			burnPercent = Math.Min(burnPercent, 1.0f);
			
			//Update the Score
			Score.Text = (100 - (int)(burnPercent * 100)).ToString() + "%";
			
			//Update the sprites
			BeerBurnt.Modulate = new Color(
				BeerBurnt.Modulate.r, 
				BeerBurnt.Modulate.g, 
				BeerBurnt.Modulate.b, 
				burnPercent
			);
			HideStickBurnt.Modulate = new Color(
				HideStickBurnt.Modulate.r, 
				HideStickBurnt.Modulate.g, 
				HideStickBurnt.Modulate.b, 
				burnPercent
			);
		}
	}
	
	private void HandleGameState(float delta) {
		//Decrement the clock
		GameTime -= delta;
		
		//Update clock
		Time.Text = ((int)GameTime).ToString() + "s";
		
		//Check for end of game
		if(burnPercent >= 1.0f || GameTime <= 0.0f) {
			isGameOver = true;
			
			//Update the context
			context._UpdateBrewBurn(100 - (int)(burnPercent * 100));
		}
	}
	
	private void HandleGameOver(float delta) {
		//Wait a bit at the end
		GameOverScreenTime -= delta;
		
		//End the game once the wait is over
		if(GameOverScreenTime <= 0.0f) {
			//Go back to the brasserie
			SceneChanger SC = GetNode<SceneChanger>("/root/SceneChanger");
			SC.GotoScene("res://scenes/Brasserie/Brasserie.tscn");
		}
	}
	
	// Called every frame. 'delta' is the elapsed time since the previous frame.  
	public override void _Process(float delta) {
		if(!isGameOver) {
			HandleStickState(delta);
			HandleBurn(delta);
			HandleGameState(delta);
			
			//Set the rotation speed
			RotateSpeed = boost;
			HandleStickMovement(delta);
		} else {
			//End the game
			HandleGameOver(delta);
		}
	}
	
	//Signal methods
	public void _on_Area2D_Exited(Area2D obj) {
		BoostPressed = 0;
	}
	
	public void _on_Top_Entered(Area2D obj) {
		Location = StickLocation.UP;
	}
	
	public void _on_Left_Entered(Area2D obj) {
		Location = StickLocation.LEFT;
	}
	
	public void _on_Down_Entered(Area2D obj) {
		Location = StickLocation.DOWN;
	}
	
	public void _on_Right_Entered(Area2D obj) {
		Location = StickLocation.RIGHT;
	}
}
