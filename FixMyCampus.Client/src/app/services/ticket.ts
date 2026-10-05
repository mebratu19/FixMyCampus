import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class TicketService {

  private http = inject(HttpClient);

  private apiUrl = 'http://localhost:5219/api/Ticket';

  assignTicket(
    ticketId: number,
    technicianName: string
  ): Observable<any> {
    return this.http.put(
      `${this.apiUrl}/${ticketId}/assign`,
      {
        technicianName
      }
    );
  }

  updateStatus(
    ticketId: number,
    status: string
  ): Observable<any> {
    return this.http.put(
      `${this.apiUrl}/${ticketId}/status`,
      {
        status
      }
    );
  }
}