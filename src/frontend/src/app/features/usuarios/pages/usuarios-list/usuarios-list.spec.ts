import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';

import { UsuariosList } from './usuarios-list';

describe('UsuariosList', () => {
  let component: UsuariosList;
  let fixture: ComponentFixture<UsuariosList>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [UsuariosList],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();

    fixture = TestBed.createComponent(UsuariosList);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
