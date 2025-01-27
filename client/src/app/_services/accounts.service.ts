import { HttpClient } from '@angular/common/http';
import { computed, inject, Injectable, signal } from '@angular/core';
import { IUser } from '../_models/user';
import { map } from 'rxjs';
import { environment } from '../../environments/environment';
import { LikesService } from './likes.service';
import { PresenceService } from './presence.service';

@Injectable({
  providedIn: 'root'
})
export class AccountsService {
  private http = inject(HttpClient);
  private likesService = inject(LikesService);
  private presenceService = inject(PresenceService);
  baseUrl = environment.apiUrl;
  currentUser = signal<IUser | null>(null);
  roles = computed(() => {
    const user = this.currentUser();
    
    if (user && user.token) {
      const role = JSON.parse(atob(user.token.split('.')[1])).role;
      return Array.isArray(role) ? role : [role];
    }

    return [];
  });

  login(model: any) {
    return this.http.post<IUser>(`${this.baseUrl}account/login`, model).pipe(
      map(user => {
        if (user) {
          this.setCurrentUser(user);
        }
      })
    );
  }

  register(model: any) {
    return this.http.post<IUser>(`${this.baseUrl}account/register`, model).pipe(
      map(user => {
        if (user) {
          this.setCurrentUser(user);
        }
      })
    );
  }

  setCurrentUser(user: IUser) {
    localStorage.setItem('dating-app-user', JSON.stringify(user));
    this.currentUser.set(user);
    this.likesService.getLikeIds();
    this.presenceService.createHubConnection(user);
  }

  logout() {
    localStorage.removeItem('dating-app-user');
    this.currentUser.set(null);
    this.presenceService.stopHubConnection();
  }
}
