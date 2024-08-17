using Godot;
using System;
using System.Collections.Generic;

public partial class World : Node2D
{
	internal Timer dropPieceTimer;
	private List<PackedScene> packedSceneCollection = new List<PackedScene>();
	private Queue<Piece> pieceQueue = new Queue<Piece>();
	private Piece selectedPiece;
	private Random random = new Random((DateTime.Now.ToString() + "GMTK").GetHashCode());
	private int counter = 0;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		base._Ready();
		var tree = GetTree();
		dropPieceTimer = GetNode<Timer>("%DropPieceTimer");
		dropPieceTimer.WaitTime = 2;
		dropPieceTimer.Timeout += dropPieceTimer_Timeout;

		packedSceneCollection.Add(GD.Load<PackedScene>(@"res://Scenes/Block_Piece.tscn"));
		packedSceneCollection.Add(GD.Load<PackedScene>(@"res://Scenes/L_Piece.tscn"));
		packedSceneCollection.Add(GD.Load<PackedScene>(@"res://Scenes/RL_Piece.tscn"));
		packedSceneCollection.Add(GD.Load<PackedScene>(@"res://Scenes/ST_Piece.tscn"));
		packedSceneCollection.Add(GD.Load<PackedScene>(@"res://Scenes/S_Piece.tscn"));
		packedSceneCollection.Add(GD.Load<PackedScene>(@"res://Scenes/T_Piece.tscn"));
		packedSceneCollection.Add(GD.Load<PackedScene>(@"res://Scenes/Z_Piece.tscn"));

		for (int i = 0; i < 150; i++)
		{
			EnqueObject();
		}
		dropPieceTimer.Start();
	}

	private void EnqueObject()
	{
		pieceQueue.Enqueue(packedSceneCollection[random.Next(0, packedSceneCollection.Count)].Instantiate() as Piece);
	}

	private void Spawn_Piece()
	{
		System.Console.WriteLine("Spawn_Piece");
		selectedPiece = null;
		if (pieceQueue.Count == 0)
		{
			GD.Print("Spiel Ende");
			return;
		}
		selectedPiece = pieceQueue.Dequeue();
		counter++;
		selectedPiece.Position = new Vector2(320, -20);
		selectedPiece.IsMovable = true;
		//TODO:  Scaling prüfen wie ich das object dauerhaft gescaled bekomme
		//Selected_Piece.GlobalScale = new Vector2(2, 2);		
		selectedPiece.On_Settled = Object_Settled;
		selectedPiece.ContactMonitor = true;
		selectedPiece.MaxContactsReported = 1;
		selectedPiece.Name = "piece" + counter;
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
	}

	private void dropPieceTimer_Timeout()
	{
		CallDeferred("Spawn_Piece");
		//Spawn_Piece();
		dropPieceTimer.Stop();
	}

	public void Object_Settled(Piece oldPiece)
	{
		CallDeferred("Spawn_Piece");
		//Spawn_Piece();
	}

	private long lastinput;
	private long ticks_diff = TimeSpan.FromMilliseconds(50).Ticks;
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (selectedPiece != null && lastinput < DateTime.Now.Ticks - ticks_diff)
		{
			if (Input.IsActionPressed("Right"))
			{
				selectedPiece.MoveRigh = true;
				lastinput = DateTime.Now.Ticks;
			}
			if (Input.IsActionPressed("Left"))
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
	}


}
