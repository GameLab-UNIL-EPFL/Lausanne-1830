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

public class NotebookController : TextureButton
{
	private AnimationPlayer AnimPlayer;
	private TextureButton MB;
	private Notebook NB;
	private Context context;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		AnimPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		MB = GetNode<TextureButton>("MapButton");
		NB = GetNode<Notebook>("../../Notebook");
		context = GetNode<Context>("/root/Context");
		
		if(!MB.IsConnected("pressed", NB, "_on_MapB_pressed")){
			MB.Connect("pressed", NB, "_on_MapB_pressed");
		}
	}
	
	private void _on_Player_SlideInNotebookController() {
		if(AnimPlayer == null) {
			_Ready();
		}
		if(context._GetQuest() == Quests.TUTORIAL) {
			AnimPlayer.Play("SlideCarnet");
		} else {
			AnimPlayer.Play("Slide");
		}
	}
}

