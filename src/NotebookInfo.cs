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

public class NotebookInfo : Button {
	[Signal]
	public delegate void OpenOptions(string attributeName);
	[Signal]
	public delegate void UpadateNotebook();
	
	[Export]
	public string AttributeName;
	
	public static Color C = new Color("#876853");
	public static Color C1 = new Color("#ceb29f");
	public static Color Hover = new Color("#9c2323");
	private NinePatchRect N;
	private Texture NHover;
	private Texture NTexture;
	private Notebook NB;


	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		// Fetch child node
		N = GetNode<NinePatchRect>("NinePatchRect");
		
		NB = GetNode<Notebook>("../../../Notebook");
		
		NHover = (Texture)GD.Load("res://assets/04_notebook/notebookBox2.png");
		NTexture = (Texture)GD.Load("res://assets/04_notebook/notebookBox.png");
		
		// Sanity Check
		if(AttributeName == null) {
			throw new Exception("NoteBookInfo must have an attribute name!");
		}
		if(Text == ""){
			N.Show();
		}
		
		//Connect signal to notebook
		Connect(nameof(UpadateNotebook), NB, "_on_NotebookInfo_UpdateInfo");
		
	}
	
	private void _on_UpdateInfo(string attribute, string newVal) {
		// Check that the update signal was for this info
		if(attribute == AttributeName) {
			Text = newVal;
		}
		EmitSignal(nameof(UpadateNotebook));
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



