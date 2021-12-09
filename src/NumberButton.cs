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
