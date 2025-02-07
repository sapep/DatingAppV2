import { Component, inject, OnInit } from '@angular/core';
import { AdminService } from '../../_services/admin.service';
import { IPhoto } from '../../_models/photo';

@Component({
  selector: 'app-photo-management',
  standalone: true,
  imports: [],
  templateUrl: './photo-management.component.html',
  styleUrl: './photo-management.component.css'
})
export class PhotoManagementComponent implements OnInit {
  private adminService = inject(AdminService);

  photos: IPhoto[] = [];

  ngOnInit(): void {
    this.getUnapprovedPhotos();
  }

  getUnapprovedPhotos() {
    this.adminService.getUnapprovedRoles().subscribe({
      next: photos => {
        this.photos = photos;
      }
    });
  }

  handleApprovePhoto(photoId: number) {
    this.adminService.approvePhoto(photoId).subscribe({
      next: approvedPhoto => {
        this.photos = this.photos.filter(photo => (
          photo.id !== approvedPhoto.id
        ));
      }
    });
  }

  // Currently not used
  handleUnapprovePhoto(photoId: number) {
    this.adminService.unapprovePhoto(photoId).subscribe({
      next: photo => {
        // TODO
      }
    });
  }
}
