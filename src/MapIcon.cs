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

public class MapIcon : TextureButton {
	[Export]
	public string id = "Brasserie";
	
	[Export]
	public string text = "Brasserie";
	
	private AnimationPlayer AnimPlayer;
	private Label L;
	private Sprite RR;
	private Context context;
	
	private Vector2 INTRO_POS = new Vector2(254, 111);
	private Vector2 PALUD_POS = new Vector2(208, 152);
	private Vector2 BRASSERIE_POS = new Vector2(138, 108);
	private Vector2 CASINO_POS = new Vector2(278, 234);
	private Vector2 MOULIN_POS = new Vector2(184, 223);

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		AnimPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		L = GetNode<Label>("Label");
		L.Text = text;
		context = GetNode<Context>("/root/Context");
		RR = GetNode<Sprite>("../YouAreHere");
		
		//Update You are Here
		SetYouAreHerePos(context._GetLocation());
	}
	
	private void SetYouAreHerePos(Locations l) {
		switch(l) {
			case Locations.INTRO:
				RR.Position = INTRO_POS;
				break;
			case Locations.PALUD:
				RR.Position = PALUD_POS;
				break;
			case Locations.CASINO:
				RR.Position = CASINO_POS;
				break;
			case Locations.BRASSERIE:
				RR.Position = BRASSERIE_POS;
				break;
			case Locations.MOULIN:
			case Locations.FLON:
				RR.Position = MOULIN_POS;
				break;
			default:
				break;
		}
	}

	private void _on_MapIcon_pressed() {
		SceneChanger SC = GetNode<SceneChanger>("/root/SceneChanger");
		SC.GotoScene("res://scenes/" + id + ".tscn");
			
		//Update the context
		context._UpdateLocation(id);
	}
	
	private void _on_MapIcon_mouse_entered() {
		if(AnimPlayer == null) {
			_Ready();
		}
		if(L.Text != "") {
			L.Show();
		}
	}


	private void _on_MapIcon_mouse_exited() {
		L.Hide();
	}
	
}





