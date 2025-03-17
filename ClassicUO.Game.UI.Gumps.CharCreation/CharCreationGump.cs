using System.Linq;
using ClassicUO.Data;
using ClassicUO.Game.Data;
using ClassicUO.Game.GameObjects;
using ClassicUO.Game.Managers;
using ClassicUO.Game.Scenes;
using ClassicUO.Game.UI.Controls;
using ClassicUO.Game.UI.Gumps.Login;
using ClassicUO.IO.Resources;

namespace ClassicUO.Game.UI.Gumps.CharCreation;

internal class CharCreationGump : Gump
{
	private enum CharCreationStep
	{
		Appearence,
		ChooseProfession,
		ChooseTrade,
		ChooseCity
	}

	private PlayerMobile _character;

	private int _cityIndex;

	private CharCreationStep _currentStep;

	private LoadingGump _loadingGump;

	private readonly LoginScene _loginScene;

	private ProfessionInfo _selectedProfession;

	internal static int _skillsCount
	{
		get
		{
			if (Client.Version < ClientVersion.CV_70160)
			{
				return 3;
			}
			return 4;
		}
	}

	public CharCreationGump(LoginScene scene)
		: base(0u, 0u)
	{
		_loginScene = scene;
		Add(new CreateCharAppearanceGump(), 1);
		SetStep(CharCreationStep.Appearence);
		base.CanCloseWithRightClick = false;
	}

	public void SetCharacter(PlayerMobile character)
	{
		_character = character;
	}

	public void SetAttributes(bool force = false)
	{
		SetStep((_selectedProfession.DescriptionIndex >= 0 || force) ? CharCreationStep.ChooseCity : CharCreationStep.ChooseTrade);
	}

	public void SetCity(int cityIndex)
	{
		_cityIndex = cityIndex;
	}

	public void SetProfession(ProfessionInfo info)
	{
		for (int i = 0; i < _skillsCount; i++)
		{
			int num = info.SkillDefVal[i, 0];
			if (num >= _character.Skills.Length)
			{
				continue;
			}
			if (!CUOEnviroment.IsOutlands && (World.ClientFeatures.Flags & CharacterListFlags.CLF_SAMURAI_NINJA) == 0 && (num == 52 || num == 53))
			{
				for (int j = 0; j < i; j++)
				{
					Skill obj = _character.Skills[info.SkillDefVal[j, 0]];
					obj.ValueFixed = 0;
					obj.BaseFixed = 0;
					obj.CapFixed = 0;
					obj.Lock = Lock.Locked;
				}
				MessageBoxGump messageBoxGump = new MessageBoxGump(400, 300, ClilocLoader.Instance.GetString(1063016), null, hasBackground: true, MessageButtonType.OK, 1);
				messageBoxGump.X = 135;
				messageBoxGump.Y = 56;
				messageBoxGump.CanMove = false;
				UIManager.Add(messageBoxGump);
				return;
			}
			Skill obj2 = _character.Skills[num];
			obj2.ValueFixed = (ushort)info.SkillDefVal[i, 1];
			obj2.BaseFixed = 0;
			obj2.CapFixed = 0;
			obj2.Lock = Lock.Locked;
		}
		_selectedProfession = info;
		_character.Strength = (ushort)_selectedProfession.StatsVal[0];
		_character.Intelligence = (ushort)_selectedProfession.StatsVal[1];
		_character.Dexterity = (ushort)_selectedProfession.StatsVal[2];
		SetAttributes();
		SetStep((_selectedProfession.DescriptionIndex > 0) ? CharCreationStep.ChooseCity : CharCreationStep.ChooseTrade);
	}

	public void CreateCharacter()
	{
		_loginScene.CreateCharacter(_character);
	}

	public void StepBack(int steps = 1)
	{
		if (_currentStep == CharCreationStep.Appearence)
		{
			_loginScene.StepBack();
		}
		else
		{
			SetStep(_currentStep - steps);
		}
	}

	public void ShowMessage(string message)
	{
		int currentPage = base.ActivePage;
		if (_loadingGump != null)
		{
			Remove(_loadingGump);
		}
		Add(_loadingGump = new LoadingGump(message, LoginButtons.OK, delegate
		{
			ChangePage(currentPage);
		}), 4);
		ChangePage(4);
	}

	private void SetStep(CharCreationStep step)
	{
		_currentStep = step;
		switch (step)
		{
		default:
			ChangePage(1);
			break;
		case CharCreationStep.ChooseProfession:
		{
			Control control = base.Children.FirstOrDefault((Control page) => page.Page == 2);
			if (control != null)
			{
				Remove(control);
			}
			Add(new CreateCharProfessionGump(), 2);
			ChangePage(2);
			break;
		}
		case CharCreationStep.ChooseTrade:
		{
			Control control = base.Children.FirstOrDefault((Control page) => page.Page == 3);
			if (control != null)
			{
				Remove(control);
			}
			Add(new CreateCharTradeGump(_character, _selectedProfession), 3);
			ChangePage(3);
			break;
		}
		case CharCreationStep.ChooseCity:
		{
			Control control = base.Children.FirstOrDefault((Control page) => page.Page == 4);
			if (control != null)
			{
				Remove(control);
			}
			Add(new CreateCharSelectionCityGump((byte)_selectedProfession.DescriptionIndex, _loginScene), 4);
			ChangePage(4);
			break;
		}
		}
	}
}
