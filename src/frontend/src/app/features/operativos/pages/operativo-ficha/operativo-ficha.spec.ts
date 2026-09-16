import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';

import { OperativoFicha } from './operativo-ficha';

describe('OperativoFicha', () => {
  let component: OperativoFicha;
  let fixture: ComponentFixture<OperativoFicha>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [OperativoFicha],
      providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([])],
    }).compileComponents();

    fixture = TestBed.createComponent(OperativoFicha);
    component = fixture.componentInstance;
    fixture.componentRef.setInput('publicId', '00000000-0000-0000-0000-000000000001');
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
