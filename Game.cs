using Godot;
using System;

public partial class Game : Control
{
	public TextureRect NextPiece_0_display { get; set; }
	public TextureRect NextPiece_1_display { get; set; }
	public TextureRect NextPiece_2_display { get; set; }
	public Label ScoreLabel { get; set; }
	public World Gameworld { get; private set; }

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		NextPiece_0_display = GetNode<TextureRect>("%NextPiece");
		NextPiece_1_display = GetNode<TextureRect>("%NextNextPiece");
		NextPiece_2_display = GetNode<TextureRect>("%NextNextNextPiece");
		ScoreLabel = GetNode<Label>("%ScoreLabel");
		Gameworld = GetNode<Node>("%World") as World;
		Gameworld.UpdateDisplay += UpdateDisplay;
		Gameworld.UpdateScore += UpdateScore;
	}

	public void UpdateDisplay(Texture2D item_0, Texture2D item_1, Texture2D item_2)
	{
		NextPiece_0_display.Texture = item_0;
		NextPiece_1_display.Texture = item_1;
		NextPiece_2_display.Texture = item_2;
	}

	public void UpdateScore(int score)
	{
		ScoreLabel.Text = score.ToString();
	}

}
