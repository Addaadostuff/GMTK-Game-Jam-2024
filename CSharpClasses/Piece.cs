using System.Diagnostics;
using Godot;
using System;
using System.Threading;
using System.Timers;

public partial class Piece : RigidBody2D
{
	[Export]
	public float Playermovement = 16f;

	[Export]
	public bool Hashit { get; private set; } = false;
	public bool Speedup { get; set; }
	public bool MoveLeft { get; set; }
	public bool MoveRigh { get; set; }
	public Action On_Settled { get; internal set; }

	public override void _Ready()
	{
		base._Ready();
		GravityScale = 0.1f;
		Mass = 2;
		DisableMode = DisableModeEnum.KeepActive;
		CanSleep = true;
		LockRotation = false;
		LinearDamp = 0.8f;
		//PhysicsMaterialOverride.Bounce = 1f;

	}
	public override void _Process(double delta)
	{



	}
	public override void _IntegrateForces(PhysicsDirectBodyState2D state)
	{
		if (MoveRigh)
		{
			GlobalPosition = new Vector2(this.GlobalPosition.X + Playermovement, this.GlobalPosition.Y);
			MoveRigh = false;
		}
		if (MoveLeft)
		{
			GlobalPosition = new Vector2(this.GlobalPosition.X - Playermovement, this.GlobalPosition.Y);
			MoveLeft = false;
		}
		if (Speedup)
		{
			//GD.Print("Speedup");
			ApplyCentralForce(new Vector2(0, 100f));
			//GravityScale += GravityScale * 0.2f;
			//GlobalPosition = new Vector2(this.GlobalPosition.X, this.GlobalPosition.Y + Playermovement);
			Speedup = false;
		}
	}

	public void _on_body_entered(Node node)
	{
		if (!Hashit)
		{
			GD.Print("hit with grav: " + GravityScale + " damp: " + LinearDamp);
			On_Settled?.Invoke();
			var ding = new PhysicsMaterial();
			ding.Friction = 1;
			ding.Rough = true;
			ding.Bounce = 1;
			ding.Absorbent = true;
			this.PhysicsMaterialOverride = ding;
			//this.PhysicsMaterialOverride.Absorbent = true;
			//GravityScale = 1f;
			Hashit = true;
			// if (node is Piece piece)
			// {
			// 	CallDeferred(nameof(Dostuff), piece);
			// }
		}
	}

	internal void Dostuff(Piece piece)
	{
		var pos = piece.GlobalPosition;
		piece.GetParent().RemoveChild(piece);
		AddChild(piece);
		//piece.GlobalPosition = pos;		
	}


}
