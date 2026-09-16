import { TestBed } from '@angular/core/testing';

import { Operativos } from './operativos';

describe('Operativos', () => {
  let service: Operativos;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(Operativos);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
