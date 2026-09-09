import { TestBed } from '@angular/core/testing';

import { Anamnesis } from './anamnesis';

describe('Anamnesis', () => {
  let service: Anamnesis;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(Anamnesis);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
