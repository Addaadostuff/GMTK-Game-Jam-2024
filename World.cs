using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class World : Node2D
{
	public Action<Texture2D, Texture2D, Texture2D> UpdateDisplay { get; set; }
	internal Timer dropPieceTimer;
	private PackedScene checkpointScene;
	private PackedScene checkpoinshadowtScene;
	private List<PackedScene> pieceSceneCollection = new List<PackedScene>();
	private List<Piece> pieceWaitlist = new List<Piece>();
	private Piece selectedPiece;
	private Random random = new Random((DateTime.Now.ToString() + "GMTK").GetHashCode());
	private int checkpointInterval = 200;
	private int nextCheckpoint = 148;
	private int lvl = 1;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		base._Ready();
		Scoreline = GetNode<Line2D>("%Scoreline");
		dropPieceTimer = GetNode<Timer>("%DropPieceTimer");
		dropPieceTimer.WaitTime = 2;
		dropPieceTimer.Timeout += dropPieceTimer_Timeout;

		checkpointScene = GD.Load<PackedScene>(@"res://checkpoint.tscn");
		checkpoinshadowtScene = GD.Load<PackedScene>(@"res://checkpoint_shadow.tscn");
		pieceSceneCollection.Add(GD.Load<PackedScene>(@"res://Scenes/Block_Piece.tscn"));
		pieceSceneCollection.Add(GD.Load<PackedScene>(@"res://Scenes/L_Piece.tscn"));
		pieceSceneCollection.Add(GD.Load<PackedScene>(@"res://Scenes/RL_Piece.tscn"));
		pieceSceneCollection.Add(GD.Load<PackedScene>(@"res://Scenes/ST_Piece.tscn"));
		pieceSceneCollection.Add(GD.Load<PackedScene>(@"res://Scenes/S_Piece.tscn"));
		pieceSceneCollection.Add(GD.Load<PackedScene>(@"res://Scenes/T_Piece.tscn"));
		pieceSceneCollection.Add(GD.Load<PackedScene>(@"res://Scenes/Z_Piece.tscn"));

		prepareWaitlist();
		UpdateDisplay?.Invoke(pieceWaitlist[0].GetChild<Sprite2D>(0).Texture, pieceWaitlist[1].GetChild<Sprite2D>(0).Texture, pieceWaitlist[2].GetChild<Sprite2D>(0).Texture);
		dropPieceTimer.Start();
	}

	private void prepareWaitlist()
	{
		for (int i = 0; i < 20; i++)
		{
			AddObjecttoWaitlist();
		}
	}

	private void AddObjecttoWaitlist()
	{
		pieceWaitlist.Add(pieceSceneCollection[random.Next(0, pieceSceneCollection.Count)].Instantiate() as Piece);
	}

	private void Spawn_Piece()
	{

		System.Console.WriteLine("Spawn_Piece");
		selectedPiece = null;
		if (pieceWaitlist.Count == 0)
		{
			GD.Print("Spiel Ende");
			return;
		}

		selectedPiece = pieceWaitlist.First();
		pieceWaitlist.Remove(selectedPiece);
		AddObjecttoWaitlist();
		UpdateDisplay?.Invoke(pieceWaitlist[0].GetChild<Sprite2D>(0).Texture, pieceWaitlist[1].GetChild<Sprite2D>(0).Texture, pieceWaitlist[2].GetChild<Sprite2D>(0).Texture);
		var minspawnpoint = -0;
		var spawn = Math.Min(minspawnpoint, highestpoint - 500);
		selectedPiece.Position = new Vector2(320, spawn);

		GD.Print("GravityScale now: " + selectedPiece.GravityScale);
		//TODO:  Scaling prüfen wie ich das object dauerhaft gescaled bekomme
		//Selected_Piece.GlobalScale = new Vector2(2, 2);		
		selectedPiece.On_Settled = Object_Settled;
		selectedPiece.ContactMonitor = true;
		selectedPiece.MaxContactsReported = 1;
		AddChild(selectedPiece);
		switch (random.Next(0, 4))
		{
			case 3:
				selectedPiece.GlobalRotationDegrees = 270;
				break;
			case 2:
				selectedPiece.GlobalRotationDegrees = 180;
				break;
			case 1:
				selectedPiece.GlobalRotationDegrees = 90;
				break;
			case 0:
			default:
				break;
		}
		selectedPiece.GravityScale = lvl * 0.1f;

	}

	private void CheckforCheckpoint()
	{
		if (score >= nextCheckpoint)
		{
			GD.Print("Checkpoint reached: " + nextCheckpoint);
			CallDeferred(nameof(TriggerCheckpoint));
			lvl++;
			nextCheckpoint += (checkpointInterval + (lvl * 10));
			GD.Print("nextCheckpoint " + nextCheckpoint);
			GD.Print("lvl " + lvl);
			CallDeferred(nameof(DrawCheckpointShadow), (int)(nextCheckpoint * -1));
		}
	}

	private void DrawCheckpointShadow(int ding)
	{
		GD.Print("DrawCheckpointShadow: " + ding);
		var shadow = checkpoinshadowtScene.Instantiate() as TileMapLayer;
		shadow.Position = new Vector2(150, ding - 32);
		AddChild(shadow);
	}
	private void TriggerCheckpoint()
	{
		GD.Print("Checkpoint erreicht bei Score: " + score);
		var checkpoint = checkpointScene.Instantiate() as StaticBody2D;
		checkpoint.Position = new Vector2(150, highestpoint - 32);
		AddChild(checkpoint);

	}

	private void dropPieceTimer_Timeout()
	{
		CallDeferred("Spawn_Piece");
		//Spawn_Piece();
		dropPieceTimer.Stop();
	}

	public void Object_Settled()
	{
		CallDeferred(nameof(Spawn_Piece));
	}

	private long lastinput;
	private long ticks_diff = TimeSpan.FromMilliseconds(100).Ticks;

	public Line2D Scoreline { get; set; }
	public Action<int> UpdateScore { get; set; }

	int children_count = 0;
	float highestpoint = 600f;
	private int score;

	public override void _Process(double delta)
	{
		var children = GetChildren().Where(c => c is Piece);
		if (children.Count() != children_count)
		{
			children_count = children.Count();
			GD.Print("_Process Piece Children: " + children_count);
			foreach (Node item in children)
			{
				if (item is Piece piece)
				{
					if (piece.Hashit && piece.GlobalPosition.Y < highestpoint)
					{
						highestpoint = piece.GlobalPosition.Y;
						Scoreline.GlobalPosition = new Vector2(0, highestpoint);
						score = (int)(highestpoint * -1) + 500;
						UpdateScore?.Invoke(score);
						CheckforCheckpoint();
					}
				}
			}
		}


		#region inputs
		if (selectedPiece != null)
		{
			if (Input.IsActionJustPressed("Slowdown"))
			{
				GD.Print("Slowdown");
				selectedPiece.ApplyCentralForce(new Vector2(0, -500));
			}
			if (Input.IsActionJustPressed("Right"))
			{
				selectedPiece.MoveRigh = true;
				lastinput = DateTime.Now.Ticks;
			}
			if (Input.IsActionJustPressed("Left"))
			{
				selectedPiece.MoveLeft = true;
				lastinput = DateTime.Now.Ticks;
			}
			if (Input.IsActionPressed("Down"))
			{
				selectedPiece.Speedup = true;
				lastinput = DateTime.Now.Ticks;
			}
			if (Input.IsActionJustPressed("Up"))
			{
				var newvalue = selectedPiece.GlobalRotationDegrees;
				if (newvalue < 270)
				{
					newvalue += 90;
				}
				else
				{
					newvalue = 0;
				}
				selectedPiece.GlobalRotationDegrees = newvalue;
			}
		}
		#endregion inputs
	}


}
