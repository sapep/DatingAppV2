import { Component, inject, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { IMember } from '../../_models/member';
import { TabDirective, TabsetComponent, TabsModule } from 'ngx-bootstrap/tabs';
import { GalleryItem, GalleryModule, ImageItem } from 'ng-gallery';
import { TimeagoModule } from 'ngx-timeago';
import { DatePipe } from '@angular/common';
import { MemberMessagesComponent } from "../member-messages/member-messages.component";
import { Message } from '../../_models/message';
import { MessageService } from '../../_services/message.service';

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
export class MemberDetailComponent implements OnInit {
  @ViewChild("memberTabs", { static: true }) memberTabs?: TabsetComponent;

  private messageService = inject(MessageService);
  private route = inject(ActivatedRoute);

  member: IMember = {} as IMember;
  images: GalleryItem[] = [];
  activeTab?: TabDirective;
  messages: Message[] = [];

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
    })

    this.route.queryParams.subscribe({
      next: (params) => {
        params['tab'] && this.selectTab(params['tab']);
      }
    });
  }

  onTabActivated(data: TabDirective) {
    this.activeTab = data;

    if (this.activeTab.heading === "Messages" && this.messages.length === 0 && this.member) {
      this.messageService.getMessageThread(this.member.username)
        .subscribe({
          next: (messages) => {
            this.messages = messages;
          }
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

  onUpdateMessages(event: Message) {
    this.messages.push(event);
  }
}
