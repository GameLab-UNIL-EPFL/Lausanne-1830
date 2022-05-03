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

public class NumberButton : Button {
	
	[Signal]
	public delegate void InsertNumber(int num);
	
	[Signal]
	public delegate void RemoveNumber();
	
	[Signal]
	public delegate void EnterNumber();
	
	private int number;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		if(Text != "delete" && Text != "enter") {
			number = int.Parse(Text);
			Connect("pressed", this, "_on_Num_Pressed");
		} else {
			number = -1;
			if(Text == "delete") {
				Connect("pressed", this, "_on_Delete_Pressed");
			} else {
				Connect("pressed", this, "_on_Enter_Pressed");
			}
		}
	}
	
	private void _on_Delete_Pressed()  {
		EmitSignal(nameof(RemoveNumber));
	}
	
	private void _on_Enter_Pressed() {
		EmitSignal(nameof(EnterNumber));
	}
	
	private void _on_Num_Pressed() {
		// Propagate pressed message with number attached
		EmitSignal(nameof(InsertNumber), number);
	}
}
