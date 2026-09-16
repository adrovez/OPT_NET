import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';

import { ORDEN_TICKET } from '../../models/orden-de-trabajo.fixture';
import { ImprimirTicketDialog } from './imprimir-ticket-dialog';

describe('ImprimirTicketDialog', () => {
  let component: ImprimirTicketDialog;
  let fixture: ComponentFixture<ImprimirTicketDialog>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ImprimirTicketDialog, NoopAnimationsModule],
      providers: [
        { provide: MatDialogRef, useValue: { close: () => undefined } },
        { provide: MAT_DIALOG_DATA, useValue: { orden: ORDEN_TICKET } },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(ImprimirTicketDialog);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('muestra el N° de la orden a reimprimir', () => {
    expect((fixture.nativeElement as HTMLElement).textContent).toContain('17067');
  });
});
