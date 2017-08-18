using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
	private MenuState[] menus;
	private MenuState currentMenu;
	private Text randomMessageText;

	/**********************************************************/
	// MonoBehaviour Interface

	public void Awake()
	{
		menus = new MenuState[(int)MenuType.NumTypes];
		foreach (MenuState state in GetComponentsInChildren<MenuState>())
		{
			menus[(int)state.GetMenuType()] = state;
		}

		randomMessageText = GameObject.Find("RandomMessageText").GetComponent<Text>();
		randomMessageText.text = GetRandomMessage();

		VideoSettings.Load();
		AudioSettings.Load();
		ControlSettings.Load();
	}

	public void Start()
	{
		GoToMenu(MenuType.Main);
	}

	public void Update()
	{
		currentMenu.StateUpdate();

		if (Input.GetKeyDown(KeyCode.M))
		{
			randomMessageText.text = GetRandomMessage();
		}
	}

	/**********************************************************/
	// Interface

	public void GoToMenu(MenuType menu)
	{
		MenuState prevMenu = currentMenu;
		currentMenu = menus[(int)menu];

		if (prevMenu)
		{
			prevMenu.StateEnd();
		}
		currentMenu.StateBegin();
	}

	/**********************************************************/
	// Helper Functions

	private string GetRandomMessage()
	{
		switch (Random.Range(-1, 72))
		{
			case -1: return "Collect all 72!";
			case 0: return "Lost? Call 481-516-2342";
			case 1: return "Don't attack a straw man";
			case 2: return "Flying is easier with wings";
			case 3: return "They weren't dead the whole time";
			case 4: return "01100011 problems but a bit ain't one";
			case 5: return "One is outside, one is inside";
			case 6: return "Zoo driven rules system";
			case 7: return "Imported Exitrons";
			case 8: return "It's int max, don't you ever forget it";
			case 9: return "I'll get you my little pretty";
			case 10: return "And your little dog, too!";
			case 11: return "Pen attackers, beware!";
			case 12: return "Throw it out and start over";
			case 13: return "So big that I can't even see it";
			case 14: return "This is a warning";
			case 15: return "Who's gonna play with me this time?";
			case 16: return "Stole your kill, stole your life";
			case 17: return "Casually mentioning something";
			case 18: return "Is this the 4th wall?";
			case 19: return "Clean as a pickle";
			case 20: return "The lizard who never learned to love";
			case 21: return "Royal flush? Nah, too political";
			case 22: return "The cosmos of jinx land";
			case 23: return "Spaghetti in my hair";
			case 24: return "Always check expiration dates";
			case 25: return "The namer of Moon Base is unknown";
			case 26: return "Here at Senwan's gathering";
			case 27: return "Rearrange me 'till I'm sane";
			case 28: return "Sunlight through the blanket";
			case 29: return "Pass the torch, gotta get lit";
			case 30: return "This is positive reinforcement";
			case 31: return "We'll never find the tower";
			case 32: return "On a riverboat to Mars";
			case 33: return "Captions left at the alter";
			case 34: return "The 100 yard leap";
			case 35: return "There's gotta be something hidden in one of these, right?";
			case 36: return "Playing with sludge";
			case 37: return "Foreshadowing four shadows";
			case 38: return "Area to play made of synthetic materials";
			case 39: return "Case 39";
			case 40: return "Pop vs. Soda";
			case 41: return "The lord of the valley";
			case 42: return "Testing the limits of your neurons";
			case 43: return "Dealing chips, but not too salty";
			case 44: return "Genius captured in a bottle";
			case 45: return "A 12 pack of numbers";
			case 46: return "Sequential words perceived as a sentence";
			case 47: return "Haunted by ghost gorillas";
			case 48: return "Can't even with these odds";
			case 49: return "A charging bullfighter";
			case 50: return "sean coded this";
			case 51: return "Brainwashed into neutralism";
			case 52: return "This one fits the formula";
			case 53: return "Rock, paper, scissors to decide who's right";
			case 54: return "This wall agrees with you";
			case 55: return "Drinkin' food and smokin' water";
			case 56: return "Case 39 is a lie";
			case 57: return "It's time for clocks";
			case 58: return "Programmatic literature for kids";
			case 59: return "Approaching infinity with class";
			case 60: return "Manual auto repair";
			case 61: return "737 down over ABQ";
			case 62: return "Committed and wit it";
			case 63: return "Acting like it's my job";
			case 64: return "Stale words on a chalkboard";
			case 65: return "All hail the Deathbat";
			case 66: return "Religious salad in the cookie kingdom";
			case 67: return "Tacos without the crust";
			case 68: return "The tree contains a bush";
			case 69: return "The sphinx cat has returned";
			case 70: return "Good morning, Worm your honor";
			case 71: return "Is there anybody out there?";
		}

		return "";
	}
}
