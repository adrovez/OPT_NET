import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter } from '@angular/router';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';

import { PagosRegistro } from './pagos-registro';

describe('PagosRegistro', () => {
  let component: PagosRegistro;
  let fixture: ComponentFixture<PagosRegistro>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PagosRegistro, NoopAnimationsModule],
      providers: [provideRouter([]), provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();

    fixture = TestBed.createComponent(PagosRegistro);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('parte pidiendo elegir la orden de trabajo', () => {
    expect((fixture.nativeElement as HTMLElement).textContent).toContain('Elige la orden');
  });
});
