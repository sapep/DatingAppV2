import { Component, computed, inject, input } from '@angular/core';
import { IMember } from '../../_models/member';
import { RouterLink } from '@angular/router';
import { LikesService } from '../../_services/likes.service';

@Component({
  selector: 'app-member-card',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './member-card.component.html',
  styleUrl: './member-card.component.css'
})
export class MemberCardComponent {
  private likesService = inject(LikesService);

  member = input.required<IMember>();
  hasLiked = computed(() => this.likesService.likeIds().includes(this.member().id));

  toggleLike() {
    this.likesService.toggleLike(this.member().id)
      .subscribe({
        next: () => {
          if (this.hasLiked()) {
            this.likesService.likeIds.update(ids => ids.filter(id => id !== this.member().id));
          } else {
            this.likesService.likeIds.update(ids => [...ids, this.member().id]);
          }
        }
      });
  }
}
