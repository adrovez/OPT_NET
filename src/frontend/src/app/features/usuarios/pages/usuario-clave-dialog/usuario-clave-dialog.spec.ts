import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';

import { UsuarioClaveDialog, UsuarioClaveDialogData } from './usuario-clave-dialog';

describe('UsuarioClaveDialog', () => {
  let component: UsuarioClaveDialog;
  let fixture: ComponentFixture<UsuarioClaveDialog>;

  const usuario = {
    publicId: 'a-guid',
    rut: '11111111-1',
    nombre: 'Test',
    apellido: 'Usuario',
    email: null,
    rolId: 1,
    rolNombre: 'Administrador',
    sucursalActivaId: null,
    activo: true,
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [UsuarioClaveDialog],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: MatDialogRef, useValue: { close: () => undefined } },
        { provide: MAT_DIALOG_DATA, useValue: { usuario } satisfies UsuarioClaveDialogData },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(UsuarioClaveDialog);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
