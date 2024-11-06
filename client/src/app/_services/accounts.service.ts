import { HttpClient } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { IUser } from '../_models/user';
import { map } from 'rxjs';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class AccountsService {
  private http = inject(HttpClient);
  baseUrl = environment.apiUrl;
  currentUser = signal<IUser | null>(null);

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
  }

  logout() {
    localStorage.removeItem('dating-app-user');
    this.currentUser.set(null);
  }
}
