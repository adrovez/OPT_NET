import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter } from '@angular/router';

import { ClienteFicha } from './cliente-ficha';

describe('ClienteFicha', () => {
  let component: ClienteFicha;
  let fixture: ComponentFixture<ClienteFicha>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ClienteFicha],
      providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([])],
    }).compileComponents();

    fixture = TestBed.createComponent(ClienteFicha);
    fixture.componentRef.setInput('publicId', 'cliente-1');
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
