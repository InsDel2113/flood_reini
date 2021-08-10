using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

public class MoneyHud : Panel
{
	public Label Label;

	public MoneyHud()
	{
		Label = Add.Label( "0", "value" );
	}

	public override void Tick()
	{
		var player = Local.Pawn as FloodPlayer;
		if ( player == null )
		{
			return;
		}

		Label.Text = $"${player.Money}";
	}
}
