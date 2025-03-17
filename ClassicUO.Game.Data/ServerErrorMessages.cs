using System;
using ClassicUO.IO.Resources;
using ClassicUO.Resources;

namespace ClassicUO.Game.Data;

internal static class ServerErrorMessages
{
	private static readonly Tuple<int, string>[] _loginErrors = new Tuple<int, string>[10]
	{
		Tuple.Create(3000007, ResErrorMessages.IncorrectPassword),
		Tuple.Create(3000009, ResErrorMessages.CharacterDoesNotExist),
		Tuple.Create(3000006, ResErrorMessages.CharacterAlreadyExists),
		Tuple.Create(3000016, ResErrorMessages.ClientCouldNotAttachToServer),
		Tuple.Create(3000017, ResErrorMessages.ClientCouldNotAttachToServer),
		Tuple.Create(3000012, ResErrorMessages.AnotherCharacterOnline),
		Tuple.Create(3000013, ResErrorMessages.ErrorInSynchronization),
		Tuple.Create(3000005, ResErrorMessages.IdleTooLong),
		Tuple.Create(-1, ResErrorMessages.CouldNotAttachServer),
		Tuple.Create(-1, ResErrorMessages.CharacterTransferInProgress)
	};

	private static readonly Tuple<int, string>[] _errorCode = new Tuple<int, string>[6]
	{
		Tuple.Create(3000018, ResErrorMessages.CharacterPasswordInvalid),
		Tuple.Create(3000019, ResErrorMessages.ThatCharacterDoesNotExist),
		Tuple.Create(3000020, ResErrorMessages.ThatCharacterIsBeingPlayed),
		Tuple.Create(3000021, ResErrorMessages.CharacterIsNotOldEnough),
		Tuple.Create(3000022, ResErrorMessages.CharacterIsQueuedForBackup),
		Tuple.Create(3000023, ResErrorMessages.CouldntCarryOutYourRequest)
	};

	private static readonly Tuple<int, string>[] _pickUpErrors = new Tuple<int, string>[5]
	{
		Tuple.Create(3000267, ResErrorMessages.YouCanNotPickThatUp),
		Tuple.Create(3000268, ResErrorMessages.ThatIsTooFarAway),
		Tuple.Create(3000269, ResErrorMessages.ThatIsOutOfSight),
		Tuple.Create(3000270, ResErrorMessages.ThatItemDoesNotBelongToYou),
		Tuple.Create(3000271, ResErrorMessages.YouAreAlreadyHoldingAnItem)
	};

	private static readonly Tuple<int, string>[] _generalErrors = new Tuple<int, string>[9]
	{
		Tuple.Create(3000007, ResErrorMessages.IncorrectNamePassword),
		Tuple.Create(3000034, ResErrorMessages.SomeoneIsAlreadyUsingThisAccount),
		Tuple.Create(3000035, ResErrorMessages.YourAccountHasBeenBlocked),
		Tuple.Create(3000036, ResErrorMessages.YourAccountCredentialsAreInvalid),
		Tuple.Create(-1, ResErrorMessages.CommunicationProblem),
		Tuple.Create(-1, ResErrorMessages.TheIGRConcurrencyLimitHasBeenMet),
		Tuple.Create(-1, ResErrorMessages.TheIGRTimeLimitHasBeenMet),
		Tuple.Create(-1, ResErrorMessages.GeneralIGRAuthenticationFailure),
		Tuple.Create(3000037, ResErrorMessages.CouldntConnectToUO)
	};

	public static string GetError(byte packetID, byte code)
	{
		ClilocLoader instance = ClilocLoader.Instance;
		switch (packetID)
		{
		case 83:
		{
			if (code >= 10)
			{
				code = 9;
			}
			Tuple<int, string> tuple = _loginErrors[code];
			return instance.GetString(tuple.Item1, tuple.Item2);
		}
		case 133:
		{
			if (code >= 6)
			{
				code = 5;
			}
			Tuple<int, string> tuple = _errorCode[code];
			return instance.GetString(tuple.Item1, tuple.Item2);
		}
		case 39:
		{
			if (code >= 5)
			{
				code = 4;
			}
			Tuple<int, string> tuple = _pickUpErrors[code];
			return instance.GetString(tuple.Item1, tuple.Item2);
		}
		case 130:
		{
			if (code >= 9)
			{
				code = 8;
			}
			Tuple<int, string> tuple = _generalErrors[code];
			return instance.GetString(tuple.Item1, tuple.Item2);
		}
		default:
			return string.Empty;
		}
	}
}
