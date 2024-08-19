using Godot;
using System;
using System.Collections.Generic;
using System.IO;
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
	private AudioStreamPlayer ASP_Spin;
	private AudioStreamPlayer ASP_Hit;
	private TileMapLayer DynamicLayer;
	private PackedScene spindsound;
	private int checkpointInterval = 200;
	private int nextCheckpoint = 148;
	private int lvl = 1;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		base._Ready();
		ProcessMode = Node.ProcessModeEnum.Pausable;
		GetTree().Paused = false;
		ASP_Spin = GetNode<AudioStreamPlayer>("%ASP_Spin");
		ASP_Hit = GetNode<AudioStreamPlayer>("%ASP_Hit");
		DynamicLayer = GetNode<TileMapLayer>("%DynamicLayer");


		spindsound = GD.Load<PackedScene>(@"res://checkpoint.tscn");
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

		score = 0;
		UpdateScore(score);

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

			return;
		}

		selectedPiece = pieceWaitlist.First();
		pieceWaitlist.Remove(selectedPiece);
		AddObjecttoWaitlist();
		UpdateDisplay?.Invoke(pieceWaitlist[0].GetChild<Sprite2D>(0).Texture, pieceWaitlist[1].GetChild<Sprite2D>(0).Texture, pieceWaitlist[2].GetChild<Sprite2D>(0).Texture);
		var minspawnpoint = -0;
		var spawn = Math.Min(minspawnpoint, highestpoint - 500);
		selectedPiece.Position = new Vector2(320, spawn);

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
			CallDeferred(nameof(TriggerCheckpoint));
			lvl++;
			nextCheckpoint += (checkpointInterval + (lvl * 10));
		}
	}

	private void DrawCheckpointShadow(int ding)
	{
		var shadow = checkpoinshadowtScene.Instantiate() as TileMapLayer;
		shadow.Position = new Vector2(176, ding - 32);
		AddChild(shadow);
	}
	private void TriggerCheckpoint()
	{

		var checkpoint = checkpointScene.Instantiate() as StaticBody2D;
		checkpoint.Position = new Vector2(176, highestpoint - 32);
		// TODO: draw on tilemap
		//altas id 1 
		//atlas coordinate 2,4
		var tilepos1 = DynamicLayer.LocalToMap(new Vector2(176, highestpoint - 32));
		tilepos1.X = 11;
		while (tilepos1.Y < 21)
		{
			tilepos1.Y++;
			DynamicLayer.SetCell(tilepos1, 1, new Vector2I(2, 4));
			DynamicLayer.SetCell(new Vector2I(29, tilepos1.Y), 1, new Vector2I(2, 4));
		}
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
		ASP_Hit.Play();
		CallDeferred(nameof(Spawn_Piece));
	}

	private long lastinput;
	private long ticks_diff = TimeSpan.FromMilliseconds(100).Ticks;

	public Line2D Scoreline { get; set; }
	public Action<int> UpdateScore { get; set; }
	public Action OnPaused { get; set; }

	int children_count = 0;
	float highestpoint = 600f;
	private int score = 0;

	public override void _Process(double delta)
	{
		var children = GetChildren().Where(c => c is Piece);
		if (children.Count() != children_count)
		{
			children_count = children.Count();
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
			if (Input.IsActionJustPressed("Quit"))
			{
				GetTree().Paused = true;
				Visible = false;
				OnPaused?.Invoke();
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
				ASP_Spin.Play();
			}
		}
		#endregion inputs
	}

	internal void Continue()
	{
		Visible = true;
		GetTree().Paused = false;
	}

}
