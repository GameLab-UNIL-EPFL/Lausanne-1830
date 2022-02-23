using Godot;
using System;

public class MusicPlayer : Node2D
{
	private AudioStreamPlayer Music;
	
	public override void _Ready()
	{
		Music = GetNode<AudioStreamPlayer>("Music");
	}

	public void PlayMusic(string fileName, float db = 0) {
		var audioStream = (AudioStream)GD.Load("res://assets/07_sounds/Music/" + fileName);
		Music.Stream = audioStream;
		Music.VolumeDb = db;
		Music.Play();
	}
}
