import { TestBed } from '@angular/core/testing';

import { Empresas } from './empresas';

describe('Empresas', () => {
  let service: Empresas;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(Empresas);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
