import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { IMember } from '../_models/member';

@Injectable({
  providedIn: 'root'
})
export class MembersService {
  private http = inject(HttpClient);
  baseUrl = environment.apiUrl;

  getMembers() {
    return this.http.get<IMember[]>(`${this.baseUrl}appuser`);
  }

  getMember(username: string) {
    return this.http.get<IMember>(`${this.baseUrl}appuser/${username}`);
  }
}
