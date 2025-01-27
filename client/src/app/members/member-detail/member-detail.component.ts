import { Component, inject, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { IMember } from '../../_models/member';
import { TabDirective, TabsetComponent, TabsModule } from 'ngx-bootstrap/tabs';
import { GalleryItem, GalleryModule, ImageItem } from 'ng-gallery';
import { TimeagoModule } from 'ngx-timeago';
import { DatePipe } from '@angular/common';
import { MemberMessagesComponent } from "../member-messages/member-messages.component";
import { Message } from '../../_models/message';
import { MessageService } from '../../_services/message.service';
import { PresenceService } from '../../_services/presence.service';
import { AccountsService } from '../../_services/accounts.service';
import { HubConnectionState } from '@microsoft/signalr';

@Component({
  selector: 'app-member-detail',
  standalone: true,
  imports: [
    TabsModule,
    GalleryModule,
    TimeagoModule,
    DatePipe,
    MemberMessagesComponent
  ],
  templateUrl: './member-detail.component.html',
  styleUrl: './member-detail.component.css'
})
export class MemberDetailComponent implements OnInit, OnDestroy {
  @ViewChild("memberTabs", { static: true }) memberTabs?: TabsetComponent;

  private messageService = inject(MessageService);
  private accountService = inject(AccountsService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  presenceService = inject(PresenceService);

  member: IMember = {} as IMember;
  images: GalleryItem[] = [];
  activeTab?: TabDirective;

  ngOnInit(): void {
    this.route.data.subscribe({
      next: data => {
        this.member = data['member'];
        this.member && this.member.photos.map(photo =>
          this.images.push(
            new ImageItem({ src: photo.url, thumb: photo.url })
          )
        );
      }
    });

    this.route.paramMap.subscribe({
      next: _ => this.onRouteParamsChange()
    });

    this.route.queryParams.subscribe({
      next: (params) => {
        params['tab'] && this.selectTab(params['tab']);
      }
    });
  }

  ngOnDestroy(): void {
    this.messageService.stopHubConnection();
  }

  onTabActivated(data: TabDirective) {
    this.activeTab = data;
    this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { tab: this.activeTab.heading },
      queryParamsHandling: 'merge'
    });

    if (this.activeTab.heading === "Messages" && this.member) {
      const user = this.accountService.currentUser();

      if (!user) return;

      this.messageService.createHubConnection(user, this.member.username);
    } else {
      this.messageService.stopHubConnection();
    }
  }

  onRouteParamsChange() {
    const user = this.accountService.currentUser();
    if (!user) return;
    if (this.messageService.hubsConnection?.state === HubConnectionState.Connected && this.activeTab?.heading === 'Messages') {
      this.messageService.hubsConnection.stop().then(() => {
        this.messageService.createHubConnection(user, this.member.username);
      });
    }
  }

  selectTab(heading: string) {
    if (this.memberTabs) {
      const selectedTab = this.memberTabs.tabs.find(tab => tab.heading === heading);

      if (selectedTab) {
        selectedTab.active = true;
      }
    }
  }
}
