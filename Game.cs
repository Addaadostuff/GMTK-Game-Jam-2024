using Godot;
using System;

public partial class Game : Control
{
	public TextureRect NextPiece_0_display { get; set; }
	public TextureRect NextPiece_1_display { get; set; }
	public TextureRect NextPiece_2_display { get; set; }

	private AudioStreamPlayer menu_sounds;

	public PanelContainer Menu { get; set; }
	public Button Continuebutton { get; set; }
	public SubViewport View { get; set; }
	public Label ScoreLabel { get; set; }
	public PackedScene Gameworld_scene { get; set; }
	private World gameworldinstance;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		ProcessMode = Node.ProcessModeEnum.Always;
		Gameworld_scene = GD.Load<PackedScene>("res://world.tscn");
		NextPiece_0_display = GetNode<TextureRect>("%NextPiece");
		NextPiece_1_display = GetNode<TextureRect>("%NextNextPiece");
		NextPiece_2_display = GetNode<TextureRect>("%NextNextNextPiece");
		menu_sounds = GetNode<AudioStreamPlayer>("%menu_sounds");

		View = GetNode<SubViewport>("%View");
		ScoreLabel = GetNode<Label>("%ScoreLabel");
		Menu = GetNode<PanelContainer>("%Menu");
		Continuebutton = GetNode<Button>("%Continuebutton");


	}

	public void UpdateDisplay(Texture2D item_0, Texture2D item_1, Texture2D item_2)
	{
		NextPiece_0_display.Texture = item_0;
		NextPiece_1_display.Texture = item_1;
		NextPiece_2_display.Texture = item_2;
	}

	public void _on_play_button_pressed()
	{
		menu_sounds.Play();
		Menu.Visible = false;
		Start_Game();
	}

	public void _on_continuebutton_pressed()
	{
		menu_sounds.Play();
		Menu.Visible = false;
		gameworldinstance.Continue();
	}

	public void Start_Game()
	{
		if (IsInstanceValid(gameworldinstance))
		{

			gameworldinstance.Free();
		}
		var ding = Gameworld_scene.Instantiate();
		if (ding is World inst)
		{
			gameworldinstance = inst;
			gameworldinstance.UpdateDisplay += UpdateDisplay;
			gameworldinstance.UpdateScore += UpdateScore;
			gameworldinstance.OnPaused += OnPaused;
		}
		View.AddChild(ding);
	}

	private void OnPaused()
	{
		Menu.Visible = true;
		Continuebutton.Visible = true;
	}

	public void UpdateScore(int score)
	{
		ScoreLabel.Text = score.ToString();
	}

}
