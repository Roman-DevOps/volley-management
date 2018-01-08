import { Component, OnInit, Input, OnDestroy, Output, EventEmitter } from '@angular/core';

import 'rxjs/add/operator/toPromise';

import { ScheduleModel } from '../../Models/Schedule/Schedule';
import { ScheduleService } from '../../Services/schedule.service';
import { GameResult } from '../../Models/Schedule/GameResult';
import { DivisionHeader } from '../../Models/Schedule/DivisionHeader';
import { Result } from '../../Models/Schedule/Result';
import { ScheduleDay } from '../../Models/Schedule/ScheduleDay';
import { Week } from '../../Models/Schedule/Week';
import { DummyDivisionHeader } from './DummyDivisionHeader';


@Component({
    selector: 'schedule',
    templateUrl: './schedule.component.html',
    styleUrls: ['./schedule.component.scss']
})
export class ScheduleComponent implements OnInit {

    @Input() scheduleId: number;
    @Output() ready: EventEmitter<void> = new EventEmitter<void>();

    data: ScheduleModel = {} as ScheduleModel;
    divisionsIds: number[] = [];

    constructor(private scheduleService: ScheduleService) { }

    ngOnInit() {
        this.scheduleService
            .getSchedule(this.scheduleId)
            .toPromise()
            .then(data => {
                this.data = data;
                this.ready.emit();
                this._getSortedDivisionsIds();
            });
    }

    gameIsPlayed(gameResult: GameResult) {
        return gameResult.AwayTeamName &&
            gameResult.Result &&
            (!gameResult.Result.TotalScore.IsEmpty || gameResult.Result.IsTechnicalDefeat);
    }

    getdivisionsHeader(divisionHeader: DivisionHeader): string {
        if (divisionHeader.Id === DummyDivisionHeader.DummyHeaderId) {
            return '';
        }
        return `${divisionHeader.Name}: ${divisionHeader.Rounds.join()} тур.`;
    }

    getDivisionAccentColor(divisionId: number): string {
        if (divisionId === DummyDivisionHeader.DummyHeaderId) {
            return '';
        }
        let index = this.divisionsIds.indexOf(divisionId);
        return `division${++index}`;
    }

    isFreeDay(gameResult: GameResult): boolean {
        return !gameResult.AwayTeamName;
    }

    getGameTotalBallsScore(gameResult: Result): string {
        const totalHomeTeamBalls = gameResult.SetScores.map(item => item.Home).reduce((prev, next) => prev + next);
        const totalAwayTeamBalls = gameResult.SetScores.map(item => item.Away).reduce((prev, next) => prev + next);

        return `${totalHomeTeamBalls}:${totalAwayTeamBalls}`;
    }

    getNumberOfEmptyDivisionHeaders(day: ScheduleDay, week: Week): Array<number> {
        const maxHeaders = week.Days.map(item => item.Divisions.length).reduce(function (a, b) { return Math.max(a, b); });
        const difference = maxHeaders - day.Divisions.length;
        return difference > 0 ? new Array(difference) : new Array(0);
    }

    private _getSortedDivisionsIds() {
        this.data.Schedule.forEach((week) => {
            week.Days.forEach((day) => {
                day.Divisions.forEach(division => {
                    if (this.divisionsIds.indexOf(division.Id) === -1) {
                        this.divisionsIds.push(division.Id);
                    }
                });
            });
        });

        this.divisionsIds.sort((a, b) => a - b);
    }

    private _fillUpDivisionHeadersInDays() {
        this.data.Schedule.forEach((week) => {
            const maxNumberOfRoundsInWeek = week.Days.map(item => item.Divisions.length).reduce(function (a, b) { return Math.max(a, b); });
            week.Days.forEach(day => {
                while (day.Divisions.length < maxNumberOfRoundsInWeek) {
                    day.Divisions.push(new DummyDivisionHeader())
                }
            });
        });
    }
}
