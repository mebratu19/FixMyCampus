import { Component, OnInit, inject } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { TicketService } from '../../services/ticket';
import {
  AdminService,
  AdminDashboard,
  AdminTicket
} from '../../services/admin';

@Component({
  selector: 'app-admin',
  standalone: true,
  templateUrl: './admin.html',
  styleUrl: './admin.scss',
  imports: [FormsModule],
})
export class Admin implements OnInit {

  private adminService = inject(AdminService);
  private ticketService = inject(TicketService);

  dashboard: AdminDashboard | null = null;
selectedBuilding = '';
selectedStatus = '';
  loading = true;
  error = '';

  ngOnInit(): void {
    this.loadDashboard();
  }

  loadDashboard(): void {
    this.loading = true;
    this.error = '';

    this.adminService.getDashboard().subscribe({
      next: (response: AdminDashboard) => {
        console.log('ADMIN DASHBOARD:', response);
console.log('OPEN TICKETS:', response.openTickets);
console.log('SELECTED BUILDING:', this.selectedBuilding);
console.log('SELECTED STATUS:', this.selectedStatus);

        this.dashboard = response;
this.loading = false;

console.log('LOADING VALUE:', this.loading);
console.log('DASHBOARD VALUE:', this.dashboard);
      },

      error: (error: HttpErrorResponse) => {
        console.error('ADMIN DASHBOARD ERROR:', error);

        this.error = 'Unable to load dashboard data.';
        this.loading = false;
      }
    });
  }
get filteredTickets(): AdminTicket[] {
  if (!this.dashboard) {
    return [];
  }
console.log('FILTERED TICKETS:', this.dashboard.openTickets);
  return this.dashboard.openTickets.filter(ticket => {
    const buildingMatch =
      !this.selectedBuilding ||
      ticket.building === this.selectedBuilding;

    const statusMatch =
      !this.selectedStatus ||
      ticket.status === this.selectedStatus;

    return buildingMatch && statusMatch;
  });
}
  getStatusCount(status: string): number {
    return this.dashboard?.openTickets.filter(
      ticket => ticket.status === status
    ).length ?? 0;
  }
  assignTicket(ticketId: number, technicianName: string): void {
  if (!technicianName.trim()) {
    return;
  }

  this.ticketService.assignTicket(
    ticketId,
    technicianName.trim()
  ).subscribe({
    next: () => {
      this.loadDashboard();
    },
    error: (error) => {
      console.error('ASSIGN TICKET ERROR:', error);
      alert(
        error?.error?.message ??
        'Unable to assign ticket.'
      );
    }
  });
}

updateTicketStatus(
  ticketId: number,
  status: string
): void {
  this.ticketService.updateStatus(
    ticketId,
    status
  ).subscribe({
    next: () => {
      this.loadDashboard();
    },
    error: (error) => {
      console.error('UPDATE STATUS ERROR:', error);

      alert(
        error?.error?.message ??
        'Unable to update ticket status.'
      );
    }
  });
}
}