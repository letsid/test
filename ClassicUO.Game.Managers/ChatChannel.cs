namespace ClassicUO.Game.Managers;

internal sealed class ChatChannel
{
	public readonly bool HasPassword;

	public readonly string Name;

	public ChatChannel(string name, bool hasPassword)
	{
		Name = name;
		HasPassword = hasPassword;
	}
}
