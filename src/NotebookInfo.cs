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
	private NinePatchRect N;
	private Texture NHover;
	private Texture NTexture;


	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		// Fetch child node
		N = GetNode<NinePatchRect>("NinePatchRect");
		
		NHover = (Texture)GD.Load("res://assets/04_notebook/notebookBox2.png");
		NTexture = (Texture)GD.Load("res://assets/04_notebook/notebookBox.png");
		
		// Sanity Check
		if(AttributeName == null) {
			throw new Exception("NoteBookInfo must have an attribute name!");
		}
		if(Text == ""){
			N.Show();
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
	
	private void _on_NotebookInfo_mouse_entered() {
		N.Texture = NHover;
	}


	private void _on_NotebookInfo_mouse_exited() {
		N.Texture = NTexture;
	}
	
}



