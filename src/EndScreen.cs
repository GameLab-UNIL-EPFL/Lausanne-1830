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

public class EndScreen : Node2D {
	Context context;
	SceneChanger SC;
	
	public override void _Ready() {
		context = GetNode<Context>("/root/Context");
		SC = GetNode<SceneChanger>("/root/SceneChanger");
	}
	
	private void _on_Button_pressed() {
		//Clear context and restart game
		context._Clear();
		SC.GotoScene("res://scenes/Interaction/Menu.tscn");
	}
	
		private void _on_Button2_pressed() {
		OS.ShellOpen("https://lausanne1830.ch/");
	}
}

