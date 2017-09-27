import { ShortGameResult } from './ShortGameResult';

export class PivotStandingsGame {
    constructor(
        public HomeTeamId: number,
        public AwayTeamId: number,
        public Results: ShortGameResult
    ) { }

    public static getNonPlayableCell(): PivotStandingsGame {
        return new PivotStandingsGame(
            0,
            0,
            ShortGameResult.getNonPlayableCell());
    }

    public clone(): PivotStandingsGame {
        return new PivotStandingsGame(
            this.HomeTeamId,
            this.AwayTeamId,
            new ShortGameResult(
                this.Results.HomeSetsScore,
                this.Results.AwaySetsScore,
                this.Results.IsTechnicalDefeat));
    }

    public transposeResult(): PivotStandingsGame {
        return new PivotStandingsGame(
            this.AwayTeamId,
            this.HomeTeamId,
            new ShortGameResult(
                this.Results.AwaySetsScore,
                this.Results.HomeSetsScore,
                this.Results.IsTechnicalDefeat));
    }
}
