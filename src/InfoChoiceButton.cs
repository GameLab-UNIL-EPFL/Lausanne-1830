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
