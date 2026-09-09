import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter } from '@angular/router';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';

import { AbonosRegistro } from './abonos-registro';

describe('AbonosRegistro', () => {
  let component: AbonosRegistro;
  let fixture: ComponentFixture<AbonosRegistro>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AbonosRegistro, NoopAnimationsModule],
      providers: [provideRouter([]), provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();

    fixture = TestBed.createComponent(AbonosRegistro);
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
