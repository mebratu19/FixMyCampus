import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface AdminTicket {
  id: number;
  category: string;
  building: string;
  room: string;
  description: string;
  status: string;
  urgency: string;
  reporterName: string;
  technicianName: string | null;
  createdAt: string;
}

export interface AdminDashboard {
  totalOpenTickets: number;
  openTickets: AdminTicket[];
  buildingCounts: {
    building: string;
    count: number;
  }[];
}

@Injectable({
  providedIn: 'root'
})
export class AdminService {
  private http = inject(HttpClient);

  private apiUrl = 'http://localhost:5219/api/Admin';

  getDashboard(): Observable<AdminDashboard> {
    return this.http.get<AdminDashboard>(
      `${this.apiUrl}/dashboard`
    );
  }
}