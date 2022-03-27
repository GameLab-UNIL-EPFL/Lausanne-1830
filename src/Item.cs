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
	
	public void _Notify(Player p) {
		if(ItemSprite.Visible) {
			N.Visible = !N.Visible;
			if(N.Visible) {
				p.CurrentState = PlayerStates.BLOCKED;
			} else {
				p.CurrentState = PlayerStates.IDLE;
			}
		}
	}

	private void _on_Area2D_area_entered(Area2D tb) {
		if(tb.Owner is Player) {
			Player p = (Player)tb.Owner;
			if(p._CanInteract()) {
				ItemSprite.Show();
				p._AddItemInRange(this);
			}
		}
	}


	private void _on_Area2D_area_exited(Area2D tb) {
		if(tb.Owner is Player) {
			Player p = (Player)tb.Owner;
			ItemSprite.Hide();
			p._RemoveItemInRange(this);
		}
	}

}
