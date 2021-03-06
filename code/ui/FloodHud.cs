using Sandbox;
using Sandbox.UI;

[Library]
public partial class FloodHud : HudEntity<RootPanel>
{
	public FloodHud()
	{
		if ( !IsClient )
			return;

		RootPanel.StyleSheet.Load( "/ui/FloodHud.scss" );

		RootPanel.AddChild<NameTags>();
		RootPanel.AddChild<ChatBox>();
		RootPanel.AddChild<VoiceList>();
		RootPanel.AddChild<KillFeed>();
		RootPanel.AddChild<FloodScoreboard<FloodScoreboardEntry>>();
		RootPanel.AddChild<Health>();
		RootPanel.AddChild<InventoryBar>();
		RootPanel.AddChild<CurrentTool>();
		RootPanel.AddChild<SpawnMenu>();
		RootPanel.AddChild<RoundInfo>();
		RootPanel.AddChild<MoneyHud>();
		RootPanel.AddChild<Scope>();
		RootPanel.AddChild<WaterOverlay>();
	}
}
