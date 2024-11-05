import { HttpClient } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { environment } from '../../environments/environment';
import { IMember } from '../_models/member';
import { of, tap } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class MembersService {
  private http = inject(HttpClient);
  baseUrl = environment.apiUrl;
  members = signal<IMember[]>([]);

  getMembers() {
    return this.http.get<IMember[]>(`${this.baseUrl}appuser`).subscribe({
      next: members => this.members.set(members)
    });
  }

  getMember(username: string) {
    const member = this.members().find(member => member.username === username);
    if (member) {
      return of(member);
    }

    return this.http.get<IMember>(`${this.baseUrl}appuser/${username}`);
  }

  updateMember(member: IMember) {
    return this.http.put<IMember>(`${this.baseUrl}appuser`, member).pipe(
      tap(() => {
        this.members.update(members =>
          members.map(m =>
            m.username === member.username
              ? member
              : m
          )
        );
      })
    );
  }
}
