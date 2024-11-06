import { HttpClient } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { environment } from '../../environments/environment';
import { IMember } from '../_models/member';
import { of, tap } from 'rxjs';
import { IPhoto } from '../_models/photo';

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

  setMainPhoto(photo: IPhoto) {
    return this.http
      .put(`${this.baseUrl}appuser/set-main-photo/${photo.id}`, {})
      .pipe(tap(() => {
        this.members.update(members => members.map(m => {
          if (m.photos.includes(photo)) {
            m.photoUrl = photo.url;
          }
          return m;
        }))
      }));
  }

  deletePhoto(photo: IPhoto) {
    return this.http
      .delete(`${this.baseUrl}appuser/delete-photo/${photo.id}`)
      .pipe(tap(() => {
        this.members.update(members => members.map(m => {
          if (m.photos.includes(photo)) {
            m.photos = m.photos.filter(p => p.id != photo.id);
          }
          return m;
        }))
      }));
  }
}
