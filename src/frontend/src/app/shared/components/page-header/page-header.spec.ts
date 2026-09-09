import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';

import { PageHeader } from './page-header';

describe('PageHeader', () => {
  let fixture: ComponentFixture<PageHeader>;

  beforeEach(async () => {
    TestBed.configureTestingModule({
      imports: [PageHeader],
      providers: [provideRouter([])],
    });
    fixture = TestBed.createComponent(PageHeader);
    fixture.componentRef.setInput('title', 'Clientes');
    await fixture.whenStable();
  });

  it('muestra el título', () => {
    const titulo = fixture.nativeElement.querySelector('.page-header__titulo');
    expect(titulo.textContent).toContain('Clientes');
  });

  it('no muestra el botón de volver sin backTo', () => {
    expect(fixture.nativeElement.querySelector('.page-header__volver')).toBeNull();
  });

  it('muestra el botón de volver con backTo', async () => {
    fixture.componentRef.setInput('backTo', '/clientes');
    await fixture.whenStable();
    expect(fixture.nativeElement.querySelector('.page-header__volver')).not.toBeNull();
  });
});
