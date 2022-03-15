using Godot;
using System;

public class Item : Node2D {
	private Sprite ItemSprite;
	private Node2D N;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		ItemSprite = GetNode<Sprite>("Sprite");
		N = GetNode<Node2D>("Open");
	}

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(float delta) {
		if(Input.IsActionJustPressed("ui_interact")) {
			if(ItemSprite.Visible) {
				N.Visible = !N.Visible;
			}
		}
	}



	private void _on_Area2D_area_entered(Area2D tb) {
		if(tb.Owner is Player) {
			ItemSprite.Show();
			}
	}


	private void _on_Area2D_area_exited(Area2D tb)
	{
		if(tb.Owner is Player) {
			ItemSprite.Hide();
		}
	}

}
