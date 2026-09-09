import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter } from '@angular/router';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';

import { OrdenesDeTrabajoList } from './ordenes-de-trabajo-list';

describe('OrdenesDeTrabajoList', () => {
  let component: OrdenesDeTrabajoList;
  let fixture: ComponentFixture<OrdenesDeTrabajoList>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [OrdenesDeTrabajoList, NoopAnimationsModule],
      providers: [provideRouter([]), provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();

    fixture = TestBed.createComponent(OrdenesDeTrabajoList);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
