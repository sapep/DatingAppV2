import { inject, Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { HttpClient } from '@angular/common/http';
import { IUser } from '../_models/user';
import { IPhoto } from '../_models/photo';

@Injectable({
  providedIn: 'root'
})
export class AdminService {
  baseUrl = environment.apiUrl;
  private http = inject(HttpClient);

  getUsersWithRoles() {
    return this.http.get<IUser[]>(`${this.baseUrl}admin/users-with-roles`);
  }

  getUnapprovedRoles() {
    return this.http.get<IPhoto[]>(`${this.baseUrl}admin/photos-for-moderation`);
  }

  updateUserRoles(username: string, roles: string[]) {
    return this.http.post<string[]>(`${this.baseUrl}admin/edit-roles/${username}?roles=${roles}`, {})
  }

  approvePhoto(photoId: number) {
    return this.http.put<IPhoto>(`${this.baseUrl}admin/approve-photo/${photoId}`, {});
  }

  unapprovePhoto(photoId: number) {
    return this.http.put<IPhoto>(`${this.baseUrl}admin/disapprove-photo/${photoId}`, {});
  }
}
