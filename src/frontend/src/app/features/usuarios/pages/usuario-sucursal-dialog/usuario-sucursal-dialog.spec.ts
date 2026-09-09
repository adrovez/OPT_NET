import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';

import { UsuarioSucursalDialog, UsuarioSucursalDialogData } from './usuario-sucursal-dialog';

describe('UsuarioSucursalDialog', () => {
  let component: UsuarioSucursalDialog;
  let fixture: ComponentFixture<UsuarioSucursalDialog>;

  const dialogData: UsuarioSucursalDialogData = {
    usuario: {
      publicId: 'a-guid',
      rut: '11111111-1',
      nombre: 'Test',
      apellido: 'Usuario',
      email: null,
      rolId: 1,
      rolNombre: 'Administrador',
      sucursalActivaId: null,
      activo: true,
    },
    sucursales: [],
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [UsuarioSucursalDialog],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: MatDialogRef, useValue: { close: () => undefined } },
        { provide: MAT_DIALOG_DATA, useValue: dialogData },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(UsuarioSucursalDialog);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
