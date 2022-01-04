using Godot;
using System;

public class Item : Node2D
{
	private Sprite ItemSprite;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		ItemSprite = GetNode<Sprite>("Sprite");
	}

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(float delta) {
		if(Input.IsActionJustPressed("ui_interact")) {
			if(ItemSprite.Visible) {
				GD.Print("Ouvrir l'affiche or whatever");
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
