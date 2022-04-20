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

public class InfoChoiceButton : Button {
	[Signal]
	public delegate void UpdateNotebookInfo(string newVal);
	
	private void _on_Pressed() {
		EmitSignal(nameof(UpdateNotebookInfo), Text);
	}
	
	// Setup the button to look like the notebook info
	private void SetupButton() {
		Flat = true;
		Align = 0; // Align left
		Set("focus_mode", 0); // None focus mode
		Set("custom_colors/font_color", NotebookInfo.C1);
		Set("custom_colors/font_color_hover", NotebookInfo.Hover);
		Set("custom_fonts/font", ResourceLoader.Load("res://assets/05_fonts/InfoFont.tres"));
		
		//Connect to a trigger function to update the NotebookInfo
		Connect("pressed", this, "_on_Pressed");
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		SetupButton();
	}
}
