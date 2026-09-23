import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';

import { Shell } from './shell';

describe('Shell', () => {
  let component: Shell;
  let fixture: ComponentFixture<Shell>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Shell],
      // El selector de sucursal del constructor de Shell (sesión 2026-09-15) llama a
      // Sucursales.listar() vía HttpClient — sin provideHttpClientTesting() esa llamada sale
      // como una petición HTTP real que falla de forma asíncrona tras terminar el test
      // ("should create"), quedando como un Unhandled Error/Uncaught Exception que ensucia la
      // salida de `npm test` sin hacer fallar ningún test.
      providers: [provideRouter([]), provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();

    fixture = TestBed.createComponent(Shell);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
