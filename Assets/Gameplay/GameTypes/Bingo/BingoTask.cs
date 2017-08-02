using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BingoTask
{
	protected int id;
	protected string title;
	protected string description;
	protected int difficulty;
	protected BingoTaskType type;
}

public enum BingoTaskType
{
	Gun,
	Medal,
}
