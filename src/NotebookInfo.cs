using Godot;
using System;

public class NotebookInfo : Button {
	[Signal]
	public delegate void OpenOptions(string attributeName);
	
	[Export]
	public string AttributeName;
	
	public static Color C = new Color("#876853");
	public static Color C1 = new Color("#ceb29f");
	public static Color Hover = new Color("#9c2323");

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		// Sanity Check
		if(AttributeName == null) {
			throw new Exception("NoteBookInfo must have an attribute name!");
		}
	}
	
	private void _on_UpdateInfo(string attribute, string newVal) {
		// Check that the update signal was for this info
		if(attribute == AttributeName) {
			Text = newVal;
		}
	}
	
	private void _on_NotebookInfo_pressed() {
		EmitSignal(nameof(OpenOptions), AttributeName);
	}
}
