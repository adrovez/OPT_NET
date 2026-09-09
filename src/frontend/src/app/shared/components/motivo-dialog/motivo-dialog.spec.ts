import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';

import { MotivoDialog, MotivoDialogData } from './motivo-dialog';

const datos: MotivoDialogData = {
  titulo: 'Anular orden',
  mensaje: '¿Anular la OT N° 1?',
  etiqueta: 'Motivo de la anulación',
};

describe('MotivoDialog', () => {
  let component: MotivoDialog;
  let fixture: ComponentFixture<MotivoDialog>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [MotivoDialog, NoopAnimationsModule],
      providers: [
        { provide: MatDialogRef, useValue: { close: () => undefined } },
        { provide: MAT_DIALOG_DATA, useValue: datos },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(MotivoDialog);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('exige el motivo antes de confirmar', () => {
    expect((fixture.nativeElement as HTMLElement).querySelector('textarea')).toBeTruthy();
  });
});
