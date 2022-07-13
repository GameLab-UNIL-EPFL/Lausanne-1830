using Godot;
using System;

public class Map : Node2D
{
	private TextureButton office;
	private TextureButton brasserie;
	private TextureButton moulin;
	private TextureButton palud;
	private TextureButton casino;
	private Context context;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		office = GetNode<TextureButton>("office_button");
		brasserie = GetNode<TextureButton>("brasserie_button");
		moulin = GetNode<TextureButton>("moulin_button");
		palud = GetNode<TextureButton>("palud_button");
		casino = GetNode<TextureButton>("casino_button");
		context = GetNode<Context>("/root/Context");
		
	}
	
	private void GrabFocusPos(Locations l) {
		switch(l) {
			case Locations.INTRO:
				office.GrabFocus();
				break;
			case Locations.PALUD:
				palud.GrabFocus();
				break;
			case Locations.CASINO:
				casino.GrabFocus();
				break;
			case Locations.BRASSERIE:
				brasserie.GrabFocus();
				break;
			case Locations.MOULIN:
			case Locations.FLON:
				moulin.GrabFocus();
				break;
			default:
				break;
		}
	}
	

	private void _on_Map_visibility_changed() {
		if(Visible) {
			GrabFocusPos(context._GetLocation());
		}
	}
}



