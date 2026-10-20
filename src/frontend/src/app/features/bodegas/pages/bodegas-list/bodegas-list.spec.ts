import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter } from '@angular/router';

import { BodegasList } from './bodegas-list';

describe('BodegasList', () => {
  let component: BodegasList;
  let fixture: ComponentFixture<BodegasList>;
  let http: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [BodegasList],
      providers: [provideRouter([]), provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();

    http = TestBed.inject(HttpTestingController);
    fixture = TestBed.createComponent(BodegasList);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('consulta /bodegas paginado al entrar', () => {
    const peticion = http.expectOne((r) => r.url.endsWith('/bodegas'));
    expect(peticion.request.params.get('pagina')).toBe('1');
    peticion.flush({ items: [], pagina: 1, tamanioPagina: 20, total: 0 });
  });
});
