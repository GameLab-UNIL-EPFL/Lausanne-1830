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

public class Menu : Control
{
	Context context;
	SceneChanger SC;
	MusicPlayer MP;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		context = GetNode<Context>("/root/Context");
		SC = (SceneChanger)GetNode("/root/SceneChanger");
		MP = (MusicPlayer)GetNode("/root/MusicPlayer");
		if(!MP.Music.Playing){
			MP.PlayMusic("Schubert_Sonata13_2.mp3", -12);
		}
	}

	private void _on_Button_pressed() {
		SC.GotoScene("res://scenes/Intro/Intro.tscn");
		MP.ChangeMusic("InsideRoom.mp3", 2);
	}
	
	private void _on_Button2_pressed() {
		SC.GotoScene("res://scenes/Interaction/Credits.tscn");
	}
}







