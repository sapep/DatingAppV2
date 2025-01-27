import { inject, Injectable, signal } from '@angular/core';
import { environment } from '../../environments/environment';
import { HttpClient } from '@angular/common/http';
import { PaginatedResult } from '../_models/pagination';
import { Message } from '../_models/message';
import { setPaginatedResponse, setPaginationHeaders } from './paginationHelper';
import { HubConnection, HubConnectionBuilder, HubConnectionState } from '@microsoft/signalr';
import { IUser } from '../_models/user';
import { Group } from '../_models/group';

@Injectable({
  providedIn: 'root'
})
export class MessageService {
  private http = inject(HttpClient);

  hubsConnection?: HubConnection;

  baseUrl = environment.apiUrl;
  hubsUrl = environment.hubsUrl;

  paginatedResult = signal<PaginatedResult<Message[]> | null>(null);
  messageThread = signal<Message[]>([]);

  createHubConnection(user: IUser, otherUsername: string) {
    this.hubsConnection = new HubConnectionBuilder()
      .withUrl(`${this.hubsUrl}message?user=${otherUsername}`, {
        accessTokenFactory: () => user.token
      })
      .withAutomaticReconnect()
      .build();
      
    this.hubsConnection.start().catch(error => console.error(error));

    this.hubsConnection.on("ReceiveMessageThread", (messages: Message[]) => {
      this.messageThread.set(messages);
    });

    this.hubsConnection.on("NewMessage", (message: Message) => {
      this.messageThread.update(messages => [...messages, message]);
    });

    this.hubsConnection.on("UpdatedGroup", (group: Group) => {
      if (group.connections.some(x => x.username === otherUsername)) {
        this.messageThread.update(messages => {
          messages.forEach(message => {
            if (!message.dateRead) {
              message.dateRead = new Date(Date.now());
            }
          });

          return messages;
        });
      }
    });
  }

  stopHubConnection() {
    if (this.hubsConnection?.state === HubConnectionState.Connected) {
      this.hubsConnection.stop().catch(error => console.error(error));
    }
  }

  getMessages(pageNumber: number, pageSize: number, container: string) {
    let params = setPaginationHeaders(pageNumber, pageSize);

    params = params.append('Container', container);

    return this.http.get<Message[]>(`${this.baseUrl}message`, { observe: "response", params })
      .subscribe({
        next: (response) => {
          setPaginatedResponse(response, this.paginatedResult);
        }
      });
  }

  getMessageThread(username: string) {
    return this.http.get<Message[]>(`${this.baseUrl}message/thread/${username}`);
  }

  async sendMessage(username: string, content: string) {
    return this.hubsConnection?.invoke("SendMessage", { recipientUsername: username, content: content });
  }

  deleteMessage(id: number) {
    return this.http.delete(`${this.baseUrl}message/${id}`);
  }
}
