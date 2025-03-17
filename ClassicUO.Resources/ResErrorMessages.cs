using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace ClassicUO.Resources;

[DebuggerNonUserCode]
[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "15.0.0.0")]
[CompilerGenerated]
public class ResErrorMessages
{
	private static ResourceManager resourceMan;

	private static CultureInfo resourceCulture;

	[EditorBrowsable(EditorBrowsableState.Advanced)]
	public static ResourceManager ResourceManager
	{
		get
		{
			if (resourceMan == null)
			{
				resourceMan = new ResourceManager("ClassicUO.Resources.ResErrorMessages", typeof(ResErrorMessages).Assembly);
			}
			return resourceMan;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Advanced)]
	public static CultureInfo Culture
	{
		get
		{
			return resourceCulture;
		}
		set
		{
			resourceCulture = value;
		}
	}

	public static string AnotherCharacterOnline => ResourceManager.GetString("AnotherCharacterOnline", resourceCulture);

	public static string CharacterAlreadyExists => ResourceManager.GetString("CharacterAlreadyExists", resourceCulture);

	public static string CharacterDoesNotExist => ResourceManager.GetString("CharacterDoesNotExist", resourceCulture);

	public static string CharacterIsNotOldEnough => ResourceManager.GetString("CharacterIsNotOldEnough", resourceCulture);

	public static string CharacterIsQueuedForBackup => ResourceManager.GetString("CharacterIsQueuedForBackup", resourceCulture);

	public static string CharacterPasswordInvalid => ResourceManager.GetString("CharacterPasswordInvalid", resourceCulture);

	public static string CharacterTransferInProgress => ResourceManager.GetString("CharacterTransferInProgress", resourceCulture);

	public static string ClientCouldNotAttachToServer => ResourceManager.GetString("ClientCouldNotAttachToServer", resourceCulture);

	public static string ClientPathIsNotAValidUODirectory => ResourceManager.GetString("ClientPathIsNotAValidUODirectory", resourceCulture);

	public static string CommunicationProblem => ResourceManager.GetString("CommunicationProblem", resourceCulture);

	public static string CouldNotAttachServer => ResourceManager.GetString("CouldNotAttachServer", resourceCulture);

	public static string CouldntCarryOutYourRequest => ResourceManager.GetString("CouldntCarryOutYourRequest", resourceCulture);

	public static string CouldntConnectToUO => ResourceManager.GetString("CouldntConnectToUO", resourceCulture);

	public static string ErrorInSynchronization => ResourceManager.GetString("ErrorInSynchronization", resourceCulture);

	public static string GeneralIGRAuthenticationFailure => ResourceManager.GetString("GeneralIGRAuthenticationFailure", resourceCulture);

	public static string IdleTooLong => ResourceManager.GetString("IdleTooLong", resourceCulture);

	public static string IncorrectNamePassword => ResourceManager.GetString("IncorrectNamePassword", resourceCulture);

	public static string IncorrectPassword => ResourceManager.GetString("IncorrectPassword", resourceCulture);

	public static string SomeoneIsAlreadyUsingThisAccount => ResourceManager.GetString("SomeoneIsAlreadyUsingThisAccount", resourceCulture);

	public static string ThatCharacterDoesNotExist => ResourceManager.GetString("ThatCharacterDoesNotExist", resourceCulture);

	public static string ThatCharacterIsBeingPlayed => ResourceManager.GetString("ThatCharacterIsBeingPlayed", resourceCulture);

	public static string ThatIsOutOfSight => ResourceManager.GetString("ThatIsOutOfSight", resourceCulture);

	public static string ThatIsTooFarAway => ResourceManager.GetString("ThatIsTooFarAway", resourceCulture);

	public static string ThatItemDoesNotBelongToYou => ResourceManager.GetString("ThatItemDoesNotBelongToYou", resourceCulture);

	public static string TheIGRConcurrencyLimitHasBeenMet => ResourceManager.GetString("TheIGRConcurrencyLimitHasBeenMet", resourceCulture);

	public static string TheIGRTimeLimitHasBeenMet => ResourceManager.GetString("TheIGRTimeLimitHasBeenMet", resourceCulture);

	public static string YouAreAlreadyHoldingAnItem => ResourceManager.GetString("YouAreAlreadyHoldingAnItem", resourceCulture);

	public static string YouCanNotPickThatUp => ResourceManager.GetString("YouCanNotPickThatUp", resourceCulture);

	public static string YourAccountCredentialsAreInvalid => ResourceManager.GetString("YourAccountCredentialsAreInvalid", resourceCulture);

	public static string YourAccountHasBeenBlocked => ResourceManager.GetString("YourAccountHasBeenBlocked", resourceCulture);

	internal ResErrorMessages()
	{
	}
}
