import { TestBed } from '@angular/core/testing';

import { RecetaCristales } from './receta-cristales';

describe('RecetaCristales', () => {
  let service: RecetaCristales;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(RecetaCristales);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
