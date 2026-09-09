import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SearchBox } from './search-box';

const esperar = (ms: number) => new Promise((resolve) => setTimeout(resolve, ms));

describe('SearchBox', () => {
  let component: SearchBox;
  let fixture: ComponentFixture<SearchBox>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SearchBox],
    }).compileComponents();

    fixture = TestBed.createComponent(SearchBox);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('emite el término trimmeado tras el debounce', async () => {
    const emitidos: string[] = [];
    component.buscar.subscribe((v) => emitidos.push(v));

    component['control'].setValue('  ana  ');
    await esperar(450);

    expect(emitidos).toEqual(['ana']);
  });

  it('al limpiar emite cadena vacía de inmediato', async () => {
    const emitidos: string[] = [];
    component.buscar.subscribe((v) => emitidos.push(v));

    component['control'].setValue('ana');
    await esperar(450);
    component['limpiar']();

    expect(emitidos).toEqual(['ana', '']);
  });
});
