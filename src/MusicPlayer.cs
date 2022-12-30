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

public class MusicPlayer : Node2D
{
	public AudioStreamPlayer Music;
	private AudioStreamPlayer Music2;
	private AnimationPlayer AP;
	
	public override void _Ready()
	{
		Music = GetNode<AudioStreamPlayer>("Music");
		Music2 = GetNode<AudioStreamPlayer>("Music2");
		AP = GetNode<AnimationPlayer>("AnimationPlayer");
	}
	
	// sets the last animation key of the chosen track to the desired value
	private void _SetAnimKey(string anim_name, string track_name, float db) {
		var animation = AP.GetAnimation(anim_name);
		var track = animation.FindTrack(track_name);
		var last_key = animation.TrackGetKeyCount(track) - 1;
		animation.TrackSetKeyValue(track, last_key, db);
	}

	public void PlayMusic(string fileName, float db = 0) {
		var audioStream = (AudioStream)GD.Load("res://assets/07_sounds/Music/" + fileName);
		Music.Stream = audioStream;
		Music.VolumeDb = db;
		Music.Play();
	}
	
	// Crossfades to a new audio stream, allowing for a smoother transition
	public void ChangeMusic(string fileName, float db = 0) {
		var audioStream = (AudioStream)GD.Load("res://assets/07_sounds/Music/" + fileName);
		if (Music.Playing && Music2.Playing) return;
		
		if (Music2.Playing) {
			// sets the music volume db track's last key to the chosen value
			_SetAnimKey("FadeToTrack1", "Music:volume_db", db);
			GD.Print("here");
			Music.Stream = audioStream;
			Music.Play();
			AP.Play("FadeToTrack1");
			//Music.VolumeDb = db;
		} else {
			_SetAnimKey("FadeToTrack2", "Music2:volume_db", db);
			Music2.Stream = audioStream;
			Music2.Play();
			AP.Play("FadeToTrack2");
			//Music2.VolumeDb = db;
		}
	}
		
	public void MusicFadeIn(float db) {
		_SetAnimKey("FadeIn", "Music:volume_db", db);
		_SetAnimKey("FadeIn", "Music2:volume_db", db);
		AP.Play("FadeIn");
	}
		
	public void MusicFadeOut() {
		AP.Play("FadeOut");
	}
}
