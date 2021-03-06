import { Component, OnInit, Output, EventEmitter } from '@angular/core';
import { AlertifyService } from './../_services/alertify.service';
import { AuthService } from './../_services/auth.service';
import { UserService } from 'src/app/_services/user.service';
import { GamesService } from './../_services/games.service';
import { Game } from '../_models/game';
import { Gender } from '../_models/gender';
import { User } from '../_models/user';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.scss']
})
export class RegisterComponent implements OnInit {
  @Output() cancelRegister = new EventEmitter();
  listOfGames: Game[];
  listOfGenders: Gender[];
  userModel: User;

  constructor(private authService: AuthService,
              private alertify: AlertifyService,
              private userService: UserService,
              private gamesService: GamesService) { }

  ngOnInit() {
    this.getGames();
    this.getGenders();
  }

  private getGames() {
    this.gamesService.getGames().subscribe({
      next: games => {
        this.listOfGames = games;
      },
      error: error => {
        this.alertify.error(error);
      }
    });
  }

  private getGenders() {
    this.userService.getGenders().subscribe({
      next: genders => {
        this.listOfGenders = genders;
      },
      error: error => {
        this.alertify.error(error);
      }
    });
  }

  register() {
    this.authService.register(this.userModel).subscribe({
      next: (response) => {
        this.alertify.success('Registration successful.');
      },
      error: (error: any) => {
        this.alertify.error(error);
      }
    });
  }

  cancel() {
    this.cancelRegister.emit(false);
    this.alertify.message('Registration cancelled.');
  }

}
