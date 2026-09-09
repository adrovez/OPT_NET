import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';

import { SelectorOrden } from './selector-orden';

describe('SelectorOrden', () => {
  let component: SelectorOrden;
  let fixture: ComponentFixture<SelectorOrden>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SelectorOrden, NoopAnimationsModule],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();

    fixture = TestBed.createComponent(SelectorOrden);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
