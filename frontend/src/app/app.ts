import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Component, OnInit, computed, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AUTO_API_KEY } from './app.constants';
import { AuthService } from './services/auth.service';

interface DocumentType {
  code: string;
  name: string;
}

@Component({
  selector: 'app-root',
  imports: [CommonModule, FormsModule],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App implements OnInit {
  readonly status = signal('');
  readonly documents = signal<DocumentType[]>([]);
  readonly documentsStatus = signal('Sin datos.');
  readonly secureResponse = signal('Sin datos.');
  readonly loading = signal(false);

  readonly sessionBadge = computed(() => (this.auth.authenticated() ? 'Activa' : 'Inactiva'));

  constructor(public auth: AuthService, private http: HttpClient) {}

  ngOnInit() {
    this.autoLogin();
  }

  autoLogin() {
    this.loading.set(true);
    this.status.set('Autenticando automáticamente...');

    this.auth.login(AUTO_API_KEY).subscribe({
      next: (response) => {
        const expires = new Date(response.expiresAt).toLocaleString();
        this.status.set(`Autenticado como ${response.user}. Expira: ${expires}.`);
        this.loading.set(false);
        this.loadDocuments();
      },
      error: (err) => {
        const message = err?.message ?? 'Credenciales inválidas o ApiKey malformada.';
        this.status.set(message);
        this.loading.set(false);
        this.auth.authenticated.set(false);
      }
    });
  }

  logout() {
    this.auth.logout().subscribe({
      next: () => {
        this.status.set('Sesión cerrada.');
        this.documents.set([]);
        this.documentsStatus.set('Sin datos.');
        this.secureResponse.set('Sin datos.');
      },
      error: () => {
        this.status.set('Error al cerrar sesión.');
      }
    });
  }

  loadDocuments() {
    this.documentsStatus.set('Cargando...');
    this.http.get<DocumentType[]>('/api/getDocuments').subscribe({
      next: (docs) => {
        this.documents.set(docs);
        this.documentsStatus.set('Listo.');
      },
      error: (err) => {
        const detail = err?.error?.error ?? err?.message ?? 'Solicitud rechazada.';
        this.documentsStatus.set(`Error ${err.status}: ${detail}`);
      }
    });
  }

  callSecure(endpoint: 'secure-data' | 'secure-profile') {
    this.secureResponse.set('Cargando...');
    this.http.get(`/api/${endpoint}`).subscribe({
      next: (response) => {
        this.secureResponse.set(JSON.stringify(response, null, 2));
      },
      error: (err) => {
        const detail = err?.error?.error ?? err?.message ?? 'Solicitud rechazada.';
        this.secureResponse.set(`Error ${err.status}: ${detail}`);
      }
    });
  }
}
