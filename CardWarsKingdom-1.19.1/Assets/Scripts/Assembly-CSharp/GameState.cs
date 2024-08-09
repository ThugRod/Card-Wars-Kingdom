public enum GameState
{
	LoadingData,
	LoadingLevel,
	WaitForLevelLoad,
	Intro,
	WaitForIntro,
	Waiting,
	WaitingResultSequence,
	FirstTurnCoinFlip,
	DealCreatureCards,
	DealCreatureCardsWait,
	P1StartTurn,
	P1Turn,
	P1EndTurn,
	P2StartTurn,
	P2Turn,
	P2EndTurn,
	LootCollect,
	EndGameWait,
	P1Defeated,
	P1DefeatedWaiting,
	P2Defeated,
	P2DefeatedWaiting,
	PlayerVictory,
	EnemyVictory,
	RevivePlayer
}