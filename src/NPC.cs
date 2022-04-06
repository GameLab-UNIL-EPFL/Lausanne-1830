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
using System.Collections.Generic;

public class NPC : KinematicBody2D {
	
	public enum Direction {RIGHT, LEFT, UP, DOWN};
	private Vector2 RightDir = new Vector2(1.0f, 0.0f);
	private Vector2 LeftDir = new Vector2(-1.0f, 0.0f);
	private Vector2 UpDir = new Vector2(0.0f, -1.0f);
	private Vector2 DownDir = new Vector2(0.0f, 1.0f);
	
	[Signal]
	public delegate void EndDialogue();
	
	[Signal]
	public delegate void StartDialogue();
	
	private const int MAX_CHAR_PER_LINE = 35;
	private const int MAX_LINES = 3;
	
	private DialogueController DC;
	private QuestController QC;
	
	private string[] InnerLines;
	private int InnerLinesCount = 0;
	
	//Used to display text
	private TextBox TB;
	
	//For Idle animation starting
	private AnimationTree AT;
	private AnimationNodeStateMachinePlayback AS;
	private bool IdleAnimIsPlaying = false;
	
	//Randomness
	private Random random = new Random();
	
	//For wandering
	private bool isWandering = false;
	private Vector2 Velocity = Vector2.Zero;
	private Vector2 InputVec = Vector2.Zero;
	private const int ACC = 950;
	private const int FRIC = 1000;
	private float cooldown = 0.0f;
	private float wanderTime = 1.0f;
	private Vector2 NextDir = Vector2.Zero;
	
	private Vector2 InitialDirection = Vector2.Zero;
	
	[Export]
	public Direction InitDir = Direction.DOWN;
	[Export]
	public bool CanTurn = true;
	[Export]
	public int ProbRight = 2; //max weight of right movement
	[Export]
	public int ProbLeft = 2; //max weight of left movement
	[Export]
	public int ProbUp = 2; //max weight of up movement
	[Export]
	public int ProbDown = 2; //max weight of down movement
	[Export]
	public bool CanWander = false;
	[Export]
	public float WanderingCooldown = 5.0f;
	[Export]
	public float WanderingDist = 1.0f;
	[Export]
	public int WalkSpeed = 50;
	[Export]
	public int IdleStartProb = 25; //0 to 100
	[Export]
	public string AutoDialogueID;
	[Export]
	public string DemandDialogueID;
	[Export]
	public bool HasAutoDialogue = true;
	[Export]
	public bool HasDemandDialogue = true;
	[Export]
	public bool isQuestNPC = false;
	[Export]
	public bool isBrewer = false;
	[Export]
	public float BrewBadThreshold = 40f;
	[Export]
	public float BrewPerfectThreshold = 90f;
	[Export]
	public bool isTrueschel = false;
	
	private bool inDialogue = false;
	private bool inAutoDialogue = false;
	
	private Context context;
	
	private string[] BrewQuestText() {
		//Check if the game has been played yet
		if(context._CheckBrewBurn() == -1.0f) {
			return FormatText("Le travail de brasseur peut être très fatiguant.¢" +
				"Je ne sais pas si on va réussir à finir cette cuvée à temps...¢" +
				"Tu veux nous aider à brasser la bière ?¢" +
				"Merci!");
		} else if(context._CheckBrewBurn() < BrewBadThreshold) {
			return FormatText("Mais tu as fait n'importe quoi !¢" +
				"La bière est complètement brûlée !¢"+
				"Mme Trüschel ne sera pas du tout contente...");
		} else if(context._CheckBrewBurn() < BrewPerfectThreshold) {
			return FormatText("Bien...¢" +
				"La bière n'est pas trop brûlée et encore vendable.¢"+
				"Merci pour l'aide.");
		} else {
			return FormatText("Wow la bière est parfaite!¢" +
				"Tu as vraiment un don pour ça.¢"+
				"Merci énormément pour l'aide!¢"+
				"Allez parler à Mme Trüschel...¢"+
				"Je pense qu'elle vous remerciera.");
		}
	}
	
	private string[] QuestText(InfoValue_t res) {
		string[] outliers = res.Outliers().ToArray();
		if(res.IsCorrect()) {
			if(context._IsGameComplete()) {
				return FormatText("Alors...¢"+
				"Voyons voir ce registre.¢"+
				"Bravo ! Vous avez fait du bon travail !¢"+
				"Je vais garder ça dans nos documents importants.¢"+
				"Peut-être qu'un jour des historiens pourront utiliser ces informations¢"+
				"et en faire un jeu vidéo.¢"+
				"Je vous ouvre la porte. Vous pouvez aller les remettre à l'intérieur.");
			} 
			return FormatText("Alors...¢"+
			"Il me semble que toutes les informations sur cette personne sont correctes.¢"+
			"Il faut maintenant passer aux prochaines.¢"+
			"Plus que " + context._GetNCorrectTabs() + " pages à compléter!");
		}
		
		string d = "Voyons voir ce registre.¢" +
		//"Il y a encore plusieurs données qui sont eronnées, comme:¢";
		"Sur la page actuelle...¢"+
		"Il y a encore ";
		var i = 0;
		foreach(var o in outliers) {
			//d += o + ", ";
			i++;
		}
		d += i;
		//d += "et ...¢" + "Je crois que les ai tous cités.¢";
		if(i>1) {
			d += " données eronnées.¢";
			d += "Revenez me voir lorsque vous les aurez corrigées.";
		} else {
			d += " donnée eronnée.¢";
			d += "Revenez me voir lorsque vous l'aurez corrigée.";
		}

		return FormatText(d);
	}
	
	private void LookInInitalDir() {
		InputVec = InitialDirection;
		HandleMovement(0.03f);
		InputVec = Vector2.Zero;
		HandleMovement(0.03f);
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		Show();
		//Fetch the scene's Dialogue controller and the TextBox
		context = GetNode<Context>("/root/Context");
		DC = Owner.GetNode<DialogueController>("DialogueController");
		QC = Owner.GetNode<QuestController>("QuestController");
		TB = GetNode<TextBox>("TextBox");
		AT = GetNode<AnimationTree>("AnimationTree");
		AS = (AnimationNodeStateMachinePlayback)AT.Get("parameters/playback");
		//Sanity Check
		if(HasDemandDialogue || HasAutoDialogue) {
			if(DC == null) {
				throw new Exception("Every scene must have its own dialogue controller!!");
			} 
		}
		
		//Set initial direction
		switch(InitDir) {
			case Direction.LEFT:
				InitialDirection = LeftDir;
				break;
			case Direction.RIGHT:
				InitialDirection = RightDir;
				break;
			case Direction.UP:
				InitialDirection = UpDir;
				break;
			default:
			case Direction.DOWN:
				InitialDirection = DownDir;
				break;
		}
		LookInInitalDir();
	}
	
	public void _StopTalking() {
		if(!inDialogue) {
			TB._HideText();
		}
	}
	
	private void HandleMovement(float delta) {
		//Update velocity
		if(InputVec == Vector2.Zero) {
			Velocity = Velocity.MoveToward(Vector2.Zero, FRIC * delta);
		} else {
			//Set blend positions for animation
			AT.Set("parameters/Walk/blend_position", InputVec);
			AT.Set("parameters/Idle/blend_position", InputVec);
			Velocity = Velocity.MoveToward(InputVec * WalkSpeed, ACC * delta);
		}
	}
	
	//Generate a new random position within the wandering distance
	private Vector2 NewInputVec() {
		float horizontalMov = 0.0f;
		float verticalMov = 0.0f;
		
		//Check if a collision has happened since the last movement
		if(NextDir != Vector2.Zero) {
			//If so, move away from the collision
			horizontalMov = NextDir[0];
			verticalMov = NextDir[1];
			NextDir = Vector2.Zero;
		} else {
			horizontalMov = (float)(random.Next(ProbRight) - random.Next(ProbLeft));
			verticalMov = (float)(random.Next(ProbDown) - random.Next(ProbUp));
		}
		
		if((random.Next(2) > 0 || verticalMov == 0.0f) && horizontalMov != 0.0f) {
			return new Vector2(horizontalMov, 0.0f);
		}
		return new Vector2(0.0f, verticalMov);
	}
	
	private void StopWandering() {
		wanderTime = 0.0f;
		isWandering = false;
		InputVec = Vector2.Zero;
		Velocity = Vector2.Zero;
		AS.Travel("Idle");
		
		//Start cooldown
		cooldown = (random.Next(100)/100.0f) * WanderingCooldown;
	}
	
	public override void _Process(float delta) {
		if(!IdleAnimIsPlaying) {
			if(random.Next(100) < IdleStartProb) {
				AT.Active = true;
				IdleAnimIsPlaying = true;
			}
		}
		//Check for movement
		if(!inDialogue && IdleAnimIsPlaying && CanWander) {
			if(cooldown > 0.0f) {
				cooldown -= delta;
			} else {
				cooldown = 0.0f;
				//Check for destination
				if(!isWandering) {
					AS.Travel("Walk");
					isWandering = true;
					wanderTime = (random.Next(100)/100.0f) * WanderingDist;
					//Set input vec
					InputVec = NewInputVec();
				} else {
					wanderTime -= delta;
					
					//Check for collision
					if(NextDir == Vector2.Zero) {
						if(IsOnWall() || IsOnFloor() || IsOnCeiling()) {
							KinematicCollision2D col = GetLastSlideCollision();
							NextDir = (Position - col.Position).Normalized(); 
						}
					}
					
					//Check if destination was reached
					if(wanderTime <= 0.0f) {
						StopWandering();
					}
				}
				
			}
		}
		HandleMovement(delta);
		
		if(Velocity == Vector2.Zero) {
			//Goto idle
			AS.Travel("Idle");
		} else {
			//Scale velocity and move
			Velocity = MoveAndSlide(Velocity);
			AS.Travel("Walk");
		}
	}
	
	/**
	 * @brief Handles what happens when the player enters the ListenBox area,
	 * meaning that the player has entered the zone where they should be able to 
	 * hear the NPC's dialogue. This also causes the NPC to subscribe to the player
	 * allowing for onDemand dialogue to take place.
	 * @param tb, the TalkBox of the player that has entered the zone.
	 */
	private void _on_ListenBox_area_entered(Area2D tb) {
		//Check if player is around
		if(tb.Owner is Player) {
			Player p = (Player)tb.Owner;
			//Subscribe to the player
			p._Subscribe(this);
		}
	}
	
	public bool _RequestAutoDialogue() {
		//Show auto dialogue if the NPC has one
		if(HasAutoDialogue && !inDialogue && !inAutoDialogue) {
			//Fetch the right dialogue
			string next = DC._StartDialogue(AutoDialogueID, true);
			
			//Show it in the box
			if(next != null) {
				TB._ShowText(next);
			}
			
			//Set state
			inAutoDialogue = true;
		}
		return inAutoDialogue;
	}
	
	public bool _EndAutoDialogue() {
		//Hide auto dialogue if the NPC is in one
		if(HasAutoDialogue && inAutoDialogue) {
			DC._EndDialogue();
			inAutoDialogue = false;
			//Hide the text box
			TB._HideText();
		}
		return inAutoDialogue;
	}
	
	/**
	 * @brief Handles what happens the the player is no longer in range to hear dialogue.
	 * This causes the NPC to unsubscribe to the player, making it no longer
	 * possible to generate onDemand dialogue.
	 * @param tb, the TalkBox of the player who has left the zone.
	 */
	private void _on_ListenBox_area_exited(Area2D tb) {
		if(tb.Owner is Player) {
			Player p = (Player)tb.Owner;
			//Unsubscribe to the player
			p._Unsubscribe(this);
			
			//End dialogue for DialogueController when needed
			if(HasAutoDialogue && inAutoDialogue) {
				DC._EndDialogue();
				inAutoDialogue = false;
			}
			
			//Hide the text box
			TB._HideText();
		}
	}
	
	private string[] FormatText(string text) {
		//Sanity check
		if(text == null) {
			throw new Exception("Can't format null");
		}
		
		string newText = "";
		int count = MAX_CHAR_PER_LINE;
		int lines = MAX_LINES;
		List<string> textLines = new List<string>();
		foreach(char c in text) {
			// Ignore new lines
			if(c == '\n') continue;
			
			// Only 3 lines per entry
			if(lines == 0 || c == '¢') {
				textLines.Add(newText);
				newText = "";
				lines = MAX_LINES;
				continue;
			}
			
			// Max 25 characters per line
			if(count-- == 0) {
				count = MAX_CHAR_PER_LINE;
				lines--;
			} 
			newText += c;
		}
		if(textLines.Count > 1) {
			textLines.Add(newText);
		}
		return textLines.ToArray();
	}
	
	private void BeginDialogue(Player player, ref string d) {
		if(isTrueschel && context._CheckBrewBurn() != -1.0f) {
			if(context._CheckBrewBurn() < BrewBadThreshold) {
				DemandDialogueID = "demandAngeliqueBad";
			} else {
				DemandDialogueID = "demandAngeliqueGood";
			}
		}
		inDialogue = true;
		player._StartDialogue();
		if(isBrewer) {
			InnerLines = BrewQuestText();
			InnerLinesCount = InnerLines.Length;
			d = InnerLines[0];
		} else {
			d = DC._StartDialogue(DemandDialogueID);
		}
		
		//Turn to player
		if(CanTurn) {
			InputVec = (player.Position - Position).Normalized();
			HandleMovement(0.03f);
			InputVec = Vector2.Zero;
			HandleMovement(0.03f);
		}
		
	}
	
	private void FinishDialogue(Player player) {
		inDialogue = false;
		TB._HideText();
		player._EndDialogue();
		DC._EndDialogue();
		
		//Start the brewing minigame
		if(isBrewer) {
			if(context._CheckBrewBurn() == -1.0f) {
				SceneChanger SC = GetNode<SceneChanger>("/root/SceneChanger");
				SC.GotoScene("res://scenes/Brasserie/BrewGame.tscn");
			} else {
				isBrewer = false;
				isQuestNPC = false;
				context._EndBrewGameCutscene();
			}
		}
		
		if(!CanWander) {
			LookInInitalDir();
		}
	}
	
	/**
	 * @brief Called by the player when the NPC should be notified of an interaction.
	 */
	public void _Notify(Player player) {
		TB._HideAll();
		if(HasDemandDialogue) {
			string d = null;
			
			if(InnerLinesCount != 0) {
				d = InnerLines[InnerLines.Length - InnerLinesCount--];
			} else {
				//Check if this is the start of a dialogue
				if(!inDialogue) {
					BeginDialogue(player, ref d);
					
					if(d == null) {
						throw new Exception("No starting dialogue given");
					}
				} else {
					d = DC._NextDialogue();
					
					//Check if it's the end of the dialogue
					if(d == null) {
						FinishDialogue(player);
						return;
					}
				}
				
				if(!isBrewer) {
					//Format the text to fit in the dialogue boxes
					InnerLines = FormatText(d);
					InnerLinesCount = InnerLines.Length;
				}
				
				//Update the dialogue
				if(InnerLinesCount != 0) {
					d = InnerLines[InnerLines.Length - InnerLinesCount--];
				}
			}
			
			//Show it in the box
			if(d != null) {
				TB._ShowText(d);
				TB._ShowPressE();
			}
		}
	}
	
	public InfoValue_t _CompareSolutions(CharacterInfo_t characterInfo, int tabId) {
		CharacterInfo_t solution = QC._QueryQuestSolution(tabId);
		solution = QC._QueryQuestSolution(tabId);
		return QC._CompareCharInfo(solution, characterInfo);
	}
	
	public InfoValue_t _EvaluateQuest(Player player, CharacterInfo_t characterInfo, int tabId) {
		InfoValue_t res = _CompareSolutions(characterInfo, tabId);
		
		if(!inDialogue) {
			inDialogue = true;
			player._StartDialogue();
			QC._InitBuffer(QuestText(res));
			TB._ShowText(QC._NextLine());
			TB._ShowPressE();
		} else {
			string l = QC._NextLine();
			//Check that the dialogue isn't over
			if(l == null) {
				TB._HideText();
				player._EndDialogue();
				inDialogue = false;
			} else {
				TB._ShowText(l);
				TB._ShowPressE();
			}
		}
		return res;
	}
}

