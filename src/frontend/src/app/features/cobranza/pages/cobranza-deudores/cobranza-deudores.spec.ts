import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter } from '@angular/router';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';

import { CobranzaDeudores } from './cobranza-deudores';

describe('CobranzaDeudores', () => {
  let component: CobranzaDeudores;
  let fixture: ComponentFixture<CobranzaDeudores>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CobranzaDeudores, NoopAnimationsModule],
      providers: [provideRouter([]), provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();

    fixture = TestBed.createComponent(CobranzaDeudores);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
