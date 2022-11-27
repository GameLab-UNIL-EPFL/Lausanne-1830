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

public class FountainAudio : AudioStreamPlayer2D {
	private Player p;
	//private StaticBody2D fountain;
	private float baseVolume;
	private const float RANDOM_EMPIRICAL_QUOTIENT = 15.0f;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		p = GetNode<Player>("../YSort/Player");
		//fountain = GetNode<StaticBody2D>("../YSort/Fountain");
		baseVolume = VolumeDb;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(float delta) {
		//Honestly, this was devised completely at random
		VolumeDb = baseVolume - (Position.DistanceTo(p.Position)/RANDOM_EMPIRICAL_QUOTIENT);
	}
}
