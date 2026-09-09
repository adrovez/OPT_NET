import { TestBed } from '@angular/core/testing';

import { Regiones } from './regiones';

describe('Regiones', () => {
  let service: Regiones;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(Regiones);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
