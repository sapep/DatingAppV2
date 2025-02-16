import { HttpClient, HttpParams, HttpResponse } from '@angular/common/http';
import { inject, Injectable, model, signal } from '@angular/core';
import { environment } from '../../environments/environment';
import { IMember } from '../_models/member';
import { IPhoto } from '../_models/photo';
import { PaginatedResult } from '../_models/pagination';
import { UserParams } from '../_models/userParams';
import { of } from 'rxjs';
import { AccountsService } from './accounts.service';
import { setPaginatedResponse, setPaginationHeaders } from './paginationHelper';

@Injectable({
  providedIn: 'root'
})
export class MembersService {
  private http = inject(HttpClient);
  private accountService = inject(AccountsService);
  baseUrl = environment.apiUrl;
  paginatedResult = signal<PaginatedResult<IMember[]> | null>(null);
  memberCache = new Map();
  user = this.accountService.currentUser();
  userParams = signal<UserParams>(new UserParams(this.user));

  resetUserParams() {
    this.userParams.set(new UserParams(this.user));
  }

  getMembers() {
    const response = this.memberCache.get(Object.values(this.userParams()).join('-'));

    if (response) {
      return setPaginatedResponse(response, this.paginatedResult);
    }

    let params = setPaginationHeaders(this.userParams().pageNumber, this.userParams().pageSize);

    params = params.append('minAge', this.userParams().minAge);
    params = params.append('maxAge', this.userParams().maxAge);
    params = params.append('gender', this.userParams().gender);
    params = params.append('orderBy', this.userParams().orderBy);

    return this.http.get<IMember[]>(`${this.baseUrl}appuser`, { observe: 'response', params }).subscribe({
      next: response => {
        setPaginatedResponse(response, this.paginatedResult);
        this.memberCache.set(Object.values(this.userParams()).join('-'), response);
      }
    });
  }

  getMember(username: string) {
    const member: IMember = [...this.memberCache.values()]
      .reduce((arr, elem) => arr.concat(elem.body), [])
      .find((m: IMember) => m.username === username);

    if (member) {
      return of(member);
    }
    return this.http.get<IMember>(`${this.baseUrl}appuser/${username}`);
  }

  updateMember(member: IMember) {
    return this.http.put<IMember>(`${this.baseUrl}appuser`, member).pipe(
      // tap(() => {
      //   this.members.update(members =>
      //     members.map(m =>
      //       m.username === member.username
      //         ? member
      //         : m
      //     )
      //   );
      // })
    );
  }

  setMainPhoto(photo: IPhoto) {
    return this.http
      .put(`${this.baseUrl}appuser/set-main-photo/${photo.id}`, {})
      .pipe(
        // tap(() => {
        //   this.members.update(members => members.map(m => {
        //     if (m.photos.includes(photo)) {
        //       m.photoUrl = photo.url;
        //     }
        //     return m;
        //   }))
        // })
      );
  }

  deletePhoto(photo: IPhoto) {
    return this.http
      .delete(`${this.baseUrl}appuser/delete-photo/${photo.id}`)
      .pipe(
      // tap(() => {
      //   this.members.update(members => members.map(m => {
      //     if (m.photos.includes(photo)) {
      //       m.photos = m.photos.filter(p => p.id != photo.id);
      //     }
      //     return m;
      //   }))
      // })
    );
  }
}
