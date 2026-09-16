import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';

import { OperativosList } from './operativos-list';

describe('OperativosList', () => {
  let component: OperativosList;
  let fixture: ComponentFixture<OperativosList>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [OperativosList],
      providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([])],
    }).compileComponents();

    fixture = TestBed.createComponent(OperativosList);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
