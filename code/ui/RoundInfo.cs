using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

public class RoundInfo : Panel
{
	public Label Label;

	public RoundInfo()
	{
		Label = Add.Label( "0", "value" );
	}

	public override void Tick()
	{
		var player = Local.Pawn;
		if ( player == null )
		{
			return;
		}

		string curmode = "Unknown";
		int totalTime = 0;

		switch ( FloodGame.Instance.CurrentRound )
		{
			case FloodGame.Round.PreGame:
				curmode = "Pregame";
				totalTime = FloodGame.Instance.PreGameTime;
				break;

			case FloodGame.Round.Fight:
				curmode = "Fight";
				totalTime = FloodGame.Instance.FightTime;
				break;

			case FloodGame.Round.PostGame:
				curmode = "Postgame";
				totalTime = FloodGame.Instance.PostGameTime;
				break;
		}
		Label.Text = FloodGame.Instance.RoundTime.ToString() + "/" + totalTime + " - " + curmode;
	}
}
