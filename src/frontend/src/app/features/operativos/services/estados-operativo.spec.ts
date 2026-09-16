import { TestBed } from '@angular/core/testing';

import { EstadosOperativo } from './estados-operativo';

describe('EstadosOperativo', () => {
  let service: EstadosOperativo;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(EstadosOperativo);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
