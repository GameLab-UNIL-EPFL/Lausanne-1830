using Godot;
using System;

public class Player : Area2D {
	
	//Walking speed
	[Export]
	public int Speed = 10; //Pixels per second
	
	//Keep game window size for potential viewport translations
	public Vector2 ScreenSize;
	
	/**
	 * Handles the user's input
	 */
	private void HandleInput(ref Vector2 velocity) {
		//Scan keys to update velocity
		if(Input.IsActionPressed("ui_right")) {
			velocity.x += 1; 
		}
		if(Input.IsActionPressed("ui_left")) {
			velocity.x -= 1;
		}
		if(Input.IsActionPressed("ui_down")) {
			velocity.y += 1;
		}
		if(Input.IsActionPressed("ui_up")) {
			velocity.y -= 1;
		}
	}
	
	/**
	 * Runs at the start of the scene.
	 */
	public override void _Ready() {
		//Sample game window size
		ScreenSize = GetViewportRect();
	}
	
	public void Start(Vector2 pos) {
		Position = pos;
		GetNode<CollisionShape2D>("CollisionShape2D").Disabled = false;
	}
	
	/**
	 * Runs on every frame
	 * @param delta, the time elapsed since the last frame
	 */
	public override void _Process(float delta) {
		var velocity = Vector2.Zero;
		
		//Handle inputs
		HandleInput(ref velocity);
		
		//TODO: Potentially add animations once the sprites are ready
		
		Position += velocity * delta;
		Position = new Vector2(
			x: Mathf.Clamp(Position.x, 0, SreenSize.x),
			y: Mathf.Clamp(Position.y, 0, ScreenSize.y)
		);
	}
}
