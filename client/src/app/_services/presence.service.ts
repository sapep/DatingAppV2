import { inject, Injectable, signal } from '@angular/core';
import { environment } from '../../environments/environment';
import { HubConnection, HubConnectionBuilder, HubConnectionState } from '@microsoft/signalr';
import { ToastrService } from 'ngx-toastr';
import { IUser } from '../_models/user';
import { take } from 'rxjs';
import { Router } from '@angular/router';

@Injectable({
  providedIn: 'root'
})
export class PresenceService {
  hubsUrl = environment.hubsUrl;
  private hubConnection?: HubConnection;
  private toastr = inject(ToastrService);
  private router = inject(Router);
  onlineUsers = signal<string[]>([]);

  createHubConnection(user: IUser) {
    this.hubConnection = new HubConnectionBuilder()
      .withUrl(`${this.hubsUrl}presence`, {
        accessTokenFactory: () => user.token
      })
      .withAutomaticReconnect()
      .build();
    
    this.hubConnection.start().catch(error => console.error(error));

    this.hubConnection.on('UserIsOnline', (username: string) => {
      this.onlineUsers.update(users => [...users, username]);
    });

    this.hubConnection.on('UserIsOffline', (username: string) => {
      this.onlineUsers.update(users => users.filter(x => x !== username));
    });

    this.hubConnection.on('GetOnlineUsers', (usernames: string[]) => {
      this.onlineUsers.set(usernames);
    });

    this.hubConnection.on("NewMessageReceived", ({ username, knownAs }) => {
      this.toastr.info(knownAs + ' has sent you a new message. Click me to see it!')
        .onTap
        .pipe(take(1))
        .subscribe(() => this.router.navigateByUrl(`/members/${username}?tab=Messages`))
    })
  }

  stopHubConnection() {
    if (this.hubConnection?.state === HubConnectionState.Connected) {
      this.hubConnection.stop().catch(error => console.error(error));
    }
  }
}
