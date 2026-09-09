import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';

import { SucursalesList } from './sucursales-list';

describe('SucursalesList', () => {
  let component: SucursalesList;
  let fixture: ComponentFixture<SucursalesList>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SucursalesList],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();

    fixture = TestBed.createComponent(SucursalesList);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
