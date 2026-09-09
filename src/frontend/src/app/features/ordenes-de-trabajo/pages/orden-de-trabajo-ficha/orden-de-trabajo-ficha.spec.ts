import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter } from '@angular/router';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';

import { OrdenDeTrabajoFicha } from './orden-de-trabajo-ficha';

describe('OrdenDeTrabajoFicha', () => {
  let component: OrdenDeTrabajoFicha;
  let fixture: ComponentFixture<OrdenDeTrabajoFicha>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [OrdenDeTrabajoFicha, NoopAnimationsModule],
      providers: [provideRouter([]), provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();

    fixture = TestBed.createComponent(OrdenDeTrabajoFicha);
    component = fixture.componentInstance;
    fixture.componentRef.setInput('publicId', '11111111-1111-1111-1111-111111111111');
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
