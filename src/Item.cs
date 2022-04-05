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
	
	private void _on_Notebook_visibility_changed() {
		N.Hide();
	}

}
