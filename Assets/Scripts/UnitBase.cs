using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu (fileName ="Unit", menuName ="Unit/Create new unit")]
public class UnitBase : ScriptableObject
{
	[SerializeField] string unitName;

	[TextArea]
	[SerializeField] string description;

	[SerializeField] Sprite frontSprite;
	[SerializeField] Sprite backSprite;

	[SerializeField] UnitType type1;
	[SerializeField] UnitType type2;

	//base stats
	[SerializeField] int maxHP;
	[SerializeField] int attack;
	[SerializeField] int defence;
	[SerializeField] int spAttack;
	[SerializeField] int spDefence;
	[SerializeField] int speed;

	[SerializeField] int expYield;
	[SerializeField] GrowthRate growthRate;

	[SerializeField] List<LearnableMove> learnableMoves;

	public static int MaxNumOfMoves { get; set; } = 4;

	public int GetExpForLevel(int level)
    {
		if(growthRate == GrowthRate.Fast)
        {
			return 4 * (level * level * level) / 5;
        }
		else if (growthRate == GrowthRate.MediumFast)
        {
			return level * level * level;
        }
		else if (growthRate == GrowthRate.MediumSlow)
        {
			return 6 / 5 * (level * level * level) - 15 * (level * level) + 100 * level - 140;
        }
		else if (growthRate == GrowthRate.Slow)
        {
			return 5 * (level * level * level) / 4;
        }

		return -1;
    }

	public string Name
    {
		get { return name; }
    }

	public string Description
    {
		get { return description; }
    }

	public Sprite FrontSprite
	{
		get { return frontSprite; }
	}

	public Sprite BackSprite
    {
		get { return backSprite; }
	}

	public UnitType Type1
    {
		get { return type1; }
    }

	public UnitType Type2
	{
		get { return type2; }
	}

	public int MaxHp
	{
		get { return maxHP; }
	}

	public int Attack
	{
		get { return attack; }
	}

	public int Defence
	{
		get { return defence; }
	}

	public int SpAttack
	{
		get { return spAttack; }
	}

	public int SpDefence
	{
		get { return spDefence; }
	}

	public int Speed
	{
		get { return speed; }
	}

	public List<LearnableMove> LearnableMoves
    {
		get { return learnableMoves; }
    }

	public int ExpYield => expYield;

	public GrowthRate GrowthRate => growthRate;
}

[System.Serializable]
public class LearnableMove
{
	[SerializeField] MoveBase moveBase;
	[SerializeField] int level;

	public MoveBase Base
    {
		get { return moveBase; }
    }

	public int Level
    {
		get { return level; }
    }
}

public enum UnitType 
{	None,
	Normal,
	Fire,
	Water,
	Electric,
	Grass,
	Ice,
	Fighting,
	Poison,
	Ground,
	Flying,
	Psychic,
	Bug,
	Rock,
	Ghost,
	Dragon,
	Dark,
	Steel,
	Fairy
}

public enum GrowthRate
{
	Fast, MediumFast, MediumSlow, Slow
}

public enum Stat
{
	Attack,
	Defence,
	SpAttack,
	SpDefence,
	Speed,

	//These 2 are not actual stats
	Accuracy,
	Evasion
}

public class TypeChart
{
	static float[][] chart =
	{
		//effectiveness on ====>	Nor		Fir		Wat		Ele		Gra		Ice		Fig		Poi		Gro		Fly		Psy		Bug		Roc		Gho		Dra		Dar		Ste		Fai
		/*Normal*/		new float[] {1f,    1f,     1f,     1f,     1f,     1f,     1f,     1f,     1f,     1f,     1f,     1f,     0.5f,   0f,     1f,     1f,     0.5f,   1f  },
		/*Fire*/		new float[] {1f,    0.5f,   0.5f,   1f,     2f,     2f,     1f,     1f,     1f,     1f,     1f,     2f,     0.5f,   1f,     0.5f,   1f,     2f,     1f  },
		/*Water*/		new float[] {1f,    2f,     0.5f,   1f,     0.5f,   1f,     1f,     1f,     2f,     1f,     1f,     1f,     2f,     1f,     0.5f,   1f,     1f,     1f  },
		/*Electric*/	new float[] {1f,    1f,     2f,     0.5f,   0.5f,   1f,     1f,     1f,     0f,     2f,     1f,     1f,     1f,     1f,     0.5f,   1f,     1f,     1f  },
		/*Grass*/		new float[] {1f,    0.5f,   2f,     1f,     0.5f,   1f,     1f,     0.5f,   2f,     0.5f,   1f,     0.5f,   2f,     1f,     0.5f,   1f,     0.5f,   1f  },
		/*Ice*/			new float[] {1f,    0.5f,   0.5f,   1f,     2f,     0.5f,   1f,     1f,     2f,     2f,     1f,     1f,     1f,     1f,     2f,     1f,     0.5f,   1f  },
		/*Fighting*/	new float[] {2f,    1f,     1f,     1f,     1f,     2f,     1f,     0.5f,   1f,     0.5f,   0.5f,   0.5f,   2f,     0f,     1f,     2f,     2f,     0.5f},
		/*Poison*/		new float[] {1f,    1f,     1f,     1f,     2f,     1f,     1f,     0.5f,   0.5f,   1f,     1f,     1f,     0.5f,   0.5f,   1f,     1f,     0f,     2f  },
		/*Ground*/		new float[] {1f,    2f,     1f,     2f,     0.5f,   1f,     1f,     2f,     1f,     0f,     1f,     0.5f,   2f,     1f,     1f,     1f,     2f,     1f  },
		/*Flying*/		new float[] {1f,    1f,     1f,     0.5f,   2f,     1f,     2f,     1f,     1f,     1f,     1f,     2f,     0.5f,   1f,     1f,     1f,     0.5f,   1f  },
		/*Psychic*/		new float[] {1f,    1f,     1f,     1f,     1f,     1f,     2f,     2f,     1f,     1f,     0.5f,   1f,     1f,     1f,     1f,     0f,     0.5f,   1f  },
		/*Bug*/			new float[] {1f,    0.5f,   1f,     1f,     2f,     1f,     0.5f,   0.5f,   1f,     0.5f,   2f,     1f,     1f,     0.5f,   1f,     2f,     0.5f,   0.5f},
		/*Rock*/		new float[] {1f,    2f,     1f,     1f,     1f,     2f,     0.5f,   1f,     0.5f,   2f,     1f,     2f,     1f,     1f,     1f,     1f,     0.5f,   1f  },
		/*Ghost*/		new float[] {0f,    1f,     1f,     1f,     1f,     1f,     1f,     1f,     1f,     1f,     2f,     1f,     1f,     2f,     1f,     0.5f,   1f,     1f  },
		/*Dragon*/		new float[] {1f,    1f,     1f,     1f,     1f,     1f,     1f,     1f,     1f,     1f,     1f,     1f,     1f,     1f,     2f,     1f,     0.5f,   0f  },
		/*Dark*/		new float[] {1f,    1f,     1f,     1f,     1f,     1f,     0.5f,   1f,     1f,     1f,     2f,     1f,     1f,     2f,     1f,     0.5f,   1f,     0.5f},
		/*Steel*/		new float[] {1f,    0.5f,   0.5f,   0.5f,   1f,     2f,     1f,     1f,     1f,     1f,     1f,     1f,     2f,     1f,     1f,     1f,     0.5f,   2f  },
		/*Fairy*/		new float[] {1f,    0.5f,   1f,     1f,     1f,     1f,     2f,     0.5f,   1f,     1f,     1f,     1f,     1f,     1f,     2f,     2f,     0.5f,   1f  }
	};

	public static float GetEffectiveness(UnitType attackType, UnitType defenseType)
    {
		if (attackType == UnitType.None || defenseType == UnitType.None)
			return 1;

		int row = (int)attackType - 1;
		int col = (int)defenseType - 1;

		return chart[row][col];
    }
}
