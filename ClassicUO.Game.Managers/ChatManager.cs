using System.Collections.Generic;
using ClassicUO.Resources;

namespace ClassicUO.Game.Managers;

internal static class ChatManager
{
	public static readonly Dictionary<string, ChatChannel> Channels = new Dictionary<string, ChatChannel>();

	public static ChatStatus ChatIsEnabled;

	public static string CurrentChannelName = string.Empty;

	private static readonly string[] _messages = new string[41]
	{
		ResGeneral.YouAreAlreadyIgnoringMaximum,
		ResGeneral.YouAreAlreadyIgnoring1,
		ResGeneral.YouAreNowIgnoring1,
		ResGeneral.YouAreNoLongerIgnoring1,
		ResGeneral.YouAreNotIgnoring1,
		ResGeneral.YouAreNoLongerIgnoringAnyone,
		ResGeneral.ThatIsNotAValidConferenceName,
		ResGeneral.ThereIsAlreadyAConference,
		ResGeneral.YouMustHaveOperatorStatus,
		ResGeneral.Conference1RenamedTo2,
		ResGeneral.YouMustBeInAConference,
		ResGeneral.ThereIsNoPlayerNamed1,
		ResGeneral.ThereIsNoConferenceNamed1,
		ResGeneral.ThatIsNotTheCorrectPassword,
		ResGeneral.HasChosenToIgnoreYou,
		ResGeneral.NotGivenYouSpeakingPrivileges,
		ResGeneral.YouCanNowReceivePM,
		ResGeneral.YouWillNoLongerReceivePM,
		ResGeneral.YouAreShowingYourCharName,
		ResGeneral.YouAreNotShowingYourCharName,
		ResGeneral.IsRemainingAnonymous,
		ResGeneral.HasChosenToNotReceivePM,
		ResGeneral.IsKnownInTheLandsOfBritanniaAs2,
		ResGeneral.HasBeenKickedOutOfTheConference,
		ResGeneral.AConferenceModeratorKickedYou,
		ResGeneral.YouAreAlreadyInTheConference1,
		ResGeneral.IsNoLongerAConferenceModerator,
		ResGeneral.IsNowAConferenceModerator,
		ResGeneral.HasRemovedYouFromModerators,
		ResGeneral.HasMadeYouAConferenceModerator,
		ResGeneral.NoLongerHasSpeakingPrivileges,
		ResGeneral.NowHasSpeakingPrivileges,
		ResGeneral.RemovedYourSpeakingPrivileges,
		ResGeneral.GrantedYouSpeakingPrivileges,
		ResGeneral.EveryoneWillHaveSpeakingPrivs,
		ResGeneral.ModeratorsWillHaveSpeakingPrivs,
		ResGeneral.PasswordToTheConferenceChanged,
		ResGeneral.TheConferenceNamed1IsFull,
		ResGeneral.YouAreBanning1FromThisConference,
		ResGeneral.BannedYouFromTheConference,
		ResGeneral.YouHaveBeenBanned
	};

	public static string GetMessage(int index)
	{
		if (index >= _messages.Length)
		{
			return string.Empty;
		}
		return _messages[index];
	}

	public static void AddChannel(string text, bool hasPassword)
	{
		if (!Channels.TryGetValue(text, out var value))
		{
			value = new ChatChannel(text, hasPassword);
			Channels[text] = value;
		}
	}

	public static void RemoveChannel(string name)
	{
		if (Channels.ContainsKey(name))
		{
			Channels.Remove(name);
		}
	}

	public static void Clear()
	{
		Channels.Clear();
	}
}
