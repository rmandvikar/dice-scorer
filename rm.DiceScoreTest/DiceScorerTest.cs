using System;
using NUnit.Framework;
using rm.DiceScore;

namespace rm.DiceScoreTest
{
	[TestFixture]
	public class DiceScorerTest
	{
		DiceScorer diceScorer;

		[SetUp]
		public void Setup()
		{
			diceScorer = new DiceScorer();
		}

		[Test]
		public void Validations_All()
		{
			Assert.Throws<ArgumentNullException>(() =>
				diceScorer.GetScore(null)
				);
			Assert.Throws<ArgumentException>(() =>
				diceScorer.GetScore(new int[] { 1, 1, 1, 1 })
				);
			Assert.Throws<ArgumentOutOfRangeException>(() =>
				diceScorer.GetScore(new int[] { 9, 1, 1, 1, 1 })
				);
			Assert.Throws<ArgumentOutOfRangeException>(() =>
				diceScorer.GetScore(new int[] { 0, 1, 1, 1, 1 })
				);
		}

		#region X of a kind

		[Test]
		[TestCase(new[] { 1, 1, 1, 2, 2 }, 7)]
		[TestCase(new[] { 8, 8, 8, 8, 1 }, 33, Description = "FourOfAKind is also ThreeOfAKind.")]
		[TestCase(new[] { 8, 8, 8, 8, 8 }, 40, Description = "AllOfAKind is also ThreeOfAKind.")]
		public void ThreeOfAKind_True(int[] diceValues, int scoreExpected)
		{
			var (score, scoreKind) = diceScorer.ThreeOfAKind(diceValues);
			Assert.AreEqual(scoreExpected, score);
		}

		[Test]
		[TestCase(new[] { 1, 1, 3, 2, 2 })]
		public void ThreeOfAKind_False(int[] diceValues)
		{
			var (score, scoreKind) = diceScorer.FourOfAKind(diceValues);
			Assert.AreEqual(0, score);
		}

		[Test]
		[TestCase(new[] { 1, 1, 1, 1, 8 }, 12)]
		[TestCase(new[] { 8, 8, 8, 8, 1 }, 33)]
		[TestCase(new[] { 8, 8, 8, 8, 8 }, 40, Description = "AllOfAKind is also FourOfAKind.")]
		public void FourOfAKind_True(int[] diceValues, int scoreExpected)
		{
			var (score, scoreKind) = diceScorer.FourOfAKind(diceValues);
			Assert.AreEqual(scoreExpected, score);
		}

		[Test]
		[TestCase(new[] { 1, 1, 3, 2, 2 })]
		[TestCase(new[] { 1, 1, 1, 2, 2 })]
		public void FourOfAKind_False(int[] diceValues)
		{
			var (score, scoreKind) = diceScorer.FourOfAKind(diceValues);
			Assert.AreEqual(0, score);
		}

		[Test]
		[TestCase(new[] { 8, 8, 8, 8, 8 })]
		[TestCase(new[] { 1, 1, 1, 1, 1 })]
		public void AllOfAKind_True(int[] diceValues)
		{
			var (score, scoreKind) = diceScorer.AllOfAKind(diceValues);
			Assert.AreEqual(50, score);
		}

		[Test]
		[TestCase(new[] { 1, 2, 2, 2, 2 })]
		public void AllOfAKind_False(int[] diceValues)
		{
			var (score, scoreKind) = diceScorer.AllOfAKind(diceValues);
			Assert.AreEqual(0, score);
		}

		[Test]
		[TestCase(new[] { 1, 3, 4, 7, 8 })]
		[TestCase(new[] { 1, 8, 2, 6, 3 })]
		[TestCase(new[] { 1, 2, 3, 4, 5 }, Description = "LargeStraight is also NoneOfAKind.")]
		public void NoneOfAKind_True(int[] diceValues)
		{
			var (score, scoreKind) = diceScorer.NoneOfAKind(diceValues);
			Assert.AreEqual(40, score);
		}

		[Test]
		[TestCase(new[] { 1, 2, 3, 4, 4 })]
		public void NoneOfAKind_False(int[] diceValues)
		{
			var (score, scoreKind) = diceScorer.NoneOfAKind(diceValues);
			Assert.AreEqual(0, score);
		}

		#endregion

		#region FullHouse

		[Test]
		[TestCase(new[] { 2, 2, 3, 3, 3 })]
		[TestCase(new[] { 4, 4, 5, 5, 5 })]
		public void FullHouse_True(int[] diceValues)
		{
			var (score, scoreKind) = diceScorer.FullHouse(diceValues);
			Assert.AreEqual(25, score);
		}

		[Test]
		[TestCase(new[] { 2, 3, 3, 3, 3 })]
		public void FullHouse_False(int[] diceValues)
		{
			var (score, scoreKind) = diceScorer.FullHouse(diceValues);
			Assert.AreEqual(0, score);
		}

		#endregion

		#region Straights

		[Test]
		[TestCase(new[] { 1, 2, 3, 4, 8 })]
		[TestCase(new[] { 1, 5, 6, 7, 8 })]
		[TestCase(new[] { 5, 7, 6, 8, 1 })]
		[TestCase(new[] { 5, 7, 6, 8, 8 })]
		[TestCase(new[] { 1, 2, 3, 4, 5 }, Description = "LargeStraight is also SmallStraight.")]
		public void SmallStraight_True(int[] diceValues)
		{
			var (score, scoreKind) = diceScorer.SmallStraight(diceValues);
			Assert.AreEqual(30, score);
		}

		[Test]
		[TestCase(new[] { 1, 2, 6, 7, 8 })]
		public void SmallStraight_False(int[] diceValues)
		{
			var (score, scoreKind) = diceScorer.SmallStraight(diceValues);
			Assert.AreEqual(0, score);
		}

		[Test]
		[TestCase(new[] { 1, 2, 3, 4, 5 })]
		[TestCase(new[] { 3, 4, 5, 6, 7 })]
		[TestCase(new[] { 4, 5, 6, 7, 8 })]
		[TestCase(new[] { 8, 7, 6, 5, 4 })]
		public void LargeStraight_True(int[] diceValues)
		{
			var (score, scoreKind) = diceScorer.LargeStraight(diceValues);
			Assert.AreEqual(40, score);
		}

		[Test]
		[TestCase(new[] { 1, 5, 6, 7, 8 })]
		public void LargeStraight_False(int[] diceValues)
		{
			var (score, scoreKind) = diceScorer.LargeStraight(diceValues);
			Assert.AreEqual(0, score);
		}

		#endregion

		#region Chance

		[Test]
		[TestCase(new[] { 8, 8, 8, 8, 8 }, 40)]
		[TestCase(new[] { 2, 8, 5, 6, 3 }, 24)]
		[TestCase(new[] { 1, 2, 1, 6, 1 }, 11)]
		public void Chance(int[] diceValues, int scoreExpected)
		{
			var (score, scoreKind) = diceScorer.Chance(diceValues);
			Assert.AreEqual(scoreExpected, score);
		}

		#endregion

		#region GetScoreWithKind, GetScore

		[Test]
		// AllOfAKind
		// note: highest ever score
		[TestCase(new[] { 8, 8, 8, 8, 8 }, 50, ScoreKind.AllOfAKind)]
		[TestCase(new[] { 1, 1, 1, 1, 1 }, 50, ScoreKind.AllOfAKind)]
		// NoneOfAKind
		[TestCase(new[] { 1, 2, 4, 5, 7 }, 40, ScoreKind.NoneOfAKind)]
		[TestCase(new[] { 6, 8, 3, 5, 2 }, 40, ScoreKind.NoneOfAKind)]
		// LargeStraight
		[TestCase(new[] { 1, 2, 3, 4, 5 }, 40, ScoreKind.LargeStraight)]
		[TestCase(new[] { 8, 5, 7, 6, 4 }, 40, ScoreKind.LargeStraight)]
		// FourOfAKind
		[TestCase(new[] { 5, 5, 8, 5, 5 }, 28, ScoreKind.FourOfAKind)]
		// note: lowest ever score
		[TestCase(new[] { 1, 2, 1, 1, 1 }, 6, ScoreKind.FourOfAKind)]
		//	note: max FourOfAKind score
		[TestCase(new[] { 8, 8, 8, 8, 7 }, 39, ScoreKind.FourOfAKind)]
		// SmallStraight
		[TestCase(new[] { 1, 2, 3, 4, 4 }, 30, ScoreKind.SmallStraight)]
		[TestCase(new[] { 5, 5, 7, 6, 4 }, 30, ScoreKind.SmallStraight)]
		// ThreeOfAKind
		[TestCase(new[] { 1, 2, 4, 4, 4 }, 15, ScoreKind.ThreeOfAKind)]
		[TestCase(new[] { 5, 5, 8, 5, 4 }, 27, ScoreKind.ThreeOfAKind)]
		//	note: max ThreeOfAKind score
		[TestCase(new[] { 8, 8, 8, 7, 7 }, 38, ScoreKind.ThreeOfAKind)]
		//	note: FullHouse but ThreeOfAKind score is greater
		[TestCase(new[] { 5, 5, 8, 5, 8 }, 31, ScoreKind.ThreeOfAKind)]
		//	note: FullHouse but ThreeOfAKind score is same
		[TestCase(new[] { 7, 7, 7, 2, 2 }, 25, ScoreKind.ThreeOfAKind)]
		// FullHouse
		[TestCase(new[] { 4, 2, 4, 2, 4 }, 25, ScoreKind.FullHouse)]
		// Chance
		[TestCase(new[] { 8, 8, 4, 2, 4 }, 26, ScoreKind.Chance)]
		// note: lowest Chance score
		[TestCase(new[] { 1, 1, 2, 2, 3 }, 9, ScoreKind.Chance)]
		public void GetScore(int[] diceValues,
			int scoreExpected, ScoreKind scoreKindExpected)
		{
			var (score, kind) = diceScorer.GetScoreWithKind(diceValues);
			Assert.AreEqual(scoreExpected, score);
			Assert.AreEqual(scoreKindExpected, kind);
			// also check GetScore
			Assert.AreEqual(scoreExpected, diceScorer.GetScore(diceValues));
		}

		#endregion
	}
}
