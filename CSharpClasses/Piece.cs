using Godot;
using System;

public partial class Piece : RigidBody2D
{
	[Export]
	public float Playermovement = 16f;
	[Export]
	public bool IsMovable = false;
	private bool hashit = false;
	public bool Speedup { get; set; }
	public bool MoveLeft { get; set; }
	public bool MoveRigh { get; set; }
	public Action<Piece> On_Settled { get; internal set; }
	public override void _Ready()
	{
		base._Ready();
		GravityScale = 0.2f;
		Mass = 2;
		DisableMode = DisableModeEnum.KeepActive;
		CanSleep = true;

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
			//ApplyCentralForce(new Vector2(0, 50f));
			GravityScale += GravityScale * 0.2f;
			GlobalPosition = new Vector2(this.GlobalPosition.X, this.GlobalPosition.Y + Playermovement);
			Speedup = false;
		}
	}
	public void _on_body_entered(Node node)
	{
		if (!hashit)
		{
			if (node.Name == "wall-left" || node.Name == "wall-right") return;
			On_Settled?.Invoke(this);
			this.PhysicsMaterialOverride.Absorbent = true;
			hashit = true;
		}
	}
}
