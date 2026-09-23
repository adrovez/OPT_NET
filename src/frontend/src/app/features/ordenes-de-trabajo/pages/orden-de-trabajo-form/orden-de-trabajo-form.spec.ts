import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { FormGroup } from '@angular/forms';
import { WritableSignal } from '@angular/core';
import { provideRouter } from '@angular/router';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';

import { OrdenDeTrabajoForm } from './orden-de-trabajo-form';

/** Los miembros del componente son `protected` (solo los usa su plantilla). */
interface Interno {
  formPago: FormGroup;
  lineas: WritableSignal<{ productoId: number; cantidad: number; valorUnitario: number }[]>;
  busquedaCliente: WritableSignal<string>;
  rutTecleado: () => string | null;
}

describe('OrdenDeTrabajoForm', () => {
  let component: OrdenDeTrabajoForm;
  let interno: Interno;
  let fixture: ComponentFixture<OrdenDeTrabajoForm>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [OrdenDeTrabajoForm, NoopAnimationsModule],
      providers: [provideRouter([]), provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();

    fixture = TestBed.createComponent(OrdenDeTrabajoForm);
    component = fixture.componentInstance;
    interno = component as unknown as Interno;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('reconoce el RUT tecleado para ofrecer el alta del cliente', () => {
    interno.busquedaCliente.set('12.345.678-9');
    expect(interno.rutTecleado()).toBe('12.345.678-9');

    interno.busquedaCliente.set('juana');
    expect(interno.rutTecleado()).toBeNull();
  });

  it('exige elegir una modalidad de pago cuando hay saldo pendiente (ADR 0010)', async () => {
    interno.lineas.set([{ productoId: 1, cantidad: 1, valorUnitario: 45000 }]);
    await fixture.whenStable();

    // Sin modalidad elegida, `modalidadPago` (required) mantiene el grupo inválido — nada se
    // fuerza a "paga el total" por defecto (ADR 0010, observación 6).
    expect(interno.formPago.controls['modalidadPago'].hasError('required')).toBe(true);
    expect(interno.formPago.valid).toBe(false);

    // "Paga el total" es la única modalidad que deja saldo en 0 sin pedir un plan de cuotas:
    // el `effect` de la modalidad fija `abonoInicial` al total y limpia cuotas/vencimiento.
    interno.formPago.patchValue({ modalidadPago: 'total' });
    await fixture.whenStable();

    expect(interno.formPago.hasError('planRequerido')).toBe(false);
    expect(interno.formPago.valid).toBe(true);
  });

  it('la modalidad "cuotas" exige número de cuotas y primer vencimiento', async () => {
    interno.lineas.set([{ productoId: 1, cantidad: 1, valorUnitario: 45000 }]);
    await fixture.whenStable();

    interno.formPago.patchValue({ modalidadPago: 'cuotas' });
    await fixture.whenStable();

    expect(interno.formPago.hasError('planRequerido')).toBe(true);

    interno.formPago.patchValue({ numeroCuotas: 3 });
    expect(interno.formPago.hasError('vencimientoRequerido')).toBe(true);

    interno.formPago.patchValue({ primerVencimiento: new Date() });
    expect(interno.formPago.valid).toBe(true);
  });
});
