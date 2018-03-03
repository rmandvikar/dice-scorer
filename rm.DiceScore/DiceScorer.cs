using System;
using System.Collections.Generic;
using System.Linq;

namespace rm.DiceScore
{
	public enum ScoreKind
	{
		// No need to implement,
		//	Ones, Twos, Threes, Fours, Fives, Sixes, Sevens, Eights,
		// as Chance, ThreeOfAKind will always be greater than these.
		ThreeOfAKind,
		FourOfAKind,
		AllOfAKind,
		NoneOfAKind,
		FullHouse,
		SmallStraight,
		LargeStraight,
		Chance,
	}

	/// <summary>
	/// Dice scorer class.
	/// </summary>
	public class DiceScorer
	{
		public const int NumberOfDice = 5;
		public const int NumberOfDiceSides = 8;

		/// <summary>
		/// Returns scoreKinds ordered desc by their max scores.
		/// </summary>
		static IEnumerable<ScoreKind> scoreKindsOrderedDescByMaxScore =
			Enum.GetValues(typeof(ScoreKind)).Cast<ScoreKind>()
				.Select(x => (kind: x, maxScore: GetMaxScoreForKind(x)))
				.OrderByDescending(x => x.maxScore)
				.Select(x => x.kind);

		#region GetScore

		/// <summary>
		/// Gets score.
		/// </summary>
		public int GetScore(int[] diceValues)
		{
			return GetScoreWithKind(diceValues).score;
		}

		/// <summary>
		/// Gets score with scoreKind.
		/// </summary>
		public (int score, ScoreKind scoreKind) GetScoreWithKind(int[] diceValues)
		{
			// validations
			Validate(diceValues);
			// pre-computations
			var diceValueToCountMap = ToValueCountMap(diceValues);
			var diceValueBitMap = ToValueBitMap(diceValueToCountMap);
			var valuesSum = diceValues.Sum();
			// calculate max score
			var max = GetMaxScore(diceValues, diceValueToCountMap, diceValueBitMap, valuesSum);
			return max;
		}

		void Validate(int[] diceValues)
		{
			ThrowEx.ThrowIfNull(diceValues, nameof(diceValues));
			ThrowEx.ThrowIfArgumentInValid(diceValues.Length, nameof(diceValues),
				NumberOfDice);
			ThrowEx.ThrowIfArgumentOutOfRange(diceValues, nameof(diceValues),
				min: 1, max: NumberOfDiceSides);
		}

		/// <summary>
		/// Gets max score from the scores of each scoreKind.
		/// </summary>
		(int, ScoreKind) GetMaxScore(int[] diceValues,
		   int[] diceValueToCountMap, int diceValueBitMap, int valuesSum)
		{
			var currentScore = (score: 0, scoreKind: ScoreKind.Chance);
			// Loop through ScoreKind values and calculate max.
			// For a new enum value, simply add a case here.
			foreach (var scoreKind in scoreKindsOrderedDescByMaxScore)
			{
				switch (scoreKind)
				{
					// Process in desc order of max score for scoreKind and skip if
					// current score is gte.
					case ScoreKind.ThreeOfAKind:
						if (currentScore.score >= GetMaxScoreForKind(scoreKind)) continue;
						currentScore = Max(currentScore, ThreeOfAKindInner(diceValueToCountMap, valuesSum));
						break;
					case ScoreKind.FourOfAKind:
						if (currentScore.score >= GetMaxScoreForKind(scoreKind)) continue;
						currentScore = Max(currentScore, FourOfAKindInner(diceValueToCountMap, valuesSum));
						break;
					case ScoreKind.AllOfAKind:
						if (currentScore.score >= GetMaxScoreForKind(scoreKind)) continue;
						currentScore = Max(currentScore, AllOfAKindInner(diceValueToCountMap));
						break;
					case ScoreKind.NoneOfAKind:
						if (currentScore.score >= GetMaxScoreForKind(scoreKind)) continue;
						currentScore = Max(currentScore, NoneOfAKind(diceValues));
						break;
					case ScoreKind.FullHouse:
						if (currentScore.score >= GetMaxScoreForKind(scoreKind)) continue;
						currentScore = Max(currentScore, FullHouse(diceValues));
						break;
					case ScoreKind.SmallStraight:
						if (currentScore.score >= GetMaxScoreForKind(scoreKind)) continue;
						currentScore = Max(currentScore, SmallStraight(diceValueBitMap));
						break;
					case ScoreKind.LargeStraight:
						if (currentScore.score >= GetMaxScoreForKind(scoreKind)) continue;
						currentScore = Max(currentScore, LargeStraight(diceValueBitMap));
						break;
					case ScoreKind.Chance:
						// always process
						currentScore = Max(currentScore, Chance(diceValues));
						break;
					default:
						throw new ArgumentOutOfRangeException($"Unknown ScoreKind: '{scoreKind.ToString()}'");
				}
			}
			return currentScore;
		}

		/// <summary>
		/// Returns the max score for scoreKind.
		/// </summary>
		static int GetMaxScoreForKind(ScoreKind scoreKind)
		{
			switch (scoreKind)
			{
				case ScoreKind.ThreeOfAKind:
					return NumberOfDiceSides * 3 + (NumberOfDiceSides - 1) * (NumberOfDice - 3); // 24 + 14
				case ScoreKind.FourOfAKind:
					return NumberOfDiceSides * 4 + (NumberOfDiceSides - 1) * (NumberOfDice - 4); // 32 + 7
				case ScoreKind.AllOfAKind:
					return 50;
				case ScoreKind.NoneOfAKind:
					return 40 - 1; // 2nd highest, intentionally less 1 to make it 3rd highest
				case ScoreKind.FullHouse:
					return 25;
				case ScoreKind.SmallStraight:
					return 30;
				case ScoreKind.LargeStraight:
					return 40; // 2nd highest
				case ScoreKind.Chance:
					return 0; // lowest so processed last
				default:
					throw new ArgumentOutOfRangeException($"Unknown ScoreKind: '{scoreKind.ToString()}'");
			}
		}

		#endregion

		#region X of a kind

		public (int, ScoreKind) ThreeOfAKind(int[] diceValues)
		{
			Validate(diceValues);
			return ThreeOfAKindInner(ToValueCountMap(diceValues), diceValues.Sum());
		}
		internal (int, ScoreKind) ThreeOfAKindInner(int[] diceValueToCountMap, int valuesSum)
		{
			return (IsXOfAKind(diceValueToCountMap, 3) ? valuesSum : 0, ScoreKind.ThreeOfAKind);
		}

		public (int, ScoreKind) FourOfAKind(int[] diceValues)
		{
			Validate(diceValues);
			return FourOfAKindInner(ToValueCountMap(diceValues), diceValues.Sum());
		}
		internal (int, ScoreKind) FourOfAKindInner(int[] diceValueToCountMap, int valuesSum)
		{
			return (IsXOfAKind(diceValueToCountMap, 4) ? valuesSum : 0, ScoreKind.FourOfAKind);
		}

		public (int, ScoreKind) AllOfAKind(int[] diceValues)
		{
			Validate(diceValues);
			return AllOfAKindInner(ToValueCountMap(diceValues));
		}
		internal (int, ScoreKind) AllOfAKindInner(int[] diceValueToCountMap)
		{
			// NumberOfDice for AllOfAKind as hardcoding 5 for FiveOfAKind is ok
			return (IsXOfAKind(diceValueToCountMap, NumberOfDice) ? 50 : 0, ScoreKind.AllOfAKind);
		}

		bool IsXOfAKind(int[] diceValueToCountMap, int valueCount)
		{
			for (int i = 0; i < diceValueToCountMap.Length; i++)
			{
				if (diceValueToCountMap[i] >= valueCount)
				{
					return true;
				}
			}
			return false;
		}

		public (int, ScoreKind) NoneOfAKind(int[] diceValues)
		{
			Validate(diceValues);
			bool IsNoneOfAKind(int[] values)
			{
				return values.Distinct().Count() == NumberOfDice;
			}
			return (IsNoneOfAKind(diceValues) ? 40 : 0, ScoreKind.NoneOfAKind);
		}

		#endregion

		#region FullHouse

		public (int, ScoreKind) FullHouse(int[] diceValues)
		{
			Validate(diceValues);
			bool IsFullHouse(int[] diceValueToCountMap)
			{
				var hasCountOf2 = false;
				var hasCountOf3 = false;
				for (int i = 0; i < diceValueToCountMap.Length; i++)
				{
					if (diceValueToCountMap[i] == 2)
					{
						hasCountOf2 = true;
					}
					if (diceValueToCountMap[i] == 3)
					{
						hasCountOf3 = true;
					}
					if (hasCountOf2 && hasCountOf3)
					{
						return true;
					}
				}
				return false;
			}
			return (IsFullHouse(ToValueCountMap(diceValues)) ? 25 : 0, ScoreKind.FullHouse);
		}

		#endregion

		#region Straights

		public (int, ScoreKind) SmallStraight(int[] diceValues)
		{
			Validate(diceValues);
			return SmallStraight(ToValueBitMap(ToValueCountMap(diceValues)));
		}
		(int, ScoreKind) SmallStraight(int diceValueBitMap)
		{
			var straightCount = 4;
			return (IsStraight(diceValueBitMap, straightCount) ? 30 : 0, ScoreKind.SmallStraight);
		}

		public (int, ScoreKind) LargeStraight(int[] diceValues)
		{
			Validate(diceValues);
			return LargeStraight(ToValueBitMap(ToValueCountMap(diceValues)));
		}
		(int, ScoreKind) LargeStraight(int diceValueBitMap)
		{
			var straightCount = 5;
			return (IsStraight(diceValueBitMap, straightCount) ? 40 : 0, ScoreKind.LargeStraight);
		}

		bool IsStraight(int diceValueBitMap, int straightCount)
		{
			// bitmask: 0b0_1111 for straightCount 4
			// bitmask: 0b1_1111 for straightCount 5
			var bitmask = (1 << straightCount) - 1;
			for (int windows = 0; windows < (NumberOfDiceSides - straightCount) + 1; windows++)
			{
				if ((diceValueBitMap & bitmask) == bitmask)
				{
					return true;
				}
				diceValueBitMap >>= 1;
			}
			return false;
		}

		#endregion

		#region Chance

		public (int, ScoreKind) Chance(int[] diceValues)
		{
			Validate(diceValues);
			return (diceValues.Sum(), ScoreKind.Chance);
		}

		#endregion

		#region helper methods

		/// <summary>
		/// Returns a value->count map given values.
		/// </summary>
		int[] ToValueCountMap(int[] diceValues)
		{
			var diceValueToCountMap = new int[NumberOfDiceSides];
			foreach (var value in diceValues)
			{
				diceValueToCountMap[value - 1]++;
			}
			return diceValueToCountMap;
		}

		/// <summary>
		/// Returns a value bitmap given values.
		/// </summary>
		int ToValueBitMap(int[] diceValues)
		{
			int bitmap = 0;
			for (int i = 0; i < diceValues.Length; i++)
			{
				bitmap |= ((diceValues[i] > 0 ? 1 : 0) << i);
			}
			return bitmap;
		}

		/// <summary>
		/// Returns max of two items (<paramref name="item1"/> if both are equal).
		/// </summary>
		(int score, ScoreKind kind) Max((int, ScoreKind) item1, (int, ScoreKind) item2)
		{
			var (value1, kind1) = item1;
			var (value2, kind2) = item2;
			return value1 >= value2 ? item1 : item2;
		}

		#endregion
	}
}
